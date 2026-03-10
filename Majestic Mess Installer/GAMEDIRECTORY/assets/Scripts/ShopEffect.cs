using System;
using Engine;

public class ShopEffect : Entity
{
    // These are defaults, but editor-serialized values can override them.
    public string openSfx = "assets/Audio/SFX/Scroll_Open.wav";
    public string closeSfx = "assets/Audio/SFX/Scroll_Close.wav";

    public string ShopPanelName = "Shop";
    public KeyCode toggleKey = KeyCode.P;

    public float volume = 0.5f;

    private Entity shopPanel;

    private bool isOpen = false;
    private bool prevKeyDown = false;

    public override void OnInit()
    {
        // --- Self-heal serialized strings ---
        // If the editor saved these as empty, force safe defaults at runtime.
        if (string.IsNullOrWhiteSpace(ShopPanelName))
            ShopPanelName = "Shop";

        // If SFX paths got serialized as empty, don’t try to load them.
        if (string.IsNullOrWhiteSpace(openSfx))
            openSfx = ""; // leave empty to skip safely
        if (string.IsNullOrWhiteSpace(closeSfx))
            closeSfx = ""; // leave empty to skip safely

        shopPanel = Entity.FindEntityByName(ShopPanelName);
        if (shopPanel == null)
        {
            Debug.Log($"[ShopEffect] ERROR: Could not find entity named '{ShopPanelName}'.");
            return;
        }

        // Safety: only call UIElementComponent_SetActive if it’s a UI element
        if (!shopPanel.HasComponent<UIElementComponent>())
        {
            Debug.Log($"[ShopEffect] ERROR: '{ShopPanelName}' has no UIElementComponent. Not toggling.");
            return;
        }

        // Start hidden
        isOpen = false;
        InternalCalls.UIElementComponent_SetActive(shopPanel.ID, false);
    }

    public override void OnUpdate(float dt)
    {
        if (shopPanel == null) return;

        bool keyDown = Input.IsKeyPressed(toggleKey);

        // Edge trigger: toggle only once per key press
        if (keyDown && !prevKeyDown)
            ToggleShop();

        prevKeyDown = keyDown;
    }

    public void OpenShop() => SetShop(true);
    public void CloseShop() => SetShop(false);
    public void ToggleShop() => SetShop(!isOpen);

    private void SetShop(bool open)
    {
        isOpen = open;

        if (shopPanel != null)
            InternalCalls.UIElementComponent_SetActive(shopPanel.ID, isOpen);

        if (isOpen && !string.IsNullOrEmpty(openSfx)) Audio.Play2D(openSfx, volume);
        if (!isOpen && !string.IsNullOrEmpty(closeSfx)) Audio.Play2D(closeSfx, volume);
    }
}
