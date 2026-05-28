using UnityEngine;

public class EnemyAttackArea : MonoBehaviour
{
    [SerializeField]
    private int damage = 40;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
        if (playerHealth == null) return;

        playerHealth.TakeDamage(damage);
    }
}
