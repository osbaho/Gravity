using UnityEngine;

namespace GravityDefenders
{
    public enum PermanentUpgradeType
    {
        StartingPrimaryResource,
        StartingShipHealth,
        GlobalTurretDamageBonus,
        GlobalTurretFireRateBonus,
        GlobalTurretRangeBonus,
        GlobalMiningYieldBonus,
        GlobalResourceGainOnKillBonus,
        GlobalEnemySlowBonus,
        GlobalEnemyHealthReductionBonus
    }

    [CreateAssetMenu(fileName = "New Permanent Upgrade", menuName = "Gravity Defenders/Permanent Upgrade")]
    public class PermanentUpgradeData : ScriptableObject
    {
        [Header("Info")]
        public string upgradeName;
        [TextArea]
        public string description;

        [Header("Effect")]
        public PermanentUpgradeType upgradeType;
        public float value; // The magnitude of the effect (e.g., 0.1 for 10%)

        [Header("Cost")]
        public int permanentCurrencyCost;
    }
}
