using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RunawayFolder : FolderClass
{
    #region Variables
    private Rigidbody2D rb;
    [Header("Booleans")]
    public bool runaway;

    [Header("Move Components")]
    public int movingFolderIndex = 0;

    public AudioSource RunawaySFX;

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
        moveCountCoroutine = MoveCountdownCoroutine;

        RunawaySFX = GetComponent<AudioSource>();

        movingFolderStruct = DataManager.Instance.MovingFolderStructList[movingFolderIndex];

        rb = GetComponent<Rigidbody2D>();
        countdown = countTime;
    }

    public override void InitializeFolderBehavior()
    {
        base.InitializeFolderBehavior();

        mv = GetRandomSpawnPosition().normalized * movingFolderStruct.currentSpeed * 4;

        StartCoroutine(moveCountCoroutine());
    }

    public override void OnMouseOver()
    {
        if (interactable)
        {
            if (!PopUp.popUpActive && !gameCursor.holdingFolder && !PauseMenu.isPaused)
            {
                isMouseOver = true;
                MouseOver = true;
                gameCursor.UpdateSprite(GameCursor.CursorState.Selected);
                if (Input.GetMouseButtonDown(0) && !isSelected)
                {
                    // Can pass through the invisible walls while being dragged
                    gameObject.layer = 12;

                    isSelected = true;
                    gameCursor.holdingFolder = true;
                    gameCursor.selectedFolder = folderClass;

                    // Resets countdown time
                    countdown = countTime;

                    StopCoroutine(moveCountCoroutine());
                    StartCoroutine(folderSelectCoroutine());
                }
                else if (!runaway && !affectedByAbility)
                {
                    StartCoroutine(TriggerRunawayCoroutine());
                }
            }
        }
    }

    /// <summary>
    /// Triggers the Folder to "Runaway" from the mouse, moving faster in a random direction
    /// </summary>
    /// <returns></returns>
    IEnumerator TriggerRunawayCoroutine()
    {
        RunawaySFX.Play();

        runaway = true;
        movingFolderStruct.currentSpeed *= 4;
        mv = GetRandomSpawnPosition().normalized * movingFolderStruct.currentSpeed ;
        yield return new WaitForSeconds(.25f);
        runaway = false;
        movingFolderStruct.currentSpeed /= 4;

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
    }

    protected override void DropFolder()
    {
        base.DropFolder();
        // When dropped, moving folder can be stopped by the invisible walls
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

    public override void ResetInteractable()
    {
        base.ResetInteractable();
        StartCoroutine(moveCountCoroutine());
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        moveCountCoroutine = null;
    }

}