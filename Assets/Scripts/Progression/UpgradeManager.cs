using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [Header("Upgrades")]
        [SerializeField] private List<UpgradeData> allUpgrades;

        private UpgradeData lastSelectedUpgrade;

    public event System.Action<UpgradeData[]> UpgradeOptionsGenerated;
    public event System.Action<UpgradeData> UpgradeApplied;

    [System.Serializable]
    private class UpgradeOptionsUnityEvent : UnityEvent<UpgradeData[]> { }

    [SerializeField] private UpgradeOptionsUnityEvent upgradeOptionsGeneratedEvent = new UpgradeOptionsUnityEvent();
    [SerializeField] private UnityEvent<UpgradeData> upgradeAppliedEvent = new UnityEvent<UpgradeData>();

    public UnityEvent<UpgradeData[]> UpgradeOptionsGeneratedEvent => upgradeOptionsGeneratedEvent;
    public UnityEvent<UpgradeData> UpgradeAppliedEvent => upgradeAppliedEvent;

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
            if (allUpgrades == null)
            {
                allUpgrades = new List<UpgradeData>();
            }
        }

        public List<UpgradeData> GetRandomUpgrades(int count)
        {
            List<UpgradeData> pool = new List<UpgradeData>();
            if (allUpgrades == null || allUpgrades.Count == 0) return pool;

            foreach (UpgradeData upgrade in allUpgrades)
            {
                if (upgrade == null) continue;
                if (lastSelectedUpgrade != null && upgrade == lastSelectedUpgrade) continue;
                pool.Add(upgrade);
            }

            if (pool.Count < count)
            {
                pool = allUpgrades.Where(u => u != null).ToList();
            }

            List<UpgradeData> selection = new List<UpgradeData>();
            System.Random rng = new System.Random();

            while (selection.Count < count && pool.Count > 0)
            {
                int index = rng.Next(pool.Count);
                selection.Add(pool[index]);
                pool.RemoveAt(index);
            }

            NotifyUpgradeOptionsGenerated(selection);
            return selection;
        }

        public void ApplyUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null)
            {
                Debug.LogWarning("UpgradeManager: Attempted to apply a null upgrade.");
                return;
            }

            Debug.Log($"Applying upgrade: {upgrade.upgradeName}");
            lastSelectedUpgrade = upgrade;

            // Apply upgrade effects by adjusting shared gameplay multipliers.
            switch (upgrade.upgradeType)
            {
                case UpgradeType.TurretDamage:
                    Projectile.damageMultiplier += upgrade.value;
                    Debug.Log($"Increased turret damage multiplier to {Projectile.damageMultiplier}");
                    break;
                case UpgradeType.TurretFireRate:
                    Turret.fireRateMultiplier += upgrade.value;
                    Debug.Log($"Increased turret fire rate multiplier to {Turret.fireRateMultiplier}");
                    break;
                case UpgradeType.TurretRange:
                    Turret.rangeMultiplier += upgrade.value;
                    Debug.Log($"Increased turret range multiplier to {Turret.rangeMultiplier}");
                    break;
                case UpgradeType.MiningYield:
                    MiningManager.miningYieldMultiplier += upgrade.value;
                    Debug.Log($"Increased mining yield multiplier to {MiningManager.miningYieldMultiplier}");
                    break;
                case UpgradeType.ResourceGainOnKill:
                    ResourceManager.resourceGainOnKillMultiplier += upgrade.value;
                    Debug.Log($"Increased resource gain on kill multiplier to {ResourceManager.resourceGainOnKillMultiplier}");
                    break;
                case UpgradeType.GlobalEnemySlow:
                    Enemy.globalSlowFactor += upgrade.value;
                    Debug.Log($"Increased global enemy slow factor to {Enemy.globalSlowFactor}");
                    break;
                case UpgradeType.GlobalEnemyHealthReduction:
                    Health.globalHealthReductionFactor += upgrade.value;
                    Debug.Log($"Increased global enemy health reduction factor to {Health.globalHealthReductionFactor}");
                    break;
                default:
                    Debug.LogWarning($"Upgrade type {upgrade.upgradeType} not implemented yet.");
                    break;
            }

            NotifyUpgradeApplied(upgrade);
        }

        public void ResetRunState()
        {
            lastSelectedUpgrade = null;
        }

        public static void ResetRunStateStatic()
        {
            Instance?.ResetRunState();
        }

        private void NotifyUpgradeOptionsGenerated(List<UpgradeData> options)
        {
            UpgradeData[] optionArray = options?.ToArray() ?? System.Array.Empty<UpgradeData>();
            UpgradeOptionsGenerated?.Invoke(optionArray);
            upgradeOptionsGeneratedEvent.Invoke(optionArray);
        }

        private void NotifyUpgradeApplied(UpgradeData upgrade)
        {
            UpgradeApplied?.Invoke(upgrade);
            upgradeAppliedEvent.Invoke(upgrade);
        }
    }
}