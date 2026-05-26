using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private float currentHealth;
    public float maxHealth = 100;
    public GameObject[] BloodSplatterPrefabs;
    public GameObject[] deathEffectPrefabs;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        int randomIndex = Random.Range(0, BloodSplatterPrefabs.Length);
        Instantiate(BloodSplatterPrefabs[randomIndex], transform.position, Quaternion.identity);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (deathEffectPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, deathEffectPrefabs.Length);
            Instantiate(deathEffectPrefabs[randomIndex], transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
