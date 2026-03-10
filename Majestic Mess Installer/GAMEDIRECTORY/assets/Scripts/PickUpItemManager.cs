using System;
using System.Collections.Generic;
using Engine;

public class PickUpItemManager : Entity
{
    // Static counter - shared across all collectibles
    public static int totalMonies = 0;
    public static int collectibleCount = 0;

    //public Text moneyCounter; // Reference to UI text
    // public Text lootCounter; // Reference to UI text

    public static int TreasureTotalValue = 0;   // total money earned from treasures only
    public static int MainTreasureTotalValue = 0;
    public static int MiscTreasureTotalValue = 0;

    public static bool pickedup_Treasure_1 = false;
    public static bool pickedup_Treasure_2 = false;
    public static bool pickedup_Treasure_3 = false;

    // Instance field wrapper for C++ access (GetFieldValue can't access static fields or properties)
    public bool Treasure3PickedUp = false;

    public override void OnInit()
    {
        // Per-scene reset (treasure flags only)
        pickedup_Treasure_1 = false;
        pickedup_Treasure_2 = false;
        pickedup_Treasure_3 = false;
        Treasure3PickedUp = false;
    }

    // Call on app launch / main menu / new game
    public static void ResetRun()
    {
        totalMonies = 0;
        collectibleCount = 0;
        TreasureTotalValue = 0;
        MainTreasureTotalValue = 0;
        MiscTreasureTotalValue = 0;

        pickedup_Treasure_1 = false;
        pickedup_Treasure_2 = false;
        pickedup_Treasure_3 = false;
    }

    public override void OnUpdate(float dt)
    {
        // Sync instance field with static field for C++ access
        Treasure3PickedUp = pickedup_Treasure_3;
    }

    public static void Pickedup_Treasure_1()
    {
        pickedup_Treasure_1 = true;

    }
    public static void Pickedup_Treasure_2()
    {
        pickedup_Treasure_2 = true;

    }

    public static void Pickedup_Treasure_3()
    {
        pickedup_Treasure_3 = true;

    }

    public static void AddTreasureValue(int amount, bool isMain)
    {
        if (amount < 0) amount = 0;

        TreasureTotalValue += amount;

        if (isMain) MainTreasureTotalValue += amount;
        else        MiscTreasureTotalValue += amount;

        // If you ALSO want totalMonies to include treasure, do it here:
        totalMonies += amount;
    }

    public static void UpdateCounterUI_Treasure_High()
    {
        /*
        if (moneyCounter != null)
        {
            totalMonies += 5000;    
          //  moneyCounter.text = totalMonies.ToString();
        }

        if(lootCounter !=  null)
        {
            collectibleCount += 1;
          //  lootCounter.text = collectibleCount.ToString();
        }
        */
    }
    public static void UpdateCounterUI_Treasure_Small()
    {
        /*
        if (moneyCounter != null)
        {
            totalMonies += 1000;
         //   moneyCounter.text = totalMonies.ToString();
        }
        */
    }

     public static void AddValue(int amount)
    {
        totalMonies += amount;
        Debug.Log($"[Treasure] Total monies collected = {totalMonies}");
    }
    
}
