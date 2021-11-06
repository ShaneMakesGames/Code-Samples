using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public bool SteamInitialized;

    public List<Achievement> SteamAchievementList = new List<Achievement>();
    public List<string> AchievementIDs = new List<string>();

    #region Singleton
    public static SteamManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        try
        {
            SteamClient.Init(1389850);
            AddAllAchievements();
        }
        catch (System.Exception)
        {
            SteamInitialized = false;
            //Debug.Log("Couldn't initialize Steam Client");
        }
    }
    #endregion

    void Update()
    {
        if (SteamInitialized)
        {
            SteamClient.RunCallbacks();
        }
    }

    private void AddAllAchievements()
    {
        foreach (Achievement ach in SteamUserStats.Achievements)
        {
            SteamAchievementList.Add(ach);
            AchievementIDs.Add(ach.Identifier);
        }
    }

    public void UnlockSteamAchievement(string achievementID)
    {
        if (SteamAchievementExists(achievementID))
        {
            Achievement achievement = GetSteamAchievementByID(achievementID);
            if (!achievement.State)
            {
                achievement.Trigger();
            }
        }
        else
        {
            //Debug.Log("Achievement Does Not Exists");
        }
    }

    public bool SteamAchievementExists(string ID)
    {
        if (AchievementIDs.Contains(ID))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public Achievement GetSteamAchievementByID(string ID)
    {
        if (AchievementIDs.Contains(ID))
        {
            int index = AchievementIDs.IndexOf(ID);
            return SteamAchievementList[index];
        }
        else return new Achievement();
    }

    private void OnDestroy()
    {
        if (SteamInitialized)
        {
            SteamClient.Shutdown();
        }
    }
}