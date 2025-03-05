using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public Camera gameCam;

    [Header("Spawning")]
    public int numSpawnersToUse;
    public int numEnemiesPerSpawner;

    public const int MIN_SPAWNERS_TO_USE = 3;
    public const int MAX_SPAWNERS_TO_USE = 6;
    public const int MIN_ENEMIES_PER_SPAWNER = 3;
    public const int MAX_ENEMIES_PER_SPAWNER = 5;

    public GameObject enemyPrefab;
    public Transform parentForSpawnedEnemies;
    public List<Transform> AllSpawnPointsList = new List<Transform>();

    [Header("Tokens")]
    public int numTokensToGive;
    public float tokenTimer;
    public List<Enemy> AttackingEnemiesList;
    public List<Enemy> NonAttackingEnemiesList;

    public Queue<Enemy> InactiveEnemyPool;

    public Dictionary<Enemy, EnemyPointerContainer> dictActiveEnemyPointerArrows = new Dictionary<Enemy, EnemyPointerContainer>();
    public List<EnemyPointerContainer> EnemyPointerArrowPool = new List<EnemyPointerContainer>();
    private List<Enemy> EnemiesToReturnToPool = new List<Enemy>();

    public const float ENEMY_POINTER_ANGLE_RANGE = 30f;
    public const float ENEMY_POINTER_FADE_TIME = 0.25f;

    [Header("Enemy Attack Speed")]
    public Vector2 MinAttackTimeRange;
    public Vector2 currentAttackTimeRange;
    public Vector2 MaxAttackTimeRange;

    [Header("Enemy Move Speed")]
    public Vector2 MinSpeedRange;
    public Vector2 currentSpeedRange;
    public Vector2 MaxSpeedRange;

    [Header("Enemy Move Acceleration")]
    public Vector2 MinAccelerationRange;
    public Vector2 currentAccelerationRange;
    public Vector2 MaxAccelerationRange;

    [Header("Enemy Stopping Distance")]
    public Vector2 MinStoppingDistanceRange;
    public Vector2 currentStoppingDistanceRange;
    public Vector2 MaxStoppingDistanceRange;

    #region Singleton

    public static EnemyManager singleton;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;

            InactiveEnemyPool = new Queue<Enemy>();
            for (int i = 0; i < 50; i++)
            {
                Enemy enemy = Instantiate(enemyPrefab, parentForSpawnedEnemies).GetComponent<Enemy>();
                enemy.gameObject.SetActive(false);
                InactiveEnemyPool.Enqueue(enemy);
            }
        }
        else Destroy(this.gameObject);
    }

    #endregion

    public void SpawnWaveOfEnemies()
    {
        int waveCount = GameManager.IncrementBattleStat(GameManager.WAVE_COUNT_STRING);

        TryAdjustEnemyStats(waveCount);

        int spawnersUsed = 0;
        List<Transform> spawnersToCheck = new List<Transform>(AllSpawnPointsList);

        while (spawnersUsed < numSpawnersToUse)
        {
            if (spawnersToCheck.Count == 0) // Checked all spawners
            {
                break;
            }

            Transform spawner = spawnersToCheck[Random.Range(0, spawnersToCheck.Count - 1)];
            Vector3 posToCheck = gameCam.WorldToViewportPoint(spawner.position);
            if (!PointVisibleInCamera(posToCheck))
            {
                SpawnEnemiesAtThisLocation(spawner);
                spawnersUsed++;
            }
            spawnersToCheck.Remove(spawner);
        }
    }

    public void TryAdjustEnemyStats(int waveCount)
    {
        // Adds an additional spawner every 3 rounds, to a max of 6
        if (waveCount % 3 == 0)
        {
            numSpawnersToUse++;
            if (numSpawnersToUse > MAX_SPAWNERS_TO_USE) numSpawnersToUse = MAX_SPAWNERS_TO_USE;

        }
        // Adds an additional enemy per spawner every 6 rounds, to a max of 5
        if (waveCount % 6 == 0)
        {
            numEnemiesPerSpawner++;
            if (numEnemiesPerSpawner > MAX_ENEMIES_PER_SPAWNER) numEnemiesPerSpawner = MAX_ENEMIES_PER_SPAWNER;
        }

        // Adjusts stats every other round
        if (waveCount % 2 != 0) return;

        currentAttackTimeRange.x = Mathf.Clamp(currentAttackTimeRange.x - 0.05f, MinAttackTimeRange.x, MaxAttackTimeRange.x);
        currentAttackTimeRange.y = Mathf.Clamp(currentAttackTimeRange.y - 0.15f, MinAttackTimeRange.y, MaxAttackTimeRange.y);

        currentSpeedRange.x = Mathf.Clamp(currentSpeedRange.x + 1f, MinSpeedRange.x, MaxSpeedRange.x);
        currentSpeedRange.y = Mathf.Clamp(currentSpeedRange.y + 1f, MinSpeedRange.y, MaxSpeedRange.y);

        currentAccelerationRange.x = Mathf.Clamp(currentAccelerationRange.x + 1f, MinAccelerationRange.x, MaxAccelerationRange.x);
        currentAccelerationRange.y = Mathf.Clamp(currentAccelerationRange.y + 1f, MinAccelerationRange.y, MaxAccelerationRange.y);

        currentStoppingDistanceRange.x = Mathf.Clamp(currentStoppingDistanceRange.x + 1f, MinStoppingDistanceRange.x, MaxStoppingDistanceRange.x);
        currentStoppingDistanceRange.y = Mathf.Clamp(currentStoppingDistanceRange.y + 1f, MinStoppingDistanceRange.y, MaxStoppingDistanceRange.y);
    }

    public void SpawnEnemiesAtThisLocation(Transform spawner)
    {
        for (int i = 0; i < numEnemiesPerSpawner; i++)
        {
            Vector3 offset = new Vector3(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            Enemy enemy = InactiveEnemyPool.Dequeue();
            enemy.transform.position = spawner.position + offset;
            enemy.gameObject.SetActive(true);
            enemy.SetupEnemy();
            enemy.lookAtTarget.target = gameCam.transform;
            NonAttackingEnemiesList.Add(enemy);
            enemy.player = GameManager.singleton.player.transform;
        }
    }

    public static bool PointVisibleInCamera(Vector3 pos)
    {
        if (pos.x < 0 || pos.x > 1) return false;
        if (pos.y < 0 || pos.y > 1) return false;
        if (pos.z < 0) return false;

        return true;
    }

    private void FixedUpdate()
    {
        if (!GameManager.singleton.battleActive || PauseUI.isGamePaused) return;

        if (AttackingEnemiesList.Count + NonAttackingEnemiesList.Count == 0) SpawnWaveOfEnemies();

        if (AttackingEnemiesList.Count < numTokensToGive)
        {
            TryGiveTokensToEnemies();
        }
    }

    // Checks for any Enemy Pointers that were not properly disabled after an enemy died
    private void LateUpdate()
    {
        if (PauseUI.isGamePaused) return;

        foreach (KeyValuePair<Enemy, EnemyPointerContainer> kvp in dictActiveEnemyPointerArrows)
        {
            if (kvp.Value.isFadingOut) continue;

            if (!kvp.Key.isActive || !kvp.Key.gameObject.activeSelf)
            {
                EnemiesToReturnToPool.Add(kvp.Key);
            }
        }

        if (EnemiesToReturnToPool.Count > 0)
        {
            for (int i = 0; i < EnemiesToReturnToPool.Count; i++)
            {
                DisableEnemyPointer(EnemiesToReturnToPool[i]);
            }

            EnemiesToReturnToPool.Clear();
        }
    }

    public void TryGiveTokensToEnemies()
    {
        List<Enemy> enemiesToCheck = new List<Enemy>(NonAttackingEnemiesList);

        while (AttackingEnemiesList.Count < numTokensToGive)
        {
            if (enemiesToCheck.Count == 0) // No more enemies to check
            {
                break;
            }

            Enemy enemy = enemiesToCheck[Random.Range(0, enemiesToCheck.Count - 1)];
            if (enemy != null && enemy.isActive)
            {
                Vector3 posToCheck = gameCam.WorldToViewportPoint(enemy.transform.position);
                if (PointVisibleInCamera(posToCheck) && !enemy.onAttackCooldown)
                {
                    enemy.GiveAttackToken();
                    AttackingEnemiesList.Add(enemy);
                    NonAttackingEnemiesList.Remove(enemy);
                }
            }
            enemiesToCheck.Remove(enemy);
        }
    }

    public void RemoveEnemyFromLists(Enemy enemy)
    {
        InactiveEnemyPool.Enqueue(enemy);

        AttackingEnemiesList.Remove(enemy);
        NonAttackingEnemiesList.Remove(enemy);
    }

    public bool TryRenderOffscreenEnemyArrow(Vector3 enemyPos, Enemy enemy, float enemyAngle, float distanceFromPlayer)
    {
        if (dictActiveEnemyPointerArrows.Count > 0)
        {
            // An existing arrow exists for this enemy
            if (dictActiveEnemyPointerArrows.ContainsKey(enemy))
            {
                EnemyPointerContainer existingPointer = dictActiveEnemyPointerArrows[enemy];
                existingPointer.transform.eulerAngles = new Vector3(0, 0, enemyAngle);
                existingPointer.SetCurrentAngle(enemyAngle);
                SetPointerVisualsBasedOnDistance(existingPointer, distanceFromPlayer);

                return true;
            }

            // Check that this isn't too close to another arrow currently being used
            foreach (KeyValuePair<Enemy, EnemyPointerContainer> kvp in dictActiveEnemyPointerArrows)
            {
                float existingAngleToCheck = kvp.Value.GetCurrentAngle();
                if (enemyAngle >= existingAngleToCheck - ENEMY_POINTER_ANGLE_RANGE && enemyAngle <= existingAngleToCheck + ENEMY_POINTER_ANGLE_RANGE)
                {
                    return false;
                }
            }
        }

        if (EnemyPointerArrowPool.Count <= 0) return false;
        
        // If this enemy isn't currently using an arrow, grab one
        EnemyPointerContainer newPointer = EnemyPointerArrowPool[0];
        newPointer.gameObject.SetActive(true);
        newPointer.transform.GetChild(0).localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, 11.5f / distanceFromPlayer);
        newPointer.myEnemy = enemy;

        EnemyPointerArrowPool.Remove(newPointer);
        dictActiveEnemyPointerArrows.Add(enemy, newPointer);

        newPointer.SetCurrentAngle(enemyAngle);
        StartCoroutine(FadePointerInCoroutine(newPointer, enemy, distanceFromPlayer));

        newPointer.transform.eulerAngles = new Vector3(0, 0, enemyAngle);
        return true;
    }

    public void DisableEnemyPointer(Enemy enemy)
    {
        if (dictActiveEnemyPointerArrows.ContainsKey(enemy))
        {
            EnemyPointerContainer existingPointer = dictActiveEnemyPointerArrows[enemy];
            existingPointer.myEnemy = null;
            existingPointer.isFadingIn = false;
            StartCoroutine(FadePointerOutCoroutine(existingPointer, enemy));
        }
    }

    private void SetPointerVisualsBasedOnDistance(EnemyPointerContainer pointer, float distanceFromPlayer)
    {
        pointer.myImage.transform.localScale = Vector3.Lerp(Vector3.one / 2f, Vector3.one * 1.5f, 11.5f / distanceFromPlayer);
        if (pointer.isFadingIn || pointer.isFadingOut) return;
        float colorFloat = Mathf.Lerp(0.55f, 0.85f, 11.5f / distanceFromPlayer);
        pointer.myImage.color = new Color(1, 1, 1, colorFloat);
    }

    private IEnumerator FadePointerInCoroutine(EnemyPointerContainer pointer, Enemy enemy, float distanceFromPlayer)
    {
        pointer.isFadingIn = true;
        pointer.myImage.color = new Color(1, 1, 1, 0);
        float colorFloat = Mathf.Lerp(0.55f, 0.85f, 11.5f / distanceFromPlayer);
        float timePassed = 0;
        while (timePassed < ENEMY_POINTER_FADE_TIME + 1f)
        {
            if (!pointer.isFadingIn) yield break;

            float lerpAmount = Mathf.Lerp(0, colorFloat, timePassed / ENEMY_POINTER_FADE_TIME);
            Color c = pointer.myImage.color;
            c.a = lerpAmount;
            pointer.myImage.color = c;

            timePassed += Time.deltaTime;
            yield return null;
        }

        pointer.myImage.color = new Color(1, 1, 1, colorFloat);
        pointer.isFadingIn = false;
    }


    private IEnumerator FadePointerOutCoroutine(EnemyPointerContainer pointer, Enemy enemy)
    {
        dictActiveEnemyPointerArrows.Remove(enemy);

        pointer.isFadingOut = true;
        float timePassed = 0;
        float startingAlpha = pointer.myImage.color.a;
        while (timePassed < ENEMY_POINTER_FADE_TIME)
        {
            float lerpAmount = Mathf.Lerp(startingAlpha, 0, timePassed / ENEMY_POINTER_FADE_TIME);
            Color c = pointer.myImage.color;
            c.a = lerpAmount;
            pointer.myImage.color = c;

            timePassed += Time.deltaTime;
            yield return null;
        }

        pointer.myImage.color = new Color(1, 1, 1, 0);
        pointer.transform.GetChild(0).localScale = Vector3.one;
        pointer.gameObject.SetActive(false);

        pointer.isFadingOut = false;
        pointer.myEnemy = null;

        if (!EnemyPointerArrowPool.Contains(pointer)) EnemyPointerArrowPool.Add(pointer);
    }

    public void GameOver()
    {
        numSpawnersToUse = MIN_SPAWNERS_TO_USE;
        numEnemiesPerSpawner = MIN_ENEMIES_PER_SPAWNER;

        currentAttackTimeRange = MaxAttackTimeRange;
        currentSpeedRange = MinSpeedRange;
        currentAccelerationRange = MinAccelerationRange;
        currentStoppingDistanceRange = MinStoppingDistanceRange;

        List<Enemy> enemiesToCleanUp = new List<Enemy>();
        enemiesToCleanUp.AddRange(AttackingEnemiesList);
        enemiesToCleanUp.AddRange(NonAttackingEnemiesList);

        for (int i = 0; i < enemiesToCleanUp.Count; i++)
        {
            Enemy enemy = enemiesToCleanUp[i];
            enemy.SetEnemyInactive();
        }
    }

    public void CleanUpLeftoverEnemies()
    {
        List<Enemy> enemiesToCleanUp = new List<Enemy>();
        enemiesToCleanUp.AddRange(AttackingEnemiesList);
        enemiesToCleanUp.AddRange(NonAttackingEnemiesList);

        for (int i = 0; i < enemiesToCleanUp.Count; i++)
        {
            Enemy enemy = enemiesToCleanUp[i];
            enemy.gameObject.SetActive(false);
            RemoveEnemyFromLists(enemy);
        }
    }
}