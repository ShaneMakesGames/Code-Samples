using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TitleUI : MonoBehaviour, IMenuScreen
{
    public GameObject canvasOBJ;

    public int buttonIndex;
    public List<UIButtonContainer> titleButtonList = new List<UIButtonContainer>();

    public Image cursorImage;
    public float lastTimeCursorMoved;

    private bool cursorAnimatingRight;
    private float cursorAnimTimePassed;

    public void HandleInput(Gamepad gamepad)
    {
        AnimateCursorXPosition();

        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            OnConfirm();
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

        switch (titleButtonList[buttonIndex].myButtonType)
        {
            case ButtonType.PLAY:
                InputHandler.SetGameState(GameState.BATTLE, playFadeAnimation: true);
                break;
            case ButtonType.SETTINGS:
                InputHandler.SetGameState(GameState.SETTINGS_SCREEN);
                break;
            case ButtonType.QUIT:
                Application.Quit();
                break;
        }
    }

    public void MoveCursor(int moveAmount)
    {
        // Resets horizontal cursor animation
        cursorAnimatingRight = true;
        cursorAnimTimePassed = 0;

        SFXSystem.singleton.PlaySFX("LouderMenuNav");

        // For cursor movement cooldown
        lastTimeCursorMoved = Time.time;

        // Deselects old button
        titleButtonList[buttonIndex].OnDeselect();

        buttonIndex += moveAmount;

        if (buttonIndex < 0) buttonIndex = titleButtonList.Count - 1;
        if (buttonIndex >= titleButtonList.Count) buttonIndex = 0;

        // Selects new button
        titleButtonList[buttonIndex].OnSelect();

        LeanTween.moveY(cursorImage.gameObject, titleButtonList[buttonIndex].transform.position.y, InputHandler.CURSOR_MOVE_ANIM_TIME);
    }

    public void OpenMenuScreen()
    {
        canvasOBJ.SetActive(true);

        cursorAnimatingRight = true;
        cursorAnimTimePassed = 0;
    }

    public void CloseMenuScreen(GameState newGameState)
    {
        canvasOBJ.SetActive(false);
    }
}