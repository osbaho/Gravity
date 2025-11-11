using UnityEngine;

namespace GravityDefenders
{
    public enum EnemyAttackMode
    {
        None,
        Melee,
        Ranged
    }

    [CreateAssetMenu(fileName = "Enemy Archetype", menuName = "Gravity Defenders/Enemy Archetype")]
    public class EnemyArchetype : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "Enemy";

        [Header("Base Stats")]
        [SerializeField] private int baseHealth = 100;
        [SerializeField] private float baseMoveSpeed = 3f;
        [SerializeField] private int baseDamage = 10;
        [SerializeField] private int baseResourceDrop = 5;

        [Header("Behaviour")]
        [SerializeField] private EnemyAttackMode attackMode = EnemyAttackMode.Melee;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private bool stopWhenInRange = false;

        [Header("Ranged Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Vector3 projectileSpawnOffset = new Vector3(0f, 1f, 0f);

        [Header("Presentation (Optional)")]
        [Tooltip("Optional visual prefab to instantiate as a child of the spawned enemy.")]
        [SerializeField] private GameObject visualPrefab;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public int BaseHealth => Mathf.Max(1, baseHealth);
        public float BaseMoveSpeed => Mathf.Max(0f, baseMoveSpeed);
        public int BaseDamage => Mathf.Max(0, baseDamage);
        public int BaseResourceDrop => Mathf.Max(0, baseResourceDrop);
        public EnemyAttackMode AttackMode => attackMode;
        public float AttackRange => Mathf.Max(0f, attackRange);
        public float AttackCooldown => Mathf.Max(0.01f, attackCooldown);
        public bool StopWhenInRange => stopWhenInRange;
        public GameObject ProjectilePrefab => projectilePrefab;
        public Vector3 ProjectileSpawnOffset => projectileSpawnOffset;
        public GameObject VisualPrefab => visualPrefab;
    }
}
