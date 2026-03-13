///////////////////////////////////////////////////////////////////////////////////////
//
//  \file    MenuButton.cs
//
//  \brief   Interactive menu button script for UI screens. Handles button clicks
//           for navigation between scenes (main menu, settings, win/lose screens),
//           volume control, and application exit. Uses entity name matching to
//           determine button behavior.
//
//  \author  Pearl Goh [100%]
//
// All content © 2025 DigiPen Institute of Technology Singapore.
// All rights reserved.
//
///////////////////////////////////////////////////////////////////////////////////////

using Engine;

public class MenuButton : Entity
{
    public string targetScene = "";
    public bool isQuitButton = false;

    private UISFX uisfx;

    public override void OnInit()
    {
        Debug.Log($"[MenuButton] Initialized: {Name}");
        uisfx = Entity.FindScript<UISFX>();
    }

    public override void OnUIClick(UIPointerEventInfo eventInfo)
    {
        Debug.Log($"[MenuButton] Clicked: {Name}");

        // Play click SFX
        if (uisfx != null)
            uisfx.PlaySelect();

        // Use button name to determine action (workaround for string field loading)
        if (Name == "PlayButton")
        {
            if (!CutsceneController.HasPlayed)
            {
                Debug.Log("[MenuButton] Loading intro cutscene (first play)");
                Scene.LoadScene("IntroCutscene");
            }
            else
            {
                Debug.Log("[MenuButton] Cutscene already played, loading TutorialCorridoor");
                Scene.LoadScene("TutorialCorridoor");
            }
        }
        else if (Name == "SettingsButton")
        {
            Debug.Log("[MenuButton] Loading settings");
            Scene.LoadScene("settings");
        }
        else if (Name == "MainMenuButton")
        {
            Debug.Log("[MenuButton] Loading main_menu");
            Scene.LoadScene("main_menu");
        }
        else if (Name == "QuitButton")
        {
            Debug.Log("[MenuButton] Exiting game");
            InternalCalls.Window_SetShouldClose();
        }
    }

    public override void OnUIHoverEnter(UIPointerEventInfo eventInfo)
    {
        Debug.Log($"[MenuButton] Hover enter: {Name}");
    }

    public override void OnUIHoverExit(UIPointerEventInfo eventInfo)
    {
        Debug.Log($"[MenuButton] Hover exit: {Name}");
    }
}
