using UnityEngine;

public class ShotgunAttack : MonoBehaviour
{
    [SerializeField]
    private int damage = 40;
    [SerializeField]
    private AudioSource shotgunHitSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
        if (enemyHealth == null) return;

        enemyHealth.TakeDamage(damage);
        shotgunHitSound.Play();
    }
}
