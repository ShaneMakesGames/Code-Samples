using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int floor;
    public int level;
    public int waypoint;
    public int currency;

    public bool isFullscreen;
    public Vector2 currentResolution;

    public float musicVolume;
    public float sfxVolume;

    public bool tutorialCompleted;
    public bool reachedBossFight;
    public bool gameLaunched;
    public bool dataSaved;
    public string difficultyID;
    public bool storyStarted;

    public List<int> AbilityNums = new List<int>();
    public List<int> EndlessAbilityNums = new List<int>();

    public bool[] floorReachedArray = new bool[5];
    public bool[] storyAbilityUnlocked = new bool[15];
    public bool[] endlessAbilityUnlocked = new bool[15];

    public PlayerData (DataManager dataManager)
    {
        floor = dataManager.floorNum;
        level = dataManager.levelNum;
        waypoint = dataManager.waypointNum;
        currency = dataManager.currency;

        isFullscreen = dataManager.isFullscreen;
        currentResolution = dataManager.currentResolution;

        musicVolume = dataManager.gameVolume;
        sfxVolume = dataManager.gameSFX;

        gameLaunched = dataManager.gameLaunched;
        dataSaved = dataManager.dataSaved;
        difficultyID = dataManager.DifficultyID;
        storyStarted = dataManager.storyStarted;

        AbilityNums = dataManager.abilityNums;

        floorReachedArray = dataManager.floorReachedArray;
        storyAbilityUnlocked = dataManager.storyAbilityUnlocked;
    }
}