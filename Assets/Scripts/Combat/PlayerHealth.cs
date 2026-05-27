using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private int currentHealth;
    public int maxHealth = 100;
    [SerializeField] private PlayerController playerController;

    private void Start()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        currentHealth -= damage;
        playerController.TakeDamage();
    }

    private void Die()
    {
        playerController.Die();
    }
}
