using Engine;
using System;
using System.Collections.Generic;

public class EnemyFOV : Entity
{
    // How often to ask the C++ system for a LOS check
    public float scanInterval = 0.15f;

    // This was the intended target tag; we keep it in case you want it later,
    // but the *actual* tag we print will come from the C++ system.
    public string systemTargetTag = "Player";

    // === Output states (used by EnemyMovement) ===
    public bool detected { get; private set; }
    public Entity detectedEntity { get; private set; }
    public string detectedTag { get; private set; }

    // Internal timer so we don't spam the C++ system every frame
    private float _timer = 0f;

    // Component wrapper
    private EnemyFOVComponent _fovComponent;

    public List<Entity> VisibleEntities { get; private set; } = new List<Entity>();

    public override void OnInit()
    {
        base.OnInit();

        // Create component wrapper and ensure the native EnemyFOVComponent exists
        _fovComponent = new EnemyFOVComponent(ID);
        _fovComponent.EnsureComponent();

        detected = false;
        detectedEntity = null;
        detectedTag = string.Empty;
        _timer = 0f;

        Console.WriteLine($"[EnemyFOV] {Name}: OnInit, ensured C++ EnemyFOVComponent.");
    }

    public override void OnUpdate(float dt)
    {
        base.OnUpdate(dt);

        _timer += dt;
        if (_timer < scanInterval)
            return;            // wait until next scan

        _timer = 0f;

        // Ask the C++ EnemyFOVSystem if we currently have LOS to our target
        bool hasLOS = _fovComponent.HasLineOfSight;

        if (!hasLOS)
        {
            if (detected)
            {
                //Console.WriteLine($"[EnemyFOV] {Name}: lost target.");
            }

            detected = false;
            detectedEntity = null;
            detectedTag = string.Empty;
            return;
        }

        // We have LOS. Ask what tag/name the system reports for the target.
        // This should be the TagComponent tag (e.g. "Player", "Enemy", "Wall").
        string targetTagOrName = _fovComponent.CurrentTargetName;

        detected = true;
        detectedTag = targetTagOrName ?? string.Empty;

        // Optionally try to resolve the entity by this value.
        // If your engine uses the same string for Name and TagComponent,
        // this will succeed; if not, detectedEntity may stay null.
         if (!string.IsNullOrEmpty(targetTagOrName))
         {
             detectedEntity = Entity.FindEntityByName(targetTagOrName);
         }
         else
         {
             detectedEntity = null;
         }

        // if (detectedEntity != null)
        // {
        //     // Print the tag that the detected entity has.
        //     //Console.WriteLine(
        //     //    $"[EnemyFOV] {Name} sees TAG '{detectedTag}' on entity '{detectedEntity.Name}' via EnemyFOVSystem");
        // }
        // else
        // {
        //     // Fallback: we know the tag, but couldn't resolve the entity in C#
        //     //Console.WriteLine(
        //     //    $"[EnemyFOV] {Name} sees TAG '{detectedTag}' (entity could not be resolved in C#)");
        // }

        UpdateVisibleList();

    }

    public void UpdateVisibleList()
    {
        VisibleEntities.Clear();

        ulong[] ids = _fovComponent.GetVisibleEntities();
        if (ids == null)
            return;

        foreach (ulong id in ids)
        {
            if (id == 0) // safeguard invalid IDs
                continue;

            var e = new Entity(id);
            if (e != null && e.IsValid())
                VisibleEntities.Add(e);
        }

        // ===== CLEAN DEBUG PRINT (only prints real visible list) =====
        //if (VisibleEntities.Count == 0)
        //{
        //    Console.WriteLine($"[EnemyFOV] {Name} sees NOTHING.");
        //}
        //else
        //{
        //    // Build a readable line of everything in the list
        //    string line = $"[EnemyFOV] {Name} sees {VisibleEntities.Count} entities: ";

        //    for (int i = 0; i < VisibleEntities.Count; i++)
        //    {
        //        Entity e = VisibleEntities[i];

        //        // Get tag (from native TagComponent)
        //        string tag = e.GetComponent<TagComponent>().Tag;

        //        line += $"{e.Name} (Tag:{tag}, ID:{e.ID})";

        //        if (i < VisibleEntities.Count - 1)
        //            line += ", ";
        //    }

        //    Console.WriteLine(line);
        //}
    }


}
