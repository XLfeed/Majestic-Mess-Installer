using System;
using System.Collections.Generic;
using Engine;

public class WizardBodyRotate : Entity
{
    // Cached references
    private TransformComponent tf;
    private Entity player;
    private TransformComponent playerTf;

    // Configuration - exposed for editor
    public float rotationSpeed = 5.0f;  // Radians per second for smooth rotation
    public bool smoothRotation = true;  // Set false for instant facing

    public override void OnInit()
    {
        tf = Transform;

        // Find the player entity
        player = Entity.FindEntityByName("Player");
        if (player != null && player.IsValid())
        {
            playerTf = player.Transform;
        }
    }

    public override void OnUpdate(float dt)
    {
        // Validate player still exists
        if (player == null || !player.IsValid() || playerTf == null)
            return;

        // Calculate direction to player (ignore Y for yaw-only rotation)
        Vector3 myPos = tf.Position;
        Vector3 targetPos = playerTf.Position;

        float dx = targetPos.x - myPos.x;
        float dz = targetPos.z - myPos.z;

        // Avoid rotation when too close (prevent jittering)
        if (dx * dx + dz * dz < 0.01f)
            return;

        // Calculate target yaw angle (Atan2 gives angle in radians)
        float targetYaw = MathF.Atan2(dx, dz);

        if (smoothRotation)
        {
            // Smooth rotation - interpolate towards target
            tf.Rotation = SmoothLookAt(tf.Rotation, targetYaw, rotationSpeed * dt);
        }
        else
        {
            // Instant rotation - snap to face player
            tf.Rotation = new Vector3(tf.Rotation.x, targetYaw, tf.Rotation.z);
        }
    }

    private Vector3 SmoothLookAt(Vector3 currentRot, float targetYaw, float maxStep)
    {
        float currentYaw = currentRot.y;

        // Calculate shortest rotation path (wrap around -PI to PI)
        float delta = targetYaw - currentYaw;
        while (delta > MathF.PI) delta -= 2f * MathF.PI;
        while (delta < -MathF.PI) delta += 2f * MathF.PI;

        // Clamp rotation step to max speed
        float step = MathF.Min(MathF.Abs(delta), maxStep) * MathF.Sign(delta);
        float newYaw = currentYaw + step;

        return new Vector3(currentRot.x, newYaw, currentRot.z);
    }
}
