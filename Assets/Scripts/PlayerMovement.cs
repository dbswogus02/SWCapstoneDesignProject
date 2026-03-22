using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    private Vector2 velocity;
    public float acceleration;
   

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Called by Input System (PlayerInput component)
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = moveInput.normalized * moveSpeed; 
        velocity = Vector2.MoveTowards( velocity, targetVelocity, acceleration * Time.fixedDeltaTime ); 
        rb.linearVelocity = velocity;
    }
}