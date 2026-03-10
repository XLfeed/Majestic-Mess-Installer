using Engine;

public class ShopCloseButton : Entity
{
    // Entity that has ShopEffect on it (the script that can CloseShop)
    public string controllerEntityName = "UIManager";

    // Child image entities (must match names in your scene)
    public string normalImageName = "Close_Normal";
    public string hoverImageName = "Close_Hover";
    public string pressedImageName = "Close_Pressed";

    // Optional: tiny delay so "pressed" state is visible before shop closes
    public float pressedFlashSeconds = 0.06f;

    private Entity normalImg;
    private Entity hoverImg;
    private Entity pressedImg;

    private ShopEffect shop;
    private float pressedTimer = 0.0f;
    private bool isHovering = false;

    public override void OnInit()
    {
        // Find images (children are fine as long as names are unique)
        normalImg  = Entity.FindEntityByName(normalImageName);
        hoverImg   = Entity.FindEntityByName(hoverImageName);
        pressedImg = Entity.FindEntityByName(pressedImageName);

        if (normalImg == null || hoverImg == null || pressedImg == null)
        {
            Debug.Log("[ShopCloseButton] ERROR: Missing one of the close button image entities.");
            return;
        }

        // Find controller + ShopEffect
        Entity controller = Entity.FindEntityByName(controllerEntityName);
        if (controller == null)
        {
            Debug.Log($"[ShopCloseButton] ERROR: Can't find controller entity '{controllerEntityName}'.");
            return;
        }

        shop = controller.GetComponent<ShopEffect>();
        if (shop == null)
        {
            Debug.Log("[ShopCloseButton] ERROR: Controller entity has no ShopEffect component.");
        }

        // Start in normal state
        ShowNormal();
    }

    public override void OnUpdate(float dt)
    {
        if (pressedTimer > 0.0f)
        {
            pressedTimer -= dt;
            if (pressedTimer <= 0.0f)
            {
                // After pressed flash, close shop
                if (shop != null) shop.CloseShop();

                // If shop doesn't close for some reason, restore correct visual state
                if (isHovering) ShowHover();
                else ShowNormal();
            }
        }
    }

    public override void OnUIHoverEnter(UIPointerEventInfo eventInfo)
    {
        isHovering = true;
        if (pressedTimer <= 0.0f) ShowHover();
    }

    public override void OnUIHoverExit(UIPointerEventInfo eventInfo)
    {
        isHovering = false;
        if (pressedTimer <= 0.0f) ShowNormal();
    }

    public override void OnUIClick(UIPointerEventInfo eventInfo)
    {
        // Show pressed state briefly
        //ShowPressed();
        //pressedTimer = pressedFlashSeconds;

        // If you don't care about showing "pressed", you can just do:
        if (shop != null) shop.ToggleShop();
    }

    private void ShowNormal()
    {
        InternalCalls.UIElementComponent_SetActive(normalImg.ID, true);
        InternalCalls.UIElementComponent_SetActive(hoverImg.ID, false);
        InternalCalls.UIElementComponent_SetActive(pressedImg.ID, false);
    }

    private void ShowHover()
    {
        InternalCalls.UIElementComponent_SetActive(normalImg.ID, false);
        InternalCalls.UIElementComponent_SetActive(hoverImg.ID, true);
        InternalCalls.UIElementComponent_SetActive(pressedImg.ID, false);
    }

    private void ShowPressed()
    {
        InternalCalls.UIElementComponent_SetActive(normalImg.ID, false);
        InternalCalls.UIElementComponent_SetActive(hoverImg.ID, false);
        InternalCalls.UIElementComponent_SetActive(pressedImg.ID, true);
    }
}
