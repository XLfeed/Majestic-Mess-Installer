using System;
using System.Collections.Generic;
using Engine;

public class MapEffect : Entity
{
    public string takeOut =  "assets/Audio/SFX/Scroll_Open.wav";
    public string putBack =  "assets/Audio/SFX/Scroll_Close.wav";
    public string MapPanelName = "Map";

    bool prevframeActive = false;
    bool currState = false;

    public float volume = 0.5f;

    private Entity MapPanel;

    public override void OnInit()
    {
        MapPanel = Entity.FindEntityByName(MapPanelName);
        InternalCalls.UIElementComponent_SetActive(MapPanel.ID, false);
    }

    // OnUpdate is called once per frame
    public override void OnUpdate(float dt)
    {


       // bool active = InternalCalls.UIElementComponent_GetActive(MapPanel.ID);

        if(isMPressed())
        {
            toggleMap();
        }

        if(currState != prevframeActive)
        {
            if(currState)
            {
                PlayShow();
            }
            else
            {
                PlayHide();
            }

        }
        prevframeActive = currState;

    }

    bool isMPressed()
    {
        return Input.IsKeyPressed(KeyCode.M);
    }

    void toggleMap()
    {
        currState = !currState;
        InternalCalls.UIElementComponent_SetActive(MapPanel.ID, currState);

    }

    /*
    /// <summary>
    /// Checks for ESC keypress and toggles the UI panel visibility
    /// </summary>
    private void CheckPanelToggle()
    {
        if (Input.IsKeyPressed(KeyCode.Escape))
        {
            Debug.Log("TreasureOneEffect: ESC DETECTED - Toggling panel...");
            TogglePanel();
        }
    }
  

    /// <summary>
    /// Toggles the visibility of the UI panel
    /// </summary>
    private void TogglePanel()
    {
        Debug.Log("TreasureOneEffect: TogglePanel() called!");

        // Check if the panel has a UIElementComponent
        if (!HasComponent<UIElementComponent>())
        {
            Debug.Log("TreasureOneEffect: ERROR - No UIElementComponent!");
            return;
        }

        // Get the UIElementComponent and toggle visibility based on current state
        var uiElement = GetComponent<UIElementComponent>();
        bool currentVisibility = uiElement.Visible;
        bool newVisibility = !currentVisibility;

        uiElement.Visible = newVisibility;

        Debug.Log($"TreasureOneEffect: *** TOGGLED *** Visibility: {currentVisibility} -> {newVisibility}");
    }  */

    void PlayHide()
    {
        if (!string.IsNullOrEmpty(putBack))
            Audio.Play2D(putBack, volume);
    }

    void PlayShow()
    {
        if (!string.IsNullOrEmpty(takeOut))
            Audio.Play2D(takeOut, volume);
    }
   
}
