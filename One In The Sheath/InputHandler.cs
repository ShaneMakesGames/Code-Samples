using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    TITLE_SCREEN,
    BATTLE,
    PAUSE_SCREEN,
    RESULTS_SCREEN,
    SETTINGS_SCREEN
}

public class InputHandler : MonoBehaviour
{
    public GameState previousGameState;
    public GameState currentGameState;
    public Dictionary<GameState, IMenuScreen> dictAllMenusByGameState;
    public bool transitionInProgress;

    private Gamepad gamepad;
    public string gamepadDisplayName;
    public bool isGamepadDisconnected;

    public TitleUI titleUI;
    public Player player;
    public BattleUI battleUI;
    public PauseUI pauseUI;
    public ResultsUI resultsUI;
    public SettingsUI settingsUI;

    public const float CURSOR_MOVEMENT_COOLDOWN_TIME = 0.15f; // Time between cursor movements when holding a direction on the thumbstick
    public const float CURSOR_MOVE_ANIM_TIME = 0.1f; // The time it takes to animate the cursor moving from one position to the next

    // Used for the cursor pulsing/breathing idle animation
    public const float CURSOR_X_LEFT_ANIM_TIME = 0.75f;
    public const float CURSOR_X_RIGHT_ANIM_TIME = 0.4f;

    public const float CURSOR_LEFTMOST_X_POS = -375f;
    public const float CURSOR_RIGHTMOST_X_POS = -325f;

    #region Singleton

    public static InputHandler singleton;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            Initialize();
        }
        else Destroy(this.gameObject);

        dictAllMenusByGameState = new Dictionary<GameState, IMenuScreen>();
        dictAllMenusByGameState.Add(GameState.TITLE_SCREEN, titleUI);
        dictAllMenusByGameState.Add(GameState.BATTLE, battleUI);
        dictAllMenusByGameState.Add(GameState.PAUSE_SCREEN, pauseUI);
        dictAllMenusByGameState.Add(GameState.RESULTS_SCREEN, resultsUI);
        dictAllMenusByGameState.Add(GameState.SETTINGS_SCREEN, settingsUI);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #endregion

    private void Initialize()
    {
        gamepad = Gamepad.current;
        if (gamepad == null) isGamepadDisconnected = true;
        else gamepadDisplayName = gamepad.displayName;
        
        // Handles controller disconnect & reconnect
        InputSystem.onDeviceChange += (device, change) =>
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    if (gamepad != null) break;
                    isGamepadDisconnected = false;
                    gamepad = Gamepad.current;
                    gamepadDisplayName = gamepad.displayName;
                    break;

                case InputDeviceChange.Removed:
                    isGamepadDisconnected = true;
                    gamepad = null;
                    break;
            }
        };
    }

    public static void SetGameState(GameState newGameState, bool playFadeAnimation = false)
    {
        singleton.StartCoroutine(singleton.SetGameStateCoroutine(newGameState, playFadeAnimation));
    }

    protected IEnumerator SetGameStateCoroutine(GameState newGameState, bool playFadeAnimation)
    {
        transitionInProgress = true;

        if (playFadeAnimation)
        {
            FadeManager.singleton.InitiateFade(true);
            yield return new WaitForSeconds(FadeManager.FADE_TIME + 0.15f);
        }

        dictAllMenusByGameState[currentGameState].CloseMenuScreen(newGameState);
        previousGameState = currentGameState;
        currentGameState = newGameState;
        dictAllMenusByGameState[currentGameState].OpenMenuScreen();

        if (playFadeAnimation) FadeManager.singleton.InitiateFade(false);

        transitionInProgress = false;
    }

    void Update()
    {
        if (isGamepadDisconnected) return;
        if (transitionInProgress) return;

        if (currentGameState == GameState.TITLE_SCREEN)
        {
            titleUI.HandleInput(gamepad);
            return;
        }
        if (currentGameState == GameState.BATTLE)
        {
            if (!GameManager.singleton.battleActive) return;
            player.HandleInput(gamepad);
            return;
        }
        if (currentGameState == GameState.PAUSE_SCREEN)
        {
            pauseUI.HandleInput(gamepad);
            return;
        }
        if (currentGameState == GameState.RESULTS_SCREEN)
        {
            resultsUI.HandleInput(gamepad);
            return;
        }
        if (currentGameState == GameState.SETTINGS_SCREEN)
        {
            settingsUI.HandleInput(gamepad);
            return;
        }
    }
}