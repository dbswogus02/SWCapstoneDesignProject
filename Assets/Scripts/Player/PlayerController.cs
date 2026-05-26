using System.Collections;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 4f;

    [Header("Weapon Sprite Library Assets")]
    public SpriteLibraryAsset batSpriteLibraryAsset;
    public SpriteLibraryAsset shotgunSpriteLibraryAsset;
    public SpriteLibraryAsset chainsawSpriteLibraryAsset;

    [Header("Weapon Areas")]
    public Transform aim;
    public GameObject batArea;
    public GameObject shotgunArea;
    public GameObject chainsawArea;

    [Header("Chainsaw Settings")]
    public float chainsawCooldown = 2.0f;
    public float chainsawDuration = 1.0f;
    public float chainsawSpeed = 8f;

    [Header("Shotgun Settings")]
    public float shotgunCooldown = 1.0f;
    public float shotgunDuration = 0.3f;
    public float shotgunRecoil = 2f;

    private Rigidbody2D rb;
    private Animator anim;
    private Camera mainCamera;
    private SpriteLibrary spriteLibrary;

    private Vector2 moveInput;
    public enum Direction8
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
    private Vector2 lookDirection;
    private Direction8 facingDirection;

    private string currentState;
    private bool isAttacking;
    private bool isChainsawDashing;
    private bool isFiringShotgun;
    private Vector2 chainsawDirection;
    private Vector2 shotgunRecoilDirection;
    private float lastShotgunTime = -999f;
    private float lastChainsawTime = -999f;

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

    private const string BAT_ATTACK = "Attack";

    private string debugLookDir;
    private string debugMoveType;
    private bool debugIsMoving;
    private string debugState;
    private bool debugIsAttacking;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
        spriteLibrary = GetComponent<SpriteLibrary>();
    }

    void Update()
    {
        UpdateLookDirection();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            spriteLibrary.spriteLibraryAsset = batSpriteLibraryAsset;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            spriteLibrary.spriteLibraryAsset = shotgunSpriteLibraryAsset;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            spriteLibrary.spriteLibraryAsset = chainsawSpriteLibraryAsset;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryUseWeapon();
        }

        if (isAttacking || isChainsawDashing || isFiringShotgun)
        {
            moveInput = Vector2.zero;
            return;
        }

        ReadMoveInput();
        UpdateAnimationState();
        UpdateAim();
    }

    void FixedUpdate()
    {
        if (isFiringShotgun)
        {
            return;
        }

        if (isChainsawDashing)
        {
            MoveChainsawDash();
            return;
        }

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
        if (mainCamera == null || rb == null) return;

        //Vector3 mouseScreen = Input.mousePosition;
        //mouseScreen.z = Mathf.Abs(mainCamera.transform.position.z);

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector2 playerPosition = rb.position;
        lookDirection = ((Vector2)mouseWorld - playerPosition).normalized;

        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        if (angle < 0f)
        {
            angle += 360f;
        }
        int i = Mathf.RoundToInt(angle / 45f) % 8;
        facingDirection = (Direction8)i;
    }

    void UpdateAnimationState()
    {
        string lookDirName = GetLookDirectionName();

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
        debugIsAttacking = isAttacking || isChainsawDashing || isFiringShotgun;
    }

    void UpdateAim()
    {
        if (aim == null) return;
        
        aim.rotation = facingDirection switch
        {
            Direction8.Right => Quaternion.Euler(0, 0, 0),
            Direction8.UpRight => Quaternion.Euler(0, 0, 45),
            Direction8.Up => Quaternion.Euler(0, 0, 90),
            Direction8.UpLeft => Quaternion.Euler(0, 0, 135),
            Direction8.Left => Quaternion.Euler(0, 0, 180),
            Direction8.DownLeft => Quaternion.Euler(0, 0, 225),
            Direction8.Down => Quaternion.Euler(0, 0, 270),
            Direction8.DownRight => Quaternion.Euler(0, 0, 315),
            _ => Quaternion.identity
        };
    }

    void TryUseWeapon()
    {
        if (isAttacking || isChainsawDashing || isFiringShotgun) return;
        if (spriteLibrary == null) return;
        if (spriteLibrary.spriteLibraryAsset == null) return;

        SpriteLibraryAsset currentAsset = spriteLibrary.spriteLibraryAsset;

        if (currentAsset == batSpriteLibraryAsset)
        {
            StartCoroutine(BatAttack());
            return;
        }

        if (currentAsset == shotgunSpriteLibraryAsset)
        {
            if (Time.time < lastShotgunTime + shotgunCooldown) return;
            StartCoroutine(FireShotgun());
            return;
        }

        if (currentAsset == chainsawSpriteLibraryAsset)
        {
            if (Time.time < lastChainsawTime + chainsawCooldown) return;
            StartCoroutine(ChainsawDash());
            return;
        }
    }

    IEnumerator BatAttack()
    {
        isAttacking = true;
        moveInput = Vector2.zero;

        StartCoroutine(ActivateBatArea(17, 5));

        string lookDirName = GetLookDirectionName();
        string attackStateName = lookDirName + BAT_ATTACK;
        int attackStateHash = Animator.StringToHash(attackStateName);

        ChangeAnimationState(attackStateName);

        debugLookDir = lookDirName;
        debugMoveType = BAT_ATTACK;
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

    IEnumerator ActivateBatArea(int delayFrame, int activeFrame)
    {
        if (batArea == null) yield break;

        for (int i = 0; i < delayFrame; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        batArea.SetActive(true);

        for (int i = 0; i < activeFrame; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        batArea.SetActive(false);
    }

    IEnumerator FireShotgun()
    {
        isFiringShotgun = true;
        moveInput = Vector2.zero;

        StartCoroutine(ActivateShotgunArea(5));

        shotgunRecoilDirection = facingDirection switch
        {
            Direction8.Right => Vector2.right,
            Direction8.UpRight => new Vector2(1, 1).normalized,
            Direction8.Up => Vector2.up,
            Direction8.UpLeft => new Vector2(-1, 1).normalized,
            Direction8.Left => Vector2.left,
            Direction8.DownLeft => new Vector2(-1, -1).normalized,
            Direction8.Down => Vector2.down,
            Direction8.DownRight => new Vector2(1, -1).normalized,
            _ => Vector2.down
        };

        string lookDirName = GetLookDirectionName();
        string stateName = GetAnimationStateName(lookDirName, MOVE_IDLE);

        ChangeAnimationState(stateName);

        debugLookDir = lookDirName;
        debugMoveType = "FireShotgun";
        debugIsMoving = false;
        debugState = stateName;
        debugIsAttacking = true;

        float elapsed = 0f;

        while (elapsed < shotgunDuration)
        {
            float t = elapsed / shotgunDuration;
            float power = Mathf.Lerp(3f, 0.2f, t);

            rb.MovePosition(rb.position - shotgunRecoilDirection * shotgunRecoil * power * Time.fixedDeltaTime);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        lastShotgunTime = Time.time;

        isFiringShotgun = false;
        debugIsAttacking = false;
        UpdateAnimationState();
    }

    IEnumerator ActivateShotgunArea(int frame)
    {
        if (shotgunArea == null) yield break;

        shotgunArea.SetActive(true);

        for (int i = 0; i < frame; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        shotgunArea.SetActive(false);
    }

    IEnumerator ChainsawDash()
    {
        isChainsawDashing = true;
        moveInput = Vector2.zero;

        chainsawDirection = facingDirection switch
        {
            Direction8.Right => Vector2.right,
            Direction8.UpRight => new Vector2(1, 1).normalized,
            Direction8.Up => Vector2.up,
            Direction8.UpLeft => new Vector2(-1, 1).normalized,
            Direction8.Left => Vector2.left,
            Direction8.DownLeft => new Vector2(-1, -1).normalized,
            Direction8.Down => Vector2.down,
            Direction8.DownRight => new Vector2(1, -1).normalized,
            _ => Vector2.down
        };

        string lookDirName = GetLookDirectionName();
        string stateName = GetAnimationStateName(lookDirName, MOVE_FORWARD);

        ChangeAnimationState(stateName);

        debugLookDir = lookDirName;
        debugMoveType = "ChainsawDash";
        debugIsMoving = true;
        debugState = stateName;
        debugIsAttacking = true;

        chainsawArea.SetActive(true);
        anim.speed = 2f;
        yield return new WaitForSeconds(chainsawDuration);
        anim.speed = 1f;
        lastChainsawTime = Time.time;

        isChainsawDashing = false;
        debugIsAttacking = false;
        chainsawArea.SetActive(false);
        UpdateAnimationState();
    }

    void MoveChainsawDash()
    {
        rb.MovePosition(rb.position + chainsawDirection * chainsawSpeed * Time.fixedDeltaTime);
    }

    void ChangeAnimationState(string newState)
    {
        if (currentState == newState) return;

        anim.Play(newState);

        currentState = newState;
    }

    string GetAnimationStateName(string lookDirName, string moveName)
    {
        return lookDirName + moveName;
    }

    string GetLookDirectionName()
    {
        switch (facingDirection)
        {
            case Direction8.Right:
                return DIR_EAST;

            case Direction8.UpRight:
                return DIR_NORTH_EAST;

            case Direction8.Up:
                return DIR_NORTH;

            case Direction8.UpLeft:
                return DIR_NORTH_WEST;

            case Direction8.Left:
                return DIR_WEST;

            case Direction8.DownLeft:
                return DIR_SOUTH_WEST;

            case Direction8.Down:
                return DIR_SOUTH;

            case Direction8.DownRight:
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

        if (forward.sqrMagnitude < 0.01f)
        {
            forward = Vector2.down;
        }

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

    float GetShotgunCooldownRemaining()
    {
        return Mathf.Max(0f, (lastShotgunTime + shotgunCooldown) - Time.time);
    }

    float GetChainsawCooldownRemaining()
    {
        return Mathf.Max(0f, (lastChainsawTime + chainsawCooldown) - Time.time);
    }

    void OnGUI()
    {
        GUI.Label(
            new Rect(20, 20, 500, 220),
            $"LookDir: {debugLookDir}\n" +
            $"MoveType: {debugMoveType}\n" +
            $"isMoving: {debugIsMoving}\n" +
            $"isAttacking: {debugIsAttacking}\n" +
            $"State: {debugState}\n" +
            $"Weapon: {GetCurrentWeaponDebugName()}\n" +
            $"Shotgun CD: {GetShotgunCooldownRemaining():F2}\n" +
            $"Chainsaw CD: {GetChainsawCooldownRemaining():F2}"
        );
    }


    string GetCurrentWeaponDebugName()
    {
        if (spriteLibrary == null) return "No SpriteLibrary";
        if (spriteLibrary.spriteLibraryAsset == null) return "No Asset";

        SpriteLibraryAsset currentAsset = spriteLibrary.spriteLibraryAsset;

        if (currentAsset == batSpriteLibraryAsset) return "Bat";
        if (currentAsset == shotgunSpriteLibraryAsset) return "Shotgun";
        if (currentAsset == chainsawSpriteLibraryAsset) return "Chainsaw";

        return currentAsset.name;
    }
}
