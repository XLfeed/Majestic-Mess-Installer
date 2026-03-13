using System;
using System.Collections.Generic;
using Engine;

public class PickUpItemManager : Entity
{
    //public KeyCode debugTreasureOneKey = KeyCode.D7;
    //public KeyCode debugTreasureTwoKey = KeyCode.D8;
    //public KeyCode debugTreasureThreeKey = KeyCode.D9;
    //public KeyCode debugTreasureFourKey = KeyCode.D0;

    public string treasureOneIconName = "Treasure1Icon";
    public string treasureTwoIconName = "Treasure2Icon";
    public string treasureThreeIconName = "Treasure3Icon";
    public string treasureFourIconName = "Treasure4Icon";

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
    public static bool pickedup_Treasure_4 = false;
    //public static bool debugUnlock_Cinder = false;
    //public static bool debugUnlock_Disguise = false;

    // Instance field wrapper for C++ access (GetFieldValue can't access static fields or properties)
    public bool Treasure3PickedUp = false;

    private Entity treasureOneIcon;
    private Entity treasureTwoIcon;
    private Entity treasureThreeIcon;
    private Entity treasureFourIcon;

    public override void OnInit()
    {
        Treasure3PickedUp = false;
        ResolveTreasureHud();
        SyncTreasureHud();
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
        pickedup_Treasure_4 = false;
        //debugUnlock_Cinder = false;
        //debugUnlock_Disguise = false;
    }

    public override void OnUpdate(float dt)
    {
        // Sync instance field with static field for C++ access
        Treasure3PickedUp = pickedup_Treasure_3;
        //HandleDebugTreasureKeys();
        ResolveTreasureHud();
        SyncTreasureHud();
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

    public static void Pickedup_Treasure_4()
    {
        pickedup_Treasure_4 = true;
    }
    // Change these return values to remap which treasure unlocks each skill.
    // Example: use pickedup_Treasure_3 for Cinder if Treasure 3 should unlock Cinder.
    public static bool IsCinderUnlocked() 
    {
        
        return pickedup_Treasure_2; 
    }

    public static bool IsDisguiseUnlocked()
    {
        return pickedup_Treasure_4; 
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

    private void ResolveTreasureHud()
    {
        if ((treasureOneIcon == null || !treasureOneIcon.IsValid()) && !string.IsNullOrEmpty(treasureOneIconName))
            treasureOneIcon = Entity.FindEntityByName(treasureOneIconName);

        if ((treasureTwoIcon == null || !treasureTwoIcon.IsValid()) && !string.IsNullOrEmpty(treasureTwoIconName))
            treasureTwoIcon = Entity.FindEntityByName(treasureTwoIconName);

        if ((treasureThreeIcon == null || !treasureThreeIcon.IsValid()) && !string.IsNullOrEmpty(treasureThreeIconName))
            treasureThreeIcon = Entity.FindEntityByName(treasureThreeIconName);

        if ((treasureFourIcon == null || !treasureFourIcon.IsValid()) && !string.IsNullOrEmpty(treasureFourIconName))
            treasureFourIcon = Entity.FindEntityByName(treasureFourIconName);
    }

    private void SyncTreasureHud()
    {
        SetUiVisible(treasureOneIcon, pickedup_Treasure_1);
        SetUiVisible(treasureTwoIcon, pickedup_Treasure_2);
        SetUiVisible(treasureThreeIcon, pickedup_Treasure_3);
        SetUiVisible(treasureFourIcon, pickedup_Treasure_4);
    }

    //private void HandleDebugTreasureKeys()
    //{
    //    if (Input.IsKeyPressed(debugTreasureOneKey))
    //        Pickedup_Treasure_1();

    //    if (Input.IsKeyPressed(debugTreasureTwoKey))
    //        Pickedup_Treasure_2();

    //    if (Input.IsKeyPressed(debugTreasureThreeKey))
    //        Pickedup_Treasure_3();

    //    if (Input.IsKeyPressed(debugTreasureFourKey))
    //        Pickedup_Treasure_4();
    //}

    private static void SetUiVisible(Entity entity, bool visible)
    {
        if (entity == null || !entity.IsValid())
            return;

        var uiElement = new UIElementComponent(entity.ID);
        uiElement.Active = visible;
        uiElement.Visible = visible;
    }
}
