using UnityEngine;

namespace Lovatto.DamageScreen.Demo
{
    public class bl_DemoFireball : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb = null;
        [SerializeField] private AudioClip hitSound = null;
        [SerializeField] private float speed = 10f;
        public int damage = 15;
        public float lifetime = 5f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            ConstantMovement();
        }

        private void ConstantMovement()
        {
            if (rb != null)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = transform.forward * speed;
#else
                rb.velocity = transform.forward * speed;
#endif
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.collider.TryGetComponent(out bl_DamageScreenPlayerHealth playerHealth))
            {
#if UNITY_6000_0_OR_NEWER
                Vector3 projectileVelocity = rb != null ? rb.linearVelocity : transform.forward * speed;
#else
                Vector3 projectileVelocity = rb != null ? rb.velocity : transform.forward * speed;
#endif
                Vector3 hitPoint = other.contactCount > 0
                    ? other.GetContact(0).point
                    : other.collider.bounds.ClosestPoint(transform.position);

                bl_DemoPlayer demoPlayer = other.collider.GetComponentInParent<bl_DemoPlayer>();
                if (demoPlayer != null)
                    demoPlayer.PushFromImpact(hitPoint, projectileVelocity);

                playerHealth.TakeDamage(damage);
                if (hitSound != null)
                    AudioSource.PlayClipAtPoint(hitSound, transform.position);
                Destroy(gameObject);
                return;
            }

        }
    }
}