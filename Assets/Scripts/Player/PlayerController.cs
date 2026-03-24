using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도
    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        // 오브젝트에 부착된 Rigidbody2D를 가져옵니다.
        rb = GetComponent<Rigidbody2D>();

        // 중력의 영향을 받지 않게 하려면 탑다운(Top-down) 뷰 기준 0으로 설정하세요.
        rb.gravityScale = 0f;
    }

    void Update()
    {
        // 1. 입력 받기 (W, A, S, D 또는 화살표 키)
        // GetAxisRaw는 즉각적인 반응(0 아니면 1)을 줄 때 좋습니다.
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // 대각선 이동 시 속도가 빨라지는 것을 방지하기 위해 정규화(Normalize)합니다.
        moveInput = moveInput.normalized;
    }

    void FixedUpdate()
    {
        // 2. 물리 연산은 FixedUpdate에서 처리합니다.
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}