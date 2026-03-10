using Engine;

/// <summary>
/// Static helper class for navigation mesh pathfinding
/// </summary>
public static class NavMesh
{
    /// <summary>
    /// Finds a path from start position to goal position using the navigation mesh
    /// </summary>
    /// <param name="start">Starting position</param>
    /// <param name="goal">Goal/target position</param>
    /// <returns>Array of waypoint positions representing the path, or empty array if no path found</returns>
    public static Vector3[] FindPath(Vector3 start, Vector3 goal)
    {
        return InternalCalls.NavMesh_FindPath(start, goal);
    }
}
