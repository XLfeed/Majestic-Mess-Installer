using Engine;

/// <summary>
/// Movement script for objects WITHOUT RigidBodyComponent
/// Directly sets position (kinematic movement, no physics)
/// </summary>
public class KinematicMovementScript : Entity
{
    public float moveSpeed = 2.0f;

    public override void OnInit()
    {
        Debug.Log("KinematicMovementScript initialized");

        if (HasComponent<RigidBodyComponent>())
        {
            Debug.Log("WARNING: This object has RigidBodyComponent. Physics will override kinematic movement!");
            Debug.Log("Either remove RigidBodyComponent or use PhysicsMovementScript instead.");
        }
    }

    public override void OnUpdate(float dt)
    {
        var pos = Transform.Position;
        bool moved = false;

        if (Input.IsKeyHeld(KeyCode.W))
        {
            pos.z += moveSpeed * dt;
            moved = true;
        }
        if (Input.IsKeyHeld(KeyCode.S))
        {
            pos.z -= moveSpeed * dt;
            moved = true;
        }
        if (Input.IsKeyHeld(KeyCode.A))
        {
            pos.x -= moveSpeed * dt;
            moved = true;
        }
        if (Input.IsKeyHeld(KeyCode.D))
        {
            pos.x += moveSpeed * dt;
            moved = true;
        }
        if (Input.IsKeyHeld(KeyCode.Q))
        {
            pos.y -= moveSpeed * dt;
            moved = true;
        }
        if (Input.IsKeyHeld(KeyCode.E))
        {
            pos.y += moveSpeed * dt;
            moved = true;
        }

        if (moved)
        {
            Transform.Position = pos;
        }
    }
}
