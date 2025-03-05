using UnityEngine;
using UnityEngine.UI;

using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SettingsUI : MonoBehaviour, IMenuScreen
{
    public GameObject canvasOBJ;

    public int sliderIndex;
    public List<UISliderContainer> sliderList = new List<UISliderContainer>();

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

    public void HandleInput(Gamepad gamepad)
    {
        AnimateCursorXPosition();

        if (gamepad.buttonEast.isPressed)
        {
            OnCancel();
            return;
        }

        if (Time.time - lastTimeCursorMoved < InputHandler.CURSOR_MOVEMENT_COOLDOWN_TIME) return;

        int multiplier = 1;
        if (gamepad.buttonSouth.isPressed)
        {
            multiplier = 10;
            TryUpdateInputIcons();
        }

        if (gamepad.leftStick.left.isPressed || gamepad.dpad.left.isPressed)
        {
            MoveSlider(-1 * multiplier);
            return;
        }
        if (gamepad.leftStick.right.isPressed || gamepad.dpad.right.isPressed)
        {
            MoveSlider(1 * multiplier);
            return;
        }

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

    public void OnCancel()
    {
        SFXSystem.singleton.PlaySFX("UI_Cancel");
        TryUpdateInputIcons();

        InputHandler.SetGameState(InputHandler.singleton.previousGameState);
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
        sliderList[sliderIndex].OnDeselect();

        sliderIndex += moveAmount;

        if (sliderIndex < 0) sliderIndex = sliderList.Count - 1;
        if (sliderIndex >= sliderList.Count) sliderIndex = 0;

        // Selects new button
        sliderList[sliderIndex].OnSelect();


        LeanTween.moveY(cursorImage.gameObject, sliderList[sliderIndex].transform.position.y, InputHandler.CURSOR_MOVE_ANIM_TIME);
    }

    public void MoveSlider(int moveAmount)
    {
        lastTimeCursorMoved = Time.time;

        TryUpdateInputIcons();
        sliderList[sliderIndex].MoveSliderCursor(moveAmount);
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
        canvasOBJ.SetActive(true);
        TryUpdateInputIcons();

        cursorAnimatingRight = true;
        cursorAnimTimePassed = 0;
    }

    public void CloseMenuScreen(GameState newGameState)
    {
        canvasOBJ.SetActive(false);
    }
}