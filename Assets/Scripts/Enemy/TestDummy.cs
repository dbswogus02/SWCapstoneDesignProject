using UnityEngine;

public class TestDummy : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 3f;
    public float detectionRange = 5f;
    public LayerMask obstacleLayer;

    public AudioSource idleAudioSource;
    public AudioSource walkAudioSource;

    private Rigidbody2D rb;
    private Animator anim;

    private string currentState;

    private enum Direction8
    {
        Right,
        UpRight,
        Up,
        UpLeft,
        Left,
        DownLeft,
        Down,
        DownRight
    }

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

        PlayIdleSound();
        ChangeAnimationState(GetAnimationStateName());
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            StopMoving();
            return;
        }

        if (IsPlayerInDetectionRange() && IsPlayerVisible())
        {
            MoveTowardsPlayer();
        }
        else
        {
            StopMoving();
        }

        ChangeAnimationState(GetAnimationStateName());
    }

    void UpdateFacingDirection(Vector2 direction) //벡터 방향을 8방향으로 변환하여 facingDirection 업데이트
    {
        if (direction.sqrMagnitude < 0.001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle < 0f)
            angle += 360f;
        int directionIndex = Mathf.RoundToInt(angle / 45f) % 8;
        facingDirection = (Direction8)directionIndex;
    }
    Vector2 Get8Direction(Direction8 direction) //현재 facingDirection을 8방향 벡터로 변환하여 반환
    {
        switch (direction)
        {
            case Direction8.Right:
                return Vector2.right;

            case Direction8.UpRight:
                return new Vector2(1, 1).normalized;

            case Direction8.Up:
                return Vector2.up;

            case Direction8.UpLeft:
                return new Vector2(-1, 1).normalized;

            case Direction8.Left:
                return Vector2.left;

            case Direction8.DownLeft:
                return new Vector2(-1, -1).normalized;

            case Direction8.Down:
                return Vector2.down;

            case Direction8.DownRight:
                return new Vector2(1, -1).normalized;

            default:
                return Vector2.zero;
        }
    }

    string GetFacingDirectionName()//ChangeAnimationState에게 넘기기 위해 현재 바라보는 방향을 문자열로 변환
    {
        switch (facingDirection)
        {
            case Direction8.Right:
                return EAST;

            case Direction8.UpRight:
                return NORTH_EAST;

            case Direction8.Up:
                return NORTH;

            case Direction8.UpLeft:
                return NORTH_WEST;

            case Direction8.Left:
                return WEST;

            case Direction8.DownLeft:
                return SOUTH_WEST;

            case Direction8.Down:
                return SOUTH;

            case Direction8.DownRight:
                return SOUTH_EAST;

            default:
                return SOUTH;
        }
    }

    string GetActionStateName()//ChangeAnimationState에게 넘기기 위해 현재 움직임 상태를 문자열로 변환
    {
        if (rb.linearVelocity.magnitude > 0.1f)
            return MOVE;

        return IDLE;
    }

    string GetAnimationStateName()//현재 행동 상태와 바라보는 방향을 문자열을 조합하여 애니메이션 상태 이름 생성
    {
        return GetActionStateName() + GetFacingDirectionName();
    }

    void ChangeAnimationState(string newState)// 애니메이션 상태 이름으로 애니메이션 상태 변경
    {
        if (currentState == newState) return;

        anim.Play(newState);
        currentState = newState;
    }

    void MoveTowardsPlayer()
    {
        if (!IsPlayerVisible()) return;
        Vector2 rawDirection = target.position - transform.position;

        UpdateFacingDirection(rawDirection);

        Vector2 direction = Get8Direction(facingDirection);

        rb.linearVelocity = direction * moveSpeed;

        PlayMoveSound();
    }

    void StopMoving()
    {
        rb.linearVelocity = Vector2.zero;
        PlayIdleSound();
    }

    void PlayMoveSound()
    {
        if (walkAudioSource != null && !walkAudioSource.isPlaying)
            walkAudioSource.Play();

        if (idleAudioSource != null && idleAudioSource.isPlaying)
            idleAudioSource.Stop();
    }

    void PlayIdleSound()
    {
        if (idleAudioSource != null && !idleAudioSource.isPlaying)
            idleAudioSource.Play();

        if (walkAudioSource != null && walkAudioSource.isPlaying)
            walkAudioSource.Stop();
    }

    bool IsPlayerInDetectionRange()
    {
        if (target == null) return false;

        return Vector2.Distance(transform.position, target.position) <= detectionRange;
    }

    bool IsPlayerVisible()
    {
        if (target == null) return false;

        Vector2 directionToPlayer = (target.position - transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            directionToPlayer,
            distanceToPlayer,
            obstacleLayer
        );

        return hit.collider == null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
