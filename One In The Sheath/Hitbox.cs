using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Hitbox : MonoBehaviour
{
    public bool isActive;
    public bool isDashAttack;
    public BoxCollider myCollider;

    public List<Collider> collisions = new List<Collider>();

    public List<IDamageable> DamageableCollisionList = new List<IDamageable>();
    public int collisionIndex;

    public float timeSinceBecomeActive;
    public float activeTimeToCheck;

    public const float ATTACK_ACTIVE_TIME = 0.2f;
    public const float ATTACK_RECOVERY_TIME = 0.2f;
    public const float DASH_ACTIVE_TIME = 0.25f;

    public void SetActive(bool state)
    {
        timeSinceBecomeActive = 0;
        collisionIndex = 0;

        myCollider.enabled = state;
        isActive = state;

        if (!state)
        {
            collisions.Clear();
            DamageableCollisionList.Clear();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Enemy") && !other.CompareTag("Projectile")) return;
        if (collisions.Contains(other)) return;

        collisions.Add(other);

        IDamageable damageable = other.GetComponent<IDamageable>();
        DamageableCollisionList.Add(damageable);
    }

    private void Update()
    {
        if (!isActive) return;
        if (GameManager.InHitstop || PauseUI.isGamePaused) return;

        timeSinceBecomeActive += Time.deltaTime;
        if (timeSinceBecomeActive > activeTimeToCheck)
        {
            SetActive(false);
            return;
        }

        if (collisionIndex >= DamageableCollisionList.Count) return;

        if (collisions[collisionIndex] == null) return;
        if (collisions[collisionIndex].CompareTag("Projectile"))
        {
            SFXSystem.singleton.PlaySFX("Parry1");

            GameManager.IncrementBattleStat(GameManager.PARRIES_STRING);
            BattleUI.singleton.IncrementComboCount();
            GameManager.singleton.IncreaseScore(isParry: true, isDash: false);
            BattleUI.singleton.DashChargeReset(GameManager.GetDashCharge());
            GameManager.SetDashCharge(true);
            
            BattleUI.singleton.TryPlaySkillNotification(SkillNotification.PARRY, Color.yellow);
        }
        else
        {
            SFXSystem.singleton.PlaySFX("Slash1");

            GameManager.IncrementBattleStat(GameManager.KILLS_STRING);
            BattleUI.singleton.IncrementComboCount();
            GameManager.singleton.IncreaseScore(isParry: false, isDash: isDashAttack);
            BattleUI.singleton.DashChargeReset(GameManager.GetDashCharge());
            GameManager.SetDashCharge(true);
            if (!isDashAttack) BattleUI.singleton.TryPlaySkillNotification(SkillNotification.SLASH_KILL, Color.cyan);
            else BattleUI.singleton.TryPlaySkillNotification(SkillNotification.DASH_KILL, Color.cyan);
        }

        GameManager.singleton.player.OnAttackConnected();
        GameManager.singleton.OnAttackConnected(isDashAttack);
        DamageableCollisionList[collisionIndex].TakeDamage();
        collisionIndex++;
    }
}