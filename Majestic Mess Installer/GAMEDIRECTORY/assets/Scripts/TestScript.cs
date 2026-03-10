using Engine;

/// <summary>
/// Simple test script to verify the scripting system works
/// </summary>
public class TestScript : Entity
{
    // === INSPECTOR TEST FIELDS ===

    // Basic types (Edit + Play mode)
    public float moveSpeed = 2.0f;
    public float rotationSpeed = 50.0f;
    public int health = 100;
    public bool isAlive = true;
    public double preciseValue = 3.14159;

    // Vectors (Edit + Play mode)
    public Vector2 screenPosition = new Vector2(100, 200);
    public Vector3 spawnPoint = new Vector3(0, 5, 0);
    public Vector3 targetPosition = new Vector3(10, 0, 10);

    // Entity reference (Edit + Play mode)
    public Entity targetEntity;

    // Advanced types (Play mode ONLY)
    public string playerName = "TestPlayer";
    public float[] damageMultipliers = new float[] { 1.0f, 1.5f, 2.0f };
    public int[] levelThresholds = new int[] { 100, 250, 500, 1000 };
    public Vector3[] patrolPoints = new Vector3[] {
        new Vector3(0, 0, 0),
        new Vector3(5, 0, 5)
    };

    private float elapsedTime = 0.0f;

    /// <summary>
    /// Called once when the scene starts
    /// </summary>
    public override void OnInit()
    {
        Debug.Log("=== TestScript Initialized ===");
        Debug.Log($"********** THIS SCRIPT IS ON ENTITY: {Name} **********");

        // Log inspector field values
        Debug.Log($"Move Speed: {moveSpeed}");
        Debug.Log($"Rotation Speed: {rotationSpeed}");
        Debug.Log($"Health: {health}");
        Debug.Log($"Is Alive: {isAlive}");
        Debug.Log($"Precise Value: {preciseValue}");
        Debug.Log($"Screen Position: ({screenPosition.x}, {screenPosition.y})");
        Debug.Log($"Spawn Point: ({spawnPoint.x}, {spawnPoint.y}, {spawnPoint.z})");
        Debug.Log($"Target Position: ({targetPosition.x}, {targetPosition.y}, {targetPosition.z})");

        // DEBUG: Entity reference debugging
        Debug.Log($"[ENTITY DEBUG] targetEntity is null? {targetEntity == null}");
        if (targetEntity != null)
        {
            Debug.Log($"[ENTITY DEBUG] targetEntity.ID = {targetEntity.ID}");
            Debug.Log($"[ENTITY DEBUG] targetEntity.IsValid() = {targetEntity.IsValid()}");
        }
        Debug.Log($"Target Entity: {(targetEntity != null && targetEntity.IsValid() ? targetEntity.Name : "None")}");
        Debug.Log($"Player Name: {playerName}");
        Debug.Log($"Damage Multipliers Count: {damageMultipliers.Length}");
        Debug.Log($"Level Thresholds Count: {levelThresholds.Length}");
        Debug.Log($"Patrol Points Count: {patrolPoints.Length}");

        // Log initial transform
        var pos = Transform.Position;
        Debug.Log($"********** ENTITY '{Name}' STARTING POSITION: X={pos.x}, Y={pos.y}, Z={pos.z} **********");
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    public override void OnUpdate(float dt)
    {
        elapsedTime += dt;

        // Test 1: Keyboard Input & Transform Movement
        var pos = Transform.Position;
        bool moved = false;

        if (Input.IsKeyHeld(KeyCode.W))
        {
            Debug.Log("W KEY HELDDD! Moving forward");
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

        // Test 2: Key Press Detection
        if (Input.IsKeyPressed(KeyCode.Space))
        {
            Debug.Log("SPACE pressed! Testing physics...");

            // Test 3: RigidBody Component Access & Physics
            if (HasComponent<RigidBodyComponent>())
            {
                var rb = GetComponent<RigidBodyComponent>();
                rb.ApplyForce(new Vector3(0, 10, 0));
                Debug.Log("Applied upward force!");
            }
            else
            {
                Debug.Log("No RigidBody component found - add one to test physics!");
            }
        }

        // Test 4: Rotation with arrow keys
        var rot = Transform.Rotation;
        bool rotated = false;

        if (Input.IsKeyHeld(KeyCode.LeftArrow))
        {
            rot.y -= rotationSpeed * dt;
            rotated = true;
        }
        if (Input.IsKeyHeld(KeyCode.RightArrow))
        {
            rot.y += rotationSpeed * dt;
            rotated = true;
        }

        if (rotated)
        {
            Transform.Rotation = rot;
        }

        // Test 5: Mouse Input
        if (Input.IsMouseButtonPressed(0))
        {
            var mousePos = Input.GetMousePosition();
            Debug.Log($"Left Click at: ({mousePos.x}, {mousePos.y})");
        }

        // Test 6: Time Functions
        if (Input.IsKeyPressed(KeyCode.T))
        {
            float totalTime = Time.GetTime();
            Debug.Log($"Total Time: {totalTime}s, Delta Time: {dt}s, Frame Time: {elapsedTime}s");
        }

        // Test 7: Entity Finding
        if (Input.IsKeyPressed(KeyCode.F))
        {
            var camera = FindEntityByName("Camera");
            if (camera.IsValid())
            {
                Debug.Log($"Found entity: {camera.Name}");
            }
            else
            {
                Debug.Log("No entity named 'Camera' found");
            }
        }

        // Test 8: Scene Loading
        if (Input.IsKeyPressed(KeyCode.L))
        {
            Debug.Log("Testing scene load (will reload current scene if available)");
            // Scene.LoadScene("YourSceneName"); // Uncomment and add your scene name
        }

        // Periodic status update every 5 seconds
        if ((int)elapsedTime % 5 == 0 && (int)(elapsedTime - dt) % 5 != 0)
        {
            Debug.Log($"[{elapsedTime:F1}s] TestScript is running! Position: ({pos.x:F2}, {pos.y:F2}, {pos.z:F2})");
        }
    }

    /// <summary>
    /// Called when the entity collides with another entity (if RigidBody exists)
    /// </summary>
    public override void OnCollisionEnter(CollisionInfo collision)
    {
        Debug.Log($"*** COLLISION ENTER with: {collision.OtherEntity.Name} ***");
    }

    public override void OnCollisionStay(CollisionInfo collision)
    {
        // Called every frame while colliding
        // Commented out to avoid spam
        // Debug.Log($"Collision Stay with: {collision.OtherEntity.Name}");
    }

    public override void OnCollisionExit(CollisionInfo collision)
    {
        Debug.Log($"*** COLLISION EXIT from: {collision.OtherEntity.Name} ***");
    }

    /// <summary>
    /// Called when the entity enters a trigger volume
    /// </summary>
    public override void OnTriggerEnter(ColliderComponent collider)
    {
        Debug.Log("*** TRIGGER ENTER ***");
    }

    public override void OnTriggerStay(ColliderComponent collider)
    {
        // Called every frame while in trigger
    }

    public override void OnTriggerExit(ColliderComponent collider)
    {
        Debug.Log("*** TRIGGER EXIT ***");
    }

    /// <summary>
    /// Called when the scene stops
    /// </summary>
    public override void OnExit()
    {
        Debug.Log("=== TestScript Exiting ===");
        Debug.Log($"Total runtime: {elapsedTime:F2} seconds");
    }
}
