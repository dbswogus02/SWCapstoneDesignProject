using UnityEngine;
using UnityEngine.Events;

namespace Lovatto.DamageScreen
{
    public class bl_PlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField, Min(0)] private int startHealth = 100;
        [SerializeField] private bool canTakeDamage = true;

        [Header("Regeneration")]
        [SerializeField] private bool enableRegeneration = false;
        [SerializeField, Min(0f)] private float regenerationDelay = 4f;
        [SerializeField, Min(0f)] private float regenerationPerSecond = 5f;

        [Header("Events")]
        [SerializeField] private UnityEvent onDie;

        private int currentHealth;
        private bool isDead;
        private float regenerationTimer;
        private float regenerationBuffer;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsDead => isDead;

        private void Awake()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            currentHealth = Mathf.Clamp(startHealth, 0, maxHealth);
            isDead = currentHealth <= 0;
            ResetRegenerationTimer();
            RefreshDamageScreen();
        }

        private void Update()
        {
            ProcessRegeneration();
        }

        /// <summary>
        /// Applies damage to this health component.
        /// </summary>
        public void TakeDamage(int amount)
        {
            if (!canTakeDamage || isDead || amount <= 0) return;
            SetHealth(currentHealth - amount);
            ResetRegenerationTimer();
        }

        /// <summary>
        /// Restores health if the player is alive.
        /// </summary>
        public void Heal(int amount)
        {
            if (isDead || amount <= 0) return;
            SetHealth(currentHealth + amount);
        }

        /// <summary>
        /// Sets a new health value and updates the damage screen.
        /// </summary>
        public void SetHealth(int value)
        {
            int clamped = Mathf.Clamp(value, 0, maxHealth);
            if (clamped == currentHealth) return;

            currentHealth = clamped;
            RefreshDamageScreen();

            if (currentHealth <= 0 && !isDead)
            {
                Die();
            }
            else if (currentHealth > 0)
            {
                isDead = false;
            }
        }

        /// <summary>
        /// Instantly kills this health component.
        /// </summary>
        public void Kill()
        {
            SetHealth(0);
        }

        /// <summary>
        /// Restores full health and revives this health component.
        /// </summary>
        public void RestoreFullHealth()
        {
            isDead = false;
            SetHealth(maxHealth);
            ResetRegenerationTimer();
        }

        /// <summary>
        /// Pushes current values to the blood screen system.
        /// </summary>
        public void RefreshDamageScreen()
        {
            bl_DamageScreen.UpdateHealth(currentHealth, maxHealth);
        }

        private void Die()
        {
            isDead = true;
            onDie?.Invoke();
        }

        private void ProcessRegeneration()
        {
            if (!enableRegeneration || isDead || currentHealth >= maxHealth || regenerationPerSecond <= 0f)
            {
                return;
            }

            if (regenerationTimer > 0f)
            {
                regenerationTimer -= Time.deltaTime;
                return;
            }

            regenerationBuffer += regenerationPerSecond * Time.deltaTime;
            if (regenerationBuffer < 1f) return;

            int amount = Mathf.FloorToInt(regenerationBuffer);
            regenerationBuffer -= amount;
            Heal(amount);
        }

        private void ResetRegenerationTimer()
        {
            regenerationTimer = regenerationDelay;
            regenerationBuffer = 0f;
        }

        private void OnValidate()
        {
            maxHealth = Mathf.Max(1, maxHealth);
            startHealth = Mathf.Clamp(startHealth, 0, maxHealth);
        }

    }
}