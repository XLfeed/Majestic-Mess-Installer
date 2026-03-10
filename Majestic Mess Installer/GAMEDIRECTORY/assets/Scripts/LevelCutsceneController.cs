using Engine;

/// <summary>
/// In-level cutscene overlay — keeps the level scene fully loaded.
/// The video renders fullscreen on top of the gameplay view.
/// All mid-animation, physics and AI state is preserved because nothing is
/// unloaded; we simply block player input and play video until done. [Can pause game once we have a pause state.]
///
/// Setup in editor:
///   1. Create an entity in the level scene (e.g. "LevelCutscene").
///   2. Attach VideoPlayerComponent — set any default video file path.
///      Leave Play On Awake OFF.
///   3. Attach this script to the same entity.
///   4. Set treasureOneVideoGUID, treasureTwoVideoGUID, treasureThreeVideoGUID
///      via the video dropdowns in the inspector.
///   5. From any pickup script call:
///        Entity.FindScript<LevelCutsceneController>()?.Trigger(nearest.EntityName);
/// </summary>
public class LevelCutsceneController : Entity
{
    private VideoPlayerComponent vp;
    private bool cutsceneActive = false;

    public ulong treasureTwoVideoGUID   = 0;   // shows video dropdown in inspector
    public ulong treasureThreeVideoGUID = 0;

    public override void OnInit()
    {
        vp = GetComponent<VideoPlayerComponent>();

        if (vp == null)
            Debug.Log("[LevelCutsceneController] ERROR: No VideoPlayerComponent on this entity.");
    }

    public override void OnUpdate(float dt)
    {
        if (!cutsceneActive || vp == null) return;

        bool skip = Input.IsKeyPressed(KeyCode.Escape) || Input.IsKeyPressed(KeyCode.Space);

        if (vp.IsFinished || skip)
        {
            Debug.Log($"[LevelCutsceneController] Cutscene {(skip ? "skipped" : "finished")}.");
            EndCutscene();
        }
    }

    /// <summary>
    /// Starts the cutscene for the given item.
    /// Looks up itemName and swaps the video to the matching GUID.
    /// If not found, plays whatever GUID is already set on the VideoPlayerComponent.
    /// Safe to call if already active (ignored).
    /// </summary>
    public void Trigger(string itemName = "")
    {
        if (cutsceneActive || vp == null) return;

        if (!string.IsNullOrEmpty(itemName))
        {
            ulong guid = 0;
            if      (itemName == "TreasureTwo")   guid = treasureTwoVideoGUID;
            else if (itemName == "TreasureThree")   guid = treasureThreeVideoGUID;

            if (guid != 0)
                vp.SetVideoGUID(guid);
        }

        cutsceneActive = true;
        PlayerInputBlocker.SetBlocked(true);
        vp.Play();

        Debug.Log($"[LevelCutsceneController] Triggered for '{itemName}'.");
    }

    private void EndCutscene()
    {
        cutsceneActive = false;
        vp.Stop();
        PlayerInputBlocker.SetBlocked(false);
    }
}
