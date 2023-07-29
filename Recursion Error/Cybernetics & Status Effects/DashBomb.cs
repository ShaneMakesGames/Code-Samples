using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dash Bomb", menuName = "CyberneticSO/DashBomb")]
public class DashBomb : Cybernetic
{
    // When the player dashes, spawns a bomb at the position they dashed from

    public GameObject bombPrefab;
    public float timeTilExplode;
    public float activeTime;
    
    public override void AssignTriggers()
    {
        base.AssignTriggers();

        triggers[(int)CyberneticTriggers.PRE_PLAYER_DASH] = true;
        triggerFunctions[(int)CyberneticTriggers.PRE_PLAYER_DASH] += SpawnBomb;
    }

    public void SpawnBomb(EnemyDemo enemy)
    {
        if (hasCooldown && onCooldown) return;

        Transform playerTransform = BattleManager.GetPlayerTransform();
        GameObject obj = PrefabManager.GetPrefab("Bomb");
        obj.transform.position = playerTransform.position;
        TimeBomb timeBomb = obj.GetComponent<TimeBomb>();
        timeBomb.timeTilExplosion = timeTilExplode;
        timeBomb.activeTime = activeTime;
        timeBomb.SetupBomb();

        if (hasCooldown) BattleManager.singleton.StartCybrCoroutine(CooldownCoroutine());
    }

    private void OnDestroy()
    {
        triggerFunctions[(int)CyberneticTriggers.PRE_PLAYER_DASH] -= SpawnBomb;
    }
}