using Engine;

public class SettingsSlider : Entity
{
    public string trackEntityName = "MasterSliderTrack";
    public string knobEntityName  = "MasterSliderKnob";
    public string labelEntityName = "";

    private Entity trackEntity;
    private Entity knobEntity;
    private RectTransformComponent trackRect;
    private RectTransformComponent knobRect;

    private bool isDragging = false;
    private float dragOffsetX = 0f;
    private float sliderValue = 0.5f;

    // Track bounds in UI reference space (bottom-left origin)
    private float trackLeft;
    private float trackRight;
    private float trackY;
    private float trackHeight;

    // Reference resolution (must match canvas)
    private const float REF_W = 1920f;
    private const float REF_H = 1080f;

    public override void OnInit()
    {
        trackEntity = Entity.FindEntityByName(trackEntityName);
        knobEntity  = Entity.FindEntityByName(knobEntityName);

        if (trackEntity != null && trackEntity.IsValid())
            trackRect = trackEntity.GetComponent<RectTransformComponent>();
        if (knobEntity != null && knobEntity.IsValid())
            knobRect = knobEntity.GetComponent<RectTransformComponent>();

        if (trackRect != null)
        {
            // Compute track bounds the same way the C++ CalculateRectTransform does:
            // For anchors (0.5, 0.5), parent = canvas at (0,0) size (1920,1080):
            //   anchorCenter = (960, 540)
            //   computedSize = sizeDelta  (since anchorMin == anchorMax)
            //   computedPosition = anchorCenter + anchoredPosition - pivot * computedSize
            var anchor = trackRect.AnchorMin; // both min/max are (0.5, 0.5)
            var pos = trackRect.AnchoredPosition;
            var size = trackRect.SizeDelta;
            var pivot = trackRect.Pivot;

            float anchorCenterX = anchor.x * REF_W;
            float anchorCenterY = anchor.y * REF_H;

            float computedX = anchorCenterX + pos.x - pivot.x * size.x;
            float computedY = anchorCenterY + pos.y - pivot.y * size.y;

            trackLeft   = computedX;
            trackRight  = computedX + size.x;
            trackY      = computedY;
            trackHeight = size.y;

        }

        sliderValue = AudioComponent.GetMasterVolume();
        UpdateKnobPosition();
        UpdateLabel();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (trackRect == null || knobRect == null) return;

        // Convert mouse from screen pixels (top-left origin) to UI reference space (bottom-left origin)
        Vector2 uiMouse = ScreenToUI(Input.GetMousePosition());

        if (Input.IsMouseButtonPressed(0))
        {
            if (IsMouseOverKnob(uiMouse))
            {
                isDragging = true;
                float knobCenterX = knobRect.AnchorMin.x * REF_W + knobRect.AnchoredPosition.x;
                dragOffsetX = uiMouse.x - knobCenterX;
            }
        }

        if (isDragging && Input.IsMouseButtonHeld(0))
        {
            ApplyMouseX(uiMouse.x - dragOffsetX);
        }

        if (isDragging && !Input.IsMouseButtonHeld(0))
        {
            isDragging = false;
            Debug.Log($"[SettingsSlider] Volume: {(int)(sliderValue * 100f)}%");
        }
    }

    private Vector2 ScreenToUI(Vector2 screen)
    {
        InternalCalls.GameView_GetPosition(out Vector2 gvPos);
        InternalCalls.GameView_GetSize(out Vector2 gvSize);
        // Standalone build: GameView may return zeros, fall back to window size
        if (gvSize.x < 1f || gvSize.y < 1f)
        {
            InternalCalls.Window_GetSize(out gvSize);
            gvPos = new Vector2(0f, 0f);
        }
        // Convert from window-space to game-view-relative
        float relX = screen.x - gvPos.x;
        float relY = screen.y - gvPos.y;
        // Scale to UI reference space and flip Y
        float scaleX = REF_W / gvSize.x;
        float scaleY = REF_H / gvSize.y;
        float uiX = relX * scaleX;
        float uiY = (gvSize.y - relY) * scaleY;
        return new Vector2(uiX, uiY);
    }

    private bool IsMouseOverKnob(Vector2 uiMouse)
    {
        var knobAnchor = knobRect.AnchorMin;
        var knobPos = knobRect.AnchoredPosition;

        float hitW = 200f;
        float hitH = 140f;
        float centerX = knobAnchor.x * REF_W + knobPos.x;
        float centerY = knobAnchor.y * REF_H + knobPos.y;

        return uiMouse.x >= centerX - hitW * 0.5f && uiMouse.x <= centerX + hitW * 0.5f &&
               uiMouse.y >= centerY - hitH * 0.5f && uiMouse.y <= centerY + hitH * 0.5f;
    }

    private void ApplyMouseX(float mouseUIX)
    {
        float clamped = mouseUIX;
        if (clamped < trackLeft) clamped = trackLeft;
        if (clamped > trackRight) clamped = trackRight;

        sliderValue = (clamped - trackLeft) / (trackRight - trackLeft);

        AudioComponent.SetMasterVolume(sliderValue);
        UpdateKnobPosition();
        UpdateLabel();
    }

    private void UpdateKnobPosition()
    {
        if (knobRect == null) return;
        // Position the knob so its center aligns with the slider value on the track
        // The track's anchoredPosition.x is relative to anchor center (960)
        // The knob uses the same anchor setup, so we set its anchoredPosition.x
        // relative to the same anchor center
        var trackPos = trackRect.AnchoredPosition;
        var trackSize = trackRect.SizeDelta;
        float trackLocalLeft  = trackPos.x - trackSize.x * 0.5f;
        float trackLocalRight = trackPos.x + trackSize.x * 0.5f;

        var knobPos = knobRect.AnchoredPosition;
        knobPos.x = trackLocalLeft + (trackLocalRight - trackLocalLeft) * sliderValue;
        knobRect.AnchoredPosition = knobPos;
    }

    private void UpdateLabel()
    {
        if (string.IsNullOrEmpty(labelEntityName)) return;
        Entity label = Entity.FindEntityByName(labelEntityName);
        if (label != null && label.IsValid())
        {
            int percent = (int)(sliderValue * 100f);
            InternalCalls.UITextComponent_SetText(label.ID, $"{percent}%");
        }
    }
}
