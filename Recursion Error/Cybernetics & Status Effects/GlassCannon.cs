using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Glass Cannon", menuName = "CyberneticSO/GlassCannon")]
public class GlassCannon : Cybernetic
{
    // This cybernetic increases the player's damage but breaks once the player takes damage  

    public float damageMultiplier;

    public override void AssignTriggers()
    {
        base.AssignTriggers();

        triggers[(int)CyberneticTriggers.ENEMY_HIT] = true;
        triggerFunctions[(int)CyberneticTriggers.ENEMY_HIT] += AddDamageMultiplier;
        triggers[(int)CyberneticTriggers.PLAYER_TAKES_DAMAGE] = true;
        triggerFunctions[(int)CyberneticTriggers.PLAYER_TAKES_DAMAGE] += RemoveThisCybernetic; 
    }

    public void AddDamageMultiplier(EnemyDemo enemy)
    {
        enemy.damageMultiplierOnNextHit.Add(damageMultiplier);
    }

    public void RemoveThisCybernetic(EnemyDemo enemy = null)
    {
        BattleManager.singleton.playerController.PlayCyberneticVFX(CyberneticSlot.BODY);
        BattleManager.singleton.RemoveCybernetic(this);
    }

    private void OnDestroy()
    {
        triggerFunctions[(int)CyberneticTriggers.ENEMY_HIT] += AddDamageMultiplier;
        triggerFunctions[(int)CyberneticTriggers.PLAYER_TAKES_DAMAGE] += RemoveThisCybernetic;
    }
}
