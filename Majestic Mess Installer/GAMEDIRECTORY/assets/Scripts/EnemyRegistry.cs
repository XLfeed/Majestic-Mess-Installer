using System.Collections.Generic;
using Engine;

public static class EnemyRegistry
{
    static readonly List<Entity> enemies = new List<Entity>();

    public static void Register(Entity e)
    {
        if (e != null && !enemies.Contains(e)) enemies.Add(e);
    }
    public static void Unregister(Entity e)
    {
        if (e == null) return;
        int i = enemies.IndexOf(e);
        if (i >= 0) enemies.RemoveAt(i);
    }
    public static List<Entity> Snapshot() => new List<Entity>(enemies);
}
