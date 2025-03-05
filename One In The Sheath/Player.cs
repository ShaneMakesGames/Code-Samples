using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    public Vector2 moveVector;
    public Vector2 lastNonZeroMoveVector;
    public float MovementSpeed;

    public const float ATTACK_MOVEMENT_MULT = 0.45f;
    public const float DEFAULT_MOVEMENT_SPEED = 12;
    public const float DASHING_MOVEMENT_SPEED = 35;

    public Transform myModel;

    [Header("Animation")]
    public string currentAnimationState;
    public Animator anim;

    [Header("Attacking")]
    public Hitbox myHitbox;
    public Hitbox dashHitbox;
    public bool isAttacking;
    public bool attackConnected;

    public bool preAttackInvincibility; // The slash attack is not immediately active (for animation timing) but during this time you should not be vulnerable
    public bool invincibilityFromSuccessfulAttack;
    public float timeAtLastAttackConnected;
    public const float INVINCIBILITY_WINDOW_AFTER_ATTACK = 0.25f;

    public List<string> attackSFXList = new List<string>();

    [Header("Dashing")]
    public bool isDashing;

    public ParticleSystem slashParticleSystem;

    public GameObject dashVFXPrefab;
    public Transform dashVFXParent;

    public GameObject deathVFXPrefab;

    public void HandleInput(Gamepad gamepad)
    {
        if (gamepad.startButton.wasPressedThisFrame)
        {
            BattleUI.singleton.TryUpdateInputIcons();
            TryPause();
            return;
        }

        if (gamepad.buttonWest.wasPressedThisFrame)
        {
            BattleUI.singleton.TryUpdateInputIcons();
            TryAttack();
        }
        if (gamepad.buttonNorth.wasPressedThisFrame || gamepad.rightTrigger.wasPressedThisFrame)
        {
            BattleUI.singleton.TryUpdateInputIcons();
            TryDash();
        }

        if (isDashing) return;

        moveVector = gamepad.leftStick.ReadValue().normalized;
        if (moveVector != Vector2.zero)
        {
            BattleUI.singleton.TryUpdateInputIcons();
            lastNonZeroMoveVector = moveVector;
        }

    }

    void FixedUpdate()
    {
        if (!GameManager.singleton.battleActive) return;
        if (InputHandler.singleton.isGamepadDisconnected) return;
        if (GameManager.InHitstop || PauseUI.isGamePaused) return;

        Vector3 convertedMovementVector = new Vector3(moveVector.x, 0, moveVector.y);

        // Movement
        float attackMovementMult = 1f;
        if (isAttacking && !isDashing) attackMovementMult = ATTACK_MOVEMENT_MULT;
        rb.position += convertedMovementVector * MovementSpeed * attackMovementMult * Time.deltaTime;

        // Rotation
        if (moveVector == Vector2.zero) return;

        float yRot = Mathf.Atan2(moveVector.y, moveVector.x) * Mathf.Rad2Deg;
        yRot -= 90;
        myModel.localEulerAngles = new Vector3(0, -yRot, 0);
    }

    private void LateUpdate()
    {
        if (!GameManager.singleton.battleActive) return;

        if (invincibilityFromSuccessfulAttack)
        {
            if (Time.time - timeAtLastAttackConnected > INVINCIBILITY_WINDOW_AFTER_ATTACK)
            {
                invincibilityFromSuccessfulAttack = false;
            }
        }

        if (isAttacking || isDashing) return;

        if (moveVector != Vector2.zero)
        {
            ChangeAnimationState("Run");
            return;
        }

        ChangeAnimationState("Idle");
    }

    public void ChangeAnimationState(string _newAnimationState, bool overridePlayAnimation = false)
    {
        if (!overridePlayAnimation && currentAnimationState == _newAnimationState) return;

        currentAnimationState = _newAnimationState;
        anim.Play(_newAnimationState, 0, 0);
    }

    public void TryPause()
    {
        if (GameManager.InHitstop) return;

        InputHandler.SetGameState(GameState.PAUSE_SCREEN);
    }

    private Coroutine attackCoroutine;
    public void TryAttack()
    {
        if (isAttacking && attackConnected)
        {
            if (!bufferingAttack) StartCoroutine(HitstopAttackBufferCoroutine());
            return;
        }

        if (isAttacking || myHitbox.isActive) return;

        attackCoroutine = StartCoroutine(AttackCoroutine());
    }

    // When inputting attack during hitstop, the action is buffered and will come out as soon as hitstop ends
    private bool bufferingAttack;
    private IEnumerator HitstopAttackBufferCoroutine()
    {
        bufferingAttack = true;
        while (GameManager.InHitstop || PauseUI.isGamePaused)
        {
            yield return null;
        }

        myHitbox.SetActive(false);
        bufferingAttack = false;
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        attackConnected = false;

        BattleUI.singleton.AttackInputted(isDashAttack: false);

        SFXSystem.singleton.PlayRandomSFX(attackSFXList, randomizePitch: true);
        ChangeAnimationState("Attack");

        slashParticleSystem.Stop();
        slashParticleSystem.Play();

        preAttackInvincibility = true;
        yield return new WaitForSeconds(0.075f);

        myHitbox.SetActive(true); // Active
        yield return new WaitForEndOfFrame();
        preAttackInvincibility = false;

        float timePassed = 0;
        while (timePassed < Hitbox.ATTACK_ACTIVE_TIME + Hitbox.ATTACK_RECOVERY_TIME) // Recovery
        {
            if (GameManager.InHitstop || PauseUI.isGamePaused)
            {
                yield return null;
                continue;
            }
            timePassed += Time.deltaTime;
            yield return null;
        }

        isAttacking = false;
        attackConnected = false;
    }

    private Coroutine dashCoroutine;
    public void TryDash()
    {
        if (!GameManager.GetDashCharge()) return;

        if (isAttacking && attackConnected || isDashing && attackConnected)
        {
            if (!bufferingDash) StartCoroutine(HitstopDashBufferCoroutine());
            return;
        }

        if (isDashing) return;
        if (isAttacking && !attackConnected) return;

        dashCoroutine = StartCoroutine(DashCoroutine());
    }

    // When inputting dash during hitstop, the action is buffered and will come out as soon as hitstop ends
    private bool bufferingDash;
    private IEnumerator HitstopDashBufferCoroutine()
    {
        bufferingDash = true;
        while (GameManager.InHitstop || PauseUI.isGamePaused)
        {
            yield return null;
        }

        myHitbox.SetActive(false);
        dashHitbox.SetActive(false);
        bufferingDash = false;
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            isAttacking = false;
        }
        if (dashCoroutine != null) StopCoroutine(dashCoroutine);
        dashCoroutine = StartCoroutine(DashCoroutine());
    }

    private IEnumerator DashCoroutine()
    {
        BattleUI.singleton.AttackInputted(isDashAttack: true);
        BattleUI.singleton.DashChargeUsed();
        GameManager.SetDashCharge(false);
        gameObject.layer = 6;

        isDashing = true;
        attackConnected = false;

        slashParticleSystem.Stop();
        slashParticleSystem.Play();
        GameObject dashOBJ = Instantiate(dashVFXPrefab, dashVFXParent.position, Quaternion.identity);
        dashOBJ.transform.eulerAngles = new Vector3(-90, myModel.eulerAngles.y, 0);
        LeanTween.moveZ(dashOBJ, dashOBJ.transform.position.z - 2f, 1.5f);
        Destroy(dashOBJ, 2f);

        SFXSystem.singleton.PlayRandomSFX(attackSFXList, randomizePitch: true);
        SFXSystem.singleton.PlaySFX("Menu DeSelect_Current");
        ChangeAnimationState("Dash");

        moveVector = lastNonZeroMoveVector.normalized;
        MovementSpeed = DASHING_MOVEMENT_SPEED;

        dashHitbox.SetActive(true); // Active
        
        float timePassed = 0;
        while (timePassed < Hitbox.DASH_ACTIVE_TIME) // Recovery
        {
            if (GameManager.InHitstop || PauseUI.isGamePaused)
            {
                yield return null;
                continue;
            }

            timePassed += Time.deltaTime;
            yield return null;
        }

        MovementSpeed = DEFAULT_MOVEMENT_SPEED;
        gameObject.layer = 0;

        yield return new WaitForSeconds(0.1f);
        isDashing = false;
    }

    public void OnAttackConnected()
    {
        invincibilityFromSuccessfulAttack = true;
        timeAtLastAttackConnected = Time.time;
    }

    public void HitByProjectile()
    {
        if (!GameManager.singleton.battleActive) return;

        GameObject vfx = Instantiate(deathVFXPrefab, myModel.position + new Vector3(0, 2f, 0), Quaternion.identity);
        Destroy(vfx, 2f);

        SFXSystem.singleton.PlaySFX("EnemyDeath2");
        GameManager.singleton.GameOver();

        ChangeAnimationState("Death", overridePlayAnimation: true);
    }
}