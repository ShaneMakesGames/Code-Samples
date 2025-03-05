using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Bools")]
    public bool battleActive;
    public bool isScreenShakeActive;
    public bool dashChargeAvailable;

    [Header("Camera")]
    public Camera gameCam;
    public Transform CameraTransform;
    public CameraFollow cameraFollow;

    public static Dictionary<string, int> dictBattleStats;
    public const string WAVE_COUNT_STRING = "wave_count";
    public const string HIGHEST_COMBO_STRING = "highest_combo";
    public const string KILLS_STRING = "kills";
    public const string PARRIES_STRING = "parries";

    public Player player;

    [Header("Score")]
    public int score;

    public const int SLASH_KILL_SCORE = 50;
    public const int DASH_KILL_SCORE = 100;
    public const int PARRY_SCORE = 25;

    [Header("Hitstop Scaling")]
    public static bool InHitstop;
    public float currentHitstopLength;

    public float currentHitstopScaling;
    private float lastTimeHitstopOccured;

    public const float AMOUNT_TO_SCALE_HITSTOP_BY = 0.45f;
    public const float MAX_HITSTOP_SCALING = 0.3f;
    public const float MIN_HITSTOP_TIME = 0.03f;

    public const float ATTACK_HITSTOP_DURATION = 0.2f;
    public const float DASH_HITSTOP_DURATION = 0.11f;

    #region Singleton

    public static GameManager singleton;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else Destroy(this.gameObject);
    }

    #endregion

    public void SetupBattle()
    {
        dictBattleStats = new Dictionary<string, int>();

        score = 0;
        SetDashCharge(true);
        BattleUI.singleton.DashChargeReset(true);

        EnemyManager.singleton.SpawnWaveOfEnemies();
        battleActive = true;

        player.transform.position = new Vector3(0, 0.5f, -50f);
        CameraTransform.position = new Vector3(0, 17.5f, -60f);
    }

    public void GameOver()
    {
        battleActive = false;
        InHitstop = false;

        BattleUI.singleton.GameOver();
        EnemyManager.singleton.GameOver();
        StartCoroutine(ChangeGameStateAfterDelayCoroutine(1.5f));
    }

    public static void SetBattleStat(string battleStatRef, int newAmount)
    {
        if (!dictBattleStats.ContainsKey(battleStatRef))
        {
            dictBattleStats.Add(battleStatRef, newAmount);
            return;
        }
        
        dictBattleStats[battleStatRef] = newAmount;
    }

    public static int IncrementBattleStat(string battleStatRef)
    {
        if (!dictBattleStats.TryGetValue(battleStatRef, out int currentAmount))
        {
            dictBattleStats.Add(battleStatRef, 1);
            return 1;
        }

        dictBattleStats[battleStatRef] = currentAmount + 1;
        return dictBattleStats[battleStatRef];
    }

    public static int GetBattleStat(string battleStatRef)
    {
        if (!dictBattleStats.TryGetValue(battleStatRef, out int currentAmount))
        {
            return 0;
        }

        return currentAmount;
    }

    private IEnumerator ChangeGameStateAfterDelayCoroutine(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        InputHandler.SetGameState(GameState.RESULTS_SCREEN, playFadeAnimation: true);
    }

    public void IncreaseScore(bool isParry, bool isDash)
    {
        int baseScore;
        if (isParry) baseScore = PARRY_SCORE;
        else
        {
            if (!isDash) baseScore = SLASH_KILL_SCORE;
            else baseScore = DASH_KILL_SCORE;
        }

        int multiplier = BattleUI.singleton.currentComboCount;
        if (multiplier <= 0) multiplier = 1;
        int calculatedScore = baseScore * multiplier;
        score += calculatedScore;
    }

    public void OnAttackConnected(bool dashAttack)
    {
        player.attackConnected = true;
        StartCoroutine(HitstopCoroutine(dashAttack));
        cameraFollow.TryScreenShake();
    }

    private IEnumerator HitstopCoroutine(bool dashAttack)
    {
        float timeSinceLastHitstop = Time.time - lastTimeHitstopOccured;
        if (timeSinceLastHitstop < 0.1f)
        {
            currentHitstopScaling -= AMOUNT_TO_SCALE_HITSTOP_BY;
            if (currentHitstopScaling < MAX_HITSTOP_SCALING) currentHitstopScaling = MAX_HITSTOP_SCALING;
        }
        else currentHitstopScaling = 1;

        float BaseHitstopDuration = dashAttack ? DASH_HITSTOP_DURATION : ATTACK_HITSTOP_DURATION;
        EventManager.PublishEvent(EventType.HITSTOP_STARTED);
        InHitstop = true;
        float calculatedHitstop = BaseHitstopDuration * currentHitstopScaling;
        if (calculatedHitstop < MIN_HITSTOP_TIME) calculatedHitstop = MIN_HITSTOP_TIME;
        currentHitstopLength = calculatedHitstop;
        yield return new WaitForSeconds(calculatedHitstop);
        InHitstop = false;
        EventManager.PublishEvent(EventType.HITSTOP_ENDED);
        lastTimeHitstopOccured = Time.time;
    }

    public static bool GetDashCharge()
    {
        return singleton.dashChargeAvailable;
    }

    public static void SetDashCharge(bool state)
    {
        singleton.dashChargeAvailable = state;
    }
}