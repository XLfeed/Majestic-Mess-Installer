using Engine;

/// <summary>
/// Attach this script to pause menu button entities.
/// Handles Resume, Main Menu, and Settings button clicks.
/// Works via OnUIClick which fires even when the game is paused.
///
/// Button entity names determine behavior:
///   "ResumeButton"   - Unpauses the game
///   "MainMenuButton" - Unpauses and loads the main menu scene
///   "SettingsButton"  - Placeholder for settings (not yet implemented)
/// </summary>
public class PauseMenuButton : Entity
{
    public string mainMenuSceneName = "main_menu";

    public override void OnUIClick(UIPointerEventInfo eventInfo)
    {
        if (Name == "ResumeButton")
        {
            GamePause.Resume();
        }
        else if (Name == "MainMenuButton")
        {
            GamePause.Resume();
            Scene.LoadScene(mainMenuSceneName);
        }
        else if (Name == "SettingsButton")
        {
            // Placeholder - settings functionality to be added later
            Debug.Log("[PauseMenu] Settings clicked (not yet implemented)");
        }
    }
}
