using System;
using System.Collections.Generic;
using Engine;

public class EnemyMovement : Entity
{
    // movement
    public float moveSpeed = 5f;
    public float rotSpeed = 5f;

    // goal waypoints (where the enemy should go)
    public List<Vector3> goalWaypoints = new List<Vector3>();
    public List<Vector3> waypointsRot = new List<Vector3>();
    public int currentGoal = 0;

    // navmesh path (how to get to the current goal)
    private List<Vector3> navPath = new List<Vector3>();
    private int currentPathIndex = 0;

    public float wpThreshhold = 1f;


    // ===================== FOV / CHASE CONFIG =====================

    /// <summary>
    /// Cached reference to EnemyFOV script on this same entity.
    /// </summary>
    private EnemyFOV fov;

    /// <summary>
    /// Tag we consider as a valid chase target (must match EnemyFOV.detectedTag).
    /// </summary>
    public string chaseTag = "Player";


    public override void OnInit()
    {
        EnemyRegistry.Register(this);

        // Cache FOV script (if attached)
        fov = GetScript<EnemyFOV>();
        if (fov == null)
            Console.WriteLine($"[EnemyMovement] {Name}: EnemyFOV script not found, will only patrol.");

        // Define goal positions (destinations the enemy should patrol)
        Vector3 wp1 = new Vector3(-20f, -0.699f, -18f);
       // Vector3 wp2 = new Vector3(-20f, -0.699f, 18f);
        Vector3 wp3 = new Vector3(20f, -0.699f, 18f);
       // Vector3 wp4 = new Vector3(20f, -0.699f, -18f);

        goalWaypoints.Add(wp1);
       // goalWaypoints.Add(wp2);
        goalWaypoints.Add(wp3);
      //  goalWaypoints.Add(wp4);

        Vector3 wp1Rot = new Vector3(0f, -90f, 0f);   // look left
        Vector3 wp2Rot = new Vector3(0f, 0f, 0f);    // look down
        Vector3 wp3Rot = new Vector3(0f, 90f, 0f);  // look right
        Vector3 wp4Rot = new Vector3(0f, 180f, 0f);  // look up

        waypointsRot.Add(wp1Rot);
        waypointsRot.Add(wp2Rot);
        waypointsRot.Add(wp3Rot);
        waypointsRot.Add(wp4Rot);

        // Calculate initial navmesh path to first goal
        CalculateNavPath();
    }
    public override void OnExit() 
    { 
        EnemyRegistry.Unregister(this); 
    }

    public override void OnUpdate(float dt)
    {
        if (navPath == null || navPath.Count == 0)
            return;

        Vector3 currPos = Transform.Position;

        // Follow navmesh path to current goal
        if (currentPathIndex < navPath.Count)
        {
            Vector3 targetPos = navPath[currentPathIndex];

            // Move towards current waypoint on the path
            Transform.Position = MoveTowards(currPos, targetPos, moveSpeed * dt);

            // Check if reached current waypoint
            if (Vector3.Distance(Transform.Position, targetPos) < wpThreshhold)
            {
                currentPathIndex++;

                // If reached end of navmesh path, move to next goal
                if (currentPathIndex >= navPath.Count)
                {
                    currentGoal = (currentGoal + 1) % goalWaypoints.Count;
                    CalculateNavPath();
                }
            }

            // ============================
            // FOV → check if can see player
            // ============================
            if (fov != null)
            {
                foreach (Entity e in fov.VisibleEntities)
                {
                    string tag = e.GetComponent<TagComponent>().Tag;  // ✅ Uses component wrapper

                    if (tag == chaseTag)   // "Player"
                    {
                        //Console.WriteLine("[EnemyMovement] Enemy sees PLAYER → switch to CHASE");
                        // TODO: set state = Chasing
                        return;
                    }
                }
            }



        }
    }

    private void CalculateNavPath()
    {
        if (goalWaypoints.Count == 0)
            return;

        Vector3 startPos = Transform.Position;
        Vector3 goalPos = goalWaypoints[currentGoal];

        Console.WriteLine($"[EnemyMovement] Requesting path: start({startPos.x}, {startPos.y}, {startPos.z}) goal({goalPos.x}, {goalPos.y}, {goalPos.z})");

        // Query navmesh for path
        Vector3[] pathArray = NavMesh.FindPath(startPos, goalPos);

        if (pathArray != null && pathArray.Length > 0)
        {
            navPath = new List<Vector3>(pathArray);
            currentPathIndex = 0;
            Console.WriteLine($"[EnemyMovement] Found navmesh path with {navPath.Count} waypoints");
        }
        else
        {
            Console.WriteLine("[EnemyMovement] Failed to find navmesh path, using direct movement");
            // Fallback: direct path if navmesh fails
            navPath = new List<Vector3> { goalPos };
            currentPathIndex = 0;
        }
    }

    public static Vector3 RotateTowardsY(Vector3 from, Vector3 to, float speed, float deltatime)
    {
        // Calculate shortest path around 360°
        float deltaY = ((to.y - from.y + 540f) % 360f) - 180f;

        // Max step this frame
        float maxStep = speed * deltatime;

        // Clamp delta to maxStep to avoid overshoot
        if (Math.Abs(deltaY) <= maxStep)
            return new Vector3(from.x, to.y, from.z); // reached target

        float newY = from.y + MathF.Sign(deltaY) * maxStep;

        return new Vector3(from.x, newY, from.z);
    }

    public static Vector3 MoveTowards(Vector3 from, Vector3 to, float speed)
    {
        //Vector3 dir = new Vector3(to.x - from.x, to.y - from.y, to.z - from.z);
        Vector3 dir = to - from;

        float dist = dir.Mag;

        if (dist <= 0.1f || dist == 0f)
            return to;

        //return new Vector3(from.x + dir.x / dist * speed, from.y + dir.y / dist * speed, from.z + dir.z / dist * speed);
        return from + dir / dist * speed;
    }
}
