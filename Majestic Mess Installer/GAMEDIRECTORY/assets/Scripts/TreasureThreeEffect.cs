using System;
using System.Collections.Generic;
using Engine;

public class TreasureThreeEffect : Entity
{
    public enum State
    {
        Hide,
        Show,
        Event
    }
    private State currState = State.Hide;

    public string takeOut =  "assets/Audio/TreasureThree/MobilePhone_Appear.wav";
    public string putBack =  "assets/Audio/TreasureThree/MobilePhone_Disappear.wav";
    public string phonePanelName = "Treasure_3_canvas";
    private Entity phonePanel;
    bool prevframeActive = false;

    public float volume = 0.5f;
    private static readonly Random random = new Random();
    private float timer = 0.0f;
   

    //phonescreen stuff
    private string[] textureNames = { "PhoneScreen_1", "PhoneScreen_2", "PhoneScreen_3", "PhoneScreen_4" };
    private int currentTextureIndex = 0;
    private float swapTimer = 0.0f;
    private float swapInterval = 2.0f;
    private Entity phoneScreenEntity;
    private Entity LeftHandEntity;
    private Entity RightHandEntity;
    public string phoneScreenName = "Phone_Screen";
    public string LeftHandName = "Left_Hand";
    public string RightHandName = "Right_Hand";

    // Spacebar pressing logic
    private bool previousSpacebarState = false;
    private int spacebarPressCount = 0;
    private int requiredPresses = 5;
    public int minPresses = 5; // Minimum presses required
    public int maxPresses = 7; // Maximum presses required
    public float lowerAmount = 80.0f; // How much to lower per press
    private Vector2 initialPositionScreen;
    private Vector2 initialPositionLeft;
    private Vector2 initialPositionRight;

    public override void OnInit()
    {
        //Find da entitits
        phonePanel = Entity.FindEntityByName(phonePanelName);
        phoneScreenEntity = Entity.FindEntityByName(phoneScreenName);
        LeftHandEntity = Entity.FindEntityByName(LeftHandName);
        RightHandEntity = Entity.FindEntityByName(RightHandName);
        InternalCalls.UIElementComponent_SetActive(phonePanel.ID, false);
        timer = 0.0f;
        swapTimer = 0.0f;
        currState = State.Hide;
   

        //Init positions
        var rectTransformScreen = new RectTransformComponent(phoneScreenEntity.ID);
        var rectTransformLeft = new RectTransformComponent(LeftHandEntity.ID);
        var rectTransformRight = new RectTransformComponent(RightHandEntity.ID);
        initialPositionScreen = rectTransformScreen.AnchoredPosition;
        initialPositionLeft = rectTransformLeft.AnchoredPosition;
        initialPositionRight = rectTransformRight.AnchoredPosition;

    }

    // OnUpdate is called once per frame
    public override void OnUpdate(float dt)
    {
       
        if(PickUpItemManager.pickedup_Treasure_3)
        { 
            timer -= dt;
            swapTimer += dt;
            switch (currState)
            {
                case State.Hide:
                    if (timer <= 0.0f)
                    {
                        currState = State.Show;
                    }
                    break;

                case State.Show:
                    InternalCalls.UIElementComponent_SetActive(phonePanel.ID, true);

                    //Reset inital pos
                    var rectTransformScreen = new RectTransformComponent(phoneScreenEntity.ID);
                    var rectTransformLeft = new RectTransformComponent(LeftHandEntity.ID);
                    var rectTransformRight = new RectTransformComponent(RightHandEntity.ID);

                    rectTransformScreen.AnchoredPosition = initialPositionScreen;
                    rectTransformLeft.AnchoredPosition = initialPositionLeft;
                    rectTransformRight.AnchoredPosition = initialPositionRight;
                    spacebarPressCount = 0;

                    // Randomize required presses
                    RandomizePressCount();

                    currState = State.Event;
                    break;
                
                case State.Event:
                    ScreenImageSwitchManager();  
                    // Check for spacebar press
                    if (IsSpacebarPressed())
                    {
                        spacebarPressCount++;
                        if (spacebarPressCount >= requiredPresses)
                        {
                            InternalCalls.UIElementComponent_SetActive(phonePanel.ID, false);
                            resetTimerRand();
                            spacebarPressCount = 0;
                            currState = State.Hide;
                        }
                        else
                        {
                            // Lower the phone screen
                            LowerPhoneScreen();
                        }
                    }

                    break;

                default:
                    break;
            }
        }

        bool active = InternalCalls.UIElementComponent_GetActive(phonePanel.ID);

        if(active != prevframeActive)
        {
            if(active)
            {
                PlayShow();
            }
            else
            {
                PlayHide();
            }

        }
        prevframeActive = active;

    }

    void resetTimerRand()
    {
        //Max 15sec min 5 sec
        timer = (float)(random.NextDouble() * 10.0 + 5.0);
    }

    void RandomizePressCount()
    {
        // Generate random number between minPresses and maxPresses (inclusive)
        requiredPresses = random.Next(minPresses, maxPresses + 1);
    }
  

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
   
   void ScreenImageSwitchManager()
   {
        if(swapTimer >= swapInterval)
        {
            swapTimer = 0.0f;
            SwapToNextTexture();
        }

   }
   void SwapToNextTexture()
   {
        if (phoneScreenEntity == null)
        {
            return;
        }

        currentTextureIndex = (currentTextureIndex + 1) % textureNames.Length;
        var uiImage = new UIImageComponent(phoneScreenEntity.ID);
        uiImage.SetTextureByName(textureNames[currentTextureIndex]);

   }

   /// <summary>
   /// Detects spacebar press (rising edge - not held)
   /// Similar logic to C++ TreasureThreeEffect::IsSpacebarPressed()
   /// </summary>
   bool IsSpacebarPressed()
   {
        // Get current spacebar state
        bool currentState = Input.IsKeyPressed(KeyCode.Space);

        // Detect rising edge (pressed this frame, not held)
        bool pressed = currentState && !previousSpacebarState;

        // Update previous state for next frame
        previousSpacebarState = currentState;

        return pressed;
   }

   /// <summary>
   /// Lowers the phone screen by modifying its RectTransform AnchoredPosition
   /// </summary>
   void LowerPhoneScreen()
   {
        if (phoneScreenEntity == null || LeftHandEntity == null || RightHandEntity == null)
        {
            return;
        }


        //Lower screen
        var rectTransformScreen = new RectTransformComponent(phoneScreenEntity.ID);
        Vector2 currentPosScreen = rectTransformScreen.AnchoredPosition;

        Vector2 newPosScreen = new Vector2(currentPosScreen.x, currentPosScreen.y - lowerAmount);
        rectTransformScreen.AnchoredPosition = newPosScreen;


        //Lower left
        var rectTransformLeft = new RectTransformComponent(LeftHandEntity.ID);
        Vector2 currentPosLeft = rectTransformLeft.AnchoredPosition;

        Vector2 newPosLeft = new Vector2(currentPosLeft.x, currentPosLeft.y - lowerAmount);
        rectTransformLeft.AnchoredPosition = newPosLeft;

        //Lower right
        var rectTransformRight = new RectTransformComponent(RightHandEntity.ID);
        Vector2 currentPosRight = rectTransformRight.AnchoredPosition;

        Vector2 newPosRight = new Vector2(currentPosRight.x, currentPosRight.y - lowerAmount);
        rectTransformRight.AnchoredPosition = newPosRight;
   }
}
