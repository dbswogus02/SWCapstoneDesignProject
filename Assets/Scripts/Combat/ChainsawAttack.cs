using System.Collections.Generic;
using UnityEngine;

public class ChainsawAttack : MonoBehaviour
{
    [SerializeField] private float damage = 3f;
    [SerializeField] private float damageInterval = 0.1f;

    private Dictionary<Health, float> nextDamageTimes = new Dictionary<Health, float>();

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy")) return;

        Health health = collision.GetComponent<Health>();

        if (health == null) return;

        if (!nextDamageTimes.ContainsKey(health))
        {
            nextDamageTimes[health] = 0f;
        }

        if (Time.time >= nextDamageTimes[health])
        {
            health.TakeDamage(damage);
            nextDamageTimes[health] = Time.time + damageInterval;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Health health = collision.GetComponent<Health>();

        if (health != null && nextDamageTimes.ContainsKey(health))
        {
            nextDamageTimes.Remove(health);
        }
    }
}
