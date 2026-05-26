using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private float currentHealth;
    public float maxHealth = 100;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
