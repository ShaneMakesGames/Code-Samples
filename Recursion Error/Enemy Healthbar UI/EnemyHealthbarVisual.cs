using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthbarVisual : MonoBehaviour
{
    public EnemyDemo enemy;

    public Image healthbarDamageFill;
    public Image healthbarFill;
    public TextMeshProUGUI healthText;

    public const float Y_OFFSET = 50f;
    public const float ANIM_TIME = 0.5f;

    private bool coroutineRunning;
    private Queue<float> queuedHP;

    public void SetupHealthbar(EnemyDemo _enemy)
    {
        queuedHP = new Queue<float>();

        enemy = _enemy;
        if (!enemy.isBoss)
        {
            StartCoroutine(OffsetHealthbarVisual());
        }
    }

    public void UpdateHealthDisplay()
    {
        float currentHP = enemy.health / enemy.maxHealth;
        healthbarFill.fillAmount = currentHP;
        if (coroutineRunning)
        {
            queuedHP.Enqueue(currentHP);   
        }
        else
        {
            StartCoroutine(AnimateHealthbarCoroutine(currentHP));
        }
    }

    private IEnumerator AnimateHealthbarCoroutine(float currentHP)
    {
        coroutineRunning = true;
        float startingPercent = healthbarDamageFill.fillAmount;
        float timePassed = 0;
        while (timePassed < ANIM_TIME)
        {
            timePassed += Time.deltaTime;
            healthbarDamageFill.fillAmount = Mathf.Lerp(startingPercent, currentHP, timePassed / ANIM_TIME);
            yield return null;
        }
     
        healthbarDamageFill.fillAmount = currentHP;
        CheckAnimationQueue();
    }

    private void CheckAnimationQueue()
    {
        if (queuedHP.Count == 0)
        {
            coroutineRunning = false;
            return;
        }

        StartCoroutine(AnimateHealthbarCoroutine(queuedHP.Dequeue()));
    }

    private IEnumerator OffsetHealthbarVisual()
    {
        while (enemy != null)
        {
            Vector3 enemyPos = enemy.transform.position;
            Vector3 newPos = BattleManager.GetMainCamera().WorldToScreenPoint(enemyPos);
            transform.position = new Vector3(newPos.x, newPos.y + Y_OFFSET, newPos.z);
            yield return null;
        }
    }

    public void ResetHealthbar()
    {
        enemy = null;
        healthbarFill.fillAmount = 1;
    }
}