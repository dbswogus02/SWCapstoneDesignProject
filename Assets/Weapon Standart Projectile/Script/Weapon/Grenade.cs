using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Biostart.Weapons
{
    public class Grenade : MonoBehaviour
    {
        public float grenadeTime = 3f; // Time until the grenade explodes
        public GameObject hitPrefab; // Prefab for the hit effect
        public float hitOffset = 0.5f; // Offset for the hit effect
        public float explosionRadius = 5f; // Explosion radius
        public float explosionForce = 10f; // Explosion force
        public bool explodeOnCollision = false; // Toggle for explosion on collision

        private void OnEnable()
        {
            if (!explodeOnCollision)
            {
                StartCoroutine(ExplodeAfterDelay(grenadeTime));
            }
        }

        private IEnumerator ExplodeAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            Explode();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (explodeOnCollision)
            {
                Explode();
            }
        }

        private void Explode()
        {
            Vector3 explosionPosition = transform.position;
            Vector3 hitPosition = explosionPosition + Vector3.up * hitOffset;

            // Create the hit effect
            if (hitPrefab != null)
            {
                Instantiate(hitPrefab, hitPosition, Quaternion.identity);
            }

            // Explosion logic
            Collider[] colliders = Physics.OverlapSphere(explosionPosition, explosionRadius);
            foreach (Collider collider in colliders)
            {
                Rigidbody rb = collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 explosionDirection = collider.transform.position - explosionPosition;
                    rb.AddForce(explosionDirection.normalized * explosionForce, ForceMode.Impulse);
                }
            }

            Destroy(gameObject); // Destroy the grenade after the explosion
        }
    }
}
