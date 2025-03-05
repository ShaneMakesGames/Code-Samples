using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SFXSystem : MonoBehaviour
{
    #region Singleton

    public static SFXSystem singleton;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
            GetSFXFromResources();
        }
        else Destroy(this.gameObject);
    }

    #endregion

    public const string path = "Sound Effects";
    public Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();

    [Header("SFX Players")]
    public List<SFXPlayer> sfxPlayerPool;
    public List<SFXPlayer> activeSFXPlayers;
    public List<SFXPlayer> pausedSFXPlayers;

    /// <summary>
    /// Adds all SFX in "Resources/SFX" to sfxDict
    /// </summary>
    private void GetSFXFromResources()
    {
        AudioClip[] sfxArray = Resources.LoadAll<AudioClip>(path);
        if (sfxArray.Length == 0) return;

        for (int i = 0; i < sfxArray.Length; i++)
        {
            sfxDict.Add(sfxArray[i].name, sfxArray[i]);
        }
    }

    /// <summary>
    /// Gets SFXPlayer from pool and plays a SFX
    /// </summary>
    /// <param name="sfxID"></param>
    public void PlaySFX(string sfxID, bool isLoop = false, bool randomizePitch = false)
    {
        AudioClip sfx;
        if (!sfxDict.TryGetValue(sfxID, out sfx))
        {
            return;
        }

        // No available SFX players
        if (sfxPlayerPool.Count == 0) return;

        SFXPlayer sfxPlayer = sfxPlayerPool[0];
        if (isLoop) sfxPlayer.PlaySFXOnLoop(sfx, randomizePitch);
        else sfxPlayer.PlaySFX(sfx, randomizePitch);
    }

    public void PlayRandomSFX(List<string> sfxIDs, bool randomizePitch = false)
    {
        AudioClip sfx;
        int randIndex = Random.Range(0, sfxIDs.Count);
        if (!sfxDict.TryGetValue(sfxIDs[randIndex], out sfx))
        {
            return;
        }

        // No available SFX players
        if (sfxPlayerPool.Count == 0) return;

        sfxPlayerPool[0].PlaySFX(sfx, randomizePitch);
    }


    public void PauseAllSFX()
    {
        for (int i = 0; i < activeSFXPlayers.Count; i++)
        {
            activeSFXPlayers[i].PauseSFX();
            pausedSFXPlayers.Add(activeSFXPlayers[i]);
            activeSFXPlayers.Remove(activeSFXPlayers[i]);
        }
    }

    public void UnpauseAllSFX()
    {
        for (int i = 0; i < pausedSFXPlayers.Count; i++)
        {
            pausedSFXPlayers[i].UnpauseSFX();
            activeSFXPlayers.Add(pausedSFXPlayers[i]);
            pausedSFXPlayers.Remove(pausedSFXPlayers[i]);
        }
    }

    /// <summary>
    /// Stops all active SFXPlayers
    /// </summary>
    public void CleanUp()
    {
        List<SFXPlayer> sfxPlayers = activeSFXPlayers;
        if (activeSFXPlayers.Count == 0) return;

        for (int i = 0; i < sfxPlayers.Count; i++)
        {
            activeSFXPlayers[i].CleanUp();
        }
    }
}