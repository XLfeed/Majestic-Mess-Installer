using Engine;

/// <summary>
/// Attach this script (alongside a VideoPlayerComponent) to an entity
/// in a dedicated cutscene scene.
///
/// Setup in editor:
///   1. Create a scene of type Cutscene.
///   2. Add an entity, attach VideoPlayerComponent — set the video file path.
///      Leave Play On Awake OFF (this script starts it manually so it works
///      on runtime scene loads, not just editor Play).
///   3. Attach this script. Pick the next scene from the targetScene dropdown.
///   4. From MenuButton (or any trigger), call Scene.LoadScene("YourCutsceneScene").
/// </summary>
public class CutsceneController : Entity
{
    // Scene to load when the video finishes or is skipped.
    // Stored as the scene asset GUID — rename-safe, shown as a dropdown in the editor.
    public ulong targetSceneGUID = 0;

    public static bool HasPlayed = false;

    private VideoPlayerComponent vp;
    private bool started = false;
    private bool done    = false;

    public override void OnInit()
    {
        vp = GetComponent<VideoPlayerComponent>();

        if (vp == null)
        {
            Debug.Log("[CutsceneController] ERROR: No VideoPlayerComponent on this entity.");
            return;
        }

        HasPlayed = true;

        // Start the video. We do this here rather than relying on playOnAwake
        // because playOnAwake only fires on editor Play, not on runtime LoadScene.
        vp.Play();
        Debug.Log($"[CutsceneController] Started cutscene. Next scene GUID: '{targetSceneGUID:X16}'");
    }

    public override void OnUpdate(float dt)
    {
        if (vp == null || done) return;

        // Wait until video is actually decoding before checking for skip,
        // so a held-down Escape from the previous scene doesn't instantly skip.
        if (!started)
        {
            if (vp.IsPlaying)
                started = true;
            return;
        }

        bool skip = Input.IsKeyPressed(KeyCode.Enter);

        if (vp.IsFinished || skip)
        {
            done = true;
            vp.Stop();

            Debug.Log($"[CutsceneController] Cutscene {(skip ? "skipped" : "finished")}. Loading GUID '{targetSceneGUID:X16}'.");

            if (targetSceneGUID != 0)
                Scene.LoadSceneByGUID(targetSceneGUID);
        }
    }
}
