using Engine;

/// <summary>
/// Static class for querying and controlling the game pause state.
/// The engine handles ESC key toggling automatically. This class allows
/// C# scripts to query or programmatically control pause state.
/// </summary>
public static class GamePause
{
    /// <summary>
    /// Returns true if the game is currently paused.
    /// </summary>
    public static bool IsPaused => InternalCalls.Game_IsPaused();

    /// <summary>
    /// Set the game pause state. When paused, scripts, physics, animation,
    /// and gameplay audio are frozen. UI events (OnUIClick) still fire.
    /// </summary>
    public static void SetPaused(bool paused) => InternalCalls.Game_SetPaused(paused);

    /// <summary>
    /// Resume the game (unpause).
    /// </summary>
    public static void Resume() => SetPaused(false);

    /// <summary>
    /// Pause the game.
    /// </summary>
    public static void Pause() => SetPaused(true);
}
