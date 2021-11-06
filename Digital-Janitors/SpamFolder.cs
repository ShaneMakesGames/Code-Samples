using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpamFolder : FolderClass
{
    #region Variables
    [Header("Spam")]
    public bool isLocked;
    public bool popUpOpen;
    private Vector2 PopUpAreaHeightBounds = new Vector2(-1.0f, 1.0f);
    private Vector2 PopUpAreaWidthBounds = new Vector2(-1.8f, 1.8f);

    [Header("Visuals")]
    public Sprite folderSprite;

    [Header("Pop Ups")]
    public bool spawnPopUps;
    public GameObject prefabPopUp;
    private int prefabPopUpIndex;
    public int numPopUps = 3;
    private bool popOpened;
    public Camera popCam;
    public AudioSource popSound;
    public List<GameObject> popList = new List<GameObject>();
    public List<int> popIndexList = new List<int>();
    public AudioSource SFXPlay;
    #endregion

    protected override void Awake()
    {
       
        StartCoroutine(FolderSpawnAnimationCoroutine());

        folderClass = this;
        gameCursor = ReferenceManager.Instance.GetGameCursor();

        folderSelectCoroutine = FolderSelectedCoroutine;
        RemoveFromListFunction = BaseRemoveFromList;

        AssignEncryptedFileName();
        runDefault = true;
        StartCoroutine(DefaultFolderCoroutine());

        ChangeHighlightVisual("NoHighlight");

        numPopUps = Random.Range(3, 6);

    }

    private void OnDisable()
    {
        if (popOpened && isLocked)
        {
            for (int i = 0; i < popList.Count; i++)
            {
                Destroy(popList[i]);
            }
            popList.Clear();
            popOpened = false;
            PopIndexReset();
        }
    }

    public override void InitializeFolderBehavior()
    {
        base.InitializeFolderBehavior();
       
        StartCoroutine(IncrementPopUpNumCoroutine());
    }

    IEnumerator IncrementPopUpNumCoroutine()
    {
        float f = 2;
        while (numPopUps != 5 && !popOpened)
        {
            f -= Time.deltaTime;
            if (f < 0)
            {
                f = 2;
                numPopUps++;
            }
            yield return null;
        }
    }

    public override void OnMouseOver()
    {
        if (interactable)
        {
            if (!PopUp.popUpActive && !gameCursor.holdingFolder && !popUpOpen && !PauseMenu.isPaused)
            {
                MouseOver = true;
                isMouseOver = true;
                gameCursor.UpdateSprite(GameCursor.CursorState.Selected);
                if (highlightState.ToString() != "MouseOver")
                {
                    ChangeHighlightVisual("MouseOver");
                }
                if (Input.GetMouseButtonDown(0) && !isSelected)
                {
                    if (!popOpened)
                    {
                        SFXPlay.Play();
                        popOpened = true;
                        StartCoroutine(SpawnPopUpsCoroutine());
                        checkPopNum = numPopUps;
                        StartCoroutine(CheckPopUpNumCoroutine());
                    }
                    else if (!isLocked)
                    {
                        isSelected = true;

                        gameCursor.holdingFolder = true;
                        gameCursor.selectedFolder = folderClass;

                        StartCoroutine(folderSelectCoroutine());
                    }
                }
            }
        }
    }

    IEnumerator SpawnPopUpsCoroutine()
    {
        int popNum = numPopUps;
        for (int i = 0; i < popNum; i++)
        {
            
            SpawnPop();
            yield return new WaitForSeconds(.2f);
        }
    }

    public int checkPopNum = 5;
    private IEnumerator CheckPopUpNumCoroutine()
    {
        while (checkPopNum != 0)
        {
            yield return null;
        }
        // If all Pop-Ups have been closed, Unlock the Folder
        UnlockFolder();
        popList.Clear();
    }

    public void PopIndexReset()
    {
        popIndexList.Clear();
        for (int i = 0; i < 5; i++)
        {
            popIndexList.Add(i);
        }
    }

    public void SpawnPop()
    {
        popSound.Play();
        Vector2 spawnPosition = GetRandomPopUpSpawnPosition();
        GameObject pop = Instantiate(prefabPopUp, spawnPosition, Quaternion.identity);
        popList.Add(pop);
        FolderManager.Instance.NonFolderObjects.Add(pop);
        PopUp popUp = pop.GetComponent<PopUp>();
        if (popIndexList.Count > 1)
        {
            prefabPopUpIndex = popIndexList[Random.Range(0, popIndexList.Count)];
        }
        else
        {
            prefabPopUpIndex = popIndexList[0];
        }
        popUp.AssignAnimation(prefabPopUpIndex, true);
        popIndexList.Remove(prefabPopUpIndex);
        popUp.spam = true;
        popUp.AssignSpamFolder(this);
    }

    public Vector2 GetRandomPopUpSpawnPosition()
    {
        float spawnY = Random.Range(PopUpAreaHeightBounds.x, PopUpAreaHeightBounds.y);
        float spawnX = Random.Range(PopUpAreaWidthBounds.x, PopUpAreaWidthBounds.y);
        Vector2 spawnPosition = new Vector2(spawnX, spawnY);
        return spawnPosition;
    }

    public void UnlockFolder()
    {
        
        isLocked = false;
        RendererList[2].gameObject.SetActive(false);
        RendererList[0].sprite = folderSprite;
        RendererList[1].gameObject.SetActive(true);

        RandomAssignFolderType();
        abilityProof = false;
    }
}