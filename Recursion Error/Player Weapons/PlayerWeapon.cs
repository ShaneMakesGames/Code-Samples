using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[Serializable]
public struct AttackData
{
    public int damage;
    [Header("Animation Data")]
    public string attackAnimID;
    public bool shouldInterrupt; // Should this attack interrupt enemy attacks
    [Header("Frame Data")]
    public float startupTime; // How long until the attack starts after it is inputted 
    public float activeTime; // How long the attack stays active for, meaning it can deal damage
    public float hitstopTime; // How long it puts the player and enemy in hitstop (Purely for animation/game-feel purposes)
    public float recoveryTime; // How long you are in recovery for (cannot take action) after performing the attack 
    public float hitstunTime; // How long the enemy is in hitstun for, they cannot take action during this time
    [Header("Hitbox Data")]
    public Vector2 offset;
    public Vector2 size;
    public AttackType attackType;
    [Header("Movement")]
    public float amountToMove; // Amount the player moves forward while attacking
    public float pushbackAmount; // Amount the enemy is pushed backwards from the attack
    public bool debugShowHitbox;

    public bool gauntletDamageBuffActive;
    public CyberneticFunction attackConnectedFunction;
}

public enum AttackState
{
    STARTUP,
    ACTIVE,
    RECOVERY,
    COUNT
}

public enum AttackType
{
    LIGHT,
    HEAVY,
    COUNT
}

public class PlayerWeapon : MonoBehaviour
{
    public WeaponType WeaponType;

    public bool isAttacking;
    public AttackState attackState;
    public bool inRecovery;
    public bool chargingAttack;

    public AttackData currentAttackData;
    public AttackType currentAttackType;

    public float weaponMoveSpeedModifier;

    public Hitbox hitbox;
    public Animator anim;
    public Animator spriteAnimator;

    public float timeWhenHeldInput;

    public float lightNegativeEdgeTime = 0.4f;
    public float heavyNegativeEdgeTime = 0.3f;

    public List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    public bool bufferingAttack;
    public bool cancelBuffer;

    public AimAssist aimAssist;

    public Coroutine attackCoroutineRef;

    public List<GameObject> weaponModels = new List<GameObject>();

    public virtual void LightInputRecieved()
    {
        
    }

    public virtual void ChargedLightInputRecieved()
    {

    }

    public virtual void LightReleasedInputReceived()
    {

    }

    public virtual void HeavyInputRecieved()
    {

    }

    public virtual void ChargedHeavyInputReceived()
    {

    }

    public virtual void HeavyReleasedInputRecieved()
    {

    }

    /// <summary>
    /// Physically moves the player forwardss during the attack
    /// </summary>
    /// <param name="attack"></param>
    /// <returns></returns>
    public IEnumerator MoveFromAttackCoroutine(AttackData attack)
    {
        PlayerControllerDemo playerController = BattleManager.singleton.playerController;
        Transform directionalParent = BattleManager.singleton.inputHandler.directionalParent;

        float timePassed = 0;
        float totalTime = attack.startupTime +  attack.activeTime;

        while (attackState != AttackState.RECOVERY)
        {
            timePassed += Time.deltaTime;
            float lerpedSpeed = Mathf.Lerp(attack.amountToMove, attack.amountToMove / 2, timePassed / totalTime); 
            playerController.characterController.SimpleMove(-directionalParent.forward * lerpedSpeed);

            if (hitbox.attackConnected)
            {
                yield return new WaitForSeconds(attack.hitstopTime);
                hitbox.attackConnected = false;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Handles the animation, timing, hitboxes, VFX, and SFX for player attacks
    /// </summary>
    /// <param name="attack"></param>
    /// <returns></returns>
    public IEnumerator AttackCoroutine(AttackData attack)
    {
        isAttacking = true;
        aimAssist.HitboxSetup(attack);
        attackState = AttackState.STARTUP;
        
        if (attack.amountToMove > 0) StartCoroutine(MoveFromAttackCoroutine(attack));

        foreach (ParticleSystem ps in particleSystems)
        {
            ps.gameObject.SetActive(true);
        }
        anim.CrossFadeInFixedTime(attack.attackAnimID, 0.1f);

        //Play Attack Audio
        switch (WeaponType) {
            case WeaponType.BATTLE_AXE:
                if (attack.attackType == AttackType.LIGHT)
                {
                    BattleManager.singleton.playerController.leifAudio.PlayLeifAxeAttackLightVO();
                }
                else if (attack.attackType == AttackType.HEAVY)
                {
                    BattleManager.singleton.playerController.leifAudio.PlayLeifAxeAttackHeavyVO();
                }
                break;
            case WeaponType.DAGGERS:
                if (attack.attackType == AttackType.LIGHT)
                {
                    BattleManager.singleton.playerController.leifAudio.PlayLeifDaggerAttackLightVO();
                }
                else if (attack.attackType == AttackType.HEAVY)
                {
                    BattleManager.singleton.playerController.leifAudio.PlayLeifDaggerAttackHeavyVO();
                }
                break;
            case WeaponType.GAUNTLETS: //Gauntlet Light Attack SFX & VO found in Gauntlet script in ShootProjectileCoroutine
                    BattleManager.singleton.playerController.leifAudio.PlayLeifGauntletAttackHeavyVO();
                break;
            default: Debug.Log("Cannot Play Attack Audio: Unknown Weapon Type"); break;
        }

        yield return new WaitForSeconds(attack.startupTime); // Start Up
        attackState = AttackState.ACTIVE;
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.gameObject.SetActive(true);
        }
        hitbox.gameObject.SetActive(true);
        hitbox.HitboxSetup(attack);
        float timePassed = 0;
        while (timePassed < attack.activeTime) // Active
        {
            yield return null;
            timePassed += Time.deltaTime;

            if (hitbox.attackConnected)
            {
                if (attack.attackConnectedFunction != null) attack.attackConnectedFunction();
                anim.enabled = false;
                yield return new WaitForSeconds(attack.hitstopTime);
                anim.enabled = true;
                hitbox.attackConnected = false;
            }

        }
        attackState = AttackState.RECOVERY;
        hitbox.DisableHitbox();
        isAttacking = false;
        inRecovery = true;

        currentAttackType = AttackType.COUNT;
        yield return new WaitForSeconds(attack.recoveryTime); // Recovery
        attackState = AttackState.COUNT;
        inRecovery = false;

        if (isAttacking) yield break; // If another attack has been started

        foreach (ParticleSystem ps in particleSystems)
        {
            ps.gameObject.SetActive(false); 
        }
    }
    
    /// <summary>
    /// If the player tries to attack while they are halfway through an attack or are currently dashing, we wait for their current action to end and then start the attack
    /// </summary>
    /// <param name="attack"></param>
    /// <returns></returns>
    protected IEnumerator BufferAttackCoroutine(AttackData attack)
    {
        bufferingAttack = true;

        while (isAttacking || inRecovery || BattleManager.singleton.playerController.dashed)
        {
            yield return null;
        }

        if (!cancelBuffer)
        {
            currentAttackData = attack;
            StartCoroutine(AttackCoroutine(attack));
        }

        bufferingAttack = false;
        cancelBuffer = false;
    }


    public virtual void WeaponSetup()
    {
        for (int i = 0; i < weaponModels.Count; i++)
        {
            weaponModels[i].SetActive(true);
        }
    }

    public void DisableModels()
    {
        for (int i = 0; i < weaponModels.Count; i++)
        {
            weaponModels[i].SetActive(false);
        }
    }
}
