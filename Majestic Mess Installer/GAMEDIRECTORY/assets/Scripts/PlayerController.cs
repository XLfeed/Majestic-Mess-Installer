using Engine;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class PlayerController : Entity
{
    // ===== Movement =====
    public float moveSpeed = 7.0f;
    public float sprintMultiplier = 2.3f;
    public float crouchMultiplier = 0.5f;

    // ===== Crouch =====
    private const float normalHeightMultiplier = 1.0f;
    private const float crouchHeightMultiplier = 0.75f;
    private const float disguisedHeightMultiplier = 1.0f;

    // ===== Camera-relative movement =====
    private const string CameraEntityName = "MainCamera";
    private Entity cameraEntity;
    private TransformComponent cameraTransform;

    // ===== cached components =====
    private RigidBodyComponent rb;
    private TransformComponent tf;

    // ===== runtime state =====
    public bool isSprinting = false;
    public bool isCrouching = false;
    private Vector3 initialScale;          // e.g. (0.001, 0.001, 0.001) for skinned meshes
    private float initialBottomY;          // world Y of the feet when standing

    // ===== Model facing / rotation =====
    public float modelYawOffset = 0.0f;
    private const float Rad2Deg = (float)(180.0 / Math.PI);
    public float turnSpeed = (float)(3.0 * Math.PI);         // degrees per second for model turning
    private const float ModelForwardOffset = (float)Math.PI;
    public float disguiseFacingOffset = (float)(Math.PI); // face forward when disguised

    public PlayerController() : base() { }
    public PlayerController(ulong id) : base(id) { }

    public bool isHidden = false;
   
    public override void OnInit()
    {
        if (!HasComponent<RigidBodyComponent>())
        {
            Debug.Log("[PlayerController] Missing RigidBodyComponent!");
            return;
        }
        rb = RigidBody;
        tf = Transform;

        // Resolve camera for camera-relative movement
        cameraEntity = Entity.FindEntityByName(CameraEntityName);
        if (cameraEntity == null || !cameraEntity.IsValid())
        {
            Debug.Log($"[PlayerController] Warning: unable to find camera entity '{CameraEntityName}'. " +
                      "Falling back to world-relative movement.");
            cameraEntity = null;
            cameraTransform = null;
        }
        else
        {
            cameraTransform = cameraEntity.Transform;
            Debug.Log($"[PlayerController] Using camera '{CameraEntityName}' for camera-relative movement.");
        }

        Debug.Log("[PlayerController] Init (movement + crouch + disguise).");

        // Store the initial scale set in the editor (e.g., 0.001 for skinned meshes)
        initialScale = tf.Scale;

        // Calculate the initial bottom Y position (feet position when standing)
        initialBottomY = tf.Position.y - (initialScale.y * 0.5f);

        Debug.Log("[PlayerController] Init (movement + crouch + cinder + disguise). Initial scale: " + initialScale);
    }

    public override void OnUpdate(float dt)
    {
        if (rb == null || tf == null)
            return;

        if (PlayerInputBlocker.IsBlocked)
        {
            Vector3 v = rb.Velocity;
            rb.Velocity = new Vector3(0f, v.y, 0f);
            isSprinting = false;
            isCrouching = false;
            return;
        }

        //var rot = tf.Rotation;
        //rot.x += 30.0f * dt;
        //rot.y += 45.0f * dt;   // 45 degrees per second
        //tf.Rotation = rot;

        HandleMovement(dt);
        HandleCrouch();
        HandleModelInputRotation(dt);
        AlignModelToCamera(); 
        //HandleInteractions(dt); // F
        //HandleScrollOfCinder(); // E
        //HandleDisguise();       // R

        // Apply final height last (disguise has priority over crouch)
        ApplyHeight();
    }


    // ---------------- Movement ----------------
    private void HandleMovement(float dt)
    {
        // Input in "local" WASD space
        float inputX = 0f; // A/D
        float inputZ = 0f; // W/S

        if (Input.IsKeyHeld(KeyCode.W)) inputZ += 1f;
        if (Input.IsKeyHeld(KeyCode.S)) inputZ -= 1f;
        if (Input.IsKeyHeld(KeyCode.A)) inputX -= 1f;
        if (Input.IsKeyHeld(KeyCode.D)) inputX += 1f;

        // No input: stop horizontal motion (model can still rotate with camera)
        if (Math.Abs(inputX) < 0.0001f && Math.Abs(inputZ) < 0.0001f)
        {
            Vector3 v = rb.Velocity;
            rb.Velocity = new Vector3(0f, v.y, 0f);
            isSprinting = false;
            return;
        }

        // Compute camera-relative forward/right on the XZ plane
        Vector3 moveDir;

        if (cameraTransform != null)
        {
            Vector3 playerPos = tf.Position;
            Vector3 camPos = cameraTransform.Position;

            // Direction from camera to player (camera look dir)
            Vector3 camToPlayer = playerPos - camPos;
            camToPlayer.y = 0f;

            float sqrMag = camToPlayer.x * camToPlayer.x + camToPlayer.z * camToPlayer.z;
            if (sqrMag > 0.0001f)
            {
                float invLen = 1.0f / (float)Math.Sqrt(sqrMag);
                Vector3 forward = new Vector3(camToPlayer.x * invLen, 0f, camToPlayer.z * invLen); // "W"
                Vector3 right = new Vector3(-forward.z, 0f, forward.x);               // "D"

                moveDir = forward * inputZ + right * inputX;
            }
            else
            {
                // Degenerate case: camera exactly at player, fall back to world axes
                moveDir = new Vector3(inputX, 0f, inputZ);
            }
        }
        else
        {
            // Fallback: old world-relative WASD
            moveDir = new Vector3(inputX, 0f, inputZ);
        }

        // Normalize movement direction so diagonals aren't faster
        float mag = (float)Math.Sqrt(moveDir.x * moveDir.x + moveDir.z * moveDir.z);
        if (mag > 0.0001f)
        {
            float invMag = 1f / mag;
            moveDir.x *= invMag;
            moveDir.z *= invMag;
        }

        isSprinting = Input.IsKeyHeld(KeyCode.LeftShift) && !isCrouching;

        float currentSpeed = moveSpeed;
        if (isCrouching) currentSpeed *= crouchMultiplier;
        else if (isSprinting) currentSpeed *= sprintMultiplier;

        Vector3 desiredVelocity = new Vector3(moveDir.x * currentSpeed, 0f, moveDir.z * currentSpeed);
        Vector3 currentVel = rb.Velocity;

        rb.Velocity = new Vector3(
            desiredVelocity.x,
            currentVel.y,
            desiredVelocity.z
        );
    }

    private void HandleModelInputRotation(float dt)
    {
        // Read WASD again here purely for facing
        float inputX = 0f;
        float inputZ = 0f;

        if (Input.IsKeyHeld(KeyCode.W)) inputZ += 1f;
        if (Input.IsKeyHeld(KeyCode.S)) inputZ -= 1f;
        if (Input.IsKeyHeld(KeyCode.A)) inputX -= 1f;
        if (Input.IsKeyHeld(KeyCode.D)) inputX += 1f;

        // No movement input = keep current facing
        if (Math.Abs(inputX) < 0.0001f && Math.Abs(inputZ) < 0.0001f)
            return;

        // Note: still using radians
        float desiredOffsetRad = (float)Math.Atan2(inputX, -inputZ);

        // Rotate modelYawOffset toward desiredOffsetRad using shortest path (in radians)
        float maxStep = turnSpeed * dt; // radians we can turn this frame

        float delta = NormalizeAnglePi(desiredOffsetRad - modelYawOffset);
        if (Math.Abs(delta) <= maxStep)
        {
            modelYawOffset = desiredOffsetRad;
        }
        else
        {
            modelYawOffset += Math.Sign(delta) * maxStep;
        }

        modelYawOffset = NormalizeAnglePi(modelYawOffset);
    }

    private void AlignModelToCamera()
    {
        if (cameraTransform == null)
            return;

        Vector3 camRot = cameraTransform.Rotation;
        float camYaw = camRot.y;

        var rot = tf.Rotation;
        rot.x = 0f;   // no pitch
        rot.z = 0f;   // no roll
                      
        float extraDisguiseYaw = DisguiseFlag.IsOn(this.ID) ? disguiseFacingOffset : 0f; // Extra yaw only when disguised
        rot.y = -camYaw + modelYawOffset + ModelForwardOffset + extraDisguiseYaw;

        tf.Rotation = rot;
    }

    private static float NormalizeAnglePi(float angle)
    {
        float twoPi = (float)(2.0 * Math.PI);
        angle %= twoPi;
        if (angle > Math.PI) angle -= twoPi;
        if (angle < -Math.PI) angle += twoPi;
        return angle;
    }

    // ---------------- Crouch ----------------
    private void HandleCrouch()
    {
        isCrouching = Input.IsKeyHeld(KeyCode.Space);
    }

    private void ApplyHeight()
    {
        bool disguised = DisguiseFlag.IsOn(this.ID);

        float heightMultiplier = disguised
            ? disguisedHeightMultiplier
            : (isCrouching ? crouchHeightMultiplier : normalHeightMultiplier);

        // Desired vertical scale based on original standing height
        float desiredHeight = initialScale.y * heightMultiplier;

        // 1) Apply new scale
        Vector3 s = tf.Scale;
        if (Math.Abs(s.y - desiredHeight) > 0.0001f)
        {
            s.y = desiredHeight;
            tf.Scale = s;
        }
    }
}
