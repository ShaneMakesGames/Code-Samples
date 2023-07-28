using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dagger : PlayerWeapon
{

    public List<AttackData> lightAttackSequence = new List<AttackData>();

    public AttackData lightAttackData;
    public AttackData heavyAttackData;

    public int attackIndex;
    public float timeBeforeResetSequence = 0.25f;

    private Coroutine resetCoroutine;

    public override void LightInputRecieved()
    {
        // If currently in an action
        if (BattleManager.singleton.playerController.dashed)
        {
            if (!bufferingAttack)
            {
                StartCoroutine(BufferAttackSequenceCoroutine());
                return;
            }
            else
            {
                return;
            }
        }
        if (isAttacking || inRecovery)
        {
            if (attackState != AttackState.RECOVERY) return;

            if (!bufferingAttack) StartCoroutine(BufferAttackSequenceCoroutine());
            return;
        }
        
        // No action being done, start the attack
        DoLightAttack();
    }

    /// <summary>
    /// The dagger has a sequence of 3 light attacks
    /// </summary>
    private void DoLightAttack()
    {
        currentAttackType = AttackType.LIGHT;
        StartCoroutine(AttackCoroutine(lightAttackSequence[attackIndex]));

        attackIndex++;

        if (attackIndex >= lightAttackSequence.Count)
        {
            attackIndex = 0;
        }
        else
        {
            if (resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
            }

            resetCoroutine = StartCoroutine(AttemptResetAttackSequenceCoroutine(attackIndex));
        }
    }

    /// <summary>
    /// After a certain amount of time has passed without an additional light input, the attack sequence will be reset
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public IEnumerator AttemptResetAttackSequenceCoroutine(int index)
    {
        float timePassed = 0;
        while (attackIndex == index)
        {
            if (timePassed > timeBeforeResetSequence)
            {
                attackIndex = 0;
                resetCoroutine = null;
                break;
            }

            timePassed += Time.deltaTime;
            yield return null;
        }
    }

    protected IEnumerator BufferAttackSequenceCoroutine()
    {
        bufferingAttack = true;

        while (isAttacking || inRecovery || BattleManager.singleton.playerController.dashed)
        {
            yield return null;
        }

        if (!cancelBuffer)
        {
            DoLightAttack();
        }

        bufferingAttack = false;
        cancelBuffer = false;
    }


    /// <summary>
    /// No charged light attack, this input gives a normal light attack
    /// </summary>
    public override void ChargedLightInputRecieved()
    {
        if (isAttacking || inRecovery)
        {
            if (attackState != AttackState.RECOVERY) return;

            if (!bufferingAttack) StartCoroutine(BufferAttackSequenceCoroutine());
            return;
        }

        DoLightAttack();
    }

    public override void HeavyInputRecieved()
    {
        if (isAttacking) return;

        currentAttackType = AttackType.HEAVY;
        StartCoroutine(AttackCoroutine(heavyAttackData));
    }

    public override void ChargedHeavyInputReceived()
    {
        if (isAttacking) return;

        currentAttackType = AttackType.HEAVY;
        StartCoroutine(AttackCoroutine(heavyAttackData));
    }
}
