using Engine;
using System;
using System.Collections.Generic;
// This script is to hold a flag for whether an entity is disguised or not, to be used by other scripts.
public class DisguiseFlag : Entity 
{
    private static readonly HashSet<ulong> active = new HashSet<ulong>();

    public static void Set(ulong entityId, bool value)
    {
        if (value)
            active.Add(entityId);
        else
            active.Remove(entityId);
    }

    public static bool IsOn(ulong entityId)
    {
        return active.Contains(entityId);
    }

}
