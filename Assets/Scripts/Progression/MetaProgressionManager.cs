using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace GravityDefenders
{
    public class MetaProgressionManager : MonoBehaviour
    {
        public static MetaProgressionManager Instance { get; private set; }

        [Header("Permanent Upgrades")]
        [SerializeField] private List<PermanentUpgradeData> allPermanentUpgrades;

        [Header("Ship Repair Progress")]
        [SerializeField] private int shipPartsRequiredForVictory = 5;
        [SerializeField] private int shipPartsCollected;

        private readonly List<PermanentUpgradeData> purchasedUpgrades = new List<PermanentUpgradeData>();
        private int startingPrimaryResourceBonus;
        private int startingShipHealthBonus;

    public event System.Action<PermanentUpgradeData> PermanentUpgradePurchased;
    public event System.Action<int, int> ShipPartsProgressChanged;

    [System.Serializable]
    private class PermanentUpgradeUnityEvent : UnityEvent<PermanentUpgradeData> { }

    [SerializeField] private PermanentUpgradeUnityEvent permanentUpgradePurchasedEvent = new PermanentUpgradeUnityEvent();
    [SerializeField] private UnityEvent<int, int> shipPartsProgressChangedEvent = new UnityEvent<int, int>();

    public UnityEvent<PermanentUpgradeData> PermanentUpgradePurchasedEvent => permanentUpgradePurchasedEvent;
    public UnityEvent<int, int> ShipPartsProgressChangedEvent => shipPartsProgressChangedEvent;

        private const float DefaultDamageMultiplier = 1f;
        private const float DefaultFireRateMultiplier = 1f;
        private const float DefaultRangeMultiplier = 1f;
        private const float DefaultMiningYieldMultiplier = 1f;
        private const float DefaultResourceGainMultiplier = 1f;
        private const float DefaultEnemySlowFactor = 0f;
        private const float DefaultEnemyHealthReductionFactor = 0f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            // Load purchased upgrades from PlayerPrefs or a save system
            // For now, we'll assume no upgrades are purchased initially
            ApplyAllPurchasedUpgrades();
            BroadcastShipPartsProgress();
        }

        public void PurchaseUpgrade(PermanentUpgradeData upgrade)
        {
            if (upgrade == null)
            {
                Debug.LogWarning("MetaProgressionManager: Tried to purchase a null upgrade.");
                return;
            }

            if (purchasedUpgrades.Contains(upgrade))
            {
                Debug.Log("MetaProgressionManager: Upgrade already purchased.");
                return;
            }

            if (!ResourceManager.Instance.HasEnoughPermanentCurrency(upgrade.permanentCurrencyCost))
            {
                Debug.Log("Not enough permanent currency to purchase this upgrade.");
                return;
            }

            ResourceManager.Instance.SpendPermanentCurrency(upgrade.permanentCurrencyCost);
            purchasedUpgrades.Add(upgrade);
            ApplyUpgradeEffect(upgrade);
            // TODO: Save purchased upgrades
            Debug.Log($"Purchased permanent upgrade: {upgrade.upgradeName}");
            NotifyPermanentUpgradePurchased(upgrade);
        }

        private void ApplyAllPurchasedUpgrades()
        {
            ResetPersistentEffects();

            foreach (var upgrade in purchasedUpgrades)
            {
                ApplyUpgradeEffect(upgrade);
            }
        }

        public void ReapplyPermanentUpgrades()
        {
            ApplyAllPurchasedUpgrades();
        }

        private void ApplyUpgradeEffect(PermanentUpgradeData upgrade)
        {
            // This will apply the permanent effects at the start of each run
            // These effects are typically applied once and persist.
            switch (upgrade.upgradeType)
            {
                case PermanentUpgradeType.StartingPrimaryResource:
                    startingPrimaryResourceBonus += Mathf.RoundToInt(upgrade.value);
                    break;
                case PermanentUpgradeType.StartingShipHealth:
                    startingShipHealthBonus += Mathf.RoundToInt(upgrade.value);
                    break;
                case PermanentUpgradeType.GlobalTurretDamageBonus:
                    Projectile.damageMultiplier += upgrade.value;
                    Debug.Log($"Permanent turret damage bonus: {upgrade.value}");
                    break;
                case PermanentUpgradeType.GlobalTurretFireRateBonus:
                    Turret.fireRateMultiplier += upgrade.value;
                    Debug.Log($"Permanent turret fire rate bonus: {upgrade.value}");
                    break;
                case PermanentUpgradeType.GlobalTurretRangeBonus:
                    Turret.rangeMultiplier += upgrade.value;
                    Debug.Log($"Permanent turret range bonus: {upgrade.value}");
                    break;
                case PermanentUpgradeType.GlobalMiningYieldBonus:
                    MiningManager.miningYieldMultiplier += upgrade.value;
                    Debug.Log($"Permanent mining yield bonus: {upgrade.value}");
                    break;
                case PermanentUpgradeType.GlobalResourceGainOnKillBonus:
                    ResourceManager.resourceGainOnKillMultiplier += upgrade.value;
                    Debug.Log($"Permanent resource gain on kill bonus: {upgrade.value}");
                    break;
                case PermanentUpgradeType.GlobalEnemySlowBonus:
                    Enemy.globalSlowFactor += upgrade.value;
                    Debug.Log($"Permanent enemy slow bonus: {upgrade.value}");
                    break;
                case PermanentUpgradeType.GlobalEnemyHealthReductionBonus:
                    Health.globalHealthReductionFactor += upgrade.value;
                    Debug.Log($"Permanent enemy health reduction bonus: {upgrade.value}");
                    break;
                default:
                    Debug.LogWarning($"Permanent upgrade type {upgrade.upgradeType} not implemented yet.");
                    break;
            }
        }

        private void ResetPersistentEffects()
        {
            Projectile.damageMultiplier = DefaultDamageMultiplier;
            Turret.fireRateMultiplier = DefaultFireRateMultiplier;
            Turret.rangeMultiplier = DefaultRangeMultiplier;
            MiningManager.miningYieldMultiplier = DefaultMiningYieldMultiplier;
            ResourceManager.resourceGainOnKillMultiplier = DefaultResourceGainMultiplier;
            Enemy.globalSlowFactor = DefaultEnemySlowFactor;
            Health.globalHealthReductionFactor = DefaultEnemyHealthReductionFactor;
            startingPrimaryResourceBonus = 0;
            startingShipHealthBonus = 0;
        }

        public void ApplyRunModifiers(GameManager gameManager)
        {
            if (gameManager == null) return;
            gameManager.SetInitialGameValues(startingPrimaryResourceBonus, startingShipHealthBonus);
        }

        public bool TryRecordShipPart()
        {
            if (shipPartsCollected >= shipPartsRequiredForVictory)
            {
                return false;
            }

            shipPartsCollected++;
            // TODO: Persist ship part progress.
            BroadcastShipPartsProgress();
            return true;
        }

        public bool HasCollectedAllShipParts()
        {
            return shipPartsCollected >= shipPartsRequiredForVictory;
        }

        public int ShipPartsCollected => shipPartsCollected;
        public int ShipPartsRequiredForVictory => shipPartsRequiredForVictory;
        public int StartingPrimaryResourceBonus => startingPrimaryResourceBonus;
        public int StartingShipHealthBonus => startingShipHealthBonus;

        public IReadOnlyList<PermanentUpgradeData> GetAllPermanentUpgrades()
        {
            if (allPermanentUpgrades == null)
            {
                allPermanentUpgrades = new List<PermanentUpgradeData>();
            }

            return allPermanentUpgrades.AsReadOnly();
        }

        public IReadOnlyList<PermanentUpgradeData> GetPurchasedUpgrades()
        {
            return purchasedUpgrades.AsReadOnly();
        }

        public bool IsUpgradePurchased(PermanentUpgradeData upgrade)
        {
            return upgrade != null && purchasedUpgrades.Contains(upgrade);
        }

        private void NotifyPermanentUpgradePurchased(PermanentUpgradeData upgrade)
        {
            PermanentUpgradePurchased?.Invoke(upgrade);
            permanentUpgradePurchasedEvent.Invoke(upgrade);
        }

        private void BroadcastShipPartsProgress()
        {
            ShipPartsProgressChanged?.Invoke(shipPartsCollected, shipPartsRequiredForVictory);
            shipPartsProgressChangedEvent.Invoke(shipPartsCollected, shipPartsRequiredForVictory);
        }
    }
}