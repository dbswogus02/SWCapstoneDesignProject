using UnityEngine;
using System.Collections;

public class Boss2 : MonoBehaviour
{
    public AudioSource dashAudioSource;
    public GameObject AttackEffectPrefab;

    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float minDashInterval = 2f;
    public float maxDashInterval = 5f;

    private float nextDashTime;
    private bool isDashing = false;
    private Enemy enemy;

    void Start()
    {
        enemy = GetComponent<Enemy>();
        nextDashTime = Time.time + Random.Range(minDashInterval, maxDashInterval);
    }

    void FixedUpdate()
    {
        if (enemy == null)
        {
            return;
        }

        if (enemy.isDead)
        {
            return;
        }

        if (enemy.isActionLocked)
        {
            return;
        }

        if (isDashing)
        {
            return;
        }

        CheckDashTimer();
    }

    void CheckDashTimer()
    {
        if (Time.time >= nextDashTime)
        {
            StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;

        float originalSpeed = enemy.moveSpeed;
        enemy.moveSpeed = dashSpeed;

        if (dashAudioSource != null)
        {
            dashAudioSource.Play();
        }

        yield return new WaitForSeconds(dashDuration);

        enemy.moveSpeed = originalSpeed;
        isDashing = false;

        nextDashTime = Time.time + Random.Range(minDashInterval, maxDashInterval);
    }

    public void AttackEffect()
    {
        {
            if (AttackEffectPrefab != null)
            {
                Instantiate(AttackEffectPrefab, transform.position, Quaternion.identity);
            }
        }
    }
}
