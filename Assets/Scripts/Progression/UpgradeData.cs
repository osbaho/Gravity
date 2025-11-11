using UnityEngine;

namespace GravityDefenders
{
    public enum UpgradeType
    {
        // Turret Buffs
        TurretDamage,
        TurretFireRate,
        TurretRange,

        // Economy Buffs
        MiningYield,
        ResourceGainOnKill,

        // Enemy Debuffs
        GlobalEnemySlow,
        GlobalEnemyHealthReduction
    }

    [CreateAssetMenu(fileName = "New Upgrade", menuName = "Gravity Defenders/Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Info")]
        public string upgradeName;
        [TextArea]
        public string description;

        [Header("Effect")]
        public UpgradeType upgradeType;
        public float value; // The magnitude of the effect (e.g., 0.1 for 10%)
    }
}
