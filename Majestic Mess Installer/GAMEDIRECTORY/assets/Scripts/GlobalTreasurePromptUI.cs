using Engine;

public class GlobalTreasurePromptUI : Entity
{
    private UITextComponent _text;

    private ulong _ownerEntityId = 0;   // 0 = no owner (since IDs are non-zero in most engines)

    public override void OnInit()
    {
        _text = new UITextComponent(ID);

        _text.FontSize = 50.0f;
        _text.Color = new Vector4(1, 1, 1, 1);
        _text.SetFontByName("MedievalSharp-Book");

        _text.Alignment = UITextAlignment.Center;
        _text.VerticalAlignment = UITextVerticalAlignment.Middle;

        _text.LineSpacing = 1.2f;
        _text.CharacterSpacing = 1.0f;

        _text.WordWrap = true;
        _text.Overflow = UITextOverflow.Ellipsis;

        _text.Enabled = false;
    }

    public void ShowForOwner(ulong ownerEntityId, string message)
    {
        _ownerEntityId = ownerEntityId;
        _text.Text = message;
        _text.Enabled = true;
    }

    public void HideForOwner(ulong ownerEntityId)
    {
        if (_ownerEntityId != ownerEntityId)
            return;

        _ownerEntityId = 0;
        _text.Enabled = false;
    }
}
