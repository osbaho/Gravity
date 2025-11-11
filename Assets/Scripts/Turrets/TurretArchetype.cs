using UnityEngine;

namespace GravityDefenders
{
    public enum TurretTargetingMode
    {
        ClosestToTurret,
        ClosestToShip,
        FarthestFromShip,
        HighestHealth
    }

    public enum TurretAttackType
    {
        Projectile,
        SlowPulse,
        DirectDamage
    }

    [CreateAssetMenu(fileName = "Turret Archetype", menuName = "Gravity Defenders/Turret Archetype")]
    public class TurretArchetype : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "Turret";

        [Header("Core Stats")]
        [SerializeField] private float range = 15f;
        [SerializeField] private float minRange = 0f;
        [SerializeField] private float turnSpeed = 10f;
        [SerializeField, Tooltip("Attacks per second.")] private float fireRate = 1f;
        [SerializeField] private float targetScanInterval = 0.25f;

        [Header("Targeting")]
        [SerializeField] private string enemyTag = "Enemy";
        [SerializeField] private TurretTargetingMode targetingMode = TurretTargetingMode.ClosestToTurret;

        [Header("Attack")]
        [SerializeField] private TurretAttackType attackType = TurretAttackType.Projectile;
        [SerializeField] private int damagePerShot = 10;

        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 70f;
        [SerializeField] private float projectileExplosionRadius = 0f;
        [SerializeField] private Vector3 muzzleOffset = new Vector3(0f, 1f, 0f);

        [Header("Slow Pulse Settings")]
        [SerializeField, Range(0f, 1f)] private float slowFactor = 0.5f;
        [SerializeField] private float slowDuration = 2f;

        [Header("Direct Damage Settings")]
        [SerializeField] private bool applyGlobalDamageMultiplier = true;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public float Range => Mathf.Max(0f, range);
        public float MinRange => Mathf.Max(0f, minRange);
        public float TurnSpeed => Mathf.Max(0f, turnSpeed);
        public float FireRate => Mathf.Max(0.01f, fireRate);
        public float TargetScanInterval => Mathf.Max(0.05f, targetScanInterval);
        public string EnemyTag => string.IsNullOrWhiteSpace(enemyTag) ? "Enemy" : enemyTag;
        public TurretTargetingMode TargetingMode => targetingMode;
        public TurretAttackType AttackType => attackType;
        public int DamagePerShot => Mathf.Max(0, damagePerShot);
        public GameObject ProjectilePrefab => projectilePrefab;
        public float ProjectileSpeed => Mathf.Max(0.1f, projectileSpeed);
        public float ProjectileExplosionRadius => Mathf.Max(0f, projectileExplosionRadius);
        public Vector3 MuzzleOffset => muzzleOffset;
        public float SlowFactor => Mathf.Clamp01(slowFactor);
        public float SlowDuration => Mathf.Max(0f, slowDuration);
        public bool ApplyGlobalDamageMultiplier => applyGlobalDamageMultiplier;
    }
}
