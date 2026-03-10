using Engine;

public class GammaSlider : Entity
{
    public string trackEntityName = "GammaSliderTrack";
    public string knobEntityName  = "GammaSliderKnob";
    public string labelEntityName = "";

    private Entity trackEntity;
    private Entity knobEntity;
    private RectTransformComponent trackRect;
    private RectTransformComponent knobRect;

    private bool isDragging = false;
    private float dragOffsetX = 0f;
    private float sliderValue = 0.5f;

    // Gamma range
    private const float GAMMA_MIN = 1.0f;
    private const float GAMMA_MAX = 3.0f;

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
            var anchor = trackRect.AnchorMin;
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

        // Convert current gamma to slider value (0-1)
        float currentGamma = RenderSettings.GetGamma();
        sliderValue = (currentGamma - GAMMA_MIN) / (GAMMA_MAX - GAMMA_MIN);
        if (sliderValue < 0f) sliderValue = 0f;
        if (sliderValue > 1f) sliderValue = 1f;

        UpdateKnobPosition();
        UpdateLabel();
    }

    public override void OnUpdate(float deltaTime)
    {
        if (trackRect == null || knobRect == null) return;

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
            float gamma = GAMMA_MIN + (GAMMA_MAX - GAMMA_MIN) * sliderValue;
            Debug.Log($"[GammaSlider] Gamma: {gamma:F2}");
        }
    }

    private Vector2 ScreenToUI(Vector2 screen)
    {
        InternalCalls.GameView_GetPosition(out Vector2 gvPos);
        InternalCalls.GameView_GetSize(out Vector2 gvSize);
        if (gvSize.x < 1f || gvSize.y < 1f)
        {
            InternalCalls.Window_GetSize(out gvSize);
            gvPos = new Vector2(0f, 0f);
        }
        float relX = screen.x - gvPos.x;
        float relY = screen.y - gvPos.y;
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

        float gamma = GAMMA_MIN + (GAMMA_MAX - GAMMA_MIN) * sliderValue;
        RenderSettings.SetGamma(gamma);
        UpdateKnobPosition();
        UpdateLabel();
    }

    private void UpdateKnobPosition()
    {
        if (knobRect == null) return;
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
            float gamma = GAMMA_MIN + (GAMMA_MAX - GAMMA_MIN) * sliderValue;
            InternalCalls.UITextComponent_SetText(label.ID, $"{gamma:F1}");
        }
    }
}
