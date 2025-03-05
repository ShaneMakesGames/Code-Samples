using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable, IEventSubscriber
{
    public GameObject myModel;
    public SkinnedMeshRenderer myMeshRenderer;

    public bool isActive;

    [Header("AI")]
    public Transform player;
    [SerializeField]
    private NavMeshAgent agent;

    public BoxCollider myCollider;
    public Animator anim;

    [Header("Attack")]
    public bool hasAttackToken;
    public bool onAttackCooldown;
    public bool updateAttackVisual;
    public GameObject projectilePrefab;

    public float attackCooldownTimer;
    public float attackTimer; // For attack start-up time
    public float timeBetweenAttacks;
    public SpriteRenderer attackVisual;
    public LookAtTarget lookAtTarget; // For the attack visual sprite

    public Material defaultEnemyMat;
    public Material takingDamageEnemyMat;

    public Material defaultAttackVisualMat;
    public Material activeAttackVisualMat;

    public GameObject deathVFXPrefab;
    public List<string> enemyDeathSFXList = new List<string>();

    public bool renderingPointer;

    public const float ATTACK_WARNING_TIME = 0.3f;
    public const float ATTACK_COOLDOWN_TIME = 0.25f;

    public void SetupEnemy()
    {
        isActive = true;
        anim.Play("Idle");
        attackVisual.transform.localScale = Vector3.zero;
        LeanTween.scale(myModel, Vector3.one * 1.25f, 0.5f);
        myCollider.enabled = true;

        attackTimer = 0;
        attackCooldownTimer = 0;

        // Random chase speed & other parameters
        timeBetweenAttacks = Random.Range(EnemyManager.singleton.currentAttackTimeRange.x, EnemyManager.singleton.currentAttackTimeRange.y);
        agent.speed = Random.Range(EnemyManager.singleton.currentSpeedRange.x, EnemyManager.singleton.currentSpeedRange.y);
        agent.acceleration = Random.Range(EnemyManager.singleton.currentAccelerationRange.x, EnemyManager.singleton.currentAccelerationRange.y);
        agent.stoppingDistance = Random.Range(EnemyManager.singleton.currentStoppingDistanceRange.x, EnemyManager.singleton.currentStoppingDistanceRange.y);

        EventManager.Subscribe(EventType.HITSTOP_STARTED, this);
        EventManager.Subscribe(EventType.HITSTOP_ENDED, this);
        EventManager.Subscribe(EventType.GAME_PAUSED, this);
        EventManager.Subscribe(EventType.GAME_UNPAUSED, this);
    }

    private void Update()
    {
        if (!isActive) return;
        if (GameManager.InHitstop || PauseUI.isGamePaused || !GameManager.singleton.battleActive) return;

        agent.destination = player.position;

        Vector3 playerPosWithoutY = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(playerPosWithoutY);

        float distanceFromPlayer = Vector3.Distance(GameManager.singleton.player.transform.position, transform.position);

        // Rendering pointers if enemy is offscreen
        Vector3 posToCheck = EnemyManager.singleton.gameCam.WorldToViewportPoint(transform.position);
        bool visibleInCamera = EnemyManager.PointVisibleInCamera(posToCheck);
        if (!visibleInCamera)
        {   
            renderingPointer = EnemyManager.singleton.TryRenderOffscreenEnemyArrow(transform.position, this, -transform.eulerAngles.y - 180f, distanceFromPlayer);
        }
        else if (renderingPointer)
        {
            EnemyManager.singleton.DisableEnemyPointer(this);
            renderingPointer = false;
        }

        if (onAttackCooldown)
        {
            attackCooldownTimer += Time.deltaTime;
            attackVisual.transform.localScale = Vector3.Lerp(Vector3.one * 2, Vector3.zero, attackCooldownTimer / ATTACK_COOLDOWN_TIME);

            if (attackCooldownTimer > ATTACK_COOLDOWN_TIME)
            {
                onAttackCooldown = false;
            }
            else return;
        }

        if (hasAttackToken)
        {            
            attackTimer += Time.deltaTime;
            attackVisual.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 2f, attackTimer / (timeBetweenAttacks - ATTACK_WARNING_TIME));
            if (!updateAttackVisual && attackTimer >= (timeBetweenAttacks - ATTACK_WARNING_TIME))
            {
                // Attack visual turns bright red & shakes right before the attack occurs
                StartCoroutine(ShakeAttackVisualCoroutine());
                updateAttackVisual = true;
                attackVisual.material = activeAttackVisualMat;
            }

            // If distance is too close to the player, delay the attack slightly
            float distanceModifier = distanceFromPlayer < 5f ? 0.25f : 0f;
            if (attackTimer >= timeBetweenAttacks + distanceModifier)
            {
                Attack();
            }
        }
    }

    public void GiveAttackToken()
    {
        hasAttackToken = true;
        attackVisual.color = Color.white;
        attackVisual.transform.localScale = Vector3.zero;
    }

    public void RemoveAttackToken()
    {
        hasAttackToken = false;

        EnemyManager.singleton.AttackingEnemiesList.Remove(this);
        EnemyManager.singleton.NonAttackingEnemiesList.Add(this);
    }

    public void EventReceived(EventType eventType)
    {
        if (eventType == EventType.HITSTOP_STARTED || eventType == EventType.GAME_PAUSED)
        {
            agent.speed = 0;
        }
        else if (eventType == EventType.HITSTOP_ENDED || eventType == EventType.GAME_UNPAUSED)
        {
            agent.speed = 5;
        }
    }

    public void Attack()
    {
        if (!hasAttackToken) return;

        SFXSystem.singleton.PlaySFX("FutureCrossbowShot", randomizePitch: true);

        attackVisual.color = Color.white;
        Projectile projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<Projectile>();
        projectile.transform.eulerAngles = new Vector3(0, transform.eulerAngles.y - 90f, 0);
        projectile.FireProjectileForwards();
        attackTimer = 0;
        attackCooldownTimer = 0;
        onAttackCooldown = true;
        attackVisual.material = defaultAttackVisualMat;
        updateAttackVisual = false;

        RemoveAttackToken();
    }

    public void TakeDamage()
    {
        isActive = false;
        attackVisual.transform.localScale = Vector3.zero;
        myCollider.enabled = false;

        if (renderingPointer) EnemyManager.singleton.DisableEnemyPointer(this);

        GameObject vfx = Instantiate(deathVFXPrefab, myModel.transform.position, Quaternion.identity);
        Destroy(vfx, 2f);

        StartCoroutine(HitstopCoroutine());
        StartCoroutine(HitShakeCoroutine());
    }

    private IEnumerator HitstopCoroutine()
    {
        myMeshRenderer.material = takingDamageEnemyMat;
        float timePassed = 0;
        while (GameManager.InHitstop)
        {
            timePassed += Time.deltaTime;
            yield return null;
        }
        myMeshRenderer.material = defaultEnemyMat;

        CleanUpEnemy();
    }

    public IEnumerator HitShakeCoroutine()
    {
        float totalTimePassed = 0;
        float currentMoveTimePassed = 0;

        float shakeAmount = 0.25f;
        float shakeDuration = GameManager.singleton.currentHitstopLength / 4;

        Vector3 startPos = myModel.transform.position;
        Vector3 posAtMoveStart = startPos;
        int sign = 1;
        Vector3 targetPos = new Vector3(startPos.x + (shakeAmount * sign), startPos.y, startPos.z + (shakeAmount * sign));

        while (totalTimePassed < GameManager.singleton.currentHitstopLength)
        {
            if (currentMoveTimePassed < shakeDuration)
            {
                myModel.transform.position = Vector3.Lerp(posAtMoveStart, targetPos, currentMoveTimePassed / shakeDuration);
                currentMoveTimePassed += Time.deltaTime;
            }
            else
            {
                posAtMoveStart = myModel.transform.position;
                sign = -sign;
                targetPos = new Vector3(startPos.x + (shakeAmount * sign), startPos.y, startPos.z + (shakeAmount * sign));
                currentMoveTimePassed = 0;
            }

            totalTimePassed += Time.deltaTime;
            yield return null;
        }

        posAtMoveStart = myModel.transform.position;
        totalTimePassed = 0;

        while (totalTimePassed < shakeDuration)
        {
            myModel.transform.position = Vector3.Lerp(posAtMoveStart, startPos, totalTimePassed / shakeDuration);
            totalTimePassed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator ShakeAttackVisualCoroutine()
    {
        float totalTimePassed = 0;
        float currentMoveTimePassed = 0;

        float shakeAmount = 0.125f;
        float shakeDuration = ATTACK_WARNING_TIME / 6f;

        Vector3 startPos = attackVisual.transform.localPosition;
        Vector3 posAtMoveStart = startPos;
        int sign = 1;
        Vector3 targetPos = new Vector3(startPos.x + (shakeAmount * sign), startPos.y, startPos.z + (shakeAmount * sign));

        while (totalTimePassed < ATTACK_WARNING_TIME)
        {
            if (currentMoveTimePassed < shakeDuration)
            {
                attackVisual.transform.localPosition = Vector3.Lerp(posAtMoveStart, targetPos, currentMoveTimePassed / shakeDuration);
                currentMoveTimePassed += Time.deltaTime;
            }
            else
            {
                posAtMoveStart = attackVisual.transform.localPosition;
                sign = -sign;
                targetPos = new Vector3(startPos.x + (shakeAmount * sign), startPos.y, startPos.z + (shakeAmount * sign));
                currentMoveTimePassed = 0;
            }

            totalTimePassed += Time.deltaTime;
            yield return null;
        }

        posAtMoveStart = attackVisual.transform.localPosition;
        totalTimePassed = 0;

        while (totalTimePassed < shakeDuration)
        {
            attackVisual.transform.localPosition = Vector3.Lerp(posAtMoveStart, startPos, totalTimePassed / shakeDuration);
            totalTimePassed += Time.deltaTime;
            yield return null;
        }
    }

    // Occurs for enemies that were still alive when the player dies, freezes the enemy's movements & disables attack visuals
    public void SetEnemyInactive()
    {
        EventManager.Unsubscribe(EventType.HITSTOP_STARTED, this);
        EventManager.Unsubscribe(EventType.HITSTOP_ENDED, this);
        EventManager.Unsubscribe(EventType.GAME_PAUSED, this);
        EventManager.Unsubscribe(EventType.GAME_UNPAUSED, this);

        isActive = false;
        renderingPointer = false;
        attackVisual.transform.localScale = Vector3.zero;
        attackVisual.material = defaultAttackVisualMat;

        onAttackCooldown = false;
        hasAttackToken = false;

        agent.destination = transform.position;
    }

    public void CleanUpEnemy()
    {
        EventManager.Unsubscribe(EventType.HITSTOP_STARTED, this);
        EventManager.Unsubscribe(EventType.HITSTOP_ENDED, this);
        EventManager.Unsubscribe(EventType.GAME_PAUSED, this);
        EventManager.Unsubscribe(EventType.GAME_UNPAUSED, this);

        isActive = false;
        renderingPointer = false;
        attackVisual.transform.localScale = Vector3.zero;
        attackVisual.material = defaultAttackVisualMat;

        EnemyManager.singleton.RemoveEnemyFromLists(this);
        onAttackCooldown = false;
        hasAttackToken = false;

        StartCoroutine(DeathAnimationCoroutine());
    }

    private IEnumerator DeathAnimationCoroutine()
    {
        anim.Play("Death");
        yield return new WaitForSeconds(0.5f);
        SFXSystem.singleton.PlayRandomSFX(enemyDeathSFXList, randomizePitch: true);
        yield return new WaitForSeconds(0.5f);
        LeanTween.scale(myModel, Vector3.zero, 0.5f);
        yield return new WaitForSeconds(0.5f);
        gameObject.SetActive(false);
    }
}