using System;
using Engine;

public class TreasureValueUI : Entity
{
    private UITextComponent _text;

    public override void OnInit()
    {
        // Create / fetch your UI text component
        _text = new UITextComponent(ID); // If your engine uses entity ID (common)

        // Basic properties
        _text.Text = "Hello World";
        _text.FontSize = 50.0f;
        _text.Color = new Vector4(1, 1, 1, 1);
        _text.SetFontByName("MedievalSharp-Book");

        // Alignment
        _text.Alignment = UITextAlignment.Left;
        _text.VerticalAlignment = UITextVerticalAlignment.Middle;

        // Spacing
        _text.LineSpacing = 1.2f;
        _text.CharacterSpacing = 1.0f;

        // Layout
        _text.WordWrap = true;
        _text.Overflow = UITextOverflow.Ellipsis;

        // Visibility
        _text.Enabled = true;
    }

    public override void OnUpdate(float dt)
    {
        // Example: display treasure total value live
        _text.Text = $"Monies: {PickUpItemManager.TreasureTotalValue}";
    }
}
