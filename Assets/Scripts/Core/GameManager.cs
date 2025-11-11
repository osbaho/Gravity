using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace GravityDefenders
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game References")]
        [SerializeField] private CentralShip centralShip;

        private bool isGameOver;
        private bool gameWon;
        private int initialPrimaryResourceBonus;
        private int initialShipHealthBonus;

        public Transform CentralShipTransform => centralShip != null ? centralShip.transform : null;
        public CentralShip CentralShip => centralShip;
        public bool ShipPartCollectedThisRun { get; private set; }

        public event System.Action ShipPartCollected;
        public event System.Action RunStarted;

        [SerializeField] private UnityEvent shipPartCollectedEvent = new UnityEvent();
        [SerializeField] private UnityEvent runStartedEvent = new UnityEvent();
        [SerializeField] private UnityEvent gameOverEvent = new UnityEvent();
        [SerializeField] private UnityEvent winEvent = new UnityEvent();

        public UnityEvent ShipPartCollectedEvent => shipPartCollectedEvent;
        public UnityEvent RunStartedEvent => runStartedEvent;
        public UnityEvent GameOverEvent => gameOverEvent;
        public UnityEvent WinEvent => winEvent;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ShipPartCollectedThisRun = false;
            isGameOver = false;
            gameWon = false;
        }

        void Start()
        {
            if (centralShip == null)
            {
                centralShip = FindFirstObjectByType<CentralShip>();
            }

            if (centralShip == null)
            {
                Debug.LogError("GameManager: Central Ship reference is missing!");
                return;
            }

            MetaProgressionManager meta = MetaProgressionManager.Instance;
            meta?.ReapplyPermanentUpgrades();
            meta?.ApplyRunModifiers(this);
            UpgradeManager.ResetRunStateStatic();

            Health shipHealth = centralShip.GetComponent<Health>();
            if (shipHealth != null)
            {
                shipHealth.OnDeath.AddListener(GameOver);
                if (initialShipHealthBonus > 0)
                {
                    shipHealth.AddHealth(initialShipHealthBonus);
                }
            }

            ResourceManager.Instance?.GrantStartingPrimaryResource(initialPrimaryResourceBonus);

            RunStarted?.Invoke();
            runStartedEvent.Invoke();
        }

        public void SetInitialGameValues(int primaryResourceBonus, int shipHealthBonus)
        {
            initialPrimaryResourceBonus = primaryResourceBonus;
            initialShipHealthBonus = shipHealthBonus;
        }

        public bool RegisterShipPartCollected()
        {
            if (ShipPartCollectedThisRun)
            {
                return false;
            }

            ShipPartCollectedThisRun = true;
            Debug.Log("GameManager: Ship part secured for the run.");

            MetaProgressionManager meta = MetaProgressionManager.Instance;
            if (meta != null)
            {
                if (meta.TryRecordShipPart())
                {
                    Debug.Log($"Meta Progression: Ship parts collected {meta.ShipPartsCollected}/{meta.ShipPartsRequiredForVictory}.");
                }

                if (meta.HasCollectedAllShipParts() && !gameWon)
                {
                    WinGame();
                }
            }

            ShipPartCollected?.Invoke();
            shipPartCollectedEvent.Invoke();
            return true;
        }

        public void GameOver()
        {
            if (isGameOver) return;

            isGameOver = true;
            Time.timeScale = 0f;
            Debug.Log("GAME OVER! Your ship was destroyed.");

            int investedResources = ResourceManager.Instance != null ? ResourceManager.Instance.GetTotalResourcesInvestedInRun() : 0;
            int permanentCurrencyGained = Mathf.RoundToInt(investedResources * 0.10f);
            ResourceManager.Instance?.AddPermanentCurrency(permanentCurrencyGained);
            ResourceManager.Instance?.ResetRunResources();

            ShipPartCollectedThisRun = false;

            gameOverEvent.Invoke();

            StartCoroutine(RestartGameAfterDelay(3f));
        }

        public void WinGame()
        {
            if (gameWon) return;

            gameWon = true;
            Time.timeScale = 0f;
            Debug.Log("YOU WON! Ship repaired!");
            winEvent.Invoke();
            // TODO: Display Win UI and handle transition.
        }

        private System.Collections.IEnumerator RestartGameAfterDelay(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
