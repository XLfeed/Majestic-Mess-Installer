using Engine;

// Maps to your C++ SceneType enum order
// (MainMenu, Gameplay, Pause, Settings, Loading, Cutscene, WinUI, GameOver)
public enum NativeSceneType
{
    MainMenu = 0,
    Gameplay = 1,
    Pause = 2,
    Settings = 3,
    Loading = 4,
    Cutscene = 5,
    WinUI = 6,
    GameOver = 7
}

public enum UIControllerMode
{
    MainMenu,
    Setting,
    GameScene,
    WinScreen,
    LoseScreen
}

/// <summary>
/// Unified UI controller for:
/// - Main Menu
/// - Game Scene
/// - Win UI
/// - Lose UI
/// - Settings
///
/// Auto-sets its mode based on the active native scene.
/// </summary>
public class UIBaseController : Entity
{
    // Auto-set, you don't need to touch this in inspector
    public UIControllerMode mode = UIControllerMode.MainMenu;

    // Scene names (must match SceneManager registration in C++)
    public string gameSceneName    = "TestLevel";     // gameplay
    public string menuSceneName    = "MainMenu";      // main menu
    public string winSceneName     = "WinScene";      // win UI (GameOver)
    public string loseSceneName    = "LoseScene";     // lose UI (WinUI)
    public string settingSceneName = "SettingScene";  // settings

    // Keys (can still tweak in inspector if you want)
    public KeyCode startKey   = KeyCode.Space; // Main menu -> game
    public KeyCode settingKey = KeyCode.S;     // Main menu -> settings

    public KeyCode loseKey    = KeyCode.L;     // Game -> lose
    public KeyCode winKey     = KeyCode.I;     // Game -> win
    public KeyCode menuKey    = KeyCode.Space;     // Any (mode that supports it) -> menu
    public KeyCode restartKey = KeyCode.P;     // Win/Lose -> game
    public KeyCode exitKey = KeyCode.E;     // Main -> Exit


    private bool _initialized = false;
    private bool _mainMenuResetDone = false;

    public override void OnUpdate(float deltaTime)
    {
        // One-time auto configuration from active scene
        if (!_initialized)
        {
            AutoConfigureModeFromScene();
            _initialized = true;
        }

        switch (mode)
        {
            case UIControllerMode.MainMenu:
                HandleMainMenu();
                break;

            // case UIControllerMode.GameScene:
            //     HandleGameScene();
            //     break;

            case UIControllerMode.Setting:
                HandleSetting();
                break;

            case UIControllerMode.WinScreen:
                HandleWinScreen();
                break;

            case UIControllerMode.LoseScreen:
                HandleLoseScreen();
                break;
        }
    }

    // =======================
    // Auto mode detection
    // =======================
    private void AutoConfigureModeFromScene()
    {
        int sceneTypeInt = InternalCalls.Scene_GetActiveSceneType();
        string sceneName = InternalCalls.Scene_GetActiveSceneName();

        mode = UIControllerMode.GameScene; // safe default

        if (sceneTypeInt >= 0)
        {
            NativeSceneType native = (NativeSceneType)sceneTypeInt;

            switch (native)
            {
                case NativeSceneType.MainMenu:
                    mode = UIControllerMode.MainMenu;
                    break;

                // case NativeSceneType.Gameplay:
                //     mode = UIControllerMode.GameScene;
                //     break;

                case NativeSceneType.Settings:
                    mode = UIControllerMode.Setting;
                    break;

                case NativeSceneType.WinUI:
                    // your C++ uses WinUI for LoseScene
                    mode = UIControllerMode.LoseScreen;
                    break;

                case NativeSceneType.GameOver:
                    // used for WinScene and GameOver
                    mode = UIControllerMode.WinScreen;
                    break;

                default:
                    mode = UIControllerMode.GameScene;
                    break;
            }
        }

        Debug.Log($"[UIBaseController] Active scene '{sceneName}' -> mode {mode} (native type {sceneTypeInt})");

        // Reset persistent run data when entering main menu
        if (mode == UIControllerMode.MainMenu && !_mainMenuResetDone)
        {
            PickUpItemManager.ResetRun();
            _mainMenuResetDone = true;
        }
    }

    // =======================
    // Mode handlers
    // =======================

    private void HandleMainMenu()
    {
        // Space -> start game
        if (Input.IsKeyPressed(startKey) && !string.IsNullOrEmpty(gameSceneName))
        {
            Debug.Log($"[UIBaseController/MainMenu] Loading game scene: {gameSceneName}");
            Scene.LoadScene(gameSceneName);
        }

        // S -> settings
        if (Input.IsKeyPressed(settingKey) && !string.IsNullOrEmpty(settingSceneName))
        {
            Debug.Log($"[UIBaseController/MainMenu] Loading settings scene: {settingSceneName}");
            Scene.LoadScene(settingSceneName);
        }

        //L -> lose scene
        if (Input.IsKeyPressed(loseKey) && !string.IsNullOrEmpty(loseSceneName))
        {
            Debug.Log($"[UIBaseController/GameScene] Loading lose scene: {loseSceneName}");
            Scene.LoadScene(loseSceneName);
        }

        // I -> win scene
        if (Input.IsKeyPressed(winKey) && !string.IsNullOrEmpty(winSceneName))
        {
            Debug.Log($"[UIBaseController/GameScene] Loading win scene: {winSceneName}");
            Scene.LoadScene(winSceneName);
        }

        // E -> Exit
        if (Input.IsKeyPressed(exitKey))
        {
            Debug.Log("[UIBaseController/MainMenu] Exiting game");
            InternalCalls.Window_SetShouldClose();
        }
    }

    // private void HandleGameScene()
    // {
    //     // L -> lose scene
    //     if (Input.IsKeyPressed(loseKey) && !string.IsNullOrEmpty(loseSceneName))
    //     {
    //         Debug.Log($"[UIBaseController/GameScene] Loading lose scene: {loseSceneName}");
    //         Scene.LoadScene(loseSceneName);
    //     }

    //     // I -> win scene
    //     if (Input.IsKeyPressed(winKey) && !string.IsNullOrEmpty(winSceneName))
    //     {
    //         Debug.Log($"[UIBaseController/GameScene] Loading win scene: {winSceneName}");
    //         Scene.LoadScene(winSceneName);
    //     }

    //     // O -> main menu
    //     if (Input.IsKeyPressed(menuKey) && !string.IsNullOrEmpty(menuSceneName))
    //     {
    //         Debug.Log($"[UIBaseController/GameScene] Loading menu scene: {menuSceneName}");
    //         Scene.LoadScene(menuSceneName);
    //     }
    // }

    private void HandleSetting()
    {
        // Space -> main menu
        if (Input.IsKeyPressed(menuKey) && !string.IsNullOrEmpty(menuSceneName))
        {
            Debug.Log($"[UIBaseController/Setting] Loading menu scene: {menuSceneName}");
            Scene.LoadScene(menuSceneName);
        }
    }

    private void HandleWinScreen()
    {
        // Space -> main menu
        if (Input.IsKeyPressed(menuKey) && !string.IsNullOrEmpty(menuSceneName))
        {
            Debug.Log($"[UIBaseController/WinScreen] Loading menu scene: {menuSceneName}");
            Scene.LoadScene(menuSceneName);
        }
    }

    private void HandleLoseScreen()
    {
        // Space -> main menu
        if (Input.IsKeyPressed(menuKey) && !string.IsNullOrEmpty(menuSceneName))
        {
            Debug.Log($"[UIBaseController/LoseScreen] Loading menu scene: {menuSceneName}");
            Scene.LoadScene(menuSceneName);
        }
    }
}
