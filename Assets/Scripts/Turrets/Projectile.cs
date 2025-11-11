using UnityEngine;

namespace GravityDefenders
{
    public class Projectile : MonoBehaviour
    {
        [Header("Projectile Stats")]
        [SerializeField] private float speed = 70f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float explosionRadius = 0f; // Set > 0 for AoE

        public static float damageMultiplier = 1f; // Global multiplier for all projectiles

        private Transform target;

        public void Configure(float newSpeed, int newDamage, float newExplosionRadius)
        {
            speed = Mathf.Max(0.1f, newSpeed);
            damage = Mathf.Max(0, newDamage);
            explosionRadius = Mathf.Max(0f, newExplosionRadius);
        }

        public void Seek(Transform _target)
        {
            target = _target;
        }

        void Update()
        {
            if (target == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 dir = target.position - transform.position;
            float distanceThisFrame = speed * Time.deltaTime;

            // A simple arcing motion can be simulated by adding an upward force that diminishes
            // This is a simple visual effect and not a true ballistic arc.
            // For a real arc, a more complex calculation would be needed.
            transform.LookAt(target);

            if (dir.magnitude <= distanceThisFrame)
            {
                HitTarget();
                return;
            }

            transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        }

        void HitTarget()
        {
            // TODO: Add explosion visual effect here

            if (explosionRadius > 0f)
            {
                // Area of Effect Damage
                Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
                foreach (Collider collider in colliders)
                {
                    if (collider.CompareTag("Enemy"))
                    {
                        IDamageable damageable = collider.GetComponent<IDamageable>();
                        if (damageable != null)
                        {
                            damageable.TakeDamage(Mathf.RoundToInt(damage * damageMultiplier));
                        }
                    }
                }
            }
            else
            {
                // Single Target Damage
                if (target != null)
                {
                    IDamageable damageable = target.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(Mathf.RoundToInt(damage * damageMultiplier));
                    }
                }
            }

            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
