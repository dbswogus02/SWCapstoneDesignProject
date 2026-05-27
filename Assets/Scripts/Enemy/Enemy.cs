using UnityEngine;
using System.Collections;

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

    public GameObject attackArea;
    public Transform aim;
    private Boss2 boss;

    private Rigidbody2D rb;
    private Animator anim;

    private string currentState;
    public bool isDead = false;
    public bool isActionLocked = false;
    private Coroutine attackAreaCoroutine;

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
    [SerializeField] private float directionChangeThreshold = 28f;


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
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boss = GetComponent<Boss2>();

        if (rb != null)
        {
            rb.freezeRotation = true;
        }

        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                target = player.transform;
            }
        }

        if (attackArea != null)
        {
            attackArea.SetActive(false);
        }

        ChangeAnimationState(GetAnimationStateName());
    }

    void FixedUpdate()
    {
        if (isDead || isActionLocked || target == null)
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer <= attackRange)
        {
            StopMoving();
            UpdateFacingDirection(target.position - transform.position);
            RotateAim();

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PerformAttack();
                lastAttackTime = Time.time;
                return;
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

        RotateAim();
        ChangeAnimationState(GetAnimationStateName());
    }

    void ChangeAnimationState(string newState)
    {
        if (isDead || isActionLocked)
        {
            return;
        }

        PlayAnimationState(newState);
    }

    void PlayAnimationState(string newState)
    {
        if (string.IsNullOrEmpty(newState))
        {
            return;
        }

        if (currentState == newState)
        {
            return;
        }

        anim.Play(newState);
        currentState = newState;
    }

    public void PerformAttack()
    {
        if (isDead || isActionLocked || target == null)
        {
            return;
        }

        UpdateFacingDirection(target.position - transform.position);

        isActionLocked = true;
        StopMoving();

        if (anim != null)
        {
            anim.speed = 2.5f;
        }

        PlayAnimationState(ATTACK + GetFacingDirectionName());
        if (boss != null)
        {
            boss.AttackEffect();
        }

        if (attackAreaCoroutine != null)
        {
            StopCoroutine(attackAreaCoroutine);
        }

        attackAreaCoroutine = StartCoroutine(ActivateAttackArea(0.3f, 0.1f));

        if (attackAudioSource != null)
        {
            attackAudioSource.Play();
        }

        Invoke(nameof(UnlockAction), 0.5f);
    }

    IEnumerator ActivateAttackArea(float delay, float activateTime)
    {
        yield return new WaitForSeconds(delay);

        if (attackArea != null)
        {
            attackArea.SetActive(true);
        }

        yield return new WaitForSeconds(activateTime);

        if (attackArea != null)
        {
            attackArea.SetActive(false);
        }

        attackAreaCoroutine = null;
    }

    public void TakeDamage()
    {
        if (isDead)
        {
            return;
        }

        if (damageEffectPrefab != null)
        {
            Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        }

        isActionLocked = true;
        StopMoving();

        if (anim != null)
        {
            anim.speed = 2.5f;
        }

        PlayAnimationState(HURT + GetFacingDirectionName());

        if (hurtAudioSource != null)
        {
            hurtAudioSource.Play();
        }

        Invoke(nameof(UnlockAction), 0.5f);
    }

    public void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        isActionLocked = true;

        CancelInvoke();

        if (attackAreaCoroutine != null)
        {
            StopCoroutine(attackAreaCoroutine);
            attackAreaCoroutine = null;
        }

        if (attackArea != null)
        {
            attackArea.SetActive(false);
        }

        StopAllSounds();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
            rb.simulated = false;
        }

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            sr.sortingLayerName = "Effects";
        }

        if (anim != null)
        {
            anim.speed = 1.0f;
        }

        PlayAnimationState(DIE + GetFacingDirectionName());

        if (dieAudioSource != null)
        {
            dieAudioSource.Play();
        }

        enabled = false;
    }

    void UnlockAction()
    {
        if (isDead)
        {
            return;
        }

        isActionLocked = false;

        if (anim != null)
        {
            anim.speed = 1.0f;
        }

        currentState = null;
    }

    string GetAnimationStateName()
    {
        string action = rb != null && rb.linearVelocity.magnitude > 0.1f ? MOVE : IDLE;
        return action + GetFacingDirectionName();
    }

    void UpdateFacingDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle < 0f)
        {
            angle += 360f;
        }

        float currentAngle = GetDirectionAngle(facingDirection);
        float delta = Mathf.Abs(Mathf.DeltaAngle(currentAngle, angle));

        if (delta < directionChangeThreshold)
        {
            return;
        }

        int directionIndex = Mathf.RoundToInt(angle / 45f) % 8;
        facingDirection = (Direction8)directionIndex;
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
    float GetDirectionAngle(Direction8 direction)
    {
        switch (direction)
        {
            case Direction8.Right:
                return 0f;
            case Direction8.UpRight:
                return 45f;
            case Direction8.Up:
                return 90f;
            case Direction8.UpLeft:
                return 135f;
            case Direction8.Left:
                return 180f;
            case Direction8.DownLeft:
                return 225f;
            case Direction8.Down:
                return 270f;
            case Direction8.DownRight:
                return 315f;
            default:
                return 270f;
        }
    }
    
    void RotateAim()
    {
        if (aim == null)
        {
            return;
        }

        switch (facingDirection)
        {
            case Direction8.Right:
                aim.rotation = Quaternion.Euler(0, 0, 0);
                break;
            case Direction8.UpRight:
                aim.rotation = Quaternion.Euler(0, 0, 45);
                break;
            case Direction8.Up:
                aim.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case Direction8.UpLeft:
                aim.rotation = Quaternion.Euler(0, 0, 135);
                break;
            case Direction8.Left:
                aim.rotation = Quaternion.Euler(0, 0, 180);
                break;
            case Direction8.DownLeft:
                aim.rotation = Quaternion.Euler(0, 0, 225);
                break;
            case Direction8.Down:
                aim.rotation = Quaternion.Euler(0, 0, 270);
                break;
            case Direction8.DownRight:
                aim.rotation = Quaternion.Euler(0, 0, 315);
                break;
        }
    }

    void MoveTowardsPlayer()
    {
        if (isDead || target == null || rb == null)
        {
            return;
        }

        Vector2 rawDirection = target.position - transform.position;
        UpdateFacingDirection(rawDirection);

        rb.linearVelocity = GetDirectionVector(facingDirection) * moveSpeed;

        PlayMoveSound();
    }

    Vector2 GetDirectionVector(Direction8 direction)
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

    void StopMoving()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        PlayIdleSound();
    }

    void PlayMoveSound()
    {
        if (isDead)
        {
            return;
        }

        if (walkAudioSource != null && !walkAudioSource.isPlaying)
        {
            walkAudioSource.Play();
        }

        if (idleAudioSource != null && idleAudioSource.isPlaying)
        {
            idleAudioSource.Stop();
        }
    }

    void PlayIdleSound()
    {
        if (isDead)
        {
            if (idleAudioSource != null)
            {
                idleAudioSource.Stop();
            }

            return;
        }

        if (idleAudioSource != null && !idleAudioSource.isPlaying)
        {
            idleAudioSource.Play();
        }

        if (walkAudioSource != null && walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
        }
    }

    void StopAllSounds()
    {
        if (walkAudioSource != null)
        {
            walkAudioSource.Stop();
        }

        if (idleAudioSource != null)
        {
            idleAudioSource.Stop();
        }

        if (attackAudioSource != null)
        {
            attackAudioSource.Stop();
        }

        if (hurtAudioSource != null)
        {
            hurtAudioSource.Stop();
        }
    }

    bool IsPlayerInDetectionRange()
    {
        if (isDead || target == null)
        {
            return false;
        }

        return Vector2.Distance(transform.position, target.position) <= detectionRange;
    }

    bool IsPlayerVisible()
    {
        if (isDead || target == null)
        {
            return false;
        }

        Vector2 direction = target.position - transform.position;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
        {
            return true;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction.normalized,
            distance,
            obstacleLayer
        );

        return hit.collider == null;
    }
}
