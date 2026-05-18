using UnityEngine;

namespace Biostart.Enemy
{
    public class Health : MonoBehaviour
    {
        public float maxHealth = 100f; // Maximum health
        public float currentHealth; // Current health
        private Renderer objectRenderer; // Renderer to change the object's color
        private Color originalColor; // Original color of the object
        private Color darkRed = new Color(0.5f, 0, 0); // Dark red color

        void Start()
        {
            currentHealth = maxHealth; // Set current health to maximum
            objectRenderer = GetComponent<Renderer>(); // Get the object's renderer

            if (objectRenderer != null)
            {
                originalColor = objectRenderer.material.color; // Save the original color
            }

            UpdateColor(); // Update the object's color
        }

        // Method for taking damage
        public void TakeDamage(float damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Clamp health
            UpdateColor(); // Update the object's color

            // Death check (if necessary)
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        // Method for healing (optional)
        public void Heal(float amount)
        {
            currentHealth += amount;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Clamp health
            UpdateColor(); // Update the object's color
        }

        // Update the object's color based on current health
        private void UpdateColor()
        {
            if (objectRenderer != null)
            {
                // Calculate the health percentage
                float healthPercentage = currentHealth / maxHealth;

                // Change the color to dark red based on the remaining health
                Color newColor = Color.Lerp(darkRed, originalColor, healthPercentage);
                objectRenderer.material.color = newColor;
            }
        }

        // Method for handling the object's death
        private void Die()
        {
            // Death logic (e.g., destroying the object)
            Destroy(gameObject);
        }
    }
}
