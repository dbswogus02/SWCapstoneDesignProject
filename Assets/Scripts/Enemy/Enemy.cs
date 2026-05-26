using UnityEngine;

public class Enemy : MonoBehaviour
{

    public GameObject damageEffectPrefab;
    public float attackRange = 1.5f;
    public float attackCooldown = 2.0f;
    private float lastAttackTime = -99f;

    public Transform target;
    public float moveSpeed = 3f;
    public float detectionRange = 5f;
    public LayerMask obstacleLayer;

    public AudioSource idleAudioSource;
    public AudioSource walkAudioSource;
    public AudioSource attackAudioSource;
    public AudioSource hurtAudioSource;
    public AudioSource dieAudioSource;

    private Rigidbody2D rb;
    private Animator anim;

    private string currentState;
    private bool isDead = false;
    private bool isActionLocked = false;

    private enum Direction8 { Right, UpRight, Up, UpLeft, Left, DownLeft, Down, DownRight }
    private Direction8 facingDirection = Direction8.Down;

    private const string EAST = "_E";
    private const string NORTH_EAST = "_NE";
    private const string NORTH = "_N";
    private const string NORTH_WEST = "_NW";
    private const string WEST = "_W";
    private const string SOUTH_WEST = "_SW";
    private const string SOUTH = "_S";
    private const string SOUTH_EAST = "_SE";

    private const string IDLE = "Idle";
    private const string MOVE = "Run";
    private const string ATTACK = "Attack1";
    private const string HURT = "TakeDamage";
    private const string DIE = "Die";

    void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null) target = player.transform;
        }

        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        anim = GetComponent<Animator>();

        ChangeAnimationState(GetAnimationStateName());
    }

    void FixedUpdate()
    {
        // [완벽 차단] 죽으면 여기서 물리, 애니메이션, 이동 로직 전부 종료
        if (isDead)
        {
            return;
        }

        if (isActionLocked)
        {
            rb.linearVelocity = Vector2.zero;
            ChangeAnimationState(GetAnimationStateName());
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        if (distanceToPlayer <= attackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
            }
            else
            {
                StopMoving();
            }
        }
        else if (IsPlayerInDetectionRange() && IsPlayerVisible())
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMoving();
        }

        ChangeAnimationState(GetAnimationStateName());
    }

    void ChangeAnimationState(string newState)
    {
        // 이미 죽었다면 어떤 변경도 무시합니다
        if (isDead) return;

        if (isActionLocked) return;
        if (currentState == newState) return;

        anim.Play(newState);
        currentState = newState;
    }

    public void PerformAttack()
    {
        // 죽었으면 공격 불가
        if (isDead || isActionLocked) return;

        isActionLocked = true;
        anim.speed = 2.5f;
        anim.Play(ATTACK + GetFacingDirectionName());

        // 사망 오디오와 겹치지 않도록 확인
        if (attackAudioSource != null) attackAudioSource.Play();
        Invoke("UnlockAction", 0.5f);
    }

    public void TakeDamage(int damage)
    {
        // 죽은 상태면 피격 사운드 생략
        if (isDead) return;

        // 추가: 피격 이펙트 생성
        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }

        isActionLocked = true;
        anim.speed = 2.5f;
        anim.Play(HURT + GetFacingDirectionName());

        if (hurtAudioSource != null) hurtAudioSource.Play();
        Invoke("UnlockAction", 0.5f);
    }
    public void Die()
    {
        if (isDead) return;

        isDead = true;

        // 1. 모든 오디오 즉시 정지
        if (walkAudioSource != null) walkAudioSource.Stop();
        if (idleAudioSource != null) idleAudioSource.Stop();
        if (attackAudioSource != null) attackAudioSource.Stop();
        if (hurtAudioSource != null) hurtAudioSource.Stop();

        // 2. 물리 엔진 무력화
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
            rb.simulated = false; // [핵심] 이것이 꺼지면 어떠한 물리 호출도 엔진이 무시합니다.
        }

        // 3. 콜라이더 제거
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 4. 애니메이션 고정
        anim.Play(DIE + GetFacingDirectionName());

        // 5. [중요] 이 스크립트 비활성화
        this.enabled = false;
    }

    void UnlockAction()
    {
        isActionLocked = false;
        anim.speed = 1.0f; // 기본 속도로 복귀
    }

    string GetAnimationStateName()
    {
        // 죽은 상태에서는 애니메이션을 아예 갱신하지 않습니다
        if (isDead) return currentState;

        if (isActionLocked) return currentState;

        string action = (rb.linearVelocity.magnitude > 0.1f) ? MOVE : IDLE;
        return action + GetFacingDirectionName();
    }

    // ... (방향 계산 및 이동 로직은 동일)
    void UpdateFacingDirection(Vector2 direction) { if (direction.sqrMagnitude < 0.001f) return; float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; if (angle < 0f) angle += 360f; facingDirection = (Direction8)(Mathf.RoundToInt(angle / 45f) % 8); }
    string GetFacingDirectionName() { return facingDirection switch { Direction8.Right => EAST, Direction8.UpRight => NORTH_EAST, Direction8.Up => NORTH, Direction8.UpLeft => NORTH_WEST, Direction8.Left => WEST, Direction8.DownLeft => SOUTH_WEST, Direction8.Down => SOUTH, Direction8.DownRight => SOUTH_EAST, _ => SOUTH }; }
    void MoveTowardsPlayer() {
        if (isDead) return;
        Vector2 rawDirection = target.position - transform.position; UpdateFacingDirection(rawDirection); rb.linearVelocity = (facingDirection switch { Direction8.Right => Vector2.right, Direction8.UpRight => new Vector2(1, 1).normalized, Direction8.Up => Vector2.up, Direction8.UpLeft => new Vector2(-1, 1).normalized, Direction8.Left => Vector2.left, Direction8.DownLeft => new Vector2(-1, -1).normalized, Direction8.Down => Vector2.down, Direction8.DownRight => new Vector2(1, -1).normalized, _ => Vector2.zero }) * moveSpeed; PlayMoveSound(); }
    void StopMoving()
    {
        // [중요] 죽었으면 물리 연산 자체를 수행하지 않음
        if (isDead) return;

        rb.linearVelocity = Vector2.zero;
        PlayIdleSound();
    }
    void PlayMoveSound()
    {
        // 죽었으면 아무 소리도 안 남
        if (isDead) return;

        if (walkAudioSource != null && !walkAudioSource.isPlaying) walkAudioSource.Play();
        if (idleAudioSource != null && idleAudioSource.isPlaying) idleAudioSource.Stop();
    }

    void PlayIdleSound()
    {
        // 죽었다면 재생하지 않고, 재생 중이라면 즉시 정지
        if (isDead)
        {
            if (idleAudioSource != null) idleAudioSource.Stop();
            return;
        }

        if (idleAudioSource != null && !idleAudioSource.isPlaying) idleAudioSource.Play();
        if (walkAudioSource != null && walkAudioSource.isPlaying) walkAudioSource.Stop();
    }

    bool IsPlayerInDetectionRange()
    {
        if (isDead) return false; // 죽었으면 감지 불가
        return target != null && Vector2.Distance(transform.position, target.position) <= detectionRange;
    }

    bool IsPlayerVisible() {
        if (isDead) return false; // 죽었으면 무조건 안 보임
        if (target == null) return false; 
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (target.position - transform.position).normalized, Vector2.Distance(transform.position, target.position), obstacleLayer); 
        return hit.collider == null; 
    }
}