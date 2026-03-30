using UnityEngine;

public class EnemyMove : MonoBehaviour
{
    public Transform target;
    public float moveSpeed = 3f;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 
        rb.freezeRotation = true;
    }

    void FixedUpdate() // 
    {
        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;

            // 
            rb.linearVelocity = direction * moveSpeed;

            // 
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}