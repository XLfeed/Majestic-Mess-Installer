using Engine;
using System.Collections.Generic;

public class WizardTutorialTemp : Entity
{
    public string GlobalPanelEntityName = "Tutorial Prompt Panel";
    public bool IgnoreY = true;

    private GlobalTreasurePromptUI _ui;
    private TransformComponent _tf;

    private ulong _activeBeaconId = 0;

    public override void OnInit()
    {
        _tf = Transform;
        _ui = Entity.FindScriptByName<GlobalTreasurePromptUI>(GlobalPanelEntityName);
        _ui?.HideForOwner(ID);
    }

    public override void OnUpdate(float dt)
    {
        if (_tf == null || _ui == null) return;

        WizardInteractableBeacon nearest = null;
        float nearestDistSq = float.MaxValue;

        Vector3 myPos = _tf.Position;
        List<WizardInteractableBeacon> beacons = WizardInteractableRegistry.Snapshot();

        foreach (var b in beacons)
        {
            if (b == null || !b.IsValid()) continue;

            float r = b.Radius;
            if (r <= 0.0f) r = 3.0f; // fallback
            float r2 = r * r;

            Vector3 bp = b.Transform.Position;
            float dx = bp.x - myPos.x;
            float dy = IgnoreY ? 0f : (bp.y - myPos.y);
            float dz = bp.z - myPos.z;

            float d2 = dx * dx + dy * dy + dz * dz;
            if (d2 > r2) continue;

            if (d2 < nearestDistSq)
            {
                nearest = b;
                nearestDistSq = d2;
            }
        }

        // Nothing in range
        if (nearest == null)
        {
            if (_activeBeaconId != 0)
            {
                _activeBeaconId = 0;
                _ui.HideForOwner(ID);
            }
            return;
        }

        // In range of something
        if (nearest.ID != _activeBeaconId)
        {
            _activeBeaconId = nearest.ID;
            _ui.ShowForOwner(ID, BuildMessage(nearest));
        }
        else
        {
            // (optional) keep updating text if you want live value changes
            _ui.ShowForOwner(ID, BuildMessage(nearest));
        }
    }

    private string BuildMessage(WizardInteractableBeacon b)
    {
        switch (b.Mode)
        {
            case WizardInteractableMode.Wizard1: return "Well met! Without further ado lets start! WASD TO MOVE!";
            case WizardInteractableMode.Wizard2: return "Hold 'SPACE' to hide behind objects and avoid detection!";
            case WizardInteractableMode.Wizard3: return "Approach loot like these and press 'F'";
            case WizardInteractableMode.Wizard4: return "These are Hideable barrels, approach and press 'T' to hide...This stops knights from chasing";
            case WizardInteractableMode.Wizard5: return "Final stretch! Hold 'SHIFT' to run to that portal";
            default:                            return "Press F";
        }
    }
}
