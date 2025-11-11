using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    [RequireComponent(typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        [Header("Runtime Stats")]
        [SerializeField, Tooltip("Optional starting archetype for this enemy instance.")]
        private EnemyArchetype startingArchetype;
        [SerializeField] private Transform projectileSpawnPoint;

        public float moveSpeed;
        public int damage;
        public int resourceDropAmount;

        public static float globalSlowFactor = 0f; // Global slow applied to all enemies

        protected Health health;
        protected Transform target;

        protected float originalMoveSpeed;
        private Coroutine slowCoroutine;
        private float temporarySlowFactor = 0f; // For individual slowing turret effects
        private bool deathNotified;
        private EnemyArchetype activeArchetype;
        private float attackTimer;

        public event Action<Enemy> Defeated;

        [Serializable]
        private class EnemyDefeatedUnityEvent : UnityEvent<Enemy> { }

        [SerializeField] private EnemyDefeatedUnityEvent defeatedEvent = new EnemyDefeatedUnityEvent();

        public UnityEvent<Enemy> DefeatedEvent => defeatedEvent;

        void Awake()
        {
            health = GetComponent<Health>();
        }

        void Start()
        {
            if (activeArchetype == null && startingArchetype != null)
            {
                Initialize(startingArchetype);
            }
            else
            {
                CacheMovementValues();
            }
        }

        public void Initialize(EnemyArchetype archetype)
        {
            if (archetype == null)
            {
                Debug.LogWarning($"Enemy {name} initialized without an archetype. Stats will remain at default values.");
                CacheMovementValues();
                return;
            }

            activeArchetype = archetype;
            moveSpeed = archetype.BaseMoveSpeed;
            damage = archetype.BaseDamage;
            resourceDropAmount = archetype.BaseResourceDrop;
            CacheMovementValues();

            if (health == null)
            {
                health = GetComponent<Health>();
            }

            if (health != null)
            {
                int adjustedHealth = Mathf.Max(1, Mathf.RoundToInt(archetype.BaseHealth * (1f - Health.globalHealthReductionFactor)));
                health.SetMaxHealth(adjustedHealth);
            }

            attackTimer = archetype.AttackCooldown;

            if (activeArchetype.VisualPrefab != null && activeArchetype.VisualPrefab.GetComponent<Enemy>() == null && transform.childCount == 0)
            {
                GameObject visualInstance = Instantiate(activeArchetype.VisualPrefab, transform);
                visualInstance.transform.localPosition = Vector3.zero;
                visualInstance.transform.localRotation = Quaternion.identity;
            }
        }

        private void CacheMovementValues()
        {
            originalMoveSpeed = moveSpeed;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        void Update()
        {
            UpdateMovement();
            HandleAttack();
        }

        private void UpdateMovement()
        {
            if (target == null) return;

            if (activeArchetype != null && activeArchetype.StopWhenInRange)
            {
                float distance = Vector3.Distance(transform.position, target.position);
                if (distance <= activeArchetype.AttackRange)
                {
                    return;
                }
            }

            Vector3 direction = (target.position - transform.position).normalized;
            float currentMoveSpeed = originalMoveSpeed * (1f - globalSlowFactor) * (1f - temporarySlowFactor);
            transform.position += direction * currentMoveSpeed * Time.deltaTime;
        }

        private void HandleAttack()
        {
            if (activeArchetype == null) return;
            if (activeArchetype.AttackMode == EnemyAttackMode.None) return;
            if (target == null) return;

            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget > activeArchetype.AttackRange)
            {
                attackTimer = Mathf.Min(attackTimer, activeArchetype.AttackCooldown);
                return;
            }

            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f) return;

            switch (activeArchetype.AttackMode)
            {
                case EnemyAttackMode.Melee:
                    ExecuteMeleeAttack();
                    break;
                case EnemyAttackMode.Ranged:
                    ExecuteRangedAttack();
                    break;
            }

            attackTimer = activeArchetype.AttackCooldown;
        }

        private void ExecuteMeleeAttack()
        {
            if (target == null || damage <= 0) return;

            IDamageable damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }

        private void ExecuteRangedAttack()
        {
            if (target == null) return;
            if (activeArchetype == null || activeArchetype.ProjectilePrefab == null)
            {
                Debug.LogWarning($"Enemy {name} attempted ranged attack without a projectile prefab configured.");
                return;
            }

            Vector3 spawnPosition = projectileSpawnPoint != null
                ? projectileSpawnPoint.position
                : transform.position + activeArchetype.ProjectileSpawnOffset;

            Quaternion spawnRotation = Quaternion.LookRotation((target.position - spawnPosition).normalized, Vector3.up);
            GameObject projectileGO = Instantiate(activeArchetype.ProjectilePrefab, spawnPosition, spawnRotation);

            Projectile projectile = projectileGO.GetComponent<Projectile>();
            if (projectile != null)
            {
                projectile.Seek(target);
            }
        }

        public void ApplySlow(float duration, float slowFactor)
        {
            if (slowCoroutine != null)
            {
                StopCoroutine(slowCoroutine);
            }
            slowCoroutine = StartCoroutine(SlowCoroutine(duration, slowFactor));
        }

        private IEnumerator SlowCoroutine(float duration, float slowFactor)
        {
            temporarySlowFactor = slowFactor;
            yield return new WaitForSeconds(duration);
            temporarySlowFactor = 0f;
            slowCoroutine = null;
        }

        internal void NotifyDeath()
        {
            if (deathNotified) return;
            deathNotified = true;
            Defeated?.Invoke(this);
            defeatedEvent.Invoke(this);
        }

        public void ApplyStatScaling(float speedMultiplier, float damageMultiplier)
        {
            if (speedMultiplier > 0f && !Mathf.Approximately(speedMultiplier, 1f))
            {
                originalMoveSpeed *= speedMultiplier;
                moveSpeed = originalMoveSpeed;
            }

            if (damageMultiplier > 0f && !Mathf.Approximately(damageMultiplier, 1f))
            {
                damage = Mathf.Max(1, Mathf.RoundToInt(damage * damageMultiplier));
            }
        }

        public void SetBaseMoveSpeed(float speed)
        {
            speed = Mathf.Max(0f, speed);
            moveSpeed = speed;
            originalMoveSpeed = speed;
        }

        public void SetBaseDamage(int value)
        {
            damage = Mathf.Max(0, value);
        }

        public void SetResourceDropAmount(int amount)
        {
            resourceDropAmount = Mathf.Max(0, amount);
        }
    }
}
