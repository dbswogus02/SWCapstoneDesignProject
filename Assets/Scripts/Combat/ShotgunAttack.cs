using UnityEngine;

public class ShotgunAttack : MonoBehaviour
{
    [SerializeField]
    private float damage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
    if (!collision.CompareTag("Enemy")) return;

    Health health = collision.GetComponent<Health>();
    if (health == null) return;

    health.TakeDamage(damage);
    }
}
