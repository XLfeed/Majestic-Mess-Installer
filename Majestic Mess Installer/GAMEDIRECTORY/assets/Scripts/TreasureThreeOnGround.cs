using System;
using System.Collections.Generic;
using Engine;

public class TreasureThreeOnGround : Entity
{
    // Treasure clips (Unpicked and Near)
    public string clips =  "assets/Audio/TreasureThree/MobilePhone_Loop_NoPause.wav";

    bool playonce = true;
    private AudioComponent ac;
    // OnInit is called once when the script is initialized
    public override void OnInit()
    {

        ac = GetComponent<AudioComponent>();
        // Initialize timer with a random delay so sound doesn't play immediately
        PlaySound();
       
    }

    // OnUpdate is called once per frame
    public override void OnUpdate(float dt)
    {
        
        // Check if treasure was picked up - stop playing sounds
        //Taking the value from the class itself
        if (PickUpItemManager.pickedup_Treasure_3 && playonce)
        {
            // Treasure picked up - don't play any sounds
            ac.Stop();
            playonce = false;
            return;
        }

       // PlaySound();
       
    }


    void PlaySound()
    {
        if (ac == null)
            return;


        ac.Play(clips);
    }

    
}
