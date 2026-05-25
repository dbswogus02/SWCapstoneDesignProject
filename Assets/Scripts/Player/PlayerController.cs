using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator anim;
    private Camera mainCamera;

    private Vector2 moveInput;
    private Vector2 lookDirection;

    private string currentState;
    private bool isAttacking;

    private const string DIR_EAST = "East";
    private const string DIR_NORTH_EAST = "NorthEast";
    private const string DIR_NORTH = "North";
    private const string DIR_NORTH_WEST = "NorthWest";
    private const string DIR_WEST = "West";
    private const string DIR_SOUTH_WEST = "SouthWest";
    private const string DIR_SOUTH = "South";
    private const string DIR_SOUTH_EAST = "SouthEast";

    private const string MOVE_IDLE = "Idle";
    private const string MOVE_FORWARD = "RunForward";
    private const string MOVE_BACKWARD = "RunBackward";
    private const string MOVE_LEFT = "RunLeft";
    private const string MOVE_RIGHT = "RunRight";

    private const string ATTACK = "Attack";

    private string debugLookDir;
    private string debugMoveType;
    private bool debugIsMoving;
    private string debugState;
    private bool debugIsAttacking;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        UpdateLookDirection();

        if (Input.GetMouseButtonDown(0))
        {
            TryAttack();
        }

        if (isAttacking)
        {
            moveInput = Vector2.zero;
            return;
        }

        ReadMoveInput();
        UpdateAnimationState();
    }

    void FixedUpdate()
    {
        if (isAttacking) return;

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
        Vector3 mouseScreen = Input.mousePosition;
        mouseScreen.z = Mathf.Abs(mainCamera.transform.position.z);

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);

        Vector2 playerPosition = rb.position;
        lookDirection = ((Vector2)mouseWorld - playerPosition).normalized;
    }

    void UpdateAnimationState()
    {
        string lookDirName = GetLookDirectionName(lookDirection);

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        string moveName = MOVE_IDLE;

        if (isMoving)
        {
            moveName = GetMoveNameRelativeToLookDirection(moveInput.normalized, lookDirection);
        }

        string stateName = GetAnimationStateName(lookDirName, moveName);

        ChangeAnimationState(stateName);

        debugLookDir = lookDirName;
        debugMoveType = moveName;
        debugIsMoving = isMoving;
        debugState = stateName;
        debugIsAttacking = isAttacking;
    }

    void TryAttack()
    {
        if (isAttacking) return;

        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        moveInput = Vector2.zero;

        string lookDirName = GetLookDirectionName(lookDirection);
        string attackStateName = lookDirName + ATTACK;
        int attackStateHash = Animator.StringToHash(attackStateName);

        ChangeAnimationState(attackStateName);

        debugLookDir = lookDirName;
        debugMoveType = ATTACK;
        debugIsMoving = false;
        debugState = attackStateName;
        debugIsAttacking = true;

        yield return null;

        while (true)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.shortNameHash == attackStateHash && stateInfo.normalizedTime >= 1f)
            {
                break;
            }

            yield return null;
        }

        isAttacking = false;
        debugIsAttacking = false;

        UpdateAnimationState();
    }


    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        int stateHash = Animator.StringToHash(newState);

        if (!anim.HasState(0, stateHash))
        {
            Debug.LogWarning("Animator에 State가 없습니다: " + newState);
            return;
        }

        anim.Play(stateHash);

        currentState = newState;
    }

    string GetAnimationStateName(string lookDirName, string moveName)
    {
        return lookDirName + moveName;
    }

    string GetLookDirectionName(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.01f)
        {
            return DIR_SOUTH;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle < 0f)
        {
            angle += 360f;
        }

        int index = Mathf.RoundToInt(angle / 45f) % 8;

        switch (index)
        {
            case 0:
                return DIR_EAST;
            case 1:
                return DIR_NORTH_EAST;
            case 2:
                return DIR_NORTH;
            case 3:
                return DIR_NORTH_WEST;
            case 4:
                return DIR_WEST;
            case 5:
                return DIR_SOUTH_WEST;
            case 6:
                return DIR_SOUTH;
            case 7:
                return DIR_SOUTH_EAST;
            default:
                return DIR_SOUTH;
        }
    }

    string GetMoveNameRelativeToLookDirection(Vector2 moveDir, Vector2 lookDir)
    {
        if (moveDir.sqrMagnitude < 0.01f)
        {
            return MOVE_IDLE;
        }

        Vector2 forward = lookDir.normalized;
        Vector2 right = new Vector2(-forward.y, forward.x);

        float forwardDot = Vector2.Dot(moveDir, forward);
        float rightDot = Vector2.Dot(moveDir, right);

        if (Mathf.Abs(forwardDot) >= Mathf.Abs(rightDot))
        {
            if (forwardDot >= 0f)
            {
                return MOVE_FORWARD;
            }

            return MOVE_BACKWARD;
        }

        if (rightDot >= 0f)
        {
            return MOVE_RIGHT;
        }

        return MOVE_LEFT;
    }

    void MovePlayer()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    void OnGUI()
    {
        GUI.Label(
            new Rect(20, 20, 500, 160),
            $"LookDir: {debugLookDir}\nMoveType: {debugMoveType}\nisMoving: {debugIsMoving}\nisAttacking: {debugIsAttacking}\nState: {debugState}"
        );
    }
}
