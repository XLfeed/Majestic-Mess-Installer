using System;
using System.Collections.Generic;
using Engine;

public class TreasureOneUnpicked : Entity
{
    // Configuration
    private float minDelay = 5.0f;  // Minimum seconds between sounds
    private float maxDelay = 15.0f; // Maximum seconds between sounds

    // Play sound occasionally
    private float nextPlayAudioTime = 0;
    private Random random = new Random();

    // OnInit is called when entity is created
    public override void OnInit()
    {
        ScheduleNextPopup();
    }

    // OnUpdate is called once per frame
    public override void OnUpdate(float dt)
    {
        if(Time.GetTime() >= nextPlayAudioTime)
        {
            PlaySound_Unpicked_Treasure_1();
            ScheduleNextPopup();
        }
    }

    public void PlaySound_Unpicked_Treasure_1()
    {
        // TODO: Add sound playing logic here
    }

    void ScheduleNextPopup() // Randomise the interval
    {
        float randomDelay = (float)(random.NextDouble() * (maxDelay - minDelay) + minDelay);
        nextPlayAudioTime = Time.GetTime() + randomDelay;
    }

    // Called every frame while another collider is overlapping with this trigger
    public override void OnTriggerStay(ColliderComponent collider)
    {
        // Get the entity from the collider
        Entity otherEntity = collider.Entity;

        // Check if the entity has the "Player" tag
        if (otherEntity.CompareTag("Player"))
        {
            Debug.Log("Player is near the treasure!");
            // Add your logic here - e.g., play a different sound, show UI, etc.
        }
    }
}
