using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gauntlet : PlayerWeapon
{
    public AttackData lightAttackData;
    public AttackData heldLightAttackData;
    public AttackData perfectHeldLightAttackData;

    public AttackData heavyAttackData;

    public Transform aimingCursorParent;
    public bool inputReleased;

    public Transform playerTransform;

    public Vector3 offset;

    public Camera mainCamera;
    public LayerMask groundMask;

    public GameObject projectile;
    public Transform muzzle;

    public bool isBuffActive;
    public float currentBuffTime;
    public List<GameObject> glowVFX = new List<GameObject>();
    public const float BUFF_TIME = 3f;
    public const float BUFF_MULTIPLIER = 3f;

    public const float PROJECTILE_SPEED = 30f;
    public const float PROJECTILE_LIFETIME = 0.25f;

    public const float HELD_LIGHT_MINIMUM_WINDOW = 0.1f;
    public const float HELD_LIGHT_PERFECT_WINDOW = 0.3f;

    public override void WeaponSetup()
    {
        base.WeaponSetup();

        heavyAttackData.attackConnectedFunction += UppercutConnected; 
        SetGauntletDamageBuff(false);
        isBuffActive = false;
        currentBuffTime = 0;
    }

    /// <summary>
    /// The visuals for the ranged attack buff
    /// </summary>
    /// <param name="state"></param>
    public void SetGauntletDamageBuff(bool state)
    {
        lightAttackData.gauntletDamageBuffActive = state;
        heldLightAttackData.gauntletDamageBuffActive = state;
        perfectHeldLightAttackData.gauntletDamageBuffActive = state;

        for (int i = 0; i < glowVFX.Count; i++)
        {
            glowVFX[i].SetActive(state);
        }
    }

    /// <summary>
    /// Upon landing a successful uppercut, your ranged attack is buffed for a certain amount of time
    /// </summary>
    /// <param name="enemy"></param>
    public void UppercutConnected(EnemyDemo enemy = null)
    {
        currentBuffTime = 0;
        if (isBuffActive) return;

        StartCoroutine(AddRangedDamageBuffForShortAmount());
    }

    private IEnumerator AddRangedDamageBuffForShortAmount()
    {
        isBuffActive = true;
        SetGauntletDamageBuff(true);

        while (currentBuffTime < BUFF_TIME)
        {
            currentBuffTime += Time.deltaTime;
            yield return null;
        }

        currentBuffTime = 0;
        SetGauntletDamageBuff(false);
        isBuffActive = false;
    }

    public override void LightInputRecieved()
    {
        if (isAttacking || inRecovery) return;

        currentAttackType = AttackType.LIGHT;
        currentAttackData = lightAttackData;
        aimingCursorParent.gameObject.SetActive(false);
        
        StartCoroutine(ShootProjectileCoroutine(lightAttackData));
    }

    public IEnumerator SetupChargedLightCoroutine()
    {
        chargingAttack = true;
        timeWhenHeldInput = Time.realtimeSinceStartup;
        float timePassed = 0;
        while (timeWhenHeldInput != 0)
        {
            if (timePassed > lightNegativeEdgeTime)
            {
                spriteAnimator.Play("Flash");
                break;
            }

            timePassed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);
        if (isAttacking || !chargingAttack) yield break;
        inputReleased = true;
        chargingAttack = false;
        aimingCursorParent.gameObject.SetActive(false);
        StartCoroutine(ShootProjectileCoroutine(heldLightAttackData));
    }

    public override void ChargedLightInputRecieved()
    {
        if (isAttacking || inRecovery || BattleManager.singleton.playerController.dashed) return;

        currentAttackType = AttackType.LIGHT;

        chargingAttack = true;
        inputReleased = false;
        currentAttackData = lightAttackData;
        
        StartCoroutine(AimingCoroutine());
        StartCoroutine(SetupChargedLightCoroutine());
    }

    public IEnumerator AimingCoroutine()
    {
        anim.CrossFadeInFixedTime("Gauntlet Aim", 0.1f);

        //Play Leif Gauntlet Charge Audio 
        BattleManager.singleton.playerController.leifAudio.PlayLeifGauntletAttackLightHoldSFX();

        InputHandler inputHandler = BattleManager.singleton.inputHandler;
        Gamepad gamepad = BattleManager.singleton.inputHandler.gamepad;
        aimingCursorParent.gameObject.SetActive(true);
        while (!inputReleased)
        {
            if (BattleManager.singleton.playerController.dashed) // Dashing cancels the attack
            {
                chargingAttack = false;
                aimingCursorParent.gameObject.SetActive(false);
                yield break;
            }

            aimingCursorParent.position = playerTransform.position + offset;

            if (inputHandler.currentInputType != InputType.Mouse) // Different functionality for aiming with gamepad and mnk
            {
                Vector2 gamepadInput = gamepad.leftStick.ReadValue();
                if (gamepadInput == Vector2.zero)
                {
                    yield return null;
                    continue;
                }

                float angle = Mathf.Atan2(-gamepadInput.x, -gamepadInput.y) * Mathf.Rad2Deg;
                angle += 45;
                playerTransform.localEulerAngles = new Vector3(-angle, -90, 90);
            }
            else
            {
                Ray ray = BattleManager.GetMainCamera().ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hitInfo, float.MaxValue, groundMask))
                {
                    Vector3 targetDir = hitInfo.point - playerTransform.position;
                    targetDir.y = 0;
                    playerTransform.forward = -targetDir;
                }
            }

            yield return null;
        }
    }

    /// <summary>
    /// Releasing light attack after holding it down fires off a charged projectile
    /// </summary>
    public override void LightReleasedInputReceived()
    {
        if (currentAttackType == AttackType.HEAVY && chargingAttack || timeWhenHeldInput == 0 || isAttacking)
        {
            chargingAttack = false;
            return;
        }

        float heldTime = Time.realtimeSinceStartup - lightNegativeEdgeTime - timeWhenHeldInput;

        if (heldTime > HELD_LIGHT_MINIMUM_WINDOW)
        {
            if (heldTime < HELD_LIGHT_PERFECT_WINDOW) // Perfect Window
            {
                currentAttackData = perfectHeldLightAttackData;
                aimingCursorParent.gameObject.SetActive(false);

                //StartCoroutine(AttackCoroutine(perfectHeldLightAttackData));
                StartCoroutine(ShootProjectileCoroutine(perfectHeldLightAttackData));

                //Call perfectAttack Audio
                BattleManager.singleton.playerController.leifAudio.PlayLeifSpecialAttack();
            }
        }
        else
        {
            spriteAnimator.Play("Idle");

            if (!isAttacking)
            {
                currentAttackData = heldLightAttackData;
                aimingCursorParent.gameObject.SetActive(false);

                //StartCoroutine(AttackCoroutine(heldLightAttackData));
                StartCoroutine(ShootProjectileCoroutine(heldLightAttackData));

            }
        }

        timeWhenHeldInput = 0;
        chargingAttack = false;
        inputReleased = true;
        aimingCursorParent.gameObject.SetActive(false);
    }

    public override void HeavyInputRecieved()
    {
        if (isAttacking || inRecovery) return;

        aimingCursorParent.gameObject.SetActive(false);
        StartCoroutine(AttackCoroutine(heavyAttackData));
    }

    public override void ChargedHeavyInputReceived()
    {
        if (isAttacking || inRecovery) return;

        aimingCursorParent.gameObject.SetActive(false);
        StartCoroutine(AttackCoroutine(heavyAttackData));
    }

    public IEnumerator ShootProjectileCoroutine(AttackData attack)
    {
        aimAssist.HitboxSetup(attack);
        attackState = AttackState.STARTUP;
        anim.CrossFadeInFixedTime(attack.attackAnimID, 0.1f);
        isAttacking = true;

        //Play Leif Gauntlet Projectile Attack Audio
        BattleManager.singleton.playerController.leifAudio.PlayLeifGauntletAttackLightVO();
        BattleManager.singleton.playerController.leifAudio.PlayLeifGauntletAttackLightSFX();

        yield return new WaitForSeconds(attack.startupTime); // Start Up
        attackState = AttackState.ACTIVE;
        yield return new WaitForSeconds(attack.activeTime); // Active
        attackState = AttackState.RECOVERY;

        Rigidbody rb = Instantiate(projectile, muzzle.position, Quaternion.identity).GetComponent<Rigidbody>();
        rb.GetComponent<PlayerProjectile>().damage = attack.damage;

        // TODO : Adjust force and lifetime based on what version of the attack it is
        rb.AddForce(transform.up * PROJECTILE_SPEED, ForceMode.Impulse);
        Destroy(rb, PROJECTILE_LIFETIME);
        Destroy(rb.gameObject, PROJECTILE_LIFETIME * 2);

        isAttacking = false;
        inRecovery = true;
        yield return new WaitForSeconds(attack.recoveryTime); // Recovery
        currentAttackType = AttackType.COUNT;
        attackState = AttackState.COUNT;
        inRecovery = false;

    }

    public void OnDestroy()
    {
        heavyAttackData.attackConnectedFunction -= UppercutConnected;
    }
}