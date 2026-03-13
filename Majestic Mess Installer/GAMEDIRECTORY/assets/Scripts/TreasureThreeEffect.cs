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

    // Player timer for when they stop moving
    private float stopTimer = 0.0f;  // Timer for when player stops moving
    private const float stopThreshold = 2.0f;  // 2 seconds to trigger treasure
    private Entity playerEntity;  // Store the player entity
    private Vector3 lastPlayerPosition = Vector3.Zero;
    private float movementThreshold = 0.01f; // Minimum movement to consider as "moving"
    private bool waitingForMovement = true; // Start waiting for movement
    private bool hasBlockedInput = false;

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

        timer = 0.0f;
        swapTimer = 0.0f;
        currState = State.Hide;
        waitingForMovement = true; // Start by waiting for movement
        hasBlockedInput = false;

        // Find player entity
        playerEntity = Entity.FindEntityByName("Player");
        if (playerEntity != null && playerEntity.IsValid())
        {
            // Store initial position
            if (playerEntity.HasComponent<TransformComponent>())
            {
                lastPlayerPosition = playerEntity.Transform.Position;
            }
        }
    }

    private bool IsPlayerMoving()
    {
        if (playerEntity == null || !playerEntity.IsValid())
        {
            if (playerEntity == null)
                return false;
        }

        // Method 1: Check Transform position changes
        if (playerEntity.HasComponent<TransformComponent>())
        {
            Vector3 currentPosition = playerEntity.Transform.Position;
            float distanceMoved = Vector3.Distance(currentPosition, lastPlayerPosition);
            lastPlayerPosition = currentPosition;

            if (distanceMoved > movementThreshold)
            {
                return true;
            }
        }

        // Method 2: Check RigidBody velocity
        if (playerEntity.HasComponent<RigidBodyComponent>())
        {
            var rb = playerEntity.GetComponent<RigidBodyComponent>();
            if (rb != null)
            {
                Vector3 velocity = rb.Velocity;
                float horizontalSpeed = Math.Abs(velocity.x) + Math.Abs(velocity.z);
                
                if (horizontalSpeed > movementThreshold)
                {
                    return true;
                }
            }
        }

        // Method 3: Check input directly
        bool input = Input.IsKeyHeld(KeyCode.W) || Input.IsKeyHeld(KeyCode.A) || 
                    Input.IsKeyHeld(KeyCode.S) || Input.IsKeyHeld(KeyCode.D);
        
        if (input)
        {
            return true;
        }

        return false;
    }


    // OnUpdate is called once per frame
    public override void OnUpdate(float dt)
    {
        if(PickUpItemManager.pickedup_Treasure_3)
        { 
            swapTimer += dt;
            switch (currState)
            {
                case State.Hide:
                    // Ensure player is unblocked when in hide state
                    if (hasBlockedInput)
                    {
                        PlayerInputBlocker.SetBlocked(false);
                        hasBlockedInput = false;
                    }

                    bool isPlayerMoving = IsPlayerMoving();

                    // Step 1: Wait for player to move at least once
                    if (waitingForMovement)
                    {
                        if (isPlayerMoving)
                        {
                            waitingForMovement = false;
                            stopTimer = 0.0f;
                        }
                        break;
                    }

                    // Step 2: After movement detected, wait for player to stop
                    if (!isPlayerMoving)
                    {
                        stopTimer += dt;
                        if (stopTimer >= stopThreshold)
                        {
                            currState = State.Show;
                            stopTimer = 0.0f;
                        }
                    }
                    else
                    {
                        // Player is moving, reset timer
                        if (stopTimer > 0)
                        {
                            stopTimer = 0.0f;
                        }
                    }
                    break;

                case State.Show:
                    // Block player input when treasure appears
                    PlayerInputBlocker.SetBlocked(true);
                    hasBlockedInput = true;
                    
                    stopTimer = 0.0f;
                    waitingForMovement = false;
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
                    // Check for spacebar press
                    // Keep player input blocked during the event
                    if (!hasBlockedInput)
                    {
                        PlayerInputBlocker.SetBlocked(true);
                        hasBlockedInput = true;
                    }

                    ScreenImageSwitchManager();  

                    if (IsSpacebarPressed())
                    {
                        spacebarPressCount++;
                        if (spacebarPressCount >= requiredPresses)
                        {
                            // Unblock player input when interaction is complete
                            PlayerInputBlocker.SetBlocked(false);
                            hasBlockedInput = false;
                            
                            InternalCalls.UIElementComponent_SetActive(phonePanel.ID, false);
                            resetTimerRand();
                            spacebarPressCount = 0;

                            waitingForMovement = true;
                            stopTimer = 0.0f;
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
