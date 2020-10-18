using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpawnFolders : MonoBehaviour
{
    #region Variables
    public LevelData LevelSO;

    [Header("Files Being Used")]
    public List<GameObject> prefabList = new List<GameObject>();
    private int prefabIndex;
    public GameObject FoldersOBJ;
    public GameObject MessengerOBJ;

    [Header("Pop Ups")]
    public bool spawnPopUps;
    public GameObject prefabPopUp;
    public int prefabPopUpIndex;

    [Header("Cameras")]
    public Camera cam;
    public Camera popCam;

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
    private int random;

    public float PopTimer = 14f;
    public float PopCountdown;

    [Header("Animators and Audio")]
    public Animator bg;
    public AudioSource sound;
    public AudioSource popSound;

    [Header("Rare Chance")]
    public int defaultRareChance;
    public int curentRareChance;
    public int rareIncrement;

    [Header("Misc.")]
    public MusicScript ms;
    private GameObject newFolder;

    public GameObject abilityCanvas;
    private DataManager dataManager;
    #endregion

    void Start()
    {
        StartCoroutine(FadeIn.instance.FadeTransition());
        Money.instance.CurrencyChanged();

        cam = ReferenceManager.instance.spawnCam.GetComponent<Camera>();
        popCam = ReferenceManager.instance.popUpCam.GetComponent<Camera>();
        sound = GetComponent<AudioSource>();
        dataManager = DataManager.Instance;

        // Gets current LevelSO from DataManager
        LevelSO = dataManager.CurrentLevelSO;

        // Countdown for Spawning malware and Pop Ups and countdown for next wave
        countdown = countTime;
        PopCountdown = PopTimer;
        changeWaveCount = changeWaveTime;
        curentRareChance = defaultRareChance;

        foreach (GameObject OBJ in LevelSO.waveDataList[0].folders)
        {
            prefabList.Add(OBJ);
        }
        prefabIndex = Random.Range(0, prefabList.Count);
        prefabPopUpIndex = 0;

        waveCount = 0;
        // Plays Wave Start Animation
        sound.Play();
        bg.SetTrigger("NextWave");
    }

    void Update()
    {
        if (waveActive)
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
                waveCanEnd = true;
                countTime += .5f;
                PopTimer += 1f;
                numSlows++;
                changeWaveCount = changeWaveTime;
            }

            // Ends the current wave so no more files will be spawned
            if (waveCanEnd && FolderManager.instance.allFolders.Count == 0)
            {
                waveActive = false;
                StartCoroutine(CheckAndClearFoldersCoroutine());
            }
        }
    }

    public void NextWave()
    {
        ResetRareChance();
        // Resets the slows
        for (int i = 0; i != numSlows; i++)
        {
            countTime -= .5f;
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
            countTime -= .1f;
            prefabList.Clear();
            foreach (GameObject OBJ in LevelSO.waveDataList[waveCount].folders)
            {
                prefabList.Add(OBJ);
            }
            if (waveCount == 3)
            {
                countTime = 1f;
            }
            changeWaveCount = changeWaveTime;
            countdown = countTime;
        }
        else
        {
            // Returns player to the Overworld
            StartCoroutine(OverworldTransitionCoroutine());
        }
    }

    public IEnumerator CheckAndClearFoldersCoroutine()
    {
        if (FolderManager.instance.allFolders.Count > 0)
        {
            foreach (GameObject folder in FolderManager.instance.allFolders)
            {
                Destroy(folder);
            }
            yield return new WaitForEndOfFrame();
            DangerBar.instance.FolderCountChanged();
        }

    }

    IEnumerator OverworldTransitionCoroutine()
    {
        MessengerOBJ.SetActive(false);
        dataManager.levelNum++;
        StartCoroutine(FadeIn.instance.Fade());
        dataManager.SaveData();
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("Overworld");
    }

    public void CallSpawn()
    {
        if (LevelSO.waveDataList[waveCount].rareFolders.Length > 0)
        {
            int random = Random.Range(1, 11);
            if (random > curentRareChance)
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
    /// Increase the chance of a rare folder spawning
    /// </summary>
    public void IncrementRareChance()
    {
        rareIncrement++;
        if (rareIncrement > 2)
        {
            curentRareChance++;
            rareIncrement = 1;
        }
    }

    /// <summary>
    /// Resets the chance of a rare folder spawning
    /// </summary>
    public void ResetRareChance()
    {
        curentRareChance = defaultRareChance;
        rareIncrement = 1;
    }

    public void Spawn()
    {
        prefabIndex = Random.Range(0, prefabList.Count);
        random = Random.Range(1, 5);
        Vector3 spawnPosition = GetRandomSpawnPosition();
        newFolder = Instantiate(prefabList[prefabIndex], spawnPosition, Quaternion.identity);
        
        AddFolderToManager(newFolder);
        CheckForAbilities(newFolder);

        newFolder = null;
        DangerBar.instance.FolderCountChanged();
    }

    public void RareSpawn()
    {
        int spawnNum = Random.Range(0, LevelSO.waveDataList[waveCount].rareFolders.Length);
        Vector3 spawnPosition = GetRandomSpawnPosition();
        newFolder = Instantiate(LevelSO.waveDataList[waveCount].rareFolders[spawnNum], spawnPosition, Quaternion.identity);

        AddFolderToManager(newFolder);
        CheckForAbilities(newFolder);

        newFolder = null;
        DangerBar.instance.FolderCountChanged();
    }

    /// <summary>
    /// Returns a random spawn location for a folder
    /// </summary>
    /// <returns></returns>
    private Vector3 GetRandomSpawnPosition()
    {
        random = Random.Range(1, 5);
        if (random == 1)
        {
            float spawnY = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(Screen.height, 0)).y);
            float spawnX = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);
            return spawnPosition;
        }
        else if (random == 2)
        {
            float spawnY = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
            float spawnX = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);
            return spawnPosition;
        }
        else if (random == 3)
        {
            float spawnY = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(Screen.height, 0)).y);
            float spawnX = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(0, Screen.width)).x);
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);
            return spawnPosition;
        }
        else
        {
            float spawnY = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
            float spawnX = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(0, Screen.width)).x);
            Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);
            return spawnPosition;
        }
    }

    /// <summary>
    /// Adds folder to a list of all folders and another list depending on it's tag
    /// </summary>
    /// <param name="obj"></param>
    private void AddFolderToManager(GameObject obj)
    {
        // Adds the spawned folder to an array in FolderManager
        obj.transform.parent = FoldersOBJ.transform;
        FolderManager.instance.allFolders.Add(obj);
        if (obj.CompareTag("Malware"))
        {
            FolderManager.instance.malwareFolders.Add(obj);
        }
        else if (obj.CompareTag("Personal"))
        {
            FolderManager.instance.personalFolders.Add(obj);
        }
    }

    /// <summary>
    /// If an ability is active, give the folder appropriate helper script
    /// </summary>
    /// <param name="obj"></param>
    private void CheckForAbilities(GameObject obj)
    {
        if (AbilityManager.Instance.AbilityStatus.ContainsKey("Marquee Ability"))
        {
            newFolder.AddComponent<MarqueeAid>();
        }
        if (AbilityManager.Instance.AbilityStatus.ContainsKey("Magnet Ability"))
        {
            newFolder.AddComponent<MagnetAid>();
        }
        if (AbilityManager.Instance.AbilityStatus.ContainsKey("Binary Sort Ability"))
        {
            newFolder.AddComponent<BinarySortAid>();
        }
        if (AbilityManager.Instance.AbilityStatus.ContainsKey("Proximity Sort Ability"))
        {
            newFolder.AddComponent<ProximitySortAid>();

        }
        if (AbilityManager.Instance.AbilityStatus.ContainsKey("Filer Eraser Ability"))
        {
            newFolder.AddComponent<FileEraserAid>();
        }
    }

    public void SpawnPop()
    {
        popSound.Play();
        if (prefabPopUpIndex > 4)
        {
            prefabPopUpIndex = 0;
        }
        random = Random.Range(1, 5);

        if (random == 1)
        {
            float spawnY = UnityEngine.Random.Range(0, popCam.ScreenToWorldPoint(new Vector2(Screen.height, 0)).y);
            float spawnX = UnityEngine.Random.Range(0, popCam.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);
            Vector2 spawnPosition = new Vector2(spawnX, spawnY);
            GameObject pop = Instantiate(prefabPopUp, spawnPosition, Quaternion.identity);
            pop.GetComponent<PopUp>().AssignAnimation(prefabPopUpIndex, false);
        }
        if (random == 2)
        {
            float spawnY = UnityEngine.Random.Range(0, popCam.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
            float spawnX = UnityEngine.Random.Range(0, popCam.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);
            Vector2 spawnPosition = new Vector2(spawnX, spawnY);
            GameObject pop = Instantiate(prefabPopUp, spawnPosition, Quaternion.identity);
            pop.GetComponent<PopUp>().AssignAnimation(prefabPopUpIndex, false);
        }
        if (random == 3)
        {
            float spawnY = UnityEngine.Random.Range(0, popCam.ScreenToWorldPoint(new Vector2(Screen.height, 0)).y);
            float spawnX = UnityEngine.Random.Range(0, popCam.ScreenToWorldPoint(new Vector2(0, Screen.width)).x);
            Vector2 spawnPosition = new Vector2(spawnX, spawnY);
            GameObject pop = Instantiate(prefabPopUp, spawnPosition, Quaternion.identity);
            pop.GetComponent<PopUp>().AssignAnimation(prefabPopUpIndex, false);
        }
        if (random == 4)
        {
            float spawnY = UnityEngine.Random.Range(0, popCam.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
            float spawnX = UnityEngine.Random.Range(0, popCam.ScreenToWorldPoint(new Vector2(0, Screen.width)).x);
            Vector2 spawnPosition = new Vector2(spawnX, spawnY);
            GameObject pop = Instantiate(prefabPopUp, spawnPosition, Quaternion.identity);
            pop.GetComponent<PopUp>().AssignAnimation(prefabPopUpIndex, false);
        }
        prefabPopUpIndex++;
    }
}