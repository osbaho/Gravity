using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Health")]
        [SerializeField] private int maxHealth = 100;
        public static float globalHealthReductionFactor = 0f; // Global reduction applied to all enemies
        private int currentHealth;
        private Enemy enemy;

        public event System.Action<int, int> HealthChanged;

        [System.Serializable]
        private class HealthChangedUnityEvent : UnityEvent<int, int> { }

        [SerializeField] private HealthChangedUnityEvent onHealthChanged = new HealthChangedUnityEvent();
        [SerializeField]
        public UnityEvent OnDeath = new UnityEvent();

        public UnityEvent<int, int> OnHealthChangedEvent => onHealthChanged;
    public int MaxHealth => maxHealth;

        void Awake()
        {
            enemy = GetComponent<Enemy>();
            currentHealth = enemy != null
                ? Mathf.Max(1, Mathf.RoundToInt(maxHealth * (1f - globalHealthReductionFactor)))
                : maxHealth;

            NotifyHealthChanged();
        }

        public int GetCurrentHealth()
        {
            return currentHealth;
        }

        public void AddHealth(int amount)
        {
            if (amount <= 0) return;

            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            NotifyHealthChanged();
        }

        public void TakeDamage(int amount)
        {
            if (amount <= 0 || currentHealth <= 0) return;

            currentHealth -= amount;
            if (currentHealth > 0)
            {
                NotifyHealthChanged();
                return;
            }

            currentHealth = 0;
            NotifyHealthChanged();

            if (enemy != null)
            {
                ResourceManager.Instance?.AddResources(enemy.resourceDropAmount);
                enemy.NotifyDeath();
            }

            OnDeath.Invoke();
            // TODO: Add death feedback.
            Destroy(gameObject);
        }

        public void ScaleMaxHealth(float multiplier)
        {
            if (multiplier <= 0f) return;

            int newMax = Mathf.Max(1, Mathf.RoundToInt(maxHealth * multiplier));
            float healthRatio = maxHealth > 0 ? (float)currentHealth / maxHealth : 1f;

            maxHealth = newMax;
            currentHealth = Mathf.Max(1, Mathf.RoundToInt(maxHealth * Mathf.Clamp01(healthRatio)));
            NotifyHealthChanged();
        }

        public void SetMaxHealth(int newMaxHealth, bool refill = true)
        {
            newMaxHealth = Mathf.Max(1, newMaxHealth);
            maxHealth = newMaxHealth;
            if (refill)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
            }

            NotifyHealthChanged();
        }

        private void NotifyHealthChanged()
        {
            HealthChanged?.Invoke(currentHealth, maxHealth);
            onHealthChanged.Invoke(currentHealth, maxHealth);
        }
    }
}
