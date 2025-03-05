using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour, IDamageable, IEventSubscriber
{
    public Rigidbody rb;
    public float timeSinceSpawn;

    public const float PROJECTILE_SPEED = 22;
    public const float LIFETIME = 10f;

    public GameObject destroyVFXPrefab;

    private void Awake()
    {
        EventManager.Subscribe(EventType.HITSTOP_STARTED, this);
        EventManager.Subscribe(EventType.HITSTOP_ENDED, this);
        EventManager.Subscribe(EventType.GAME_PAUSED, this);
        EventManager.Subscribe(EventType.GAME_UNPAUSED, this);
    }

    public void FireProjectileForwards()
    {
        rb.linearVelocity = transform.right * PROJECTILE_SPEED;
    }

    void FixedUpdate()
    {
        if (!GameManager.singleton.battleActive) Destroy(gameObject);

        if (GameManager.InHitstop || PauseUI.isGamePaused) return;

        timeSinceSpawn += Time.deltaTime;
        if (timeSinceSpawn > LIFETIME)
        {
            GameObject vfx = Instantiate(destroyVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
            Destroy(gameObject);
        }
    }

    public void TakeDamage()
    {
        StartCoroutine(HitShakeCoroutine());
    }

    public IEnumerator HitShakeCoroutine()
    {
        GameObject vfx = Instantiate(destroyVFXPrefab, transform.position, Quaternion.identity);
        Destroy(vfx, 2f);

        float totalTimePassed = 0;
        float currentMoveTimePassed = 0;

        float shakeAmount = 0.25f;
        float shakeDuration = GameManager.ATTACK_HITSTOP_DURATION / 4;

        Vector3 startPos = transform.position;
        Vector3 posAtMoveStart = startPos;
        int sign = 1;
        Vector3 targetPos = new Vector3(startPos.x + (shakeAmount * sign), startPos.y, startPos.z + (shakeAmount * sign));

        while (totalTimePassed < GameManager.ATTACK_HITSTOP_DURATION)
        {
            if (currentMoveTimePassed < shakeDuration)
            {
                transform.position = Vector3.Lerp(posAtMoveStart, targetPos, currentMoveTimePassed / shakeDuration);
                currentMoveTimePassed += Time.deltaTime;
            }
            else
            {
                posAtMoveStart = transform.position;
                sign = -sign;
                targetPos = new Vector3(startPos.x + (shakeAmount * sign), startPos.y, startPos.z + (shakeAmount * sign));
                currentMoveTimePassed = 0;
            }

            totalTimePassed += Time.deltaTime;
            yield return null;
        }

        posAtMoveStart = transform.position;
        totalTimePassed = 0;

        while (totalTimePassed < shakeDuration)
        {
            transform.position = Vector3.Lerp(posAtMoveStart, startPos, totalTimePassed / shakeDuration);
            totalTimePassed += Time.deltaTime;
            yield return null;
        }

        while (GameManager.InHitstop)
        {
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.InHitstop || PauseUI.isGamePaused) return;

        if (other.CompareTag("Player"))
        {
            Player player = other.GetComponent<Player>();
            if (!player.isDashing && !player.invincibilityFromSuccessfulAttack && !player.preAttackInvincibility)
            {
                player.HitByProjectile();
                GameObject vfx = Instantiate(destroyVFXPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 2f);
                Destroy(gameObject);
            }
        }

        if (other.CompareTag("Invisible Wall"))
        {
            GameObject vfx = Instantiate(destroyVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 2f);
            Destroy(gameObject);
        }
    }

    public void EventReceived(EventType eventType)
    {
        if (eventType == EventType.HITSTOP_STARTED || eventType == EventType.GAME_PAUSED)
        {
            rb.linearVelocity = Vector3.zero;
        }
        else if (eventType == EventType.HITSTOP_ENDED || eventType == EventType.GAME_UNPAUSED)
        {
            FireProjectileForwards();
        }
    }

    private void OnDestroy()
    {
        EventManager.Unsubscribe(EventType.HITSTOP_STARTED, this);
        EventManager.Unsubscribe(EventType.HITSTOP_ENDED, this);
        EventManager.Unsubscribe(EventType.GAME_PAUSED, this);
        EventManager.Unsubscribe(EventType.GAME_UNPAUSED, this);
    }
}