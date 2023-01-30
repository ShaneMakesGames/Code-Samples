using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthbarManager : MonoBehaviour
{
    public bool isActive;

    public static Dictionary<EnemyDemo, EnemyHealthbarVisual> dictAllEnemyHealthbars;

    [SerializeField]
    private List<DamageNumber> damageNumbers = new List<DamageNumber>();
    public static Queue<DamageNumber> DamageNumberQueue;

    [SerializeField]
    private List<EnemyHealthbarVisual> enemyHealthbarVisuals = new List<EnemyHealthbarVisual>();
    public static Queue<EnemyHealthbarVisual> EnemyHealthbarQueue;

    public const float ANIM_HP_TIME = 0.2f;

    #region Singleton

    public static EnemyHealthbarManager singleton;

    void Awake()
    {
        if (singleton != null && singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        singleton = this;
        DontDestroyOnLoad(gameObject);

        isActive = true;
        dictAllEnemyHealthbars = new Dictionary<EnemyDemo, EnemyHealthbarVisual>();

        DamageNumberQueue = new Queue<DamageNumber>();
        for (int i = 0; i < damageNumbers.Count; i++)
        {
            DamageNumberQueue.Enqueue(damageNumbers[i]);
        }

        EnemyHealthbarQueue = new Queue<EnemyHealthbarVisual>();
        for (int i = 0; i < enemyHealthbarVisuals.Count; i++)
        {
            EnemyHealthbarQueue.Enqueue(enemyHealthbarVisuals[i]);
        }
    }

    #endregion

    #region Queueing

    public static void SetupDamageNumberFromQueue(EnemyDemo enemy, int damage, Color color)
    {
        DamageNumber dmg = DamageNumberQueue.Dequeue();
        dmg.gameObject.SetActive(true);
        dmg.SetupDamageNumber(enemy, damage, color);
    }

    public static void ReturnDamageNumberFromQueue(DamageNumber dmg)
    {
        dmg.gameObject.SetActive(false);
        DamageNumberQueue.Enqueue(dmg);
    }

    public static void SetupEnemyHealthbarFromQueue(EnemyDemo enemy)
    {
        EnemyHealthbarVisual ehv = EnemyHealthbarQueue.Dequeue();
        ehv.gameObject.SetActive(true);
        singleton.StartCoroutine(singleton.ScaleObjectUp(ehv.transform, 2, ANIM_HP_TIME));

        ehv.SetupHealthbar(enemy);
        dictAllEnemyHealthbars.Add(enemy, ehv);
    }

    public static void ReturnEnemyHealthbarToQueue(EnemyHealthbarVisual ehv)
    {
        singleton.StartCoroutine(singleton.ScaleObjectUp(ehv.transform, 0, ANIM_HP_TIME));
        EnemyHealthbarQueue.Enqueue(ehv);
    }

    private IEnumerator ScaleObjectUp(Transform t, float targetScale, float animTime)
    {
        float startingScale = t.localScale.x;
        float timePassed = 0;
        while (timePassed < animTime)
        {
            timePassed += Time.deltaTime;
            float newScale = Mathf.Lerp(startingScale, targetScale, timePassed / animTime);
            t.localScale = new Vector3(newScale, newScale, newScale);
            yield return null;
        }

        if (targetScale == 0)
        {
            t.gameObject.SetActive(false);
        }
    }

    #endregion

    public static void UpdateHealthDisplay(EnemyDemo enemy, int damage)
    {
        if (!singleton.isActive) return;

        SetupDamageNumberFromQueue(enemy, damage, Color.white);

        if (!dictAllEnemyHealthbars.ContainsKey(enemy)) SetupEnemyHealthbarFromQueue(enemy);

        dictAllEnemyHealthbars[enemy].UpdateHealthDisplay();
    }

    public static void DisableHealthDisplay(EnemyDemo enemy)
    {
        if (!dictAllEnemyHealthbars.ContainsKey(enemy)) return;

        ReturnEnemyHealthbarToQueue(dictAllEnemyHealthbars[enemy]);
        dictAllEnemyHealthbars.Remove(enemy);
    }

    public static void CleanUpHealthbars()
    {
        foreach (KeyValuePair<EnemyDemo, EnemyHealthbarVisual> kvp in dictAllEnemyHealthbars)
        {
            ReturnEnemyHealthbarToQueue(kvp.Value);
        }

        dictAllEnemyHealthbars.Clear();
        singleton.isActive = false;
    }
}