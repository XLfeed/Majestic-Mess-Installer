using System;
using System.Collections.Generic;
using Engine;

public class PrefabSpawnTest : Entity
{
    public string PrefabName = "FireEmitter";
    public string TargetName = "Enemy";
    public KeyCode SpawnKey = KeyCode.DownArrow;

    private bool _wasHeld;

    public override void OnInit()
    {
        Debug.Log($"[PrefabSpawnTest] Loaded. Prefab='{PrefabName}', Target='{TargetName}', Key={SpawnKey}");
    }

    public override void OnUpdate(float dt)
    {
        bool held = Input.IsKeyHeld(SpawnKey);

        // Trigger only ONCE per key press (rising edge)
        if (held && !_wasHeld)
        {
            Entity target = Entity.FindEntityByName(TargetName);
            if (target == null || !target.IsValid())
            {
                Debug.Log($"[PrefabSpawnTest] Target '{TargetName}' not found or invalid.");
                _wasHeld = held;
                return;
            }

            // Spawns prefab and (because you coded it that way in C++) parents it to targetID
            ulong spawnedID = InternalCalls.Prefab_Instantiate(PrefabName, target.ID);

            if (spawnedID == 0)
                Debug.Log($"[PrefabSpawnTest] Spawn FAILED for prefab '{PrefabName}'. Check C++ logs in Prefab_Instantiate.");
            else
                Debug.Log($"[PrefabSpawnTest] Spawn OK. Spawned EntityID={spawnedID} parent='{TargetName}'");
        }

        _wasHeld = held;
    }
}
