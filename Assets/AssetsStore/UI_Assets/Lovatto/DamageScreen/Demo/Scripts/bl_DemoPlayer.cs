using UnityEngine;
using UnityEngine.InputSystem;

namespace Lovatto.DamageScreen.Demo
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class bl_DemoPlayer : MonoBehaviour
    {
        [Header("Camera & Look")]
        [SerializeField] private Transform playerCamera;
        [Tooltip("Degrees per raw pixel. 0.10–0.20 feels like CS default at 400 DPI.")]
        [SerializeField] private float mouseSensitivity = 0.12f;
        [Tooltip("How quickly the camera catches up to the mouse input (higher = more responsive, lower = smoother). 0 disables smoothing.")]
        [SerializeField][Range(0f, 50f)] private float lookSmoothSpeed = 30f;

        [Header("Movement Physics")]
        [SerializeField] private float gravity = 20f;
        [SerializeField] private float friction = 6f;
        [SerializeField] private float jumpForce = 8f;

        [Header("Speed & Acceleration")]
        [SerializeField] private float maxGroundSpeed = 7f;
        [SerializeField] private float maxAirSpeed = 0.6f;
        [SerializeField] private float groundAccel = 10f;
        [SerializeField] private float airAccel = 150f;

        [Header("Impact Push")]
        [SerializeField] private float impactPushStrength = 6f;
        [SerializeField] private float impactSpeedMultiplier = 0.15f;
        [SerializeField] private float impactUpwardBoost = 1f;

        [Header("Damage Camera Shake")]
        [SerializeField] private float damageShakeDuration = 0.18f;
        [SerializeField] private float damageShakeMagnitude = 0.14f;
        [SerializeField] private float damageShakeFrequency = 45f;

        // Angle above which a surface is treated as a surf ramp (skip ground friction).
        private const float RampSlopeAngle = 28f;

        private CharacterController controller;
        private Transform tr;
        private Vector3 playerVelocity;
        private bool isGrounded;

        // Look state – store raw target and smoothed values separately.
        private float targetYaw;
        private float targetPitch;
        private float yaw;
        private float pitch;

        // Surface tracking: set during controller.Move(), used next frame.
        private float lastSurfaceAngle;

        private Vector3 cameraBaseLocalPosition;
        private float shakeTimeRemaining;
        private float shakeDurationCurrent;
        private float shakeMagnitudeCurrent;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            tr = transform;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Initialise yaw from the object's starting rotation so there's no snap.
            yaw = targetYaw = tr.eulerAngles.y;
            pitch = targetPitch = 0f;

            if (playerCamera != null)
                cameraBaseLocalPosition = playerCamera.localPosition;
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Apply look first so movement uses the up-to-date facing direction.
            HandleLook(dt);

            isGrounded = controller.isGrounded;

            // Consume surface data that was written during last frame's Move().
            bool onRamp = lastSurfaceAngle > RampSlopeAngle;
            lastSurfaceAngle = 0f;

            Vector2 input = Vector2.zero;
            if (Keyboard.current != null)
            {
                if (Keyboard.current.aKey.isPressed) input.x -= 1f;
                if (Keyboard.current.dKey.isPressed) input.x += 1f;
                if (Keyboard.current.sKey.isPressed) input.y -= 1f;
                if (Keyboard.current.wKey.isPressed) input.y += 1f;
            }
            if (Gamepad.current != null)
                input += Gamepad.current.leftStick.ReadValue();

            input = Vector2.ClampMagnitude(input, 1f);

            Vector3 wishDir = (tr.right * input.x + tr.forward * input.y).normalized;

            if (isGrounded)
                GroundMove(wishDir, dt, onRamp);
            else
                AirMove(wishDir, dt);

            controller.Move(playerVelocity * dt);
            UpdateDamageShake(dt);
        }

        // -------------------------------------------------------------------------
        //  Look
        // -------------------------------------------------------------------------

        private void HandleLook(float dt)
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return; // Don't look around if the cursor isn't locked (e.g. designer is open).
            }

            Vector2 rawDelta = Vector2.zero;

            if (Mouse.current != null)
                rawDelta += Mouse.current.delta.ReadValue() * mouseSensitivity;

            if (Gamepad.current != null)
            {
                // Gamepad stick needs its own rate since it has no "pixels" concept.
                Vector2 rs = Gamepad.current.rightStick.ReadValue();
                rawDelta += rs * (mouseSensitivity * 180f * Time.unscaledDeltaTime);
            }

            // Accumulate into the target angles immediately (zero input lag).
            targetYaw += rawDelta.x;
            targetPitch -= rawDelta.y;
            targetPitch = Mathf.Clamp(targetPitch, -89f, 89f);

            // Exponential smooth toward the target – frame-rate independent.
            if (lookSmoothSpeed > 0f)
            {
                float t = 1f - Mathf.Exp(-lookSmoothSpeed * Time.unscaledDeltaTime);
                yaw = Mathf.LerpAngle(yaw, targetYaw, t);
                pitch = Mathf.Lerp(pitch, targetPitch, t);
            }
            else
            {
                yaw = targetYaw;
                pitch = targetPitch;
            }

            // Assign via Quaternion to avoid Euler-angle clamping artefacts.
            tr.rotation = Quaternion.Euler(0f, yaw, 0f);
            playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        // -------------------------------------------------------------------------
        //  Movement
        // -------------------------------------------------------------------------

        private void GroundMove(Vector3 wishDir, float dt, bool onRamp)
        {
            // Surf ramps: skip friction so the player keeps their speed while sliding.
            if (!onRamp)
                ApplyFriction(dt);

            Accelerate(wishDir, maxGroundSpeed, groundAccel, dt);

            // A small constant downward push keeps the CharacterController reliably
            // grounded on slopes without resetting meaningful vertical velocity.
            if (playerVelocity.y < 0f)
                playerVelocity.y = -2f;

            bool jumpPressed = (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
                               (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);

            if (jumpPressed)
            {
                playerVelocity.y = jumpForce;
                isGrounded = false;
            }
        }

        private void AirMove(Vector3 wishDir, float dt)
        {
            Accelerate(wishDir, maxAirSpeed, airAccel, dt);
            playerVelocity.y -= gravity * dt;
        }

        private void Accelerate(Vector3 wishDir, float wishSpeed, float accel, float dt)
        {
            float currentSpeed = Vector3.Dot(playerVelocity, wishDir);
            float addSpeed = wishSpeed - currentSpeed;
            if (addSpeed <= 0f) return;

            float accelSpeed = Mathf.Min(accel * dt * wishSpeed, addSpeed);

            playerVelocity.x += accelSpeed * wishDir.x;
            playerVelocity.z += accelSpeed * wishDir.z;
        }

        private void ApplyFriction(float dt)
        {
            Vector3 hVel = new Vector3(playerVelocity.x, 0f, playerVelocity.z);
            float speed = hVel.magnitude;
            if (speed <= 0.1f) return;

            float control = Mathf.Max(speed, groundAccel);
            float newSpeed = Mathf.Max(speed - control * friction * dt, 0f) / speed;

            playerVelocity.x *= newSpeed;
            playerVelocity.z *= newSpeed;
        }

        // -------------------------------------------------------------------------
        //  External forces
        // -------------------------------------------------------------------------

        public void PushFromImpact(Vector3 collisionPosition, Vector3 projectileVelocity)
        {
            if (tr == null)
                tr = transform;

            Vector3 pushDirection = tr.position - collisionPosition;
            pushDirection = Vector3.ProjectOnPlane(pushDirection, Vector3.up);

            if (pushDirection.sqrMagnitude < 0.0001f && projectileVelocity.sqrMagnitude > 0.0001f)
                pushDirection = Vector3.ProjectOnPlane(projectileVelocity, Vector3.up);

            if (pushDirection.sqrMagnitude < 0.0001f)
                pushDirection = tr.forward;

            pushDirection.Normalize();

            float pushForce = impactPushStrength + projectileVelocity.magnitude * impactSpeedMultiplier;
            playerVelocity.x += pushDirection.x * pushForce;
            playerVelocity.z += pushDirection.z * pushForce;

            playerVelocity.y = Mathf.Max(playerVelocity.y, impactUpwardBoost);
            isGrounded = false;
        }

        public void PlayDamageCameraShake(float intensity = 1f)
        {
            if (playerCamera == null || intensity <= 0f)
                return;

            float scaledIntensity = Mathf.Max(intensity, 0f);

            shakeDurationCurrent = Mathf.Max(shakeDurationCurrent, damageShakeDuration);
            shakeTimeRemaining = Mathf.Max(shakeTimeRemaining, damageShakeDuration);
            shakeMagnitudeCurrent = Mathf.Max(shakeMagnitudeCurrent, damageShakeMagnitude * scaledIntensity);
        }

        private void UpdateDamageShake(float dt)
        {
            if (playerCamera == null)
                return;

            if (shakeTimeRemaining > 0f)
            {
                shakeTimeRemaining -= dt;

                float life = shakeDurationCurrent > 0f
                    ? Mathf.Clamp01(shakeTimeRemaining / shakeDurationCurrent)
                    : 0f;

                float amplitude = shakeMagnitudeCurrent * life;
                float noiseTime = Time.time * damageShakeFrequency;
                float shakeX = (Mathf.PerlinNoise(noiseTime, 0f) - 0.5f) * 2f * amplitude;
                float shakeY = (Mathf.PerlinNoise(0f, noiseTime) - 0.5f) * 2f * amplitude;

                playerCamera.localPosition = cameraBaseLocalPosition + new Vector3(shakeX, shakeY, 0f);

                if (shakeTimeRemaining <= 0f)
                {
                    shakeTimeRemaining = 0f;
                    shakeDurationCurrent = 0f;
                    shakeMagnitudeCurrent = 0f;
                    playerCamera.localPosition = cameraBaseLocalPosition;
                }

                return;
            }

            playerCamera.localPosition = cameraBaseLocalPosition;
        }

        // -------------------------------------------------------------------------
        //  Collision / Surf ramp sliding
        // -------------------------------------------------------------------------

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Track the steepest surface angle touched this frame.
            float angle = Vector3.Angle(Vector3.up, hit.normal);
            if (angle > lastSurfaceAngle)
                lastSurfaceAngle = angle;

            // Clip velocity into any steep surface (walls and surf ramps).
            if (hit.normal.y < 0.7f)
                ClipVelocity(hit.normal);
        }

        private void ClipVelocity(Vector3 normal)
        {
            // 1.001f slightly over-corrects to prevent micro-bouncing on ramp seams.
            float backoff = Vector3.Dot(playerVelocity, normal) * 1.001f;
            if (backoff >= 0f) return; // Moving away from the surface – nothing to clip.

            playerVelocity -= normal * backoff;
        }
    }
}