using UnityEngine;
using System.Collections;

public class WeaponAttack : MonoBehaviour
{
    private bool isAttacking = false;
    private bool isSpecialAttacking = false;
    private bool isEquipped = false;

    [Header("Attack Durations")]
    public float normalAttackDuration = 0.3f;
    public float specialAttackDuration = 0.6f;

    void Update()
    {
        // 부모가 있으면 장착 상태로 판단
        isEquipped = transform.parent != null;

        if (isEquipped && !isAttacking && !isSpecialAttacking)
        {
            RotateTowardsMouse();
        }
        if (isEquipped && !isAttacking && !isSpecialAttacking && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(Attack());
        }
        if (isEquipped && !isAttacking && !isSpecialAttacking && Input.GetMouseButtonDown(1))
        {
            StartCoroutine(SpecialAttack());
        }
    }
    void RotateTowardsMouse()
    {
        
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction = new Vector2(
            mousePosition.x - transform.position.x,
            mousePosition.y - transform.position.y
        );
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        Debug.Log("Attack!");

        // 이동 거리 설정 (앞으로 이동할 거리)
        float attackDistance = 1f;
        float forwardDuration = normalAttackDuration / 3f;
        float backwardDuration = normalAttackDuration * 2f / 3f;

        // 앞으로 이동
        float forwardTime = 0f;
        while (forwardTime < forwardDuration)
        {
            float moveStep = attackDistance * Time.deltaTime / forwardDuration;
            transform.Translate(Vector3.right * moveStep, Space.Self);
            forwardTime += Time.deltaTime;
            yield return null;
        }

        // 뒤로 이동
        float backwardTime = 0f;
        while (backwardTime < backwardDuration)
        {
            float moveStep = attackDistance * Time.deltaTime / backwardDuration;
            transform.Translate(Vector3.left * moveStep, Space.Self);
            backwardTime += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Jobs done.");
        isAttacking = false;
    }

    IEnumerator SpecialAttack()
    {
        isSpecialAttacking = true;
        Debug.Log("SpecialAttack!");

        // parent의 moveSpeed를 순간 증가
        var parentTransform = transform.parent;
        if (parentTransform != null)
        {
            // parent에 있는 스크립트를 가져온다고 가정
            var parentScript = parentTransform.GetComponent<PlayerController>(); // ParentScript는 moveSpeed가 있는 스크립트
            
            float originalSpeed = parentScript.moveSpeed;
            parentScript.moveSpeed = originalSpeed * 3f; // 순간 2배
                // 서서히 원래 값으로 돌아가게
            float duration = 0.5f; // 몇 초 동안 서서히 줄일지
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                parentScript.moveSpeed = Mathf.Lerp(originalSpeed * 2f, originalSpeed, elapsed / duration);
                yield return null;
            }
            parentScript.moveSpeed = originalSpeed; // 안전하게 원래 값으로
            
        }

        yield return new WaitForSeconds(specialAttackDuration);

        Debug.Log("Jobs done.");
        isSpecialAttacking = false;
        yield return null;
    }
}