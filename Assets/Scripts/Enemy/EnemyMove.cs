using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 3f;
    public float detectionRange = 5f;
    public LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private Animator anim;

    // ⭐ 에셋의 파라미터 이름인 'Move~'로 완벽 매칭했습니다!
    private string[] directionParams = { "MoveEast", "MoveNorthEast", "MoveNorth", "MoveNorthWest", "MoveWest", "MoveSouthWest", "MoveSouth", "MoveSouthEast" };

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        anim = GetComponent<Animator>();
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
        if (target != null)
        {
            Gizmos.color = IsPlayerVisible() ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}