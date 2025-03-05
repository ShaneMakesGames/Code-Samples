using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseUI : MonoBehaviour, IMenuScreen
{
    public GameObject canvasOBJ;

    public static bool isGamePaused;

    public int pauseButtonIndex;
    public List<UIButtonContainer> pauseButtonList = new List<UIButtonContainer>();

    public Image topCloudImage;
    public Image bottomCloudImage;

    public Image cursorImage;

    public float lastTimeCursorMoved;

    private bool cursorAnimatingRight;
    private float cursorAnimTimePassed;

    public string gamepadDisplayName;
    public Image confirmIcon;
    public Sprite xboxConfirmSprite;
    public Sprite playstationConfirmSprite;
    public Image cancelIcon;
    public Sprite xboxCancelSprite;
    public Sprite playstationCancelSprite;

    public const float INTRO_CLOUD_ANIM_TIME = 0.4f;
    public const float EXIT_CLOUD_ANIM_TIME = 0.08f;

    public void HandleInput(Gamepad gamepad)
    {
        AnimateCursorXPosition();

        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            OnConfirm();
            return;
        }
        if (gamepad.buttonEast.wasPressedThisFrame || gamepad.startButton.wasPressedThisFrame)
        {
            OnCancel();
            return;
        }

        if (Time.time - lastTimeCursorMoved < InputHandler.CURSOR_MOVEMENT_COOLDOWN_TIME) return;

        if (gamepad.leftStick.up.isPressed || gamepad.dpad.up.isPressed)
        {
            MoveCursor(-1);
            return;
        }
        if (gamepad.leftStick.down.isPressed || gamepad.dpad.down.isPressed)
        {
            MoveCursor(1);
            return;
        }
    }

    private void AnimateCursorXPosition()
    {
        float cursorXPos, animTime;
        cursorAnimTimePassed += Time.deltaTime;

        if (cursorAnimatingRight)
        {
            animTime = InputHandler.CURSOR_X_RIGHT_ANIM_TIME;
            cursorXPos = Mathf.Lerp(InputHandler.CURSOR_LEFTMOST_X_POS, InputHandler.CURSOR_RIGHTMOST_X_POS, cursorAnimTimePassed / animTime);
        }
        else
        {
            animTime = InputHandler.CURSOR_X_LEFT_ANIM_TIME;
            cursorXPos = Mathf.Lerp(InputHandler.CURSOR_RIGHTMOST_X_POS, InputHandler.CURSOR_LEFTMOST_X_POS, cursorAnimTimePassed / animTime);
        }

        Vector3 newPos = cursorImage.rectTransform.localPosition;
        newPos.x = cursorXPos;
        cursorImage.rectTransform.localPosition = newPos;

        if (cursorAnimTimePassed >= animTime)
        {
            cursorAnimatingRight = !cursorAnimatingRight;
            cursorAnimTimePassed = 0;
        }
    }

    public void OnConfirm()
    {
        SFXSystem.singleton.PlaySFX("UI_Confirm");
        TryUpdateInputIcons();

        switch (pauseButtonList[pauseButtonIndex].myButtonType)
        {
            case ButtonType.RESUME:
                InputHandler.SetGameState(GameState.BATTLE);
                break;
            case ButtonType.SETTINGS:
                InputHandler.SetGameState(GameState.SETTINGS_SCREEN);
                break;
            case ButtonType.QUIT:
                Application.Quit();
                break;
        }
    }

    public void OnCancel()
    {
        SFXSystem.singleton.PlaySFX("UI_Cancel");
        TryUpdateInputIcons();

        InputHandler.SetGameState(GameState.BATTLE);
    }

    public void MoveCursor(int moveAmount)
    {
        // Resets horizontal cursor animation
        cursorAnimatingRight = true;
        cursorAnimTimePassed = 0;

        SFXSystem.singleton.PlaySFX("LouderMenuNav");
        TryUpdateInputIcons();

        // For cursor movement cooldown
        lastTimeCursorMoved = Time.time;

        // Deselects old button
        pauseButtonList[pauseButtonIndex].OnDeselect();

        pauseButtonIndex += moveAmount;

        if (pauseButtonIndex < 0) pauseButtonIndex = pauseButtonList.Count - 1;
        if (pauseButtonIndex >= pauseButtonList.Count) pauseButtonIndex = 0;

        // Selects new button
        pauseButtonList[pauseButtonIndex].OnSelect();

        LeanTween.moveLocalY(cursorImage.gameObject, pauseButtonList[pauseButtonIndex].transform.localPosition.y, InputHandler.CURSOR_MOVE_ANIM_TIME);
    }

    public void TryUpdateInputIcons()
    {
        if (gamepadDisplayName == InputHandler.singleton.gamepadDisplayName) return;

        gamepadDisplayName = InputHandler.singleton.gamepadDisplayName;
        if (gamepadDisplayName.Contains("Xbox"))
        {
            confirmIcon.sprite = xboxConfirmSprite;
            cancelIcon.sprite = xboxCancelSprite;
        }
        else
        {
            confirmIcon.sprite = playstationConfirmSprite;
            cancelIcon.sprite = playstationCancelSprite;
        }
    }

    public void OpenMenuScreen()
    {
        isGamePaused = true;
        EventManager.PublishEvent(EventType.GAME_PAUSED);
        canvasOBJ.SetActive(true);
        TryUpdateInputIcons();

        cursorAnimatingRight = true;
        cursorAnimTimePassed = 0;
    }

    public void CloseMenuScreen(GameState newGameState)
    {
        canvasOBJ.SetActive(false);

        if (newGameState != GameState.SETTINGS_SCREEN)
        {
            isGamePaused = false;
            EventManager.PublishEvent(EventType.GAME_UNPAUSED);
        }
    }
}