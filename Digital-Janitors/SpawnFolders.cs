using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnFolders : SpawningClass
{
    #region Variables
    public LevelData LevelSO;
    public SpawnData SpawnSO;

    [Header("Files Being Used")]
    public List<GameObject> prefabList = new List<GameObject>();
    private int prefabIndex;
    public GameObject MessengerOBJ;

    [Header("Pop Ups")]
    public bool spawnPopUps;
    public GameObject prefabPopUp;
    public int prefabPopUpIndex;

    [Header("Wave Stats")]
    public bool waveActive = true;
    public bool waveCanEnd = false;
    public int numSlows;
    public int waveCount;

    [Header("Timers")]
    public float countdown;
    public float countTime = 1.5f;
    public float changeWaveCount;
    public float changeWaveTime = 1f;

    public float PopTimer = 14f;
    public float PopCountdown;

    [Header("Animators and Audio")]
    public Animator bg;
    public AudioSource music;
    public AudioSource sound;
    public AudioSource popSound;

    [Header("Rare Chance")]
    public int defaultRareChance;
    public int currentRareChance;
    public int rareIncrement;

    [Header("Misc.")]
    public MusicScript ms;
    private GameObject newFolder;
    private DataManager dataManager;
    #endregion

    void Start()
    {
        StartCoroutine(FadeIn.Instance.FadeTransition());
        Money.Instance.CurrencyChanged();

        sound = GetComponent<AudioSource>();
        dataManager = DataManager.Instance;

        LevelSO = dataManager.CurrentLevelSO; // Gets current LevelSO from DataManager
        
        if (dataManager.DifficultyID == "Normal") // Gets spawn rates based on current difficulty mode
        {
            SpawnSO = LevelSO.normalSpawnData;
        }
        else
        {
            SpawnSO = LevelSO.hardSpawnData;
        }

        music.clip = LevelSO.levelMusic;
        music.Play();


        // Countdowns for Spawning malware and Pop Ups and countdown for next wave
        countTime = SpawnSO.folderSpawnTime;
        changeWaveTime = SpawnSO.waveLength;
        countdown = countTime;
        PopTimer = SpawnSO.popUpSpawnTime;
        PopCountdown = PopTimer;
        changeWaveCount = changeWaveTime;
        currentRareChance = defaultRareChance;

        foreach (GameObject OBJ in LevelSO.waveDataList[0].folders)
        {
            prefabList.Add(OBJ);
        }
        prefabIndex = Random.Range(0, prefabList.Count);
        prefabPopUpIndex = 0;

        waveCount = 0;
        
        sound.Play();
        bg.SetTrigger("NextWave"); // Plays Wave Start Animation

        if (SceneManager.GetActiveScene().name != "NewAdventureBoss")
        {
            ReferenceManager.Instance.recycle.IncorrectSortFunction += IncorrectSort;
            ReferenceManager.Instance.secureDrive.IncorrectSortFunction += IncorrectSort;
        }

        StartCoroutine(WaveCoroutine());

        if (!AchievementManager.Instance.IsAchievementUnlocked("Overdrive"))
        {
            StartCoroutine(OverdriveCheckCoroutine());
        }
    }

    private bool overdriveSuccess = true;
    private IEnumerator OverdriveCheckCoroutine()
    {
        float timePassed = 0;
        while (overdriveSuccess)
        {
            if (waveActive)
            {
                if (timePassed >= 90)
                {
                    overdriveSuccess = false; // Achievement Failed
                }
                timePassed += Time.deltaTime;
            }
            yield return null;
        }
    }

    private int numIncorrectSorts;
    private int numTilSpawn = 3;
    private int numIncorrectSpawns;
    private void IncorrectSort()
    {
        numIncorrectSorts++;

        if (numIncorrectSorts >= numTilSpawn)
        {
            Spawn();
            numIncorrectSorts = 0;
            numIncorrectSpawns++;

            if (numIncorrectSpawns > 3 && numTilSpawn > 1)
            {
                numTilSpawn--;
                numIncorrectSpawns = 0;
            }
        }
    }

    private IEnumerator WaveCoroutine()
    {
        while (waveActive)
        {
            // Spawns Folders
            countdown -= Time.deltaTime;
            if (countdown <= 0)
            {
                CallSpawn();
                countdown = countTime;
            }
            // Spawns Pop Ups
            if (spawnPopUps)
            {
                PopCountdown -= Time.deltaTime;
                if (PopCountdown <= 0)
                {
                    SpawnPop();
                    PopCountdown = PopTimer;
                }
            }
            // Allows the current wave to be ended and slows the spawning of files and popUps
            changeWaveCount -= Time.deltaTime;
            if (changeWaveCount <= 0)
            {
                countTime += SpawnSO.folderSpawnSlowdown;
                PopTimer += 1f;
                numSlows++;
                waveCanEnd = true;
                changeWaveCount = changeWaveTime;
            }
            // Ends the current wave so no more files will be spawned
            if (waveCanEnd && FolderManager.Instance.allFolders.Count == 0)
            {
                waveActive = false;
                StartCoroutine(CheckAndClearFoldersCoroutine());
            }
            yield return null;
        }
    }

    public void DevNextWave()
    {
        waveActive = false;
        StartCoroutine(CheckAndClearFoldersCoroutine());
    }

    /// <summary>
    /// If there are more waves in the level, otherwise return to Overworld
    /// </summary>
    public void NextWave()
    {
        ResetRareChance();
        // Resets the slows
        for (int i = 0; i != numSlows; i++)
        {
            countTime -= SpawnSO.folderSpawnSlowdown;
            PopTimer -= 1f;
        }
        numSlows = 0;

        // If there are more waves in the level
        if (waveCount != LevelSO.waveDataList.Count - 1)
        {
            waveCanEnd = false;
            waveCount++;
            waveActive = true;
            sound.Play();
            bg.SetTrigger("NextWave");
            // Wave lasts longer and Folders spawn quicker
            changeWaveTime += 2f;
            countTime -= .05f;
            prefabList.Clear();
            foreach (GameObject OBJ in LevelSO.waveDataList[waveCount].folders)
            {
                prefabList.Add(OBJ);
            }
            changeWaveCount = changeWaveTime;
            countdown = countTime;

            StartCoroutine(WaveCoroutine());
        }
        else
        {
            // Returns player to the Overworld
            StartCoroutine(OverworldTransitionCoroutine());
        }
    }

    /// <summary>
    /// Clears any folders that are still in play
    /// </summary>
    /// <returns></returns>
    public IEnumerator CheckAndClearFoldersCoroutine()
    {
        if (FolderManager.Instance.allFolders.Count > 0)
        {
            foreach (FolderClass folderClass in FolderManager.Instance.allFolders)
            {
                Destroy(folderClass.gameObject);
            }
            yield return new WaitForEndOfFrame();
            DangerBar.Instance.FolderCountChanged();
        }
        if (FolderManager.Instance.NonFolderObjects.Count > 0)
        {
            foreach (GameObject _gameObject in FolderManager.Instance.NonFolderObjects)
            {
                Destroy(_gameObject);
            }
        }
    }

    /// <summary>
    /// Ends the current level and returns player to the Overworld
    /// </summary>
    /// <returns></returns>
    IEnumerator OverworldTransitionCoroutine()
    {
        Time.timeScale = 1;
        MessengerOBJ.SetActive(false);
        dataManager.levelNum++;

        if (!AchievementManager.Instance.IsAchievementUnlocked("First_Time"))
        {
            AchievementManager.Instance.UnlockAchievementByID("First_Time");
        }

        if (!AchievementManager.Instance.IsAchievementUnlocked("Overdrive"))
        {
            if (overdriveSuccess)
            {
                AchievementManager.Instance.UnlockAchievementByID("Overdrive");
            }
        }

        AchievementManager.Instance.SaveAchievementData();
        dataManager.SaveData();
        yield return new WaitForSeconds(1);
        SceneSwitchManager.Instance.TriggerSceneChange("Overworld");
    }

    /// <summary>
    /// Decides whether a rare or normal folder will be spawned
    /// </summary>
    public void CallSpawn()
    {
        if (LevelSO.waveDataList[waveCount].rareFolders.Length > 0)
        {
            int random = Random.Range(1, 11);
            if (random > currentRareChance)
            {
                Spawn();
                IncrementRareChance();
            }
            else
            {
                RareSpawn();
                ResetRareChance();
            }
        }
        else
        {
            Spawn();
        }
    }

    /// <summary>
    /// Increases the chance of a rare folder spawning
    /// </summary>
    public void IncrementRareChance()
    {
        rareIncrement++;
        if (rareIncrement > 2)
        {
            currentRareChance++;
            rareIncrement = 1;
        }
    }

    /// <summary>
    /// Resets the chance of a rare folder spawning
    /// </summary>
    public void ResetRareChance()
    {
        currentRareChance = defaultRareChance;
        rareIncrement = 1;
    }

    public void Spawn() // Spawns a folder and adds it to the Folder Manager
    {
        prefabIndex = Random.Range(0, prefabList.Count);
        Vector3 spawnPosition = GetRandomSpawnPosition();
        newFolder = Instantiate(prefabList[prefabIndex], spawnPosition, Quaternion.identity, FolderHolder.transform);
        
        AddFolderToManager(newFolder);

        newFolder = null;
        DangerBar.Instance.FolderCountChanged();
    }

    public void RareSpawn() // Spawns a rare folder and adds it to the Folder Manager
    {
        int spawnNum = Random.Range(0, LevelSO.waveDataList[waveCount].rareFolders.Length);
        Vector3 spawnPosition = GetRandomSpawnPosition();
        newFolder = Instantiate(LevelSO.waveDataList[waveCount].rareFolders[spawnNum], spawnPosition, Quaternion.identity, FolderHolder.transform);

        AddFolderToManager(newFolder);

        newFolder = null;
        DangerBar.Instance.FolderCountChanged();
    }

    public void SpawnPop() // Spawns a Pop Up
    {
        popSound.Play();
        if (prefabPopUpIndex > 4)
        {
            prefabPopUpIndex = 0;
        }
        Vector2 spawnPosition = GetRandomPopUpSpawnPosition();
        GameObject pop = Instantiate(prefabPopUp, spawnPosition, Quaternion.identity);
        pop.GetComponent<PopUp>().AssignAnimation(prefabPopUpIndex, false);
        FolderManager.Instance.NonFolderObjects.Add(pop);
        prefabPopUpIndex++;
    }

    private void OnDestroy()
    {
        if (SceneManager.GetActiveScene().name != "NewAdventureBoss")
        {
            ReferenceManager.Instance.recycle.IncorrectSortFunction -= IncorrectSort;
            ReferenceManager.Instance.secureDrive.IncorrectSortFunction -= IncorrectSort;
        }
    }
}