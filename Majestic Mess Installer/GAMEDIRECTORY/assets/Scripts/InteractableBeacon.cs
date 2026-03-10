using Engine;
using System;
using System.Collections.Generic;
using System.Text;


public enum InteractableMode
{
    Auto = -1,          // derive from Tag
    TreasureMain = 0,   // required to extract
    TreasureMisc = 1,   // optional value
    Extract = 2,
    Level1 = 3,
    Level2 = 4
}

public static class InteractableRegistry
{
    private static readonly List<InteractableBeacon> s_All = new List<InteractableBeacon>();
    public static void Register(InteractableBeacon b) { if (b != null && !s_All.Contains(b)) s_All.Add(b); }
    public static void Unregister(InteractableBeacon b) { if (b != null) s_All.Remove(b); }
    public static List<InteractableBeacon> Snapshot() => new List<InteractableBeacon>(s_All);
}

public class InteractableBeacon : Entity
{
    public InteractableBeacon() : base() { }
    public InteractableBeacon(ulong id) : base(id) { }

    public int Value = 1;

    // Editor-tweakable
    public InteractableMode Mode = InteractableMode.Auto; // default uses Tag
    public float Radius = 3.0f;
    public bool LogPrompts = false;

    // Runtime info (populated on init)
    public string EntityName { get; private set; } = "";
    public string EntityTag { get; private set; } = "";

    public override void OnInit()
    {
        // Store entity name for later use
        EntityName = Name;

        // If Auto, derive mode from Tag
        if (Mode == InteractableMode.Auto && HasComponent<TagComponent>())
        {
            string tag = GetComponent<TagComponent>().Tag;
            //string tag = InternalCalls.TagComponent_GetTag(ID);
            //EntityTag = tag; // Store the tag

            if (string.Equals(tag, "TreasureMain", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tag, "Treasure", StringComparison.OrdinalIgnoreCase) ||   // fallback alias
                string.Equals(tag, "MainTreasure", StringComparison.OrdinalIgnoreCase))
            {
                Mode = InteractableMode.TreasureMain;
            }
            else if (string.Equals(tag, "TreasureMisc", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(tag, "MiscTreasure", StringComparison.OrdinalIgnoreCase))
            {
                Mode = InteractableMode.TreasureMisc;
            }
            else if (string.Equals(tag, "Extract", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(tag, "Extraction", StringComparison.OrdinalIgnoreCase))
            {
                Mode = InteractableMode.Extract;
            }
            else if (string.Equals(tag, "Lvl1", StringComparison.OrdinalIgnoreCase))
            {
                Mode = InteractableMode.Level1;
            }
            else if (string.Equals(tag, "Lvl2", StringComparison.OrdinalIgnoreCase))
            {
                Mode = InteractableMode.Level2;
            }
        }

        InteractableRegistry.Register(this);
        if (LogPrompts)
            Debug.Log($"[Beacon] Registered '{EntityName}' | Mode={Mode}, Tag='{EntityTag}', Radius={Radius:F2}");
    }

    public override void OnExit()
    {
        InteractableRegistry.Unregister(this);
        if (LogPrompts)
            Debug.Log($"[Beacon] Unregistered '{EntityName}' (Mode={Mode})");
    }
}
