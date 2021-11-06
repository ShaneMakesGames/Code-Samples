using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncryptedFolder : FolderClass
{
    #region Variables
    [Header("Encryption")]
    public bool isEncrypted;
    private GameObject decrypterOBJ;
    private DecrypterController decrypter;
    public GameObject decrypterPrefab;

    [Header("Visuals")]
    public Sprite folderSprite;
    #endregion

    protected override void Awake()
    {
        StartCoroutine(FolderSpawnAnimationCoroutine());

        folderClass = GetComponent<FolderClass>();
        gameCursor = ReferenceManager.Instance.GetGameCursor();

        AssignEncryptedFileName();
        runDefault = true;
        StartCoroutine(DefaultFolderCoroutine());
        folderSelectCoroutine += FolderSelectedCoroutine;
        RemoveFromListFunction += BaseRemoveFromList;

        ChangeHighlightVisual("NoHighlight");
    }

    private IEnumerator DecrypterSetupCoroutine()
    {
        yield return new WaitForEndOfFrame();
        if (transform.position.y <= .75f)
        {
            decrypterOBJ = Instantiate(decrypterPrefab, new Vector3(transform.position.x, transform.position.y + 2.3f, 0), Quaternion.identity);
        }
        else
        {
            decrypterOBJ = Instantiate(decrypterPrefab, new Vector3(transform.position.x, transform.position.y - 2.6f, 0), Quaternion.identity);
        }
        FolderManager.Instance.NonFolderObjects.Add(decrypterOBJ);
        decrypter = decrypterOBJ.GetComponent<DecrypterController>();
        decrypter.encryptedFolder = this;
    }

    private void DecrypterSetup()
    {
        StartCoroutine(DecrypterSetupCoroutine());
    }

    public override void OnMouseOver()
    {
        if (interactable)
        {
            if (!PopUp.popUpActive && !gameCursor.holdingFolder && !PauseMenu.isPaused)
            {
                MouseOver = true;
                isMouseOver = true;
                gameCursor.UpdateSprite(GameCursor.CursorState.Selected);
                if (Input.GetMouseButtonDown(0) && !isSelected)
                {
                    if (isEncrypted)
                    {
                        if (!decrypter.isOpen)
                        {
                            StartCoroutine(decrypter.OpenDecrypterCoroutine());
                        }
                    }
                    else
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

    private void OnDisable()
    {
        if (decrypter != null && isEncrypted)
        {
            decrypter.anim.SetTrigger("Close");
            Destroy(decrypter.gameObject);
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (decrypter == null && isEncrypted)
        {
            DecrypterSetup();
        }
    }

    public void DecryptFolder()
    {
        RendererList[0].sprite = folderSprite;
        RendererList[1].gameObject.SetActive(true);

        RandomAssignFolderType();
        abilityProof = false;
        Destroy(decrypterOBJ);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        folderSelectCoroutine -= FolderSelectedCoroutine;
        RemoveFromListFunction -= BaseRemoveFromList;
    }
}