using Engine;

/// <summary>
/// In-level cutscene overlay — keeps the level scene fully loaded.
/// The video renders fullscreen on top of the gameplay view.
/// The game is paused during the cutscene so enemies freeze and game audio doesn't clash.
/// Press Space to skip — detected in C++ so it works even while the game is paused.
/// The cutscene ends automatically via OnVideoFinished when the video reaches its natural end.
///
/// Setup in editor:
///   1. Create an entity in the level scene (e.g. "LevelCutscene").
///   2. Attach VideoPlayerComponent — set any default video file path.
///      Leave Play On Awake OFF.
///   3. Attach this script to the same entity.
///   4. Set treasureTwoVideoGUID, treasureThreeVideoGUID via the video dropdowns in the inspector.
///   5. From any pickup script call:
///        Entity.FindScript<LevelCutsceneController>()?.Trigger(nearest.EntityName);
/// </summary>
public class LevelCutsceneController : Entity
{
    private VideoPlayerComponent vp;
    private bool cutsceneActive = false;

    public ulong treasureOneVideoGUID   = 0;
    public ulong treasureTwoVideoGUID   = 0;
    public ulong treasureThreeVideoGUID = 0;
    public ulong treasureFourVideoGUID  = 0;

    public override void OnInit()
    {
        vp = GetComponent<VideoPlayerComponent>();

        if (vp == null)
            Debug.Log("[LevelCutsceneController] ERROR: No VideoPlayerComponent on this entity.");

    }

    /// <summary>
    /// Fired by the C++ VideoSystem when the video ends naturally, OR when
    /// Space is pressed (C++ detects it during pause and calls this directly).
    /// Bypasses game pause — always fires regardless of pause state.
    /// </summary>
    public override void OnVideoFinished()
    {
        if (!cutsceneActive) return;
        Debug.Log("[LevelCutsceneController] Cutscene ended.");
        EndCutscene();
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

        ulong guid = 0;
        if (!string.IsNullOrEmpty(itemName))
        {
            if      (itemName == "TreasureOne")   guid = treasureOneVideoGUID;
            else if (itemName == "TreasureTwo")   guid = treasureTwoVideoGUID;
            else if (itemName == "TreasureThree") guid = treasureThreeVideoGUID;
            else if (itemName == "TreasureFour")  guid = treasureFourVideoGUID;
        }

        // No GUID mapped for this item, no cutscene
        if (guid == 0) return;

        vp.SetVideoGUID(guid);
        cutsceneActive = true;
        PlayerInputBlocker.SetBlocked(true);
        GamePause.Pause();
        vp.Play();

        Debug.Log($"[LevelCutsceneController] Triggered for '{itemName}'.");
    }

    private void EndCutscene()
    {
        cutsceneActive = false;
        PlayerInputBlocker.SetBlocked(false);
        GamePause.Resume();
    }
}
