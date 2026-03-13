using System;
using System.Collections.Generic;
using Engine;

public class TreasureOneEffect : Entity
{
    // Cache the panel entity to avoid searching every frame
    // private Entity panelEntity = null;

    public string[] pickedClips =
    {

        "assets/Audio/TreasureOneVoices/T1_Picked_1.wav",
        "assets/Audio/TreasureOneVoices/T1_Picked_2.wav",
        "assets/Audio/TreasureOneVoices/T1_Picked_3.wav",
        "assets/Audio/TreasureOneVoices/T1_Picked_4.wav",
        "assets/Audio/TreasureOneVoices/T1_Picked_5.wav"
             
    };
   // public AudioClip effect;
    public float volume = 0.4f;
    private float timeLeftToNext = 0f;  // Manually track time like TestScript does
    private AudioComponent ac;
    private int prevPickedIndex = -1;
    private Random random = new Random();

    // Store one audio instance per clip (5 total)
    private int[] clipInstances = new int[] { -1, -1, -1, -1, -1 };

    // OnInit is called once when the script is initialized
    public override void OnInit()
    {
        ac = GetComponent<AudioComponent>();
        if (ac != null)
        {
            // Create one audio instance for each clip (one-time setup)
            for (int i = 0; i < pickedClips.Length; i++)
            {
                if (clipInstances[i] < 0)
                    clipInstances[i] = ac.AddInstance(pickedClips[i]);

                if (clipInstances[i] >= 0)
                {
                    // Set volume for this instance
                    ac.SetInstanceVolume(clipInstances[i], volume);
                    ac.SetInstanceLoop(clipInstances[i], false);
                }
                else
                {
                    Debug.Log($"[TreasureOneEffect] ERROR: Failed to create audio instance for clip {i}");
                }
            }

            Debug.Log($"[TreasureOneEffect] Created {pickedClips.Length} audio instances at volume {volume}");
        }
        ScheduleNextSound();

    }

    // OnUpdate is called once per frame
    public override void OnUpdate(float dt)
    {
        // Check PickUItemManager 
        if(PickUpItemManager.pickedup_Treasure_1)
        {
            timeLeftToNext -= dt;
            if(timeLeftToNext <= 0.0f)
            {
               PlaySound_Treasure_1();
               ScheduleNextSound();
            }
           
            
        }
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

    public void PlaySound_Treasure_1()
    {
       if (ac == null)
            return;

        // Pick random clip (avoid repeating same one)
        int randomIndex = random.Next(pickedClips.Length);
        if (randomIndex == prevPickedIndex)
        {
            randomIndex = (randomIndex + 1) % pickedClips.Length;
        }
        prevPickedIndex = randomIndex;

        // Play the specific instance for this clip
        // This won't interfere with footsteps (which use instance 0)
        int instanceIndex = clipInstances[randomIndex];
        if (instanceIndex >= 0)
        {
            ac.PlayInstance(instanceIndex);
        }
    }

    void ScheduleNextSound()
    {
        
        timeLeftToNext = 7.0f;
        
    }   
}
