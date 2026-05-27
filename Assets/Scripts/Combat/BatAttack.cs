using UnityEngine;

public class BatAttack : MonoBehaviour
{
    [SerializeField]
    private int damage = 30;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
        if (enemyHealth == null) return;

        enemyHealth.TakeDamage(damage);
    }
}
