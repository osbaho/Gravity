using UnityEngine;

namespace GravityDefenders
{
    public class Turret : MonoBehaviour
    {
        [Header("Archetype Reference")]
        [SerializeField] private TurretArchetype startingArchetype;
        [SerializeField, Tooltip("Optional muzzle override for projectile spawning.")]
        private Transform muzzleOverride;

        public static float fireRateMultiplier = 1f;
        public static float rangeMultiplier = 1f;

        private TurretArchetype activeArchetype;
        private Transform target;
        private float fireTimer;
        private float scanTimer;

        private Transform CentralShip => GameManager.Instance?.CentralShipTransform;
    public TurretArchetype ActiveArchetype => activeArchetype;
    public Transform CurrentTarget => target;

        void Start()
        {
            if (activeArchetype == null && startingArchetype != null)
            {
                Initialize(startingArchetype);
            }
        }

        void Update()
        {
            if (activeArchetype == null) return;

            if (target == null || !target.gameObject.activeInHierarchy)
            {
                target = null;
            }

            scanTimer -= Time.deltaTime;
            if (scanTimer <= 0f)
            {
                ScanForTarget();
                scanTimer = activeArchetype.TargetScanInterval;
            }

            if (target == null) return;

            RotateTowardsTarget();

            if (activeArchetype.FireRate <= 0f) return;

            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                PerformAttack();
                float cooldown = 1f / (activeArchetype.FireRate * Mathf.Max(0.01f, fireRateMultiplier));
                fireTimer = Mathf.Max(0f, cooldown);
            }
        }

        public void Initialize(TurretArchetype archetype)
        {
            activeArchetype = archetype;
            fireTimer = 0f;
            scanTimer = 0f;

            if (activeArchetype == null)
            {
                Debug.LogWarning($"Turret {name} initialized without an archetype.");
            }
        }

        public void SetMuzzleOverride(Transform muzzle)
        {
            muzzleOverride = muzzle;
        }

        private void ScanForTarget()
        {
            string tagToUse = null;

            if (activeArchetype != null)
            {
                tagToUse = activeArchetype.EnemyTag;
            }
            else if (startingArchetype != null)
            {
                tagToUse = startingArchetype.EnemyTag;
            }

            if (string.IsNullOrWhiteSpace(tagToUse))
            {
                return;
            }

            GameObject[] enemies = GameObject.FindGameObjectsWithTag(tagToUse);
            target = SelectTarget(enemies);
        }

        private Transform SelectTarget(GameObject[] enemies)
        {
            if (activeArchetype == null) return null;

            float currentRange = activeArchetype.Range * rangeMultiplier;
            float minRange = activeArchetype.MinRange;
            Transform ship = CentralShip;
            Transform bestTarget = null;
            float bestScore = activeArchetype.TargetingMode == TurretTargetingMode.FarthestFromShip ? float.MinValue : float.MaxValue;
            float highestHealth = -1f;

            foreach (GameObject enemy in enemies)
            {
                float distanceToTurret = Vector3.Distance(transform.position, enemy.transform.position);
                if (distanceToTurret > currentRange) continue;
                if (distanceToTurret < minRange) continue;

                switch (activeArchetype.TargetingMode)
                {
                    case TurretTargetingMode.ClosestToTurret:
                        if (distanceToTurret < bestScore)
                        {
                            bestScore = distanceToTurret;
                            bestTarget = enemy.transform;
                        }
                        break;
                    case TurretTargetingMode.ClosestToShip:
                        if (ship == null)
                        {
                            if (distanceToTurret < bestScore)
                            {
                                bestScore = distanceToTurret;
                                bestTarget = enemy.transform;
                            }
                        }
                        else
                        {
                            float distanceToShip = Vector3.Distance(ship.position, enemy.transform.position);
                            if (distanceToShip < bestScore)
                            {
                                bestScore = distanceToShip;
                                bestTarget = enemy.transform;
                            }
                        }
                        break;
                    case TurretTargetingMode.FarthestFromShip:
                        if (ship == null)
                        {
                            if (distanceToTurret > bestScore)
                            {
                                bestScore = distanceToTurret;
                                bestTarget = enemy.transform;
                            }
                        }
                        else
                        {
                            float distanceToShip = Vector3.Distance(ship.position, enemy.transform.position);
                            if (distanceToShip > bestScore)
                            {
                                bestScore = distanceToShip;
                                bestTarget = enemy.transform;
                            }
                        }
                        break;
                    case TurretTargetingMode.HighestHealth:
                        Health health = enemy.GetComponent<Health>();
                        if (health == null) continue;
                        float currentHealth = health.GetCurrentHealth();
                        if (currentHealth > highestHealth)
                        {
                            highestHealth = currentHealth;
                            bestTarget = enemy.transform;
                        }
                        break;
                }
            }

            return bestTarget;
        }

        private void RotateTowardsTarget()
        {
            if (target == null) return;
            if (activeArchetype.TurnSpeed <= 0f) return;

            Vector3 direction = (target.position - transform.position).normalized;
            if (direction.sqrMagnitude <= 0f) return;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            Vector3 rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * activeArchetype.TurnSpeed).eulerAngles;
            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        }

        private void PerformAttack()
        {
            if (target == null || activeArchetype == null) return;

            switch (activeArchetype.AttackType)
            {
                case TurretAttackType.Projectile:
                    FireProjectile();
                    break;
                case TurretAttackType.SlowPulse:
                    ApplySlowPulse();
                    break;
                case TurretAttackType.DirectDamage:
                    ApplyDirectDamage();
                    break;
            }
        }

        private void FireProjectile()
        {
            Vector3 muzzlePosition = muzzleOverride != null ? muzzleOverride.position : transform.position + activeArchetype.MuzzleOffset;
            Quaternion muzzleRotation = Quaternion.identity;
            if (target != null)
            {
                Vector3 dir = (target.position - muzzlePosition).normalized;
                if (dir.sqrMagnitude > 0f)
                {
                    muzzleRotation = Quaternion.LookRotation(dir, Vector3.up);
                }
            }

            Projectile projectileComponent = null;

            if (activeArchetype.ProjectilePrefab != null)
            {
                GameObject projectileInstance = Instantiate(activeArchetype.ProjectilePrefab, muzzlePosition, muzzleRotation);
                projectileComponent = projectileInstance.GetComponent<Projectile>();
                if (projectileComponent == null)
                {
                    projectileComponent = projectileInstance.AddComponent<Projectile>();
                }
            }
            else
            {
                GameObject projectileInstance = new GameObject($"Projectile_{name}");
                projectileInstance.transform.SetPositionAndRotation(muzzlePosition, muzzleRotation);
                projectileComponent = projectileInstance.AddComponent<Projectile>();
            }

            if (projectileComponent != null)
            {
                projectileComponent.Configure(activeArchetype.ProjectileSpeed, activeArchetype.DamagePerShot, activeArchetype.ProjectileExplosionRadius);
                projectileComponent.Seek(target);
            }
        }

        private void ApplySlowPulse()
        {
            Enemy enemy = target != null ? target.GetComponent<Enemy>() : null;
            if (enemy != null)
            {
                enemy.ApplySlow(activeArchetype.SlowDuration, activeArchetype.SlowFactor);
            }

            if (activeArchetype.DamagePerShot > 0)
            {
                IDamageable damageable = target != null ? target.GetComponent<IDamageable>() : null;
                if (damageable != null)
                {
                    int damage = activeArchetype.DamagePerShot;
                    if (activeArchetype.ApplyGlobalDamageMultiplier)
                    {
                        damage = Mathf.RoundToInt(damage * Projectile.damageMultiplier);
                    }

                    damageable.TakeDamage(Mathf.Max(1, damage));
                }
            }
        }

        private void ApplyDirectDamage()
        {
            if (activeArchetype.DamagePerShot <= 0) return;

            IDamageable damageable = target != null ? target.GetComponent<IDamageable>() : null;
            if (damageable == null) return;

            int damage = activeArchetype.DamagePerShot;
            if (activeArchetype.ApplyGlobalDamageMultiplier)
            {
                damage = Mathf.RoundToInt(damage * Projectile.damageMultiplier);
            }

            damageable.TakeDamage(Mathf.Max(1, damage));
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            float radius = activeArchetype != null ? activeArchetype.Range : (startingArchetype != null ? startingArchetype.Range : 15f);
            Gizmos.DrawWireSphere(transform.position, radius * rangeMultiplier);
        }
    }
}
