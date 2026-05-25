using UnityEngine;
using System.Collections;

public class BossMove : MonoBehaviour
{
    [Header("기본 이동 설정")]
    public Transform target;
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public LayerMask obstacleLayer;

    [Header("대쉬 설정")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float minDashInterval = 2f;
    public float maxDashInterval = 5f;

    [Header("🔊 오디오 컴포넌트 연결")]
    public AudioSource idleAudioSource; // 대기(Idle) 사운드 소스
    public AudioSource walkAudioSource; // 추적 이동(Walk) 사운드 소스
    public AudioSource dashAudioSource; // 대쉬(Dash) 즉발성 사운드 소스 (인스펙터 Loop OFF 권장)

    private Rigidbody2D rb;
    private Animator anim;
    private bool isDashing = false;
    private float nextDashTime;

    private string[] directionParams = { "MoveEast", "MoveNorthEast", "MoveNorth", "MoveNorthWest", "MoveWest", "MoveSouthWest", "MoveSouth", "MoveSouthEast" };

    void Start()
    {
        // 게임 시작 시 타겟이 비어있다면 "Player" 태그를 가진 오브젝트를 자동으로 찾아 할당
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }

        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        anim = GetComponent<Animator>();
        SetNextDashTime();

        // 시작 시 기본 대기 사운드 온
        if (idleAudioSource != null && !idleAudioSource.isPlaying) idleAudioSource.Play();
        if (walkAudioSource != null) walkAudioSource.Stop();
        if (dashAudioSource != null) dashAudioSource.Stop();
    }

    void FixedUpdate()
    {
        // 대쉬 중일 때는 대쉬 사운드가 나오고 있으므로 일반 이동 연산을 타지 않습니다.
        if (isDashing) return;

        if (target != null)
        {
            float distanceToTarget = Vector2.Distance(transform.position, target.position);

            if (distanceToTarget <= detectionRange)
            {
                if (IsPlayerVisible())
                {
                    Vector2 direction = (target.position - transform.position).normalized;
                    rb.linearVelocity = direction * moveSpeed;

                    if (anim != null)
                    {
                        anim.SetBool("isWalking", true);
                        anim.SetBool("isRunning", false);
                        SetDirectionParameter(direction);
                    }

                    // 🔥 일반 추적 걷기 사운드 재생
                    PlayTrackSound();

                    CheckDashTimer();
                }
                else
                {
                    StopMoving();
                }
            }
            else
            {
                StopMoving();
            }
        }
        else
        {
            StopMoving();
        }
    }

    void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
            ResetDirectionParameters();
        }

        // 🔥 완전히 멈추면 대기 사운드로 리셋
        PlayIdleSound();
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

        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 randomDashDir = directions[Random.Range(0, directions.Length)];

        if (anim != null)
        {
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", true);
            SetDirectionParameter(randomDashDir);
        }

        // 🔥 대쉬 돌입 순간: 다른 소리 일절 끄고 대쉬 전용 사운드 컴포넌트 실행!
        PlayDashSound();

        rb.linearVelocity = randomDashDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        SetNextDashTime();
    }

    // 🔊 보스 상태별 컴포넌트 사운드 제어 메서드들
    void PlayTrackSound()
    {
        if (walkAudioSource != null && !walkAudioSource.isPlaying) walkAudioSource.Play();
        if (idleAudioSource != null && idleAudioSource.isPlaying) idleAudioSource.Stop();
        if (dashAudioSource != null && dashAudioSource.isPlaying) dashAudioSource.Stop();
    }

    void PlayIdleSound()
    {
        if (idleAudioSource != null && !idleAudioSource.isPlaying) idleAudioSource.Play();
        if (walkAudioSource != null && walkAudioSource.isPlaying) walkAudioSource.Stop();
        if (dashAudioSource != null && dashAudioSource.isPlaying) dashAudioSource.Stop();
    }

    void PlayDashSound()
    {
        if (idleAudioSource != null && idleAudioSource.isPlaying) idleAudioSource.Stop();
        if (walkAudioSource != null && walkAudioSource.isPlaying) walkAudioSource.Stop();

        // 대쉬는 보통 일시적인 효과음이므로 한 번 시원하게 뿜어주도록 처리합니다.
        if (dashAudioSource != null) dashAudioSource.Play();
    }

    void SetDirectionParameter(Vector2 dir)
    {
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        int index = Mathf.RoundToInt(angle / 45f) % 8;

        for (int i = 0; i < directionParams.Length; i++)
        {
            anim.SetBool(directionParams[i], i == index);
        }
    }

    void ResetDirectionParameters()
    {
        for (int i = 0; i < directionParams.Length; i++)
        {
            anim.SetBool(directionParams[i], false);
        }
    }

    void SetNextDashTime()
    {
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