using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class StatusEffect
{
    public void TakeBurnDamage()
    {
        myEnemy.TakeDamage(stacksApplied, Color.red, false);
    }

    public void CreateLightningStrike()
    {
        myEnemy.TakeDamage(LightningStrike.BASE_DAMAGE, Color.yellow, false);

        GameObject obj = PrefabManager.GetPrefab("Lightning Strike");
        obj.transform.position = myEnemy.transform.position;
        LightningStrike lightningStrike = obj.GetComponent<LightningStrike>();
        lightningStrike.SetupLightningStrike(this, myEnemy.enemyCollider);
    }

    public void FreezeEnemy()
    {
        myEnemy.ResetCurrentAttack();
        myEnemy.animator.enabled = false;
        myEnemy.isFrozen = true;
        myEnemy.agent.isStopped = true;
    }
}