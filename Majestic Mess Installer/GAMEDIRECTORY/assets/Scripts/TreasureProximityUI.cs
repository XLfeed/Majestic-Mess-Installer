using Engine;

public class TreasureProximityUI : Entity
{
    public string PlayerTag = "Player";
    public string GlobalPanelEntityName = "Treasure Prompt Panel";
    public string MessageNear = "Press E to collect";

    private GlobalTreasurePromptUI _globalUI;

    public override void OnInit()
    {
        _globalUI = Entity.FindScriptByName<GlobalTreasurePromptUI>(GlobalPanelEntityName);

        // Optional: ensure hidden on start
        // _globalUI?.HideForOwner(ID);  // not needed
    }

    public override void OnTriggerEnter(ColliderComponent collider)
    {
        if (_globalUI == null) return;

        var other = collider.Entity;
        if (other != null && other.IsValid() && other.CompareTag(PlayerTag))
        {
            _globalUI.ShowForOwner(ID, MessageNear);
        }
    }

    public override void OnTriggerExit(ColliderComponent collider)
    {
        if (_globalUI == null) return;

        var other = collider.Entity;
        if (other != null && other.IsValid() && other.CompareTag(PlayerTag))
        {
            _globalUI.HideForOwner(ID);
        }
    }
}
