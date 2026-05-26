using UnityEngine;
using System.Collections;

public class TestDummy : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 3f;
    public float detectionRange = 5f;
    public LayerMask obstacleLayer;

    public AudioSource idleAudioSource;
    public AudioSource walkAudioSource;
    public AudioSource hitAudioSource;
    public AudioSource deathAudioSource;

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
    private const string ATTACK = "Attack";
    private const string TAKE_DAMAGE = "TakeDamage";
    private const string DIE = "Die";

    private bool isHit = false;
    private bool isDead = false;

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
        if (isDead) return;
        if (isHit) return;

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

    void UpdateFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle < 0f)
            angle += 360f;

        int directionIndex = Mathf.RoundToInt(angle / 45f) % 8;
        facingDirection = (Direction8)directionIndex;
    }

    Vector2 Get8Direction(Direction8 direction)
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

    string GetFacingDirectionName()
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

    string GetActionStateName()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
            return MOVE;

        return IDLE;
    }

    string GetAnimationStateName()
    {
        return GetActionStateName() + GetFacingDirectionName();
    }

    void ChangeAnimationState(string newState)
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

    public void DamageReaction()
    {
        if (isDead) return;

        isHit = true;
        rb.linearVelocity = Vector2.zero;

        StopLoopSounds();
        PlayHitSound();

        ChangeAnimationState(TAKE_DAMAGE + GetFacingDirectionName());

        anim.speed = 2.0f;
        StartCoroutine(ResetHitState());
    }

    public void DieReaction()
    {
        if (isDead) return;

        isDead = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingLayerName = "Effects";

        StopPhysicsMotion();

        StopAllSounds();
        PlayDeathSound();

        ChangeAnimationState(DIE + GetFacingDirectionName());
    }

    IEnumerator ResetHitState()
    {
        yield return new WaitForSeconds(0.7f);

        if (isDead) yield break;

        anim.speed = 1.0f;
        isHit = false;
    }

    void StopPhysicsMotion()
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = Vector2.zero;
#else
        rb.velocity = Vector2.zero;
#endif
        rb.angularVelocity = 0f;
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

    void PlayHitSound()
    {
        if (hitAudioSource.isPlaying)return;
        if (hitAudioSource != null)
            hitAudioSource.Play();
    }

    void PlayDeathSound()
    {
        if (deathAudioSource != null)
            deathAudioSource.Play();
    }

    void StopLoopSounds()
    {
        if (idleAudioSource != null && idleAudioSource.isPlaying)
            idleAudioSource.Stop();

        if (walkAudioSource != null && walkAudioSource.isPlaying)
            walkAudioSource.Stop();
    }

    void StopAllSounds()
    {
        if (idleAudioSource != null)
            idleAudioSource.Stop();

        if (walkAudioSource != null)
            walkAudioSource.Stop();

        if (hitAudioSource != null)
            hitAudioSource.Stop();

        if (deathAudioSource != null)
            deathAudioSource.Stop();
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
