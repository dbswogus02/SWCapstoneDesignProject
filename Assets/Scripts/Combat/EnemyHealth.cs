using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField]
    private int currentHealth;
    public int maxHealth = 100;
    [SerializeField] private Enemy enemy;

    private void Start()
    {
        currentHealth = maxHealth;
        enemy = GetComponent<Enemy>();
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        currentHealth -= damage;
        enemy.TakeDamage();
    }

    private void Die()
    {
        enemy.Die();
    }
}
