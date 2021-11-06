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
            //TryUnlockAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

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

    private void InitializeAchievementIDs()
    {
        for (int i = 0; i < AchievementDataObjectList.Count; i++)
        {
            AchievementIDs.Add(AchievementDataObjectList[i].AchievementID);
        }
    }

    public void SaveAchievementData()
    {
        AchievementData achievementData = new AchievementData(this);
        var json = JsonUtility.ToJson(achievementData);
        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(json);
        SaveSystem.SaveData("achievement_data", plainTextBytes);
    }

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

    private void CheckAchievementProgress(AchievementDataObject achievement)
    {
        if (achievement.CurrentAmount >= achievement.AmountRequired)
        {
            UnlockAchievement(achievement);   
        }
    }

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

    public void UnlockAchievement(AchievementDataObject achievement)
    {
        achievement.Unlocked = true;
        if (SteamManager.Instance != null && SteamManager.Instance.SteamInitialized)
        {
            SteamManager.Instance.UnlockSteamAchievement(achievement.AchievementID);
        }
        SaveAchievementData();
    }

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

    public AchievementDataObject GetAchievementObjectByID(string ID)
    {
        if (AchievementIDs.Contains(ID))
        {
            int index = AchievementIDs.IndexOf(ID);
            return AchievementDataObjectList[index];
        }
        else return null;
    }

    private void IncrementAchievementProgress(AchievementDataObject achievement)
    {
        if (!achievement.Unlocked)
        {
            achievement.CurrentAmount++;
            CheckAchievementProgress(achievement);
        }
    }

    public void CallIncrementFromAchievementID(string id)
    {
        if (AchievementIDs.Contains(id))
        {
            IncrementAchievementProgress(AchievementDataObjectList[AchievementIDs.IndexOf(id)]);
        }
    }
}