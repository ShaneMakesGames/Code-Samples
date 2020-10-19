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
    public float speed;
    public float countdown;
    public float countTime = 2f;
    public float x;
    public float y;
    private Vector2 mv;
    public Vector2 randomPos;
    private int random;
    public Camera cam;
    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        countdown = countTime;
        cam = ReferenceManager.instance.spawnCam;
        RandomVector();
        StartCoroutine(MoveCountdownCoroutine());
    }

    public void OnMouseOver()
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

                StopCoroutine(MoveCountdownCoroutine());
                StartCoroutine(FolderSelectedCoroutine());
                StartCoroutine(CheckForDropCoroutine());
            }
            else if (!runaway && !affectedByAbility)
            {
                StartCoroutine(TriggerRunawayCoroutine());
            }
        }
    }

    /// <summary>
    /// Triggers the Folder to "Runaway" from the mouse, moving faster in a random direction
    /// </summary>
    /// <returns></returns>
    IEnumerator TriggerRunawayCoroutine()
    {
        runaway = true;
        Runaway();
        yield return new WaitForSeconds(.25f);
        runaway = false;

    }

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
                RandomVector();
                countdown = countTime;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator CheckForDropCoroutine()
    {
        while (isSelected)
        {
            yield return new WaitForEndOfFrame();
        }
        // When dropped, moving folder can be stopped by the invisible walls
        gameObject.layer = 10;
        StartCoroutine(MoveCountdownCoroutine());
    }



    void FixedUpdate()
    {
        if (!affectedByAbility && !isSelected)
        {
            rb.MovePosition(rb.position + mv * Time.fixedDeltaTime);
        }
    }


    void Runaway()
    {
        random = Random.Range(1, 5);
        if (random == 1)
        {
            y = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(Screen.height, 0)).y);
            x = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);
            randomPos = new Vector2(x, y);
            // Moves faster than normal while in Runaway state
            mv = randomPos.normalized * speed * 5;
        }
        if (random == 2)
        {
            y = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
            x = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);
            randomPos = new Vector2(x, y);
            mv = randomPos.normalized * speed * 5;
        }
        if (random == 3)
        {
            y = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(Screen.height, 0)).y);
            x = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(0, Screen.width)).x);
            randomPos = new Vector2(x, y);
            mv = randomPos.normalized * speed * 5;
        }
        if (random == 4)
        {
            y = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
            x = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(0, Screen.width)).x);
            randomPos = new Vector2(x, y);
            mv = randomPos.normalized * speed * 5;
        }
    }


    void RandomVector()
    {
        random = Random.Range(1, 5);
        if (random == 1)
        {
            y = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(Screen.height, 0)).y);
            x = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);
            randomPos = new Vector2(x, y);
            mv = randomPos.normalized * speed;
        }
        if (random == 2)
        {
            y = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
            x = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(Screen.width, 0)).x);
            randomPos = new Vector2(x, y);
            mv = randomPos.normalized * speed;
        }
        if (random == 3)
        {
            y = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(Screen.height, 0)).y);
            x = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(0, Screen.width)).x);
            randomPos = new Vector2(x, y);
            mv = randomPos.normalized * speed;
        }
        if (random == 4)
        {
            y = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(0, Screen.height)).y);
            x = Random.Range(0, cam.ScreenToWorldPoint(new Vector2(0, Screen.width)).x);
            randomPos = new Vector2(x, y);
            mv = randomPos.normalized * speed;
        }
    }
}