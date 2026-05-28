using System.Collections.Generic;
using UnityEngine;

public class ChainsawAttack : MonoBehaviour
{
    [SerializeField] private int damage = 3;
    [SerializeField] private float damageInterval = 0.1f;
    [SerializeField] private AudioSource chainsawHitSound;

    private Dictionary<EnemyHealth, float> nextDamageTimes = new Dictionary<EnemyHealth, float>();

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();

        if (enemyHealth == null) return;

        if (!nextDamageTimes.ContainsKey(enemyHealth))
        {
            nextDamageTimes[enemyHealth] = 0f;
        }

        if (Time.time >= nextDamageTimes[enemyHealth])
        {
            enemyHealth.TakeDamage(damage);
            nextDamageTimes[enemyHealth] = Time.time + damageInterval;
            chainsawHitSound.Play();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();

        if (enemyHealth != null && nextDamageTimes.ContainsKey(enemyHealth))
        {
            nextDamageTimes.Remove(enemyHealth);
        }
    }
}
