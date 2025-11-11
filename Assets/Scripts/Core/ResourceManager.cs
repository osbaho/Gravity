using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        public int PrimaryResource { get; private set; }
        public int MiningResourceA { get; private set; }
        public int MiningResourceB { get; private set; }
        public int PermanentCurrency { get; private set; }

        public static float resourceGainOnKillMultiplier = 1f;

        private int totalResourcesInvestedInRun;

        public event System.Action<int> PrimaryResourceChanged;
        public event System.Action<int> MiningResourceAChanged;
        public event System.Action<int> MiningResourceBChanged;
        public event System.Action<int> PermanentCurrencyChanged;

        [SerializeField] private UnityEvent<int> primaryResourceChangedEvent = new UnityEvent<int>();
        [SerializeField] private UnityEvent<int> miningResourceAChangedEvent = new UnityEvent<int>();
        [SerializeField] private UnityEvent<int> miningResourceBChangedEvent = new UnityEvent<int>();
        [SerializeField] private UnityEvent<int> permanentCurrencyChangedEvent = new UnityEvent<int>();

        public UnityEvent<int> PrimaryResourceChangedEvent => primaryResourceChangedEvent;
        public UnityEvent<int> MiningResourceAChangedEvent => miningResourceAChangedEvent;
        public UnityEvent<int> MiningResourceBChangedEvent => miningResourceBChangedEvent;
        public UnityEvent<int> PermanentCurrencyChangedEvent => permanentCurrencyChangedEvent;

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
            NotifyPrimaryResourceChanged();
            NotifyMiningResourceAChanged();
            NotifyMiningResourceBChanged();
            NotifyPermanentCurrencyChanged();
        }

        public void AddResources(int amount)
        {
            if (amount <= 0) return;

            int adjusted = Mathf.RoundToInt(amount * resourceGainOnKillMultiplier);
            PrimaryResource += adjusted;
            Debug.Log($"Added {adjusted} Primary Resource. Total: {PrimaryResource}");
            NotifyPrimaryResourceChanged();
        }

        public void AddMiningResources(int amountA, int amountB)
        {
            if (amountA > 0)
            {
                MiningResourceA += amountA;
                NotifyMiningResourceAChanged();
            }

            if (amountB > 0)
            {
                MiningResourceB += amountB;
                NotifyMiningResourceBChanged();
            }
        }

        public void AddPermanentCurrency(int amount)
        {
            if (amount <= 0) return;

            PermanentCurrency += amount;
            Debug.Log($"Added {amount} Permanent Currency. Total: {PermanentCurrency}");
            NotifyPermanentCurrencyChanged();
        }

        public bool HasEnoughResources(int primary, int miningA, int miningB)
        {
            return PrimaryResource >= primary && MiningResourceA >= miningA && MiningResourceB >= miningB;
        }

        public bool HasEnoughPermanentCurrency(int amount)
        {
            return PermanentCurrency >= amount;
        }

        public void SpendResources(int primary, int miningA, int miningB)
        {
            if (!HasEnoughResources(primary, miningA, miningB)) return;

            PrimaryResource -= primary;
            MiningResourceA -= miningA;
            MiningResourceB -= miningB;
            totalResourcesInvestedInRun += primary + miningA + miningB;
            Debug.Log($"Spent {primary}P, {miningA}A, {miningB}B. Remaining: {PrimaryResource}P, {MiningResourceA}A, {MiningResourceB}B");
            NotifyPrimaryResourceChanged();
            NotifyMiningResourceAChanged();
            NotifyMiningResourceBChanged();
        }

        public void SpendPermanentCurrency(int amount)
        {
            if (!HasEnoughPermanentCurrency(amount)) return;

            PermanentCurrency -= amount;
            Debug.Log($"Spent {amount} Permanent Currency. Remaining: {PermanentCurrency}");
            NotifyPermanentCurrencyChanged();
        }

        public int GetTotalResourcesInvestedInRun()
        {
            return totalResourcesInvestedInRun;
        }

        public void ResetRunResources()
        {
            PrimaryResource = 0;
            MiningResourceA = 0;
            MiningResourceB = 0;
            totalResourcesInvestedInRun = 0;
            NotifyPrimaryResourceChanged();
            NotifyMiningResourceAChanged();
            NotifyMiningResourceBChanged();
        }

        public void SetPermanentCurrency(int amount)
        {
            PermanentCurrency = Mathf.Max(0, amount);
            NotifyPermanentCurrencyChanged();
        }

        public void GrantStartingPrimaryResource(int amount)
        {
            if (amount <= 0) return;
            PrimaryResource += amount;
            NotifyPrimaryResourceChanged();
        }

        private void NotifyPrimaryResourceChanged()
        {
            PrimaryResourceChanged?.Invoke(PrimaryResource);
            primaryResourceChangedEvent.Invoke(PrimaryResource);
        }

        private void NotifyMiningResourceAChanged()
        {
            MiningResourceAChanged?.Invoke(MiningResourceA);
            miningResourceAChangedEvent.Invoke(MiningResourceA);
        }

        private void NotifyMiningResourceBChanged()
        {
            MiningResourceBChanged?.Invoke(MiningResourceB);
            miningResourceBChangedEvent.Invoke(MiningResourceB);
        }

        private void NotifyPermanentCurrencyChanged()
        {
            PermanentCurrencyChanged?.Invoke(PermanentCurrency);
            permanentCurrencyChangedEvent.Invoke(PermanentCurrency);
        }
    }
}

