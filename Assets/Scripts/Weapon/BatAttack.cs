using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BatAttack : MonoBehaviour
{
    private bool isNormalAttacking = false;
    private bool isSpecialAttacking = false;
    private bool isEquipped = false;

    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    [Header("Attack Durations")]
    public float normalAttackDuration = 0.3f;
    public float specialAttackDuration = 0.6f;

    [Header("Cooldown System")]
    public float specialAttackCooldown = 10f; // 최대 쿨타임
    public float currentCooldown = 0f;        // 현재 남은 쿨타임

    [Header("Damage")]
    public int normalDamage = 10;

    void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 30;
        style.normal.textColor = currentCooldown > 0f ? Color.red : Color.green;

        GUI.Label(new Rect(10, 10, 500, 50),
            currentCooldown > 0f ? $"Cooldown: {currentCooldown:F1}" : "READY",
            style);
    }

    void Update()
    {
        isEquipped = transform.parent != null;

        // 쿨타임 감소
        if (currentCooldown > 0f)
        {
            currentCooldown -= Time.deltaTime;
            currentCooldown = Mathf.Max(currentCooldown, 0f);
        }

        if (isEquipped && !isNormalAttacking && !isSpecialAttacking)
        {
            RotateTowardsMouse();
        }

        if (isEquipped && !isNormalAttacking && !isSpecialAttacking && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(Attack());
        }

        // 쿨타임 0일 때만 사용 가능
        if (isEquipped && Input.GetMouseButtonDown(1) && currentCooldown <= 0f)
        {
            StartCoroutine(SpecialAttack());
        }
    }

    void RotateTowardsMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.forward, Vector3.zero);

        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 mouseWorldPos = ray.GetPoint(rayDistance);

            Vector2 direction = new Vector2(
                mouseWorldPos.x - transform.position.x,
                mouseWorldPos.y - transform.position.y
            );

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    IEnumerator Attack()
    {
        isNormalAttacking = true;

        hitEnemies.Clear();

        Debug.Log("Attack!");

        float forwardDuration = normalAttackDuration / 5f;
        float backwardDuration = normalAttackDuration - forwardDuration;

        float attackDistance = 1.5f;

        float swingAngle = 240f;
        float halfAngle = swingAngle / 2f;

        Quaternion startRotation = transform.rotation;
        Quaternion leftRot = startRotation * Quaternion.Euler(0, 0, -halfAngle);
        Quaternion rightRot = startRotation * Quaternion.Euler(0, 0, halfAngle);

        float forwardTime = 0f;

        // 👉 앞으로 이동 + 휘두르기 동시에
        while (forwardTime < forwardDuration)
        {
            float t = forwardTime / forwardDuration;

            
            float moveStep = attackDistance * Time.deltaTime / forwardDuration;
            transform.Translate(Vector3.up * moveStep, Space.Self);

            // 회전 (휘두르기)
            transform.rotation = Quaternion.Slerp(rightRot, leftRot, t);

            forwardTime += Time.deltaTime;
            yield return null;
        }

        Vector3 startPos = transform.localPosition;
        Vector3 targetPos = new Vector3(0.3f, -0.6f, 0f);

        float backwardTime = 0f;

        // 👉 원위치 복귀 + 회전 복귀
        while (backwardTime < backwardDuration)
        {
            float t = backwardTime / backwardDuration;

            transform.localPosition = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Slerp(rightRot, startRotation, t);

            backwardTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPos;
        transform.rotation = startRotation;

        Debug.Log("Normal Attack Done");
        isNormalAttacking = false;
    }

    IEnumerator SpecialAttack()
    {
        if (isSpecialAttacking) yield break;

        isSpecialAttacking = true;

        // 👉 쿨타임 부여
        currentCooldown += specialAttackCooldown;

        Debug.Log("SpecialAttack!");

        var parentTransform = transform.parent;
        if (parentTransform != null)
        {
            var parentScript = parentTransform.GetComponent<PlayerController>();

            if (parentScript != null)
            {
                float originalSpeed = parentScript.moveSpeed;
                parentScript.moveSpeed = originalSpeed * 3f;

                float duration = 0.5f;
                float elapsed = 0f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    parentScript.moveSpeed = Mathf.Lerp(originalSpeed * 2f, originalSpeed, elapsed / duration);
                    yield return null;
                }

                parentScript.moveSpeed = originalSpeed;
            }
        }

        yield return new WaitForSeconds(specialAttackDuration);

        isSpecialAttacking = false;
    }

    // 공격 판정 
    void OnTriggerStay2D(Collider2D collision)
    {
        if (!isNormalAttacking) return;

        if (collision.CompareTag("Enemy") && !hitEnemies.Contains(collision.gameObject))
        {
            hitEnemies.Add(collision.gameObject);

            Enemy enemy = collision.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(normalDamage);
            }
        }
    }
}