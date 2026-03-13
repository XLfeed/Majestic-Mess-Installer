using Engine;
using System;
using System.Collections.Generic;

/// <summary>
/// Detects proximity to wizard beacons and shows/hides the associated 3D
/// world-space text label entity (TextMeshProComponent).
///
/// Scene setup required:
///   For each wizard, create an entity named e.g. "Wizard1_Label" that has a
///   TextMeshProComponent. Place it above the wizard's head. Set Enabled = false
///   in the editor so it starts hidden. The script sets the text content and
///   billboards the label toward the player every frame while in range.
/// </summary>
public class WizardTutorialTemp : Entity
{
    // Suffix appended to each wizard entity name to find its label entity.
    // e.g. wizard named "Wizard1" → label entity "Wizard1_Label"
    public string LabelSuffix = "_Label";

    // Ignore Y axis for distance check (player and wizards may be at different heights)
    public bool IgnoreY = true;

    // --- Runtime state ---
    private Entity _player;
    private TransformComponent _playerTf;

    private ulong _activeBeaconId = 0;
    private Entity _activeLabel;

    public override void OnInit()
    {
        _player = Entity.FindEntityByName("Player");
        if (_player != null && _player.IsValid())
            _playerTf = _player.Transform;
    }

    public override void OnUpdate(float dt)
    {
        if (_playerTf == null) return;

        WizardInteractableBeacon nearest = FindNearestBeacon();

        if (nearest == null)
        {
            // Left range — hide current label
            if (_activeBeaconId != 0)
            {
                HideLabel(_activeLabel);
                _activeLabel = null;
                _activeBeaconId = 0;
            }
            return;
        }

        // Entered a new beacon's range
        if (nearest.ID != _activeBeaconId)
        {
            HideLabel(_activeLabel);
            _activeBeaconId = nearest.ID;
            _activeLabel = ShowLabel(nearest);
        }

        // Billboard the active label toward the player every frame
        if (_activeLabel != null && _activeLabel.IsValid())
            BillboardTowardPlayer(_activeLabel.Transform);
    }

    // -------------------------------------------------------------------------

    private WizardInteractableBeacon FindNearestBeacon()
    {
        Vector3 myPos = _playerTf.Position;
        WizardInteractableBeacon nearest = null;
        float nearestDistSq = float.MaxValue;

        foreach (var b in WizardInteractableRegistry.Snapshot())
        {
            if (b == null || !b.IsValid()) continue;

            float r = b.Radius > 0f ? b.Radius : 3f;
            Vector3 bp = b.Transform.Position;
            float dx = bp.x - myPos.x;
            float dy = IgnoreY ? 0f : (bp.y - myPos.y);
            float dz = bp.z - myPos.z;
            float d2 = dx * dx + dy * dy + dz * dz;

            if (d2 > r * r) continue;
            if (d2 < nearestDistSq) { nearest = b; nearestDistSq = d2; }
        }

        return nearest;
    }

    private Entity ShowLabel(WizardInteractableBeacon beacon)
    {
        string labelName = beacon.EntityName + LabelSuffix;
        Entity label = Entity.FindEntityByName(labelName);
        if (label == null || !label.IsValid())
        {
            Debug.Log($"[WizardTutorial] Label entity '{labelName}' not found.");
            return null;
        }

        var tmp = new TextMeshProComponent(label.ID);
        tmp.Text = BuildMessage(beacon);
        tmp.Enabled = true;
        return label;
    }

    private void HideLabel(Entity label)
    {
        if (label == null || !label.IsValid()) return;
        var tmp = new TextMeshProComponent(label.ID);
        tmp.Enabled = false;
    }

    /// <summary>
    /// Yaw-only billboard: rotate the label entity so its +Z faces the player.
    /// Matches the same approach used by WizardBodyRotate.
    /// </summary>
    private void BillboardTowardPlayer(TransformComponent labelTf)
    {
        Vector3 labelPos = labelTf.Position;
        Vector3 playerPos = _playerTf.Position;

        float dx = playerPos.x - labelPos.x;
        float dz = playerPos.z - labelPos.z;
        if (dx * dx + dz * dz < 0.01f) return;

        float yaw = MathF.Atan2(dx, dz);
        Vector3 rot = labelTf.Rotation;
        labelTf.Rotation = new Vector3(rot.x, yaw, rot.z);
    }

    private string BuildMessage(WizardInteractableBeacon b)
    {
        switch (b.Mode)
        {
            case WizardInteractableMode.Wizard1: return "Well met! Without further ado lets start!\nWASD TO MOVE and 'SHIFT' TO RUN!";
            case WizardInteractableMode.Wizard2: return "Hold 'SPACE' to hide behind objects\nand avoid detection!";
            case WizardInteractableMode.Wizard3: return "Approach loot like these\nand press 'F'";
            case WizardInteractableMode.Wizard4: return "These are Hideable barrels, approach\nand press 'F' to hide...\nThis stops knights from chasing";
            case WizardInteractableMode.Wizard5: return "Before you go\nAlways remember to press 'M' \nfor your curent objective in the castle";
            case WizardInteractableMode.Wizard6: return "Lets get started on the first 2 treasures...\nThe king's 'phone' and a face altering mask\nRemember to press 'M'!";
            case WizardInteractableMode.Wizard7: return "Good work on the 1st mission...\nNow I need you to take away the king's favourite talking doll\n...And a magical staff\nDont forget to press 'M'!";
            case WizardInteractableMode.Wizard8: return "Head to the portal when you're ready to leave";
            default:                             return "Press F";
        }
    }
}
