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
        // [�Ϻ� ����] ������ ���⼭ ����, �ִϸ��̼�, �̵� ���� ���� ����
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
        // �̹� �׾��ٸ� � ���浵 �����մϴ�
        if (isDead) return;

        if (isActionLocked) return;
        if (currentState == newState) return;

        anim.Play(newState);
        currentState = newState;
    }

    public void PerformAttack()
    {
        // �׾����� ���� �Ұ�
        if (isDead || isActionLocked) return;

        isActionLocked = true;
        anim.speed = 2.5f;
        anim.Play(ATTACK + GetFacingDirectionName());

        // ��� ������� ��ġ�� �ʵ��� Ȯ��
        if (attackAudioSource != null) attackAudioSource.Play();
        Invoke("UnlockAction", 0.5f);
    }

    public void TakeDamage()
    {
        // ���� ���¸� �ǰ� ���� ����
        if (isDead) return;

        // �߰�: �ǰ� ����Ʈ ����
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

        // 1. ��� �̵�/�׼� ���� ����� ��� ����
        if (walkAudioSource != null) walkAudioSource.Stop();
        if (idleAudioSource != null) idleAudioSource.Stop();
        if (attackAudioSource != null) attackAudioSource.Stop();
        if (hurtAudioSource != null) hurtAudioSource.Stop();

        // 2. ���� �� �ݶ��̴� ����ȭ
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;
            rb.simulated = false;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingLayerName = "Effects";

        // 3. ���� �ִϸ��̼� ���
        anim.Play(DIE + GetFacingDirectionName());

        // 4. [����] ���� ����� ��� (���� ���� ����)
        if (dieAudioSource != null)
        {
            dieAudioSource.Play();
        }

        // 5. [�߿�] ��ũ��Ʈ�� �ٷ� ���� �ʰ�, 
        // ������� ����� �ð��� Ȯ���ϰų� ��Ȱ��ȭ ���� ������ �ʿ��� �� �ֽ��ϴ�.
        // �ٷ� ���� �Ѵٸ� �Ʒ�ó�� ������ ó���ϵ�, 
        // ���� �Ҹ��� ����ٸ� Invoke�� ����Ͽ� ���� �ణ�� ������ �� ��Ȱ��ȭ�ϼ���.
        this.enabled = false;
    }

    void UnlockAction()
    {
        isActionLocked = false;
        anim.speed = 1.0f; // �⺻ �ӵ��� ����
    }

    string GetAnimationStateName()
    {
        // ���� ���¿����� �ִϸ��̼��� �ƿ� �������� �ʽ��ϴ�
        if (isDead) return currentState;

        if (isActionLocked) return currentState;

        string action = (rb.linearVelocity.magnitude > 0.1f) ? MOVE : IDLE;
        return action + GetFacingDirectionName();
    }

    // ... (���� ��� �� �̵� ������ ����)
    void UpdateFacingDirection(Vector2 direction) { if (direction.sqrMagnitude < 0.001f) return; float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; if (angle < 0f) angle += 360f; facingDirection = (Direction8)(Mathf.RoundToInt(angle / 45f) % 8); }
    string GetFacingDirectionName() { return facingDirection switch { Direction8.Right => EAST, Direction8.UpRight => NORTH_EAST, Direction8.Up => NORTH, Direction8.UpLeft => NORTH_WEST, Direction8.Left => WEST, Direction8.DownLeft => SOUTH_WEST, Direction8.Down => SOUTH, Direction8.DownRight => SOUTH_EAST, _ => SOUTH }; }
    void MoveTowardsPlayer() {
        if (isDead) return;
        Vector2 rawDirection = target.position - transform.position; UpdateFacingDirection(rawDirection); rb.linearVelocity = (facingDirection switch { Direction8.Right => Vector2.right, Direction8.UpRight => new Vector2(1, 1).normalized, Direction8.Up => Vector2.up, Direction8.UpLeft => new Vector2(-1, 1).normalized, Direction8.Left => Vector2.left, Direction8.DownLeft => new Vector2(-1, -1).normalized, Direction8.Down => Vector2.down, Direction8.DownRight => new Vector2(1, -1).normalized, _ => Vector2.zero }) * moveSpeed; PlayMoveSound(); }
    void StopMoving()
    {
        // [�߿�] �׾����� ���� ���� ��ü�� �������� ����
        if (isDead) return;

        rb.linearVelocity = Vector2.zero;
        PlayIdleSound();
    }
    void PlayMoveSound()
    {
        // �׾����� �ƹ� �Ҹ��� �� ��
        if (isDead) return;

        if (walkAudioSource != null && !walkAudioSource.isPlaying) walkAudioSource.Play();
        if (idleAudioSource != null && idleAudioSource.isPlaying) idleAudioSource.Stop();
    }

    void PlayIdleSound()
    {
        // �׾��ٸ� ������� �ʰ�, ��� ���̶�� ��� ����
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
        if (isDead) return false; // �׾����� ���� �Ұ�
        return target != null && Vector2.Distance(transform.position, target.position) <= detectionRange;
    }

    bool IsPlayerVisible() {
        if (isDead) return false; // �׾����� ������ �� ����
        if (target == null) return false; 
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (target.position - transform.position).normalized, Vector2.Distance(transform.position, target.position), obstacleLayer); 
        return hit.collider == null; 
    }
}