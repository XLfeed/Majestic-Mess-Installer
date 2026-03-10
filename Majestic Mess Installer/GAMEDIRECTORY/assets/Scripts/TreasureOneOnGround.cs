using System;
using System.Collections.Generic;
using Engine;

public class TreasureOneOnGround : Entity
{
    // Treasure clips (Unpicked and Near)
    public string[] nearClips =
    {
        "assets/Audio/TreasureOneVoices/T1_Near_1.wav",
        "assets/Audio/TreasureOneVoices/T1_Near_2.wav",
        "assets/Audio/TreasureOneVoices/T1_Near_3.wav",
        "assets/Audio/TreasureOneVoices/T1_Near_4.wav",
        "assets/Audio/TreasureOneVoices/T1_Near_5.wav",
        "assets/Audio/TreasureOneVoices/T1_Near_6.wav",
        "assets/Audio/TreasureOneVoices/T1_Near_7.wav"
    
    };

    public string[] unpickedClips =
    {

        "assets/Audio/TreasureOneVoices/T1_Unpicked_1.wav",
        "assets/Audio/TreasureOneVoices/T1_Unpicked_2.wav",
        "assets/Audio/TreasureOneVoices/T1_Unpicked_3.wav",
        "assets/Audio/TreasureOneVoices/T1_Unpicked_4.wav"       
    };

    private AudioComponent ac;

    private Random random = new Random();
    private float timeLeftToNext = 0f;  // Manually track time like TestScript does
    public float minDelay = 3.0f;   // Min seconds between sounds
    public float maxDelay = 4.0f;  // Max seconds between sounds
    public float nearPlayerRadius = 5.0f;  // Distance for "near" sounds
    private int prevUnpickedIndex = -1;
    private int prevNearIndex = -1;


    private Entity playerEntity;  // Cached player reference

    // OnInit is called once when the script is initialized
    public override void OnInit()
    {

        ac = GetComponent<AudioComponent>();
        //Find player entity
        playerEntity = Entity.FindEntityByName("Player");

        // Initialize timer with a random delay so sound doesn't play immediately
        ScheduleNextSound();
       
    }

    // OnUpdate is called once per frame
    public override void OnUpdate(float dt)
    {
        
        // Check if treasure was picked up - stop playing sounds
        //Taking the value from the class itself
        if (PickUpItemManager.pickedup_Treasure_1)
        {
            // Treasure picked up - don't play any sounds
            return;
        }
 
        timeLeftToNext -= dt;

        // Check if it's time to play a sound
        if (timeLeftToNext <= 0.0f)
        {
          
            // Check distance to player (manual distance check)
            bool playerIsNear = IsPlayerNear();

            if (playerIsNear)
            {
                PlayRandomNearSound();
            }
            else
            {              
                PlayRandomUnpickedSound();
            }

            ScheduleNextSound();
           
        }
    }
    bool IsPlayerNear()
    {
        if (!playerEntity.IsValid())
            return false;

        Vector3 myPos = Transform.Position;
        Vector3 playerPos = playerEntity.Transform.Position;

        // Calculate distance (like PlayerInteraction.cs does)
        float dx = playerPos.x - myPos.x;
        float dz = playerPos.z - myPos.z;
        float distanceSquared = dx * dx + dz * dz;  // Ignore Y axis

        float radiusSquared = nearPlayerRadius * nearPlayerRadius;

        return distanceSquared <= radiusSquared;
    }

    void ScheduleNextSound()
    {
        
        timeLeftToNext = 4.0f;
        
    }   

   void PlayRandomUnpickedSound()
    {
        if (ac == null)
            return;

        // Pick random clip (avoid repeating same one)
        int randomIndex = random.Next(unpickedClips.Length);
        if (randomIndex == prevUnpickedIndex)
        {
            randomIndex = (randomIndex + 1) % unpickedClips.Length;
        }
        prevUnpickedIndex = randomIndex;
        ac.Play(unpickedClips[randomIndex]);
    }

    void PlayRandomNearSound()
    {
        if (ac == null)
            return;

        // Pick random clip (avoid repeating same one)
        int randomIndex = random.Next(nearClips.Length);
        if (randomIndex == prevNearIndex)
        {
            randomIndex = (randomIndex + 1) % nearClips.Length;
        }
        prevNearIndex = randomIndex;
        ac.Play(nearClips[randomIndex]);
    }
}
