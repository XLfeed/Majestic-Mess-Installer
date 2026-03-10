using Engine;

/// <summary>
/// Movement script that works WITH physics (using velocity instead of direct position)
/// Use this for objects that have RigidBodyComponent
/// </summary>
public class PhysicsMovementScript : Entity
{
    public float moveSpeed = 5.0f;

    public override void OnInit()
    {
        Debug.Log("PhysicsMovementScript initialized");

        if (!HasComponent<RigidBodyComponent>())
        {
            Debug.Log("WARNING: PhysicsMovementScript requires RigidBodyComponent!");
        }
    }

    public override void OnUpdate(float dt)
    {
        if (!HasComponent<RigidBodyComponent>())
            return;

        var rb = GetComponent<RigidBodyComponent>();
        Vector3 velocity = Vector3.Zero;

        // Calculate desired velocity based on input
        if (Input.IsKeyHeld(KeyCode.W))
        {
            velocity.z += moveSpeed;
        }
        if (Input.IsKeyHeld(KeyCode.S))
        {
            velocity.z -= moveSpeed;
        }
        if (Input.IsKeyHeld(KeyCode.A))
        {
            velocity.x -= moveSpeed;
        }
        if (Input.IsKeyHeld(KeyCode.D))
        {
            velocity.x += moveSpeed;
        }
        if (Input.IsKeyHeld(KeyCode.Q))
        {
            velocity.y -= moveSpeed;
        }
        if (Input.IsKeyHeld(KeyCode.E))
        {
            velocity.y += moveSpeed;
        }

        // Set the velocity (physics will handle position update)
        rb.Velocity = velocity;

        // Jump with spacebar
        if (Input.IsKeyPressed(KeyCode.Space))
        {
            rb.ApplyForce(new Vector3(0, 100, 0));
        }
    }
}
