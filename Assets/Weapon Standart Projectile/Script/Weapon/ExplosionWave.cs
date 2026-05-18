using System.Collections;
using UnityEngine;
using Biostart.CameraEffects;
using Biostart.Enemy;

namespace Biostart.Weapons
{
    public class ExplosionWave : MonoBehaviour
    {
        public float explosionRadius = 5f;       // Explosion wave radius
        public int damage = 20;                 // Damage from the explosion wave
        public float explosionForce = 5f;       // Explosion wave force

        [Header("Camera Shake")]
        public bool enableCameraShake = true;   // Toggle to enable/disable camera shake
        public float shakeDuration = 0.1f;      // Duration of the camera shake
        public float shakeMagnitude = 0.2f;     // Magnitude of the camera shake
        private CameraShake cameraShake;        // Reference to the CameraShake component

        private void Start()
        {
            // Find the CameraShake component on the main camera
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraShake = mainCamera.GetComponent<CameraShake>();
            }

            // Call the explosion method when the object is created
            Explode();
        }

        private void Explode()
        {
            // Trigger camera shake if enabled
            if (enableCameraShake && cameraShake != null)
            {
                cameraShake.ShakeCamera(shakeDuration, shakeMagnitude);
            }

            // Get all colliders within the explosion radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);

            foreach (Collider collider in colliders)
            {
                // Calculate the distance to the object
                float distance = Vector3.Distance(transform.position, collider.transform.position);

                // Calculate damage based on distance
                float damageMultiplier = 1 - (distance / explosionRadius); // Reduce damage with distance
                int calculatedDamage = Mathf.Max(0, Mathf.RoundToInt(damage * damageMultiplier)); // Damage cannot be negative

                // Apply damage to objects with the Health component
                Health enemyHealth = collider.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(calculatedDamage); // Reduce the enemy's health
                }

                // Apply explosion force
                Rigidbody enemyRb = collider.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    Vector3 forceDirection = (collider.transform.position - transform.position).normalized; // Direction from the explosion point
                    enemyRb.AddForce(forceDirection * explosionForce, ForceMode.Impulse); // Apply force
                }
            }
        }

        private void OnDrawGizmos()
        {
            // Display the explosion radius in the editor
            Gizmos.color = Color.red; // Color of the explosion wave
            Gizmos.DrawWireSphere(transform.position, explosionRadius); // Draw a circle
        }
    }
}
