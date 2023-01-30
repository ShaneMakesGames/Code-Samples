using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Apply Status Effect", menuName = "CyberneticSO/ApplyStatusEffect")]
public class ApplyStatusEffect : Cybernetic
{
    public StatusEffect se;
    public StatusType statusType;

    public CyberneticTriggers trigger;

    public override void AssignTriggers()
    {
        base.AssignTriggers();

        triggers[(int)trigger] = true;
        triggerFunctions[(int)trigger] += ApplyStatusEffectToEnemy;
    }

    public void ApplyStatusEffectToEnemy(EnemyDemo enemy)
    {
        if (enemy == null) return;

        se = new StatusEffect();
        se.SetupTriggersByStatusType(statusType);
        enemy.ApplyStatusEffect(se);
    }

    private void OnDestroy()
    {
        triggerFunctions[(int)trigger] -= ApplyStatusEffectToEnemy;
    }

}
