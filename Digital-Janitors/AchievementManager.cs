using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks.Data;

public class AchievementManager : MonoBehaviour
{
    public List<AchievementDataObject> AchievementDataObjectList = new List<AchievementDataObject>();
    public List<string> AchievementIDs = new List<string>();

    public TextAsset achievementTextAsset;

    #region Singleton
    public static AchievementManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            LoadAchievementData();
            InitializeAchievementIDs();
            TryUnlockAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    // Parses the Text File & populates a list of AchievementDataObjects 
    public void GetAchievementsFromTextFile(TextAsset achievementData)
    {
        string[] data = achievementData.text.Split(new char[] { '\n' });
        for (int i = 1; i < data.Length; i++)
        {
            string[] row = data[i].Split(new char[] { '\t' });

            AchievementDataObject achievementDataObject = new AchievementDataObject();
            achievementDataObject.Name = row[0];
            achievementDataObject.AchievementID = row[1];
            bool.TryParse(row[2], out achievementDataObject.Unlocked);
            bool.TryParse(row[3], out achievementDataObject.CheckInt);
            int.TryParse(row[4], out achievementDataObject.CurrentAmount);
            int.TryParse(row[5], out achievementDataObject.AmountRequired);
            AchievementDataObjectList.Add(achievementDataObject);
        }
    }

    // Populates a list of strings which is used to access achievements from any script
    private void InitializeAchievementIDs()
    {
        for (int i = 0; i < AchievementDataObjectList.Count; i++)
        {
            AchievementIDs.Add(AchievementDataObjectList[i].AchievementID);
        }
    }

    // Saves the achievement data into storage
    public void SaveAchievementData()
    {
        AchievementData achievementData = new AchievementData(this);
        var json = JsonUtility.ToJson(achievementData);
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(json);
        SaveSystem.SaveData("achievement_data", plainTextBytes);
    }

    // Loads the achievement data from storage
    public void LoadAchievementData()
    {
        byte[] achievementData = SaveSystem.LoadData("achievement_data");
        if (achievementData != null)
        {
            string json = System.Text.Encoding.UTF8.GetString(achievementData); // Converts the byte[] into a string
            AchievementData data = JsonUtility.FromJson<AchievementData>(json);
            AchievementDataObjectList = data.achievementDataObjects;
        }
        else
        {
            GetAchievementsFromTextFile(achievementTextAsset);
        }
    }

    // If the required amount has been met, unlock the achievement
    private void CheckAchievementProgress(AchievementDataObject achievement)
    {
        if (achievement.CurrentAmount >= achievement.AmountRequired)
        {
            UnlockAchievement(achievement);   
        }
    }

    // Whether the achievements has already been unlocked or not
    public bool IsAchievementUnlocked(string id)
    {
        if (AchievementIDs.Contains(id))
        {
            return AchievementDataObjectList[AchievementIDs.IndexOf(id)].Unlocked;
        }
        else
        {
            //Debug.Log("Achievement " + id + " Does Not Exist");
            return false;
        }
    }

    // Sets the achievement as unlocked and triggers a Steam achievement unlock if SteamManager is active 
    public void UnlockAchievement(AchievementDataObject achievement)
    {
        achievement.Unlocked = true;
        if (SteamManager.Instance != null && SteamManager.Instance.SteamInitialized)
        {
            SteamManager.Instance.UnlockSteamAchievement(achievement.AchievementID);
        }
        SaveAchievementData();
    }

    // Takes an achievementID and unlocks the associated achievement if it exists
    public void UnlockAchievementByID(string id)
    {
        if (AchievementIDs.Contains(id))
        {
            UnlockAchievement(AchievementDataObjectList[AchievementIDs.IndexOf(id)]);
        }
    }

    /// <summary>
    /// For achievements completed when offline on Steam
    /// </summary>
    void TryUnlockAchievements()
    {
        if (SteamManager.Instance != null && SteamManager.Instance.SteamInitialized)
        {
            for (int i = 0; i < AchievementDataObjectList.Count; i++)
            {
                if (AchievementDataObjectList[i].Unlocked) // If achievement should be unlocked
                {
                    SteamManager.Instance.UnlockSteamAchievement(AchievementDataObjectList[i].AchievementID);
                }
            }
        }
    }

    // Takes an achievementID and returns the associated AchievementDataObject
    public AchievementDataObject GetAchievementObjectByID(string ID)
    {
        if (AchievementIDs.Contains(ID))
        {
            int index = AchievementIDs.IndexOf(ID);
            return AchievementDataObjectList[index];
        }
        else return null;
    }

    // Increases the achievement's progress by a value of 1
    private void IncrementAchievementProgress(AchievementDataObject achievement)
    {
        if (!achievement.Unlocked)
        {
            achievement.CurrentAmount++;
            CheckAchievementProgress(achievement);
        }
    }

    // Takes an achievementID and Increases the achievement's progress by a value of 1
    public void CallIncrementFromAchievementID(string id)
    {
        if (AchievementIDs.Contains(id))
        {
            IncrementAchievementProgress(AchievementDataObjectList[AchievementIDs.IndexOf(id)]);
        }
    }
}
