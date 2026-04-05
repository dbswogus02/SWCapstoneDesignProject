using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public int health = 100;

    private SpriteRenderer sr;
    private Color originalColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Enemy Hit! HP: " + health);

        // 👉 피격 효과 실행
        StartCoroutine(HitFlash());

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator HitFlash()
    {
        sr.color = Color.red; // 빨강으로 변경
        yield return new WaitForSeconds(0.1f); // 잠깐 유지
        sr.color = originalColor; // 원래 색으로 복구
    }
}