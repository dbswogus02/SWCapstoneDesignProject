using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour
{
    [Header("기본 설정")]
    public Transform target;
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2.0f;
    public LayerMask obstacleLayer;
    public GameObject damageEffectPrefab;

    [Header("보스 대쉬 설정")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float minDashInterval = 2f;
    public float maxDashInterval = 5f;

    [Header("오디오")]
    public AudioSource idleAudioSource;
    public AudioSource walkAudioSource;
    public AudioSource attackAudioSource;
    public AudioSource dashAudioSource;
    public AudioSource hurtAudioSource;
    public AudioSource dieAudioSource;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isDead = false;
    private bool isActionLocked = false;
    private bool isDashing = false;
    private float lastAttackTime = -99f;
    private float nextDashTime;

    // 애니메이션 접미사
    private const string EAST = "_E"; private const string NORTH_EAST = "_NE";
    private const string NORTH = "_N"; private const string NORTH_WEST = "_NW";
    private const string WEST = "_W"; private const string SOUTH_WEST = "_SW";
    private const string SOUTH = "_S"; private const string SOUTH_EAST = "_SE";
    private string GetFacingDirectionName()
    {
        Vector2 dir = rb.linearVelocity.normalized;
        if (dir.sqrMagnitude < 0.1f) return SOUTH;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;
        int index = Mathf.RoundToInt(angle / 45f) % 8;
        return index switch { 0 => EAST, 1 => NORTH_EAST, 2 => NORTH, 3 => NORTH_WEST, 4 => WEST, 5 => SOUTH_WEST, 6 => SOUTH, 7 => SOUTH_EAST, _ => SOUTH };
    }

    void Start()
    {
        if (target == null) target = GameObject.FindWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        nextDashTime = Time.time + Random.Range(minDashInterval, maxDashInterval);
    }

    void FixedUpdate()
    {
        if (isDead || isActionLocked || isDashing) return;

        float dist = Vector2.Distance(transform.position, target.position);
        if (dist <= attackRange)
        {
            PerformAttack();
        }
        else if (dist <= detectionRange && IsPlayerVisible())
        {
            MoveTowardsPlayer();
            CheckDashTimer();
        }
        else
        {
            StopMoving();
        }
    }

    void MoveTowardsPlayer()
    {
        Vector2 dir = (target.position - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
        anim.Play("Run" + GetFacingDirectionName());
        PlayTrackSound();
    }

    [Header("공격 이펙트")]
    public GameObject attackEffectPrefab; // 인스펙터에서 이펙트 프리팹을 할당하세요
    public Vector3 attackEffectOffset = new Vector3(0, 0.5f, 0); // 필요 시 위치 보정값

    public void PerformAttack()
    {
        isActionLocked = true;
        rb.linearVelocity = Vector2.zero;

        // 1. 공격 방향 계산
        string attackDirection = GetAttackDirection();

        // 2. 공격 애니메이션 재생
        anim.Play("Attack1" + attackDirection);

        // 3. [추가] 공격 이펙트 생성
        if (attackEffectPrefab != null)
        {
            // 보스 위치 + 오프셋 위치에 이펙트 생성
            GameObject effect = Instantiate(attackEffectPrefab, transform.position + attackEffectOffset, Quaternion.identity);

            // 이펙트가 애니메이션 방향을 따라야 한다면 여기서 방향을 회전시킬 수 있습니다.
            // 예: effect.transform.rotation = ...;
        }

        if (attackAudioSource) attackAudioSource.Play();
        Invoke("UnlockAction", attackCooldown);
    }

    // [추가된 부분] 타겟 위치를 기반으로 방향을 계산하는 메서드
    private string GetAttackDirection()
    {
        Vector2 dir = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        int index = Mathf.RoundToInt(angle / 45f) % 8;
        return index switch
        {
            0 => "_E",
            1 => "_NE",
            2 => "_N",
            3 => "_NW",
            4 => "_W",
            5 => "_SW",
            6 => "_S",
            7 => "_SE",
            _ => "_S"
        };
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (damageEffectPrefab) Instantiate(damageEffectPrefab, transform.position, Quaternion.identity);
        isActionLocked = true;
        anim.Play("TakeDamage" + GetFacingDirectionName());
        if (hurtAudioSource) hurtAudioSource.Play();
        Invoke("UnlockAction", 0.5f);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.simulated = false;

        // [추가] 보스 사망 시 모든 재생 중인 오디오 정지
        StopAllAudio();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.sortingLayerName = "Effects";

        anim.Play("Die" + GetFacingDirectionName());
        if (dieAudioSource) dieAudioSource.Play();
        this.enabled = false;
    }

    private void StopAllAudio()
    {
        if (idleAudioSource) idleAudioSource.Stop();
        if (walkAudioSource) walkAudioSource.Stop();
        if (attackAudioSource) attackAudioSource.Stop();
        if (dashAudioSource) dashAudioSource.Stop();
        if (hurtAudioSource) hurtAudioSource.Stop();
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        rb.linearVelocity = dirs[Random.Range(0, dirs.Length)] * dashSpeed;
        if (dashAudioSource) dashAudioSource.Play();
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        nextDashTime = Time.time + Random.Range(minDashInterval, maxDashInterval);
    }

    void CheckDashTimer() { if (Time.time >= nextDashTime) StartCoroutine(DashRoutine()); }
    void UnlockAction() { isActionLocked = false; }
    void StopMoving() { rb.linearVelocity = Vector2.zero; anim.Play("Idle" + GetFacingDirectionName()); PlayIdleSound(); }
    void PlayTrackSound() { if (walkAudioSource && !walkAudioSource.isPlaying) walkAudioSource.Play(); if (idleAudioSource) idleAudioSource.Stop(); }
    void PlayIdleSound() { if (idleAudioSource && !idleAudioSource.isPlaying) idleAudioSource.Play(); if (walkAudioSource) walkAudioSource.Stop(); }
    bool IsPlayerVisible() { return Physics2D.Raycast(transform.position, (target.position - transform.position).normalized, 10f, obstacleLayer).collider == null; }
}