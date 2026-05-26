using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]
    private float currentHealth;
    public float maxHealth = 100;
    public GameObject[] BloodSplatterPrefabs;
    public GameObject[] deathEffectPrefabs;
    [SerializeField] private TestDummy testDummy;

    private void Start()
    {
        currentHealth = maxHealth;
        testDummy = GetComponent<TestDummy>();
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        currentHealth -= damage;
        int randomIndex = Random.Range(0, BloodSplatterPrefabs.Length);
        Instantiate(BloodSplatterPrefabs[randomIndex], transform.position, Quaternion.identity);
        testDummy.DamageReaction();
    }

    private void Die()
    {
        if (deathEffectPrefabs.Length > 0)
        {
            int randomIndex = Random.Range(0, deathEffectPrefabs.Length);
            Instantiate(deathEffectPrefabs[randomIndex], transform.position, Quaternion.identity);
        }
        testDummy.DieReaction();
    }
}
