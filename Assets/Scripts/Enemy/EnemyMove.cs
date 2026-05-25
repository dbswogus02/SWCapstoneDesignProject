using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 3f;
    public float detectionRange = 5f;
    public LayerMask obstacleLayer;

    [Header("🔊 오디오 컴포넌트 연결")]
    [Tooltip("에디터 인스펙터에서 해당 Audio Source 컴포넌트를 드래그해서 넣어주세요.")]
    public AudioSource idleAudioSource; // 대기(Idle)용 오디오 소스
    public AudioSource walkAudioSource; // 이동(Walk)용 오디오 소스

    private Rigidbody2D rb;
    private Animator anim;

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

        // 게임 시작 시 기본적으로 대기 사운드 재생 시작
        if (idleAudioSource != null && !idleAudioSource.isPlaying) idleAudioSource.Play();
        if (walkAudioSource != null) walkAudioSource.Stop();
    }

    void FixedUpdate()
    {
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
                        SetDirectionParameter(direction);
                    }

                    // 🔥 이동 중이므로 이동 사운드 제어
                    PlayMoveSound();
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
            ResetDirectionParameters();
        }

        // 🔥 멈췄으므로 대기 상태 사운드로 복귀
        PlayIdleSound();
    }

    // 🔊 이동할 때 사운드 컴포넌트 제어 함수
    void PlayMoveSound()
    {
        // 걷는 소리가 안 나고 있다면 재생하고, 숨소리는 정지
        if (walkAudioSource != null && !walkAudioSource.isPlaying) walkAudioSource.Play();
        if (idleAudioSource != null && idleAudioSource.isPlaying) idleAudioSource.Stop();
    }

    // 🔊 가만히 대기할 때 사운드 컴포넌트 제어 함수
    void PlayIdleSound()
    {
        // 숨소리가 안 나고 있다면 재생하고, 걷는 소리는 정지
        if (idleAudioSource != null && !idleAudioSource.isPlaying) idleAudioSource.Play();
        if (walkAudioSource != null && walkAudioSource.isPlaying) walkAudioSource.Stop();
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
    }
}