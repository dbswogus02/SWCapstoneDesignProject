using UnityEngine;
using System.Collections;

public class BossMove : MonoBehaviour
{
    [Header("기본 이동 설정")]
    public Transform target;
    public float moveSpeed = 3f;
    public float detectionRange = 10f; // 보스는 시야가 더 넓을 수 있습니다.
    public LayerMask obstacleLayer;

    [Header("대쉬 설정")]
    public float dashSpeed = 12f;      // 대쉬 속도
    public float dashDuration = 0.2f;   // 대쉬 지속 시간
    public float minDashInterval = 2f;  // 대쉬 최소 간격 (초)
    public float maxDashInterval = 5f;  // 대쉬 최대 간격 (초)

    private Rigidbody2D rb;
    private bool isDashing = false;     // 현재 대쉬 중인지 확인
    private float nextDashTime;         // 다음 대쉬까지 남은 시간

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        // 첫 번째 대쉬 시간 설정
        SetNextDashTime();
    }

    void FixedUpdate()
    {
        // 대쉬 중일 때는 일반 이동 로직을 무시함
        if (isDashing) return;

        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            if (distanceToTarget <= detectionRange)
            {
                if (IsPlayerVisible())
                {
                    // 일반 추적 이동
                    Vector2 direction = (target.position - transform.position).normalized;
                    rb.linearVelocity = direction * moveSpeed;

                    // 회전 (플레이어 응시)
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, angle);

                    // 대쉬 타이머 체크
                    CheckDashTimer();
                }
                else
                {
                    rb.linearVelocity = Vector2.zero;
                }
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    // 대쉬 타이머 관리
    void CheckDashTimer()
    {
        if (Time.time >= nextDashTime)
        {
            StartCoroutine(DashRoutine());
        }
    }

    // 무작위 방향 대쉬 코루틴
    IEnumerator DashRoutine()
    {
        isDashing = true;

        // 상, 하, 좌, 우 중 무작위 방향 선택
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 randomDashDir = directions[Random.Range(0, directions.Length)];

        // 대쉬 속도 적용
        rb.linearVelocity = randomDashDir * dashSpeed;

        // 지정된 시간(dashDuration)만큼 대기
        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        SetNextDashTime(); // 다음 대쉬 시간 갱신
    }

    void SetNextDashTime()
    {
        // 최소/최대 간격 사이의 무작위 초 뒤에 대쉬 실행
        nextDashTime = Time.time + Random.Range(minDashInterval, maxDashInterval);
    }

    bool IsPlayerVisible()
    {
        Vector2 directionToPlayer = (target.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer);
        return hit.collider == null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (target != null)
        {
            Gizmos.color = IsPlayerVisible() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}