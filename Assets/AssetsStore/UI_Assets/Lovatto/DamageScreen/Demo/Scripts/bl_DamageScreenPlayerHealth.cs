using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace Lovatto.DamageScreen.Demo
{
    public class bl_DamageScreenPlayerHealth : MonoBehaviour
    {
        [Tooltip("Maximum health value used to clamp damage and healing.")]
        public int maxHealth = 100;

        [Tooltip("Optional UI fill image used to display current health.")]
        [SerializeField] private Image healthBarFill = null;
        [SerializeField] private AudioClip healingSound = null;
        private int currentHealth;
        private bl_DemoPlayer demoPlayer;

        private void Start()
        {
            currentHealth = maxHealth;
            demoPlayer = GetComponentInParent<bl_DemoPlayer>();
            UpdateHealth(0); // Initialize the health display
        }

        /// <summary>
        /// Applies damage to the player and updates dependent systems.
        /// Call this whenever the local player is hit by a damaging source.
        /// </summary>
        /// <param name="damage">Raw damage amount to subtract from current health.</param>
        public void TakeDamage(int damage)
        {
            int healthBefore = currentHealth;
            UpdateHealth(-damage);
            int appliedDamage = Mathf.Max(healthBefore - currentHealth, 0);

            if (appliedDamage > 0 && demoPlayer != null)
            {
                float intensity = Mathf.Clamp01((float)appliedDamage / 25f);
                demoPlayer.PlayDamageCameraShake(1f + intensity);
            }
        }

        /// <summary>
        /// Restores health up to <see cref="maxHealth"/>.
        /// Call this for pickups, regeneration, or debug controls.
        /// </summary>
        /// <param name="amount">Health amount to add.</param>
        public void Heal(int amount)
        {
            UpdateHealth(amount);
            if (healingSound != null)
            {
                AudioSource.PlayClipAtPoint(healingSound, transform.position);
            }
        }

        private void UpdateHealth(int change)
        {
            currentHealth = Mathf.Clamp(currentHealth + change, 1, maxHealth);
            bl_DamageScreen.UpdateHealth(currentHealth, maxHealth);
        }

        private void Update()
        {
            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                Heal(maxHealth);
            }

            if (healthBarFill != null)
            {
                healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, (float)currentHealth / maxHealth, Time.deltaTime * 5f);
            }
        }
    }
}