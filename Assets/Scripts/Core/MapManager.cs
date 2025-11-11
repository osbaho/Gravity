using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    public class MapManager : MonoBehaviour
    {
        public static MapManager Instance { get; private set; }

        [Header("Gravitational Wave")]
        [SerializeField] private float timeBetweenWaves = 60f;

        [Header("Shield Expansion")]
        [SerializeField] private TurretCost shieldExpansionCost;

        public event System.Action<float> GravitationalWaveTimerTick;
        public event System.Action GravitationalWaveTriggered;

        [SerializeField] private UnityEvent<float> gravitationalWaveTimerTickEvent = new UnityEvent<float>();
        [SerializeField] private UnityEvent gravitationalWaveTriggeredEvent = new UnityEvent();

        public UnityEvent<float> GravitationalWaveTimerTickEvent => gravitationalWaveTimerTickEvent;
        public UnityEvent GravitationalWaveTriggeredEvent => gravitationalWaveTriggeredEvent;
    public TurretCost ShieldExpansionCost => shieldExpansionCost;

    private readonly List<MapZone> allZones = new List<MapZone>();
    private MapZone centralZone;
        private float waveTimer;

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
            LoadZones();
            if (allZones.Count == 0) return;

            centralZone?.SetShielded(true);
            waveTimer = timeBetweenWaves;
            GravitationalWaveTimerTick?.Invoke(waveTimer);
            gravitationalWaveTimerTickEvent.Invoke(waveTimer);
        }

        void Update()
        {
            if (timeBetweenWaves <= 0f || allZones.Count == 0) return;

            waveTimer -= Time.deltaTime;
            float clamped = Mathf.Max(0f, waveTimer);
            GravitationalWaveTimerTick?.Invoke(clamped);
            gravitationalWaveTimerTickEvent.Invoke(clamped);

            if (waveTimer <= 0f)
            {
                TriggerGravitationalWave();
                waveTimer = timeBetweenWaves;
            }
        }

        private void LoadZones()
        {
            allZones.Clear();
            centralZone = null;

            MapZone[] zonesInScene = FindObjectsByType<MapZone>(FindObjectsSortMode.None);
            if (zonesInScene.Length == 0)
            {
                Debug.LogError("MapManager: No MapZones found in the scene.");
                return;
            }

            foreach (MapZone zone in zonesInScene)
            {
                allZones.Add(zone);
                if (centralZone == null && zone.IsCentralCandidate())
                {
                    centralZone = zone;
                }
            }

            if (centralZone == null)
            {
                Debug.LogWarning("MapManager: Central zone not identified. Ensure one zone calls SetAsCentral in the editor.");
            }
        }

        private void TriggerGravitationalWave()
        {
            Debug.Log("Gravitational wave triggered.");
            GravitationalWaveTriggered?.Invoke();
            gravitationalWaveTriggeredEvent.Invoke();

            List<MapZone> unshieldedZones = allZones.Where(z => !z.IsShielded).ToList();
            if (unshieldedZones.Count <= 1) return;

            List<Vector3> positions = unshieldedZones.Select(z => z.transform.position).ToList();

            for (int i = positions.Count - 1; i > 0; i--)
            {
                int swapIndex = UnityEngine.Random.Range(0, i + 1);
                (positions[i], positions[swapIndex]) = (positions[swapIndex], positions[i]);
            }

            for (int i = 0; i < unshieldedZones.Count; i++)
            {
                unshieldedZones[i].TransformTo(positions[i]);
            }
        }

        public bool TryExpandShieldToZone(MapZone zoneToShield)
        {
            if (zoneToShield == null || zoneToShield.IsShielded)
            {
                Debug.Log("MapManager: Cannot expand shield to this zone.");
                return false;
            }

            if (ResourceManager.Instance == null)
            {
                Debug.LogWarning("MapManager: ResourceManager not available.");
                return false;
            }

            TurretCost cost = shieldExpansionCost;
            if (!ResourceManager.Instance.HasEnoughResources(cost.PrimaryResource, cost.MiningResourceA, cost.MiningResourceB))
            {
                Debug.Log("MapManager: Not enough resources to expand shield.");
                return false;
            }

            ResourceManager.Instance.SpendResources(cost.PrimaryResource, cost.MiningResourceA, cost.MiningResourceB);
            zoneToShield.SetShielded(true);
            Debug.Log($"Shield expanded to {zoneToShield.name}");
            return true;
        }
    }
}

