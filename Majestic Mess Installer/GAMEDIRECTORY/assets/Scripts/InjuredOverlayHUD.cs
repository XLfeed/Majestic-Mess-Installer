using Engine;
using System;

public class InjuredOverlayHUD : Entity
{
    //public static InjuredOverlayHUD Instance { get; private set; }

    //[Header("Overlay")]
    //public Image overlay;                 // your red vignette Image
    //public Color overlayColor = new Color(1f, 0f, 0f, 0f); // red, alpha will be driven by code

    //[Header("Flash On Hit")]
    //[Range(0f, 1f)] public float flashAlpha = 0.35f; // Peak opacity when hit.
    //[Min(0f)] public float holdTime = 0.05f; // How long the peak holds before fading (seconds)
    //[Min(0.01f)] public float fadeOutTime = 0.25f; // Fade-out duration (seconds).
    //public bool useUnscaledTime = true; // Use unscaled time so it still flashes during pause/slow-mo.

    //[Header("Manual Toggle")]
    //[Range(0f, 1f)] public float manualAlpha = 0.0f; // used by SetOverlay(true)
    //public bool startHidden = true;

    //[Header("Pulse Mode (Continuous Flash)")]
    //[Min(0.05f)] public float pulseInterval = 0.25f; // If enabled, PulseFlash() will keep triggering flashes at this interval until stopped.
    //bool pulsing = false;
    //float pulseTimer = 0f;

    //// runtime
    //float timer = 0f;           // counts down hold + fade
    //float peakAlpha = 0f;       // target alpha for this flash
    //bool manualOn = false;      // sticky overlay toggle

    //void Awake()
    //{
    //    if (Instance != null && Instance != this) { Destroy(gameObject); return; }
    //    Instance = this;

    //    if (!overlay) overlay = GetComponent<Image>();
    //    if (overlay)
    //    {
    //        overlay.raycastTarget = false;
    //        var c = overlayColor; c.a = startHidden ? 0f : manualAlpha;
    //        overlay.color = c;
    //    }
    //}

    //void Update()
    //{
    //    if (!overlay) return;

    //    float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    //    //  Continuous Pulse logic 
    //    if (pulsing)
    //    {
    //        pulseTimer -= dt;
    //        if (pulseTimer <= 0f)
    //        {
    //            // trigger a flash periodically
    //            PlayHitFlash();
    //            pulseTimer = pulseInterval;
    //        }
    //    }

    //    float a = 0f;

    //    // manual overlay if enabled
    //    if (manualOn)
    //    {
    //        a = Mathf.Clamp01(manualAlpha);
    //    }
    //    else
    //    {
    //        if (timer > 0f)
    //        {
    //            // during hold
    //            if (timer > fadeOutTime)
    //            {
    //                a = peakAlpha;
    //            }
    //            else
    //            {
    //                // fading out
    //                float t = Mathf.Clamp01(timer / fadeOutTime);
    //                a = peakAlpha * t;
    //            }
    //            timer -= dt;
    //            if (timer < 0f) timer = 0f;
    //        }
    //        else
    //        {
    //            a = 0f;
    //        }
    //    }

    //    var c = overlayColor;
    //    c.a = a;
    //    overlay.color = c;
    //}

    //// function calls

    //// Flash the overlay once
    //public void PlayHitFlash(float customAlpha = -1f)
    //{
    //    manualOn = false; // ensure manual mode is off
    //    peakAlpha = (customAlpha >= 0f) ? Mathf.Clamp01(customAlpha) : flashAlpha;
    //    timer = holdTime + fadeOutTime;   // full cycle
    //}
    //public void PulseFlash(bool on)
    //{
    //    if (on)
    //    {
    //        pulsing = true;
    //        pulseTimer = 0f; // start immediately
    //    }
    //    else
    //    {
    //        pulsing = false;
    //    }
    //}
    //// Turn on a constant overlay or turn off.
    //public void SetOverlay(bool on, float alpha = -1f)
    //{
    //    manualOn = on;
    //    if (alpha >= 0f) manualAlpha = Mathf.Clamp01(alpha);
    //    if (!on) { timer = 0f; } // stop any flash
    //}

    //// Immediately hides the overlay.
    //public void Clear()
    //{
    //    manualOn = false;
    //    timer = 0f;
    //    if (overlay)
    //    {
    //        var c = overlay.color; c.a = 0f; overlay.color = c;
    //    }
    //}
}