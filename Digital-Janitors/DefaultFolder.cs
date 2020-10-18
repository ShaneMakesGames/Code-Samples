using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DefaultFolder : FolderClass
{
    public void OnMouseOver()
    {
        // Hovering over the folder
        if (!PopUp.popUpActive && !gameCursor.holdingFolder && !PauseMenu.isPaused)
        {
            MouseOver = true;
            isMouseOver = true;
            gameCursor.UpdateSprite(GameCursor.CursorState.Selected);
            // Dragging the folder
            if (Input.GetMouseButtonDown(0) && !isSelected)
            {
                isSelected = true;
                gameCursor.selectedFolder = folderClass;
                gameCursor.holdingFolder = true;
                StartCoroutine(FolderSelectedCoroutine());
            }
        }
    }
}
