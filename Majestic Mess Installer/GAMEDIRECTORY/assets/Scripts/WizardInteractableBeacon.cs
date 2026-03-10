using Engine;
using System;
using System.Collections.Generic;
using System.Text;


public enum WizardInteractableMode
{
    Auto = -1,          // derive from Tag
    Wizard1 = 0,   // required to extract
    Wizard2 = 1,   // optional value
    Wizard3 = 2,
    Wizard4 = 3,
    Wizard5 = 4
}

public static class WizardInteractableRegistry
{
    private static readonly List<WizardInteractableBeacon> s_All = new List<WizardInteractableBeacon>();
    public static void Register(WizardInteractableBeacon b) { if (b != null && !s_All.Contains(b)) s_All.Add(b); }
    public static void Unregister(WizardInteractableBeacon b) { if (b != null) s_All.Remove(b); }
    public static List<WizardInteractableBeacon> Snapshot() => new List<WizardInteractableBeacon>(s_All);
}

public class WizardInteractableBeacon : Entity
{
    public WizardInteractableBeacon() : base() { }
    public WizardInteractableBeacon(ulong id) : base(id) { }

    public int Value = 1;

    // Editor-tweakable
    public WizardInteractableMode Mode = WizardInteractableMode.Auto; // default uses Tag
    public float Radius = 10.0f;
    public bool LogPrompts = false;

    // Runtime info (populated on init)
    public string EntityName { get; private set; } = "";
    public string EntityTag { get; private set; } = "";

    public override void OnInit()
    {
        // Store entity name for later use
        EntityName = Name;

        // If Auto, derive mode from Tag
        if (Mode == WizardInteractableMode.Auto && HasComponent<TagComponent>())
        {
            

            if (string.Equals(EntityName, "Wizard1", StringComparison.OrdinalIgnoreCase))
            {
                Mode = WizardInteractableMode.Wizard1;
            }
            else if (string.Equals(EntityName, "Wizard2", StringComparison.OrdinalIgnoreCase) )
            {
                Mode = WizardInteractableMode.Wizard2;
            }
            else if (string.Equals(EntityName, "Wizard3", StringComparison.OrdinalIgnoreCase))
            {
                Mode = WizardInteractableMode.Wizard3;
            }
            else if (string.Equals(EntityName, "Wizard4", StringComparison.OrdinalIgnoreCase))
            {
                Mode = WizardInteractableMode.Wizard4;
            }
            else if (string.Equals(EntityName, "Wizard5", StringComparison.OrdinalIgnoreCase))
            {
                Mode = WizardInteractableMode.Wizard5;
            }
        }

        WizardInteractableRegistry.Register(this);
        if (LogPrompts)
            Debug.Log($"[Beacon] Registered '{EntityName}' | Mode={Mode}, Tag='{EntityTag}', Radius={Radius:F2}");
    }

    public override void OnExit()
    {
        WizardInteractableRegistry.Unregister(this);
        if (LogPrompts)
            Debug.Log($"[Beacon] Unregistered '{EntityName}' (Mode={Mode})");
    }
}
