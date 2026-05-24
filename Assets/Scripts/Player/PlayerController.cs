using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private Camera mainCamera;

    private Vector2 moveInput;
    private Vector2 lookDirection;

    private const int MOVE_IDLE = 0;
    private const int MOVE_FORWARD = 1;
    private const int MOVE_BACKWARD = 2;
    private const int MOVE_LEFT = 3;
    private const int MOVE_RIGHT = 4;

    private int debugLookDir;
    private int debugMoveType;
    private bool debugIsMoving;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        ReadMoveInput();
        UpdateLookDirection();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void ReadMoveInput()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }
    }

    void UpdateLookDirection()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);

        Vector2 playerPosition = rb.position;
        lookDirection = ((Vector2)mouseWorldPosition - playerPosition).normalized;
    }

    void UpdateAnimator()
    {
        int lookDir = GetEightDirectionIndex(lookDirection);
        // 0 = East
        // 1 = NorthEast
        // 2 = North
        // 3 = NorthWest
        // 4 = West
        // 5 = SouthWest
        // 6 = South
        // 7 = SouthEast

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        int moveType = MOVE_IDLE;

        if (isMoving)
        {
            moveType = GetMoveTypeRelativeToLookDirection(moveInput.normalized, lookDirection);
        }
        // 0 = Idle
        // 1 = RunForward
        // 2 = RunBackward
        // 3 = RunLeft
        // 4 = RunRight
        debugLookDir = lookDir;
        debugMoveType = moveType;
        debugIsMoving = isMoving;

        anim.SetBool("isMoving", isMoving);
        anim.SetFloat("LookDir", lookDir);
        anim.SetInteger("MoveType", moveType);
    }

    void MovePlayer()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    int GetEightDirectionIndex(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01f)
        {
            return 6;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle < 0f)
        {
            angle += 360f;
        }

        int index = Mathf.RoundToInt(angle / 45f) % 8;

        return index;
    }

    int GetMoveTypeRelativeToLookDirection(Vector2 moveDir, Vector2 lookDir)
    {
        if (moveDir.sqrMagnitude < 0.01f)
        {
            return MOVE_IDLE;
        }

        Vector2 forward = lookDir.normalized;
        Vector2 right = new Vector2(forward.y, -forward.x);

        float forwardDot = Vector2.Dot(moveDir, forward);
        float rightDot = Vector2.Dot(moveDir, right);

        if (Mathf.Abs(forwardDot) >= Mathf.Abs(rightDot))
        {
            if (forwardDot >= 0f)
            {
                return MOVE_FORWARD;
            }
            else
            {
                return MOVE_BACKWARD;
            }
        }
        else
        {
            if (rightDot >= 0f)
            {
                return MOVE_RIGHT;
            }
            else
            {
                return MOVE_LEFT;
            }
        }
    }

    void OnGUI()
    {
        GUI.Label(
            new Rect(20, 20, 300, 100),
            $"LookDir: {debugLookDir}\nMoveType: {debugMoveType}\nisMoving: {debugIsMoving}"
        );
    }

}
