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

    public static void Register(InteractableBeacon b)
    {
        if (b != null && !s_All.Contains(b))
            s_All.Add(b);
    }

    public static void Unregister(InteractableBeacon b)
    {
        if (b != null)
            s_All.Remove(b);
    }

    public static List<InteractableBeacon> Snapshot()
    {
        return new List<InteractableBeacon>(s_All);
    }
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

    // Safety lock for all portal / transition behavior
    public bool IsPortal = false;

    // Portal config
    // true  -> change to another scene
    // false -> teleport within the same scene
    public bool UseSceneChange = true;

    // Current string scene name field (kept, but SceneTarget is the stable fallback)
    public string TargetSceneName = "";

    // Stable fallback scene selector
    // 0 = none
    // 1 = wizardroom
    // 2 = m5_level1_backup
    // 3 = m5_level2_backup
    // 4 = WinScene
    public int SceneTarget = 0;

    public Vector3 TeleportTarget = Vector3.Zero;
    public bool PreservePlayerY = false;

    // Per-portal treasure requirement checkboxes
    // Only used when IsPortal == true.
    public bool RequireTreasure1 = false;
    public bool RequireTreasure2 = false;
    public bool RequireTreasure3 = false;
    public bool RequireTreasure4 = false;

    // For treasure beacons:
    // 0 = use legacy name-based mapping
    // 1 = Treasure 1
    // 2 = Treasure 2
    // 3 = Treasure 3
    // 4 = Treasure 4
    public int TreasureSlot = 0;

    // Runtime info
    public string EntityName { get; private set; } = "";
    public string EntityTag { get; private set; } = "";

    public override void OnInit()
    {
        EntityName = Name;

        if (HasComponent<TagComponent>())
        {
            EntityTag = GetComponent<TagComponent>().Tag;
        }

        if (Mode == InteractableMode.Auto)
        {
            string tag = EntityTag;

            if (string.Equals(tag, "TreasureMain", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tag, "Treasure", StringComparison.OrdinalIgnoreCase) ||
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
        {
            Debug.Log(
                $"[Beacon] Registered '{EntityName}' | Mode={Mode}, Tag='{EntityTag}', Radius={Radius:F2}, " +
                $"IsPortal={IsPortal}, UseSceneChange={UseSceneChange}, TargetScene='{TargetSceneName}', SceneTarget={SceneTarget}, TeleportTarget={TeleportTarget}, " +
                $"ReqT1={RequireTreasure1}, ReqT2={RequireTreasure2}, ReqT3={RequireTreasure3}, ReqT4={RequireTreasure4}, TreasureSlot={TreasureSlot}"
            );
        }
    }

    public override void OnExit()
    {
        InteractableRegistry.Unregister(this);

        if (LogPrompts)
            Debug.Log($"[Beacon] Unregistered '{EntityName}' (Mode={Mode})");
    }

    public bool IsPortalMode()
    {
        return Mode == InteractableMode.Level1 || Mode == InteractableMode.Level2 || Mode == InteractableMode.Extract;
    }

    public bool HasSpecificTreasureRequirements()
    {
        if (!IsPortal)
            return false;

        return RequireTreasure1 || RequireTreasure2 || RequireTreasure3 || RequireTreasure4;
    }

    // Call this method when the portal is activated/used
    public void OnPortalUsed()
    {
        // Reset all treasure pickup flags
        //PickUpItemManager.pickedup_Treasure_1 = false;
        //PickUpItemManager.pickedup_Treasure_2 = false;
        PickUpItemManager.pickedup_Treasure_3 = false;
        //PickUpItemManager.pickedup_Treasure_4 = false;
    }
}