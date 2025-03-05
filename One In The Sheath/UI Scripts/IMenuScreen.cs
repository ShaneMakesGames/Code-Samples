using UnityEngine;

public interface IMenuScreen
{
    void OpenMenuScreen();

    void CloseMenuScreen(GameState newGameState);
}