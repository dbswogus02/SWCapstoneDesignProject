using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // 1. 입력 받기
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 2. 입력이 있는지 체크 (이 부분이 핵심입니다!)
        // sqrMagnitude는 magnitude보다 연산이 빨라 최적화에 좋습니다.
        if (moveInput.sqrMagnitude > 0.01f)
        {
            anim.SetBool("isMoving", true);
        }
        else
        {
            anim.SetBool("isMoving", false);
        }
    }

    void FixedUpdate()
    {
        // 입력값 정규화 (대각선 속도 방지)
        Vector2 normalizedInput = moveInput.normalized;
        rb.MovePosition(rb.position + normalizedInput * moveSpeed * Time.fixedDeltaTime);
    }
}