using UnityEngine;

namespace Biostart.Bullet
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private GameObject projectorWall; // Prefab for wall projectors
        [SerializeField] private GameObject projectorEnemy; // Prefab for enemy projectors
        [SerializeField] private float minProjectorLifetime = 1f; // Minimum lifetime for projectors
        [SerializeField] private float maxProjectorLifetime = 2f; // Maximum lifetime for projectors
        [SerializeField] private float projectorScale = 1f; // Scale for projectors
        private GameObject projector; // Variable to store the current projector

        void Start() 
        {
            // Get the Collider of this bullet
            Collider bulletCollider = GetComponent<Collider>();

            // Find all objects with the "Bullet" tag
            Bullet[] bullets = FindObjectsOfType<Bullet>();

            // Ignore collisions with each Bullet object
            foreach (Bullet bullet in bullets)
            {
                if (bullet != this) // Ignore itself
                {
                    Physics.IgnoreCollision(bulletCollider, bullet.GetComponent<Collider>());
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            Vector3 hitPoint = collision.contacts[0].point;
            Vector3 hitNormal = collision.contacts[0].normal;

            // Determine which projector to use depending on the object's layer
            switch (collision.transform.gameObject.layer)
            {
                case 8: // Layer number for flat objects
                    projector = projectorWall;
                    break;
                case 9: // Layer number for characters or textured objects
                    projector = projectorEnemy;
                    break;
            }

            if (projector == null)
            {
                return;
            }

            // Set the projector's orientation
            Quaternion projectorRotation = Quaternion.FromToRotation(-Vector3.forward, hitNormal);

            // Instantiate the projector
            GameObject obj = Instantiate(projector, hitPoint + hitNormal * 0.25f, projectorRotation);

            // Adjust the Projector's size on the object
            Projector proj = obj.GetComponent<Projector>();
            if (proj != null)
            {
                proj.orthographicSize *= projectorScale; // Adjust size for orthographic projectors
                // For perspective projectors, use:
                // proj.fieldOfView *= projectorScale;
            }

            // Attach the projector to the object it collided with
            obj.transform.parent = collision.transform;

            // Random rotation around the Z axis
            Quaternion randomRotZ = Quaternion.Euler(obj.transform.eulerAngles.x, obj.transform.eulerAngles.y, Random.Range(0, 360));
            obj.transform.rotation = randomRotZ;

            // Generate a random lifetime within the specified range
            float projectorLifetime = Random.Range(minProjectorLifetime, maxProjectorLifetime);
            
            // Destroy the projector after its lifetime
            Destroy(obj, projectorLifetime);

            // Destroy the bullet after the hit
            Destroy(gameObject); 
        }
    }
}
