using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axe : PlayerWeapon
{
    public AttackData lightAttackData;
    public AttackData heldLightAttackData;
    public AttackData perfectHeldLightAttackData;
    public AttackData heavyAttackData;
    public AttackData heldHeavyAttackData;
    public AttackData perfectHeavyAttackData;

    public float negativeEdgeFromDash;
    public float currentNegativeEdgeTime;

    public const float HELD_LIGHT_MINIMUM_WINDOW = 0.1f;
    public const float HELD_LIGHT_PERFECT_WINDOW = 0.25f;

    public override void LightInputRecieved()
    {
        // If currently in an action
        if (BattleManager.singleton.playerController.dashed)
        {
            if (!bufferingAttack)
            {
                StartCoroutine(BufferAttackCoroutine(lightAttackData));
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

            if (!bufferingAttack)
            {
                StartCoroutine(BufferAttackCoroutine(lightAttackData));
                return;
            }
            else return;
        }

        // No action being done, start the attack
        currentAttackType = AttackType.LIGHT;
        currentAttackData = lightAttackData;
        attackCoroutineRef = StartCoroutine(AttackCoroutine(lightAttackData));
    }

    public override void ChargedLightInputRecieved()
    {
        if (chargingAttack) return;

        if (BattleManager.singleton.playerController.dashed)
        {
            StartCoroutine(SetupChargedLightCoroutine(negativeEdgeFromDash));
            return;
        }

        if (isAttacking || inRecovery)
        {
            StartCoroutine(SetupChargedLightCoroutine(negativeEdgeFromDash));
            return;
        }

        if (!isAttacking && !inRecovery)
        {
            currentAttackData = lightAttackData;
            attackCoroutineRef = StartCoroutine(AttackCoroutine(lightAttackData));
            StartCoroutine(SetupChargedLightCoroutine(lightNegativeEdgeTime));
        }
    }


    public IEnumerator SetupChargedLightCoroutine(float negativeEdgeTime)
    {
        chargingAttack = true;
        timeWhenHeldInput = Time.realtimeSinceStartup;
        currentNegativeEdgeTime = negativeEdgeTime;
        float timePassed = 0;
        while (timeWhenHeldInput != 0)
        {
            if (timePassed > negativeEdgeTime)
            {
                spriteAnimator.Play("Flash");
                break;
            }

            timePassed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(HELD_LIGHT_PERFECT_WINDOW); // After a certain amount of time, the attack starts
        if (isAttacking || !chargingAttack) yield break;

        if (BattleManager.singleton.playerController.dashed && !bufferingAttack)
        {
            StartCoroutine(BufferAttackCoroutine(heldLightAttackData));
        }
        else
        {
            StartCoroutine(AttackCoroutine(heldLightAttackData));
        }
    }

    /// <summary>
    /// Releasing the light attack button after it has been held allows you to manually perform the follow-up attack
    /// </summary>
    public override void LightReleasedInputReceived()
    {
        if (currentAttackType == AttackType.HEAVY && chargingAttack || timeWhenHeldInput == 0 || isAttacking)
        {
            chargingAttack = false;
            return;
        }

        float heldTime = Time.realtimeSinceStartup - currentNegativeEdgeTime - timeWhenHeldInput;

        if (heldTime > HELD_LIGHT_MINIMUM_WINDOW)
        {
            if (heldTime < HELD_LIGHT_PERFECT_WINDOW) // Perfect Window
            {
                if (BattleManager.singleton.playerController.dashed && !bufferingAttack)
                {
                    StartCoroutine(BufferAttackCoroutine(perfectHeldLightAttackData));
                    
                    //Call perfectAttack Audio
                    //BattleManager.singleton.playerController.leifAudio.PlayLeifSpecialAttack();
                }
                else
                {
                    currentAttackData = perfectHeldLightAttackData;
                    StartCoroutine(AttackCoroutine(perfectHeldLightAttackData));
                    
                    //Call perfectAttack Audio
                    BattleManager.singleton.playerController.leifAudio.PlayLeifSpecialAttack();
                }
            }
        }
        else
        {
            spriteAnimator.Play("Idle");
        }


        timeWhenHeldInput = 0;
        chargingAttack = false;
    }

    public override void HeavyInputRecieved()
    {
        if (BattleManager.singleton.playerController.dashed)
        {
            if (!bufferingAttack)
            {
                StartCoroutine(BufferAttackCoroutine(heavyAttackData));
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

            if (!bufferingAttack)
            {
                StartCoroutine(BufferAttackCoroutine(heavyAttackData));
                return;
            }
            else return;
        }

        currentAttackType = AttackType.HEAVY;
        currentAttackData = heavyAttackData;
        attackCoroutineRef = StartCoroutine(AttackCoroutine(heavyAttackData));
    }

    /// <summary>
    /// There is no charged heavy attack so if this input is recieved, just do the heavy attack 
    /// </summary>
    public override void ChargedHeavyInputReceived()
    {
        if (BattleManager.singleton.playerController.dashed)
        {
            if (!bufferingAttack)
            {
                StartCoroutine(BufferAttackCoroutine(heavyAttackData));
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

            if (!bufferingAttack)
            {
                StartCoroutine(BufferAttackCoroutine(heavyAttackData));
                return;
            }
            else return;
        }

        currentAttackType = AttackType.HEAVY;
        currentAttackData = heavyAttackData;
        attackCoroutineRef = StartCoroutine(AttackCoroutine(heavyAttackData));
    }
}