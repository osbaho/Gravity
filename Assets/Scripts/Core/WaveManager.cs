using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    public class WaveManager : MonoBehaviour
    {
        public static WaveManager Instance { get; private set; }

        [Header("Wave Settings")]
        [SerializeField] private float calmPhaseDuration = 8f;
        [SerializeField] private float spawnInterval = 0.4f;
        [SerializeField] private int baseEnemiesPerWave = 8;
        [SerializeField] private int enemiesPerWaveGrowth = 2;
        [SerializeField] private int wavesPerUpgrade = 3;

        [Header("Ship Part Drops")]
        [SerializeField] private GameObject shipPartPickupPrefab;
        [SerializeField, Range(0f, 1f)] private float shipPartDropChance = 0.04f;

    [Header("Difficulty Scaling")]
    [Tooltip("Additional enemy health per wave (e.g. 0.1 => +10% per wave).")]
    [SerializeField, Range(0f, 0.5f)] private float healthGrowthPerWave = 0.1f;
    [Tooltip("Additional enemy damage per wave (e.g. 0.05 => +5% per wave).")]
    [SerializeField, Range(0f, 0.5f)] private float damageGrowthPerWave = 0.05f;
    [Tooltip("Additional enemy speed per wave (e.g. 0.02 => +2% per wave).")]
    [SerializeField, Range(0f, 0.3f)] private float speedGrowthPerWave = 0.02f;

    [Header("References")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private EnemyArchetype swarmerArchetype;
    [SerializeField] private EnemyArchetype tankArchetype;
    [SerializeField] private EnemyArchetype rangedArchetype;
    [SerializeField] private Transform centralShipOverride;

        public event System.Action<int> WaveStarted;
    public event System.Action<int> WaveCompleted;
    public event System.Action<UpgradeData[]> UpgradeOptionsGenerated;

        [SerializeField] private UnityEvent<int> waveStartedEvent = new UnityEvent<int>();
        [SerializeField] private UnityEvent<int> waveCompletedEvent = new UnityEvent<int>();
        [SerializeField] private UnityEvent<float> calmPhaseEvent = new UnityEvent<float>();
    [System.Serializable]
    private class UpgradeOptionsUnityEvent : UnityEvent<UpgradeData[]> { }
    [SerializeField] private UpgradeOptionsUnityEvent upgradeOptionsGeneratedEvent = new UpgradeOptionsUnityEvent();

        public UnityEvent<int> WaveStartedEvent => waveStartedEvent;
        public UnityEvent<int> WaveCompletedEvent => waveCompletedEvent;
        public UnityEvent<float> CalmPhaseEvent => calmPhaseEvent;
    public UnityEvent<UpgradeData[]> UpgradeOptionsGeneratedEvent => upgradeOptionsGeneratedEvent;

        private int currentWaveIndex;
        private int wavesCleared;
        private int enemiesAlive;
        private bool isSpawning;
        private Transform centralShip;
    private bool shipPartPresentInWorld;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        void Start()
        {
            centralShip = centralShipOverride != null ? centralShipOverride : GameManager.Instance?.CentralShipTransform;

            if (centralShip == null)
            {
                Debug.LogWarning("WaveManager: Central ship reference missing. Assign it in inspector or via GameManager.");
            }

            if (spawnPoints.Count == 0)
            {
                Debug.LogError("WaveManager: No spawn points configured.");
            }

            shipPartPresentInWorld = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShipPartCollected += OnShipPartCollected;
            }

            StartCoroutine(WaveCycle());
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ShipPartCollected -= OnShipPartCollected;
            }
        }

        private IEnumerator WaveCycle()
        {
            while (true)
            {
                calmPhaseEvent.Invoke(calmPhaseDuration);
                yield return new WaitForSeconds(calmPhaseDuration);
                yield return StartCoroutine(SpawnWave());
                yield return new WaitUntil(() => enemiesAlive == 0 && !isSpawning);

                wavesCleared++;
                WaveCompleted?.Invoke(currentWaveIndex);
                waveCompletedEvent.Invoke(currentWaveIndex);

                if (wavesCleared % wavesPerUpgrade == 0)
                {
                    TriggerUpgradeSelection();
                    yield return new WaitUntil(() => Time.timeScale > 0f);
                }
            }
        }

        private IEnumerator SpawnWave()
        {
            isSpawning = true;
            currentWaveIndex++;
            WaveStarted?.Invoke(currentWaveIndex);
            waveStartedEvent.Invoke(currentWaveIndex);
            Debug.Log($"Wave {currentWaveIndex} commencing.");

            WaveComposition composition = BuildCompositionForWave(currentWaveIndex);

            foreach (EnemySpawnOrder order in composition.GenerateOrder())
            {
                SpawnEnemy(order);
                yield return new WaitForSeconds(spawnInterval);
            }

            isSpawning = false;
        }

        private void SpawnEnemy(EnemySpawnOrder order)
        {
            EnemyArchetype archetype = order.archetype;
            if (archetype == null)
            {
                Debug.LogError("WaveManager: Enemy archetype missing from composition.");
                return;
            }

            if (spawnPoints.Count == 0)
            {
                Debug.LogError("WaveManager: Cannot spawn enemies without spawn points.");
                return;
            }

            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
            Enemy enemy = InstantiateEnemy(archetype, spawnPoint.position, spawnPoint.rotation);

            if (enemy == null)
            {
                Debug.LogError($"WaveManager: Failed to instantiate enemy for archetype {archetype.DisplayName}.");
                return;
            }

            enemiesAlive++;

            if (centralShip != null)
            {
                enemy.SetTarget(centralShip);
            }

            enemy.Defeated += HandleEnemyDefeated;

            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth == null)
            {
                Debug.LogWarning($"WaveManager: Enemy spawned for archetype {archetype.DisplayName} lacks a Health component.");
                HandleEnemyDefeated(enemy);
            }
            else
            {
                ApplyDifficultyScaling(enemy, enemyHealth);
            }
        }

        private Enemy InstantiateEnemy(EnemyArchetype archetype, Vector3 position, Quaternion rotation)
        {
            GameObject instance;

            if (archetype.VisualPrefab != null && archetype.VisualPrefab.GetComponent<Enemy>() != null)
            {
                instance = Instantiate(archetype.VisualPrefab, position, rotation);
            }
            else
            {
                instance = new GameObject($"Enemy_{archetype.DisplayName}");
                instance.transform.SetPositionAndRotation(position, rotation);

                if (archetype.VisualPrefab != null)
                {
                    GameObject visual = Instantiate(archetype.VisualPrefab, instance.transform);
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.identity;
                    visual.transform.localScale = Vector3.one;
                }
            }

            Enemy enemyComponent = instance.GetComponent<Enemy>();
            if (enemyComponent == null)
            {
                enemyComponent = instance.AddComponent<Enemy>();
            }

            Health healthComponent = instance.GetComponent<Health>();
            if (healthComponent == null)
            {
                healthComponent = instance.AddComponent<Health>();
            }

            enemyComponent.Initialize(archetype);
            return enemyComponent;
        }

        private void HandleEnemyDefeated(Enemy enemy)
        {
            if (enemy == null) return;

            enemy.Defeated -= HandleEnemyDefeated;
            enemiesAlive = Mathf.Max(0, enemiesAlive - 1);

            if (!isSpawning && enemiesAlive == 0)
            {
                TryDropShipPart(enemy);
            }
        }

        private void TryDropShipPart(Enemy enemy)
        {
            if (currentWaveIndex < 20) return;
            if (shipPartPickupPrefab == null) return;
            if (shipPartPresentInWorld) return;

            GameManager gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.ShipPartCollectedThisRun) return;

            if (UnityEngine.Random.value >= shipPartDropChance) return;

            Vector3 spawnPosition = enemy != null ? enemy.transform.position : (centralShip != null ? centralShip.position : Vector3.zero);
            Instantiate(shipPartPickupPrefab, spawnPosition, Quaternion.identity);
            shipPartPresentInWorld = true;
        }

        internal void NotifyShipPartPickupRemoved()
        {
            shipPartPresentInWorld = false;
        }

        private void OnShipPartCollected()
        {
            shipPartPresentInWorld = false;
        }

        private void ApplyDifficultyScaling(Enemy enemy, Health enemyHealth)
        {
            if (enemy == null || enemyHealth == null) return;
            if (currentWaveIndex <= 1) return;

            int waveOffset = currentWaveIndex - 1;

            float healthMultiplier = 1f + healthGrowthPerWave * waveOffset;
            float damageMultiplier = 1f + damageGrowthPerWave * waveOffset;
            float speedMultiplier = 1f + speedGrowthPerWave * waveOffset;

            enemyHealth.ScaleMaxHealth(healthMultiplier);
            enemy.ApplyStatScaling(speedMultiplier, damageMultiplier);
        }

        private void TriggerUpgradeSelection()
        {
            if (UpgradeManager.Instance == null)
            {
                Debug.LogWarning("WaveManager: Upgrade systems missing.");
                return;
            }

            List<UpgradeData> options = UpgradeManager.Instance.GetRandomUpgrades(3);
            NotifyUpgradeOptionsGenerated(options);
        }

        private WaveComposition BuildCompositionForWave(int waveNumber)
        {
            int totalEnemies = Mathf.Max(1, baseEnemiesPerWave + enemiesPerWaveGrowth * (waveNumber - 1));
            WaveComposition composition = new WaveComposition();

            if (waveNumber <= 3)
            {
                composition.AddGroup(new EnemySpawnGroup(swarmerArchetype, totalEnemies));
                return composition;
            }

            if (waveNumber <= 7)
            {
                int tankCount = Mathf.Max(1, Mathf.RoundToInt(totalEnemies * 0.3f));
                int swarmerCount = Mathf.Max(1, totalEnemies - tankCount);
                composition.AddGroup(new EnemySpawnGroup(swarmerArchetype, swarmerCount));
                composition.AddGroup(new EnemySpawnGroup(tankArchetype, tankCount));
                return composition;
            }

            int rangedCount = Mathf.Max(1, Mathf.RoundToInt(totalEnemies * 0.2f));
            int tankCountAdvance = Mathf.Max(1, Mathf.RoundToInt(totalEnemies * 0.3f));
            int remaining = Mathf.Max(1, totalEnemies - rangedCount - tankCountAdvance);

            composition.AddGroup(new EnemySpawnGroup(swarmerArchetype, remaining));
            composition.AddGroup(new EnemySpawnGroup(tankArchetype, tankCountAdvance));
            composition.AddGroup(new EnemySpawnGroup(rangedArchetype, rangedCount));

            return composition;
        }

        private readonly struct EnemySpawnOrder
        {
            public readonly EnemyArchetype archetype;

            public EnemySpawnOrder(EnemyArchetype archetype)
            {
                this.archetype = archetype;
            }
        }

        private readonly struct EnemySpawnGroup
        {
            public readonly EnemyArchetype archetype;
            public readonly int count;

            public EnemySpawnGroup(EnemyArchetype archetype, int count)
            {
                this.archetype = archetype;
                this.count = count;
            }
        }

        private class WaveComposition
        {
            private readonly List<EnemySpawnGroup> groups = new List<EnemySpawnGroup>();

            public void AddGroup(EnemySpawnGroup group)
            {
                if (group.archetype == null || group.count <= 0) return;
                groups.Add(group);
            }

            public IEnumerable<EnemySpawnOrder> GenerateOrder()
            {
                List<EnemySpawnOrder> orders = new List<EnemySpawnOrder>();

                foreach (EnemySpawnGroup group in groups)
                {
                    for (int i = 0; i < group.count; i++)
                    {
                        orders.Add(new EnemySpawnOrder(group.archetype));
                    }
                }

                // Shuffle to avoid predictable sequencing
                for (int i = orders.Count - 1; i > 0; i--)
                {
                    int j = UnityEngine.Random.Range(0, i + 1);
                    (orders[i], orders[j]) = (orders[j], orders[i]);
                }

                return orders;
            }
        }

        private void NotifyUpgradeOptionsGenerated(List<UpgradeData> options)
        {
            UpgradeData[] optionArray = options?.ToArray() ?? System.Array.Empty<UpgradeData>();
            UpgradeOptionsGenerated?.Invoke(optionArray);
            upgradeOptionsGeneratedEvent.Invoke(optionArray);
        }
    }
}

