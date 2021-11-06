using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovingFolder : FolderClass
{

    #region Variables
    private Rigidbody2D rb;

    [Header("Move Components")]
    public int movingFolderIndex = 0;

    public float countdown;
    public float countTime = 2f;
    public float x;
    public float y;
    private Vector2 mv;
    public Vector2 randomPos;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        moveCountCoroutine += MoveCountdownCoroutine;

        movingFolderStruct = DataManager.Instance.MovingFolderStructList[movingFolderIndex];

        rb = GetComponent<Rigidbody2D>();
        countdown = countTime;
    }

    public override void InitializeFolderBehavior()
    {
        base.InitializeFolderBehavior();

        mv = GetRandomSpawnPosition().normalized * movingFolderStruct.currentSpeed;

        StartCoroutine(moveCountCoroutine());
    }

    public override void OnMouseOver()
    {
        base.OnMouseOver();
        if (isSelected)
        {
            // Can pass through the invisible walls while being dragged
            gameObject.layer = 12;

            // Resets countdown time
            countdown = countTime;
            StopCoroutine(moveCountCoroutine());
        }
    }

    private CoroutineDelegate moveCountCoroutine;
    /// <summary>
    /// Countdown to Random Movement
    /// </summary>
    /// <returns></returns>
    IEnumerator MoveCountdownCoroutine()
    {
        while (!affectedByAbility && !isSelected)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0)
            {
                mv = GetRandomSpawnPosition().normalized * movingFolderStruct.currentSpeed;
                countdown = countTime;
            }
            yield return new WaitForEndOfFrame();
        }
        if (affectedByAbility)
        {
            StartCoroutine(CheckForDropCoroutine());
        }
    }


    private IEnumerator CheckForDropCoroutine()
    {
        while (isSelected || affectedByAbility)
        {
            yield return new WaitForEndOfFrame();
        }
        DropFolder();
    }

    protected override void DropFolder()
    {
        base.DropFolder();
        // When dropped, moving folder can be stopped by the invisible walls
        mv = GetRandomSpawnPosition().normalized * movingFolderStruct.currentSpeed;
        gameObject.layer = 9;
        StartCoroutine(moveCountCoroutine());
    }

    void FixedUpdate()
    {
        if (!affectedByAbility && !isSelected)
        {
            rb.MovePosition(rb.position + mv * Time.fixedDeltaTime);
        }
    }

    #region Wall Bounce

    // Ensures the folder doesn't get stuck moving against a wall
    private bool canBounce = true;
    private BounceAid bounceAid;
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("InvisibleWall") && canBounce)
        {
            bounceAid = collision.gameObject.GetComponent<BounceAid>();
            StartCoroutine(DelayBounceCoroutine());
        }
    }

    private IEnumerator DelayBounceCoroutine()
    {
        canBounce = false;
        MoveAwayFromWall(bounceAid.Direction);
        yield return new WaitForSeconds(.25f);
        canBounce = true;
        bounceAid = null;
    }

    private void MoveAwayFromWall(string _direction)
    {
        Vector2 direction;
        switch (_direction)
        {
            case "Right":
                direction = new Vector2(-1, 0);
                mv = direction * movingFolderStruct.currentSpeed;
                break;
            case "Left":
                direction = new Vector2(1, 0);
                mv = direction * movingFolderStruct.currentSpeed;
                break;
            case "Up":
                direction = new Vector2(0, -1);
                mv = direction * movingFolderStruct.currentSpeed;
                break;
            case "Down":
                direction = new Vector2(0, 1);
                mv = direction * movingFolderStruct.currentSpeed;
                break;
        }
    }

    #endregion

    public override IEnumerator IncreaseSpeedCoroutine()
    {
        countTime = 1;
        return base.IncreaseSpeedCoroutine();
    }

    public override IEnumerator DecreaseSpeedCoroutine()
    {
        countTime = 2;
        return base.DecreaseSpeedCoroutine();
    }

    public override void ResetInteractable()
    {
        base.ResetInteractable();
        StartCoroutine(moveCountCoroutine());
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        moveCountCoroutine -= MoveCountdownCoroutine;
    }
}