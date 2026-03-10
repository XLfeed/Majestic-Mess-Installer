using System;
using System.Collections.Generic;
using Engine;

/*
public class PickUpItem : Entity
{
  

    public override void OnInit()
    {
        Debug.Log($"PickUpItem initialized on {Name}");
    }

    public override void OnExit()
    {

    }

    public override void OnUpdate(float dt)
    {

    }

    /// <summary>
    /// Called every frame while something is in the trigger volume
    /// </summary>
    public override void OnTriggerStay(ColliderComponent collider)
    {
        // Check if the colliding entity has the "Player" tag
        if (collider.Entity.CompareTag("Player"))
        {
            
                Debug.Log($"Player is in pickup range! Press E to pick up {Name}");

                // Example: Pick up when player presses E key
                if (Input.IsKeyPressed(KeyCode.E))
                {
                    PickUp(collider.Entity);

                    if(this.Name == "Treasure_1_default")
                    {      
                       // other.gameObject.GetComponent<TreasurePickUpManager>().UpdateCounterUI_Treasure_High();
                       // other.gameObject.GetComponent<TreasurePickUpManager>().Pickedup_Treasure_1();
                    }
                    else if (this.Name == "Treasure_2_default")
                    {
                       // other.gameObject.GetComponent<TreasurePickUpManager>().UpdateCounterUI_Treasure_High();
                       // other.gameObject.GetComponent<TreasurePickUpManager>().Pickedup_Treasure_2();
                    }
                    else if (this.Name == "Treasure_3_default")
                    {
                        //other.gameObject.GetComponent<TreasurePickUpManager>().UpdateCounterUI_Treasure_High();
                       // other.gameObject.GetComponent<TreasurePickUpManager>().Pickedup_Treasure_3();
                    }
                    else
                    {
                        //other.gameObject.GetComponent<TreasurePickUpManager>().UpdateCounterUI_Treasure_Small();
                    }
                                        
                }
                            
        }
    }

    /// <summary>
    /// Handle the pickup logic
    /// </summary>
    private void PickUp(Entity player)
    {
       
        Debug.Log($"{Name} has been picked up by {player.Name}!");

        // TODO: Add your pickup logic here
        // - Add item to player inventory
        // - Play pickup sound
        // - Destroy or hide the item
        // - Show pickup UI feedback

        // Example: Destroy this entity after pickup
        // InternalCalls.Scene_DestroyEntity(ID);
    }

    /// <summary>
    /// Called when player enters the trigger
    /// </summary>
    public override void OnTriggerEnter(ColliderComponent collider)
    {
        if (collider.Entity.CompareTag("Player"))
        {
            Debug.Log("Player entered pickup zone");
        }
    }

    /// <summary>
    /// Called when player exits the trigger
    /// </summary>
    public override void OnTriggerExit(ColliderComponent collider)
    {
        if (collider.Entity.CompareTag("Player"))
        {
            Debug.Log("Player left pickup zone");
        }
    }
}
*/