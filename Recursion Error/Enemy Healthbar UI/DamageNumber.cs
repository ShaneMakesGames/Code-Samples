using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    public EnemyDemo enemy;

    public TextMeshProUGUI damageText;
    
    public const float ANIM_TIME = 0.5f;
    public const float Y_OFFSET = 75f;

    public void SetupDamageNumber(EnemyDemo _enemy, int damage, Color color)
    {
        enemy = _enemy;
        transform.position = BattleManager.GetMainCamera().WorldToScreenPoint(_enemy.transform.position);

        damageText.text = damage.ToString();

        StartCoroutine(PlayAnimationCoroutine(color));
    }

    /// <summary>
    /// Damage Text slowly fades out
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    private IEnumerator PlayAnimationCoroutine(Color color)
    {
        float timePassed = 0;
        while (timePassed < ANIM_TIME)
        {
            if (enemy == null) break;

            timePassed += Time.deltaTime;

            float alpha = Mathf.Lerp(1, 0, timePassed / ANIM_TIME);
            damageText.color = new Color(color.r, color.g, color.b, alpha);

            Vector3 enemyPos = enemy.transform.position;
            Vector3 newPos = BattleManager.GetMainCamera().WorldToScreenPoint(enemyPos);
            transform.position = new Vector3(newPos.x, newPos.y + Y_OFFSET, newPos.z);

            yield return null;
        }

        ResetDamageNumber();
        EnemyHealthbarManager.ReturnDamageNumberFromQueue(this);
    }

    public void ResetDamageNumber()
    {
        enemy = null;
        damageText.color = Color.white;
    }
}