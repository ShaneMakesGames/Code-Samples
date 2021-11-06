using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    #region Variables

    [Header("Bools")]
    public bool gameLaunched;
    public bool dataSaved;

    [Header("Ints")]
    //Stores the ID of the floor the player is currently on
    public int floorNum;
    //Stores the ID of the level the player has most recently beaten
    public int levelNum;
    // Used to determine where player should spawn in the Overworld
    public int waypointNum;
    //Stores the current currency the player has accumulated
    public int currency;

    [Header("Video")]
    public bool isFullscreen;
    public Vector2 currentResolution;

    [Header("Audio")]
    public float gameVolume;
    public float gameSFX;

    [Header("Post Processing")]
    public bool VFXActive = true;

    [Header("Level SO")]
    public LevelData CurrentLevelSO;

    [Header("Abilities")]
    public List<int> abilityNums = new List<int>();
    public bool[] floorReachedArray = new bool[5];
    public bool[] storyAbilityUnlocked = new bool[15];

    public enum CurrentMode
    {
        StoryMode,
        TimeTrials,
        EndlessMode
    }

    public CurrentMode currentMode;

    public string DifficultyID;
    public bool storyStarted;

    [Header("Endless Mode")]
    public int highestCompletedWaveNum;
    public List<int> endlessAbilityNums = new List<int>();
    public bool[] endlessAbilityUnlocked = new bool[15];

    [Header("Moving Folder Speeds")]
    public bool DefaultFolderSpeeds;
    public TextAsset MovingFolderData;
    public List<MovingFolderStruct> MovingFolderStructList = new List<MovingFolderStruct>();

    [Header("Time Trials")]
    public float[] timeTrialResults = new float[20];
    public string[] timeTrialStars = new string[20];

    #endregion

    #region Singleton
    public static DataManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DefaultFolderSpeeds = true;
            GetMovingFolderSpeeds(MovingFolderData);
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Save and Load

    public void SaveData()
    {
        dataSaved = true;
        PlayerData data = new PlayerData(this);
        var json = JsonUtility.ToJson(data);
        byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(json);
        SaveSystem.SaveData("save_data", plainTextBytes);
    }

    public void LoadData()
    {
        byte[] saveData = SaveSystem.LoadData("save_data");
        if (saveData != null)
        {
            string json = System.Text.Encoding.UTF8.GetString(saveData); // Converts the byte[] into a string
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            floorNum = data.floor;
            levelNum = data.level;
            waypointNum = data.waypoint;
            currency = data.currency;

            isFullscreen = data.isFullscreen;
            currentResolution = data.currentResolution;

            gameVolume = data.musicVolume;
            gameSFX = data.sfxVolume;

            DifficultyID = data.difficultyID;
            storyStarted = data.storyStarted;

            floorReachedArray = data.floorReachedArray;
            storyAbilityUnlocked = data.storyAbilityUnlocked;

            abilityNums = data.AbilityNums;
        }
    }

    public void AttemptAddTimeTrialResult(TimeTrialSOData timeTrialSO, int trialIndex, float timeToComplete)
    {
        if (timeTrialResults[trialIndex] == 0 || timeToComplete < timeTrialResults[trialIndex]) // If new time is faster, replace old time
        {
            timeTrialResults[trialIndex] = timeToComplete;
            bool wasAlreadyGold = false;
            if (timeTrialStars[trialIndex] == "Gold") wasAlreadyGold = true;
            timeTrialStars[trialIndex] = GetAppropriateStar(timeTrialSO, timeToComplete);
            if (!wasAlreadyGold && timeTrialStars[trialIndex] == "Gold") AchievementManager.Instance.CallIncrementFromAchievementID("Fastest_Janitor");
            SaveTimeTrialData();
        }
    }

    public string GetAppropriateStar(TimeTrialSOData timeTrialSO, float timeToComplete)
    {
        if (timeToComplete < timeTrialSO.silverTime)
        {
            if (timeToComplete < timeTrialSO.goldTime)
            {
                return "Gold";
            }
            else
            {
                return "Silver";
            }
        }
        else
        {
            return "Bronze";
        }
    }

    public void SaveTimeTrialData()
    {
        TimeTrialData timeTrialData = new TimeTrialData(this);

        var json = JsonUtility.ToJson(timeTrialData);
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(json);

        SaveSystem.SaveData("time_trial_data", plainTextBytes);
    }

    public void LoadTimeTrialData()
    {
        byte[] trialData = SaveSystem.LoadData("time_trial_data");
        if (trialData != null)
        {
            string json = System.Text.Encoding.UTF8.GetString(trialData); // Converts the byte[] into a string
            TimeTrialData data = JsonUtility.FromJson<TimeTrialData>(json);
            timeTrialResults = data.timeTrialResults;
            timeTrialStars = data.timeTrialStars;
        }
    }

    public void SaveEndlessData()
    {
        EndlessSaveData endlessSaveData = new EndlessSaveData(this);

        var json = JsonUtility.ToJson(endlessSaveData);
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(json);

        SaveSystem.SaveData("endless_data", plainTextBytes);
    }

    public void LoadEndlessData()
    {
        byte[] endlessData  = SaveSystem.LoadData("endless_data");
        if (endlessData != null)
        {
            string json = System.Text.Encoding.UTF8.GetString(endlessData); // Converts the byte[] into a string
            EndlessSaveData data = JsonUtility.FromJson<EndlessSaveData>(json);
            highestCompletedWaveNum = data.highestCompletedWaveNum;
            endlessAbilityNums = data.endlessAbilityNums;
            endlessAbilityUnlocked = data.endlessAbilityUnlocked;
        }
    }

    public void DeleteData()
    {
        SaveSystem.DeleteFile("save_data");
        ResetData();
    }

    public void ResetData()
    {
        floorNum = 1;
        levelNum = 1;
        waypointNum = 0;
        currency = 0;

        gameLaunched = true;
        dataSaved = false;

        floorReachedArray = new bool[5];
        storyAbilityUnlocked = new bool[15];

        abilityNums[0] = -1;
        abilityNums[1] = -1;
        abilityNums[2] = -1;

        CurrentLevelSO = null;

        storyStarted = false;
    }

    public void GetMovingFolderSpeeds(TextAsset _MovingFolderData)
    {
        string[] data = _MovingFolderData.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)
        {
            string[] row = data[i].Split(new char[] { '\t' });

            MovingFolderStruct movingFolderStruct = new MovingFolderStruct();
            float.TryParse(row[1], out movingFolderStruct.defaultSpeed);
            movingFolderStruct.currentSpeed = movingFolderStruct.defaultSpeed;
            float.TryParse(row[2], out movingFolderStruct.maxSpeed);
            MovingFolderStructList.Add(movingFolderStruct);
        }
    }

    #endregion
}