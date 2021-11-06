using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FolderClass : MonoBehaviour
{
    #region Variables
    [Header("Bools")]
    public bool interactable; // Whether the folder can be interacted with
    public bool isSelected; // Whether the folder is being dragged
    public bool abilityProof;   // Whether a folder can be affected by an ability
    public bool abilityDrag;    // Whether a folder is being moved by an ability
    public bool affectedByAbility;  // Whether a folder's behavior is being stopped by an ability

    protected FolderClass folderClass;

    protected GameCursor gameCursor; // Reference to the gameCursor, used for updating cursor hover states 
    
    public static bool isMouseOver = false;

    [Header("Order in Layer")]
    public List<SpriteRenderer> RendererList = new List<SpriteRenderer>();

    public bool mouseOver;
    public bool MouseOver
    {
        get { return mouseOver; }
        set
        {
            if (mouseOver != value)
            {
                mouseOver = value;
                CheckMouseOver(mouseOver);
            }
        }
    }

    public void CheckMouseOver(bool value)
    {
        if (!gameCursor.holdingFolder)
        {
            if (value)
            {
                foreach (SpriteRenderer spr in RendererList)
                {
                    spr.sortingOrder++;
                }
                fileName.sortingOrder++;
                OffsetFunction();
            }
            else
            {
                foreach (SpriteRenderer spr in RendererList)
                {
                    spr.sortingOrder--;
                }
                fileName.sortingOrder--;
            }
        }
    }

    [Header("Spawn SFX")]
    public AudioSource folderAudio;
    public FolderSpawnSFXData SFXData;

    [Header("TMPRO")]
    public TextMeshPro fileName;
    public string[] fileNameList;

    [Header("Animation")]
    public bool animatedFolder;
    public Animator folderAnim;
    public Animator highlightAnim;

    [Header("Rare Folder")]
    public bool rareFolder;

    [Header("Moving Folder")]
    public bool movingFolder;
    public MovingFolderStruct movingFolderStruct;
    #endregion

    #region Folder Type & Animation

    protected virtual void Awake()
    {
        StartCoroutine(FolderSpawnAnimationCoroutine());

        folderClass = this;
        gameCursor = ReferenceManager.Instance.GetGameCursor();


        folderSelectCoroutine += FolderSelectedCoroutine;
        RemoveFromListFunction += BaseRemoveFromList;

        RandomAssignFolderType();

        if (animatedFolder)
        {
            StartFolderAnim();
        }
        else
        {
            ChangeHighlightVisual("NoHighlight");
        }
    }

    public Sprite malwareSPR;
    public Sprite personalSPR;

    protected void RandomAssignFolderType()
    {
        int random = Random.Range(1, 3);
        if (random == 1)
        {
            gameObject.tag = "Malware";
            fileNameList = new string[] { "fr33m0n3y", "PROOFbigfootisreal", "if-you-delete-me-you-die", "clerksFullMovieHD", "64M3T0RR3NT5",
                "open.source.hacks", "install-all-games.exe", "watch-movies-online.exe", "FREE-EPHONE.exe", "0N3W31RDTR1CK.exe",
                "virus.exe", "realAntivirus.exe", "actualAntivirus.exe", "forrealAntivirus.exe", "notaVirus.exe", "DeerHunter3DCRACKED.exe"};
            RendererList[1].sprite = malwareSPR; 
        }
        else if (random == 2)
        {
            gameObject.tag = "Personal";
            fileNameList = new string[] { "HawaiiTrip94", "quarterly-reports-89", "LisaSchoolwork", "college_junk", "personal_stuff",
                "tax_info", "lisaRecital", "medicalForms", "lisaPaintings", "riskNewProject", "Solitaire", "Pirate Adventure", "Paint",
                "Calculator", "Calendar", "DVD Burner", "Mailbox", "Photo Gallery", "Deer Hunter 3D", "Techno Bowl ‘95", "quarterly-reports-88",
                "quarterly-reports-87", "netWorthAnalysis", "MapJourney", "EasyBreeze Player", "Audio Player", "3D Design Tools"};
            RendererList[1].sprite = personalSPR;
        }
        fileName.text = fileNameList[Random.Range(0, fileNameList.Length)];
    }

    protected void AssignEncryptedFileName()
    {
        fileNameList = new string[] { "ASnJNDdakfn", "En34%mdlaf*&", "MOE391341JEWEFS", "#*$&!)", "*@JFAJ#!#!n",
                "9Jnjn3$!$!@!)*&", "OJIWEF&*)*)Adewa", "KnfwnOJQ913#!#!", "oij&(*&^@", "nmfewa#!#!@#!", "jsaofw#!#4$(!I$(", "afmoaEFOW#@!#", "jnfeakwEOFNWO@!#!",
                "Jnfjewn8137&3!", "WEF#!!73719WEfwawea", "jMNEFQWO213781", "J3&&#!^FOEWno", "JFNWOoj*$!$!897", "POINOQD321#&!#&!", "JnefuniwIHQ*721763*&%%", "JOnfoewno88231&&(#!", "JNOFN8&&^!*#^!*",
                "QDQnwqoBIBDIQ*^#!67", "NNqoNQUDB@!&#!", "HQHIbiby127*!^(", "ANOQDN*&13713", "DQO!#!&#!I!$Onmo"};
        fileName.text = fileNameList[Random.Range(0, fileNameList.Length)];
    }

    public void AssignFolderType(string folderType)
    {
        gameObject.tag = folderType;
        if (CompareTag("Malware"))
        {
            fileNameList = new string[] { "fr33m0n3y", "PROOFbigfootisreal", "if-you-delete-me-you-die", "clerksFullMovieHD", "64M3T0RR3NT5",
                "open.source.hacks", "install-all-games.exe", "watch-movies-online.exe", "FREE-EPHONE.exe", "0N3W31RDTR1CK.exe",
                "virus.exe", "realAntivirus.exe", "actualAntivirus.exe", "forrealAntivirus.exe", "notaVirus.exe", "DeerHunter3DCRACKED.exe"};
            RendererList[1].sprite = malwareSPR;
        }
        else if (CompareTag("Personal"))
        {
            fileNameList = new string[] { "HawaiiTrip94", "quarterly-reports-89", "LisaSchoolwork", "college_junk", "personal_stuff",
                "tax_info", "lisaRecital", "medicalForms", "lisaPaintings", "riskNewProject", "Solitaire", "Pirate Adventure", "Paint",
                "Calculator", "Calendar", "DVD Burner", "Mailbox", "Photo Gallery", "Deer Hunter 3D", "Techno Bowl ‘95", "quarterly-reports-88",
                "quarterly-reports-87", "netWorthAnalysis", "MapJourney", "EasyBreeze Player", "Audio Player", "3D Design Tools"};
            RendererList[1].sprite = personalSPR;
        }
        fileName.text = fileNameList[Random.Range(0, fileNameList.Length)];
    }

    public void AssignFolderTypeAndName(string folderType, string folderName)
    {
        gameObject.tag = folderType;
        if (CompareTag("Malware"))
        {
            RendererList[1].sprite = malwareSPR;
        }
        else if (CompareTag("Personal"))
        {
            RendererList[1].sprite = personalSPR;
        }
        fileName.text = folderName;
    }

    protected void StartFolderAnim()
    {
        ChangeHighlightVisual("NoHighlight");
        StartCoroutine(AnimationCoroutine());
    }

    protected void ChooseSpawnSFX()
    {
        //.clip = SFXData.audioClips[Random.Range(0, SFXData.audioClips.Count)];
    }

    private void ChangeAnimation()
    {
        StartCoroutine(AnimationCoroutine());
    }

    IEnumerator AnimationCoroutine()
    {
        int random = Random.Range(0, 4);
        folderAnim.SetTrigger(random.ToString());
        highlightAnim.SetTrigger(random.ToString());
        yield return new WaitForSeconds(20);
        ChangeAnimation();
    }
    #endregion

    #region Highlight Visuals

    [Header("Highlight Componenets")]
    public List<SpriteRenderer> HighlightList = new List<SpriteRenderer>();
    public Color white;
    public Color blue;
    public HighlightState highlightState;
    public enum HighlightState
    {
        NoHighlight,
        MouseOver,
        HoldingFolder
    }

    public void ChangeHighlightVisual(string variant)
    {
        if (interactable)
        {
            if (variant != highlightState.ToString())
            {
                if (variant == "NoHighlight")
                {
                    highlightState = HighlightState.NoHighlight;
                    // Text is white and normal
                    fileName.color = white;
                    fileName.fontStyle = FontStyles.Normal;
                    // Folder and Text Highlight Off
                    foreach (SpriteRenderer rndr in HighlightList)
                    {
                        rndr.enabled = false;
                    }
                }
                else if (variant == "MouseOver")
                {
                    highlightState = HighlightState.MouseOver;
                    // Text is blue and normal
                    fileName.color = blue;
                    fileName.fontStyle = FontStyles.Normal;
                    // Folder and Text Highlight Off
                    foreach (SpriteRenderer rndr in HighlightList)
                    {
                        rndr.enabled = false;
                    }
                }
                else if (variant == "HoldingFolder")
                {
                    highlightState = HighlightState.HoldingFolder;
                    // Text is white and underlined
                    fileName.color = white;
                    fileName.fontStyle = FontStyles.Underline;
                    // Folder and Text Highlight On
                    foreach (SpriteRenderer rndr in HighlightList)
                    {
                        rndr.enabled = true;
                    }
                }
            }
        }
    }

    public void ResetHighlight()
    {
        StartCoroutine(ResetHighlightCoroutine());
    }

    public void NoHighlight()
    {
        // Text is white and normal
        fileName.color = white;
        fileName.fontStyle = FontStyles.Normal;
        // Folder and Text Highlight Off
        foreach (SpriteRenderer rndr in HighlightList)
        {
            rndr.enabled = false;
        }
        highlightState = HighlightState.NoHighlight;
    }

    private IEnumerator ResetHighlightCoroutine()
    {
        yield return new WaitForSeconds(.5f);
        NoHighlight();
    }

    private float startScale;
    public GameObject spawnAnimPrefab;
    /// <summary>
    /// Plays the animation for the folder spawn
    /// </summary>
    /// <returns></returns>
    protected IEnumerator FolderSpawnAnimationCoroutine()
    {
        if (spawnAnimPrefab != null)
        {
            //ChooseSpawnSFX();
            folderAudio.Play();
            startScale = transform.localScale.x;
            affectedByAbility = true;
            GameObject obj = Instantiate(spawnAnimPrefab, transform.position, Quaternion.identity);
            transform.localScale = new Vector3(0, 0, 0);
            Destroy(obj, 2);
            yield return new WaitForSeconds(.5f);
            transform.localScale = new Vector3(startScale, startScale, startScale);
            yield return new WaitForSeconds(.2f);
        }
        InitializeFolderBehavior();
    }

    /// <summary>
    /// Runs once the Folder is active after the spawn animation
    /// </summary>
    public virtual void InitializeFolderBehavior()
    {
        affectedByAbility = false;
        runDefault = true;
        StartCoroutine(DefaultFolderCoroutine());
    }

    public virtual void OnEnable()
    {
        runDefault = true;
        StartCoroutine(DefaultFolderCoroutine());
    }

    /// <summary>
    /// Drops the folder you are holding if you alt tab
    /// </summary>
    /// <param name="focus"></param>
    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            if (gameCursor.holdingFolder && gameCursor.selectedFolder == folderClass)
            {
                //Debug.Log("Dropped");
                DropFolder();
            }
        }
    }

    public bool runDefault = true;
    protected IEnumerator DefaultFolderCoroutine()
    {
        while (runDefault)
        {
            // Drops folder if paused
            if (PauseMenu.isPaused)
            {
                if (gameCursor.holdingFolder && gameCursor.selectedFolder == folderClass)
                {
                    DropFolder();
                }
            }
            // Handles Highlight Visuals
            else if (!abilityDrag && !PopUp.popUpActive && !isSelected)
            {
                if (MouseOver)
                {
                    if (highlightState.ToString() != "MouseOver")
                    {
                        ChangeHighlightVisual("MouseOver");
                    }
                }
                else if (highlightState.ToString() != "NoHighlight")
                {
                    ChangeHighlightVisual("NoHighlight");
                }
            }
            // Folder is dropped if Left Mouse is let go of
            else if (Input.GetMouseButtonUp(0))
            {
                if (gameCursor.holdingFolder && gameCursor.selectedFolder == folderClass)
                {
                    DropFolder();
                }
            }
            yield return null;
        }
    }

    protected virtual void DropFolder()
    {
        isSelected = false;
        gameCursor.holdingFolder = false;
        gameCursor.selectedFolder = null;

        if (highlightState.ToString() != "NoHighlight")
        {
            ChangeHighlightVisual("NoHighlight");
        }
    }


    protected delegate IEnumerator CoroutineDelegate();
    protected CoroutineDelegate folderSelectCoroutine;
    protected IEnumerator FolderSelectedCoroutine()
    {
        // Normal mouse click selection & dragging
        while (isSelected)
        {
            if (highlightState.ToString() != "HoldingFolder")
            {
                ChangeHighlightVisual("HoldingFolder");
            }
            Vector2 pos = gameCursor.transform.position;
            pos.x += xOffset;
            pos.y += yOffset;
            transform.position = pos;
            yield return new WaitForEndOfFrame();
        }
    }

    public virtual void OnMouseOver()
    {
        if (interactable)
        {
            // Hovering over the folder
            if (!PopUp.popUpActive && !gameCursor.holdingFolder && !PauseMenu.isPaused)
            {
                MouseOver = true;
                isMouseOver = true;
                gameCursor.UpdateSprite(GameCursor.CursorState.Selected);
                // Dragging the folder
                if (Input.GetMouseButtonDown(0) && !isSelected)
                {
                    isSelected = true;
                    gameCursor.selectedFolder = folderClass;
                    gameCursor.holdingFolder = true;
                    StartCoroutine(folderSelectCoroutine());
                }
            }
        }
    }

    public void ModifyFolderSpeed(bool increaseSpeed)
    {
        if (increaseSpeed)
        {
            StartCoroutine(IncreaseSpeedCoroutine());
        }
        else
        {
            StartCoroutine(DecreaseSpeedCoroutine());
        }
    }

    public virtual IEnumerator IncreaseSpeedCoroutine()
    {
        while (gameObject.activeSelf && movingFolderStruct.currentSpeed < movingFolderStruct.maxSpeed && movingFolderStruct.accelerated)
        {
            movingFolderStruct.currentSpeed += Time.deltaTime;
            yield return null;
        }
        movingFolderStruct.currentSpeed = movingFolderStruct.maxSpeed;
    }

    public virtual IEnumerator DecreaseSpeedCoroutine()
    {
        while (gameObject.activeSelf && movingFolderStruct.currentSpeed > movingFolderStruct.defaultSpeed && !movingFolderStruct.accelerated)
        {
            movingFolderStruct.currentSpeed -= Time.deltaTime * 2;
            yield return null;
        }
        movingFolderStruct.currentSpeed = movingFolderStruct.defaultSpeed;
    }

    private void OnMouseExit()
    {
        if (!abilityDrag)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0) && isSelected)
            {
                isMouseOver = true;
                MouseOver = true;
            }
            // No ability active and player not holding Left Mouse
            else
            {
                isMouseOver = false;
                MouseOver = false;
            }
        }
        if (!gameCursor.holdingFolder)
        {
            gameCursor.UpdateSprite(GameCursor.CursorState.NotSelected);
        }
    }

    #endregion

    protected Vector2 FolderAreaHeightBounds = new Vector2(-3.0f, 1.9f);
    protected Vector2 FolderAreaWidthBounds = new Vector2(-4.4f, 4.2f);

    public Vector3 GetRandomSpawnPosition()
    {
        float spawnY = Random.Range(FolderAreaHeightBounds.x, FolderAreaHeightBounds.y);
        float spawnX = Random.Range(FolderAreaWidthBounds.x, FolderAreaWidthBounds.y);
        Vector3 spawnPosition = new Vector3(spawnX, spawnY, 0);
        return spawnPosition;
    }

    #region Cursor to Folder Offset 

    protected float xOffset;
    protected float yOffset;

    protected void OffsetFunction()
    {
        StopCoroutine(OffsetCoroutine());
        StartCoroutine(OffsetCoroutine());
    }

    protected IEnumerator OffsetCoroutine()
    {
        while (!abilityDrag && !PopUp.popUpActive && MouseOver && !isSelected)
        {
            xOffset = transform.position.x - ReferenceManager.Instance.cursorOBJ.transform.position.x;
            yOffset = transform.position.y - ReferenceManager.Instance.cursorOBJ.transform.position.y;
            yield return null;
        }
    }

    #endregion

    public virtual void FreezeInteractable()
    {
        interactable = false;
        affectedByAbility = true;
    }

    public virtual void ResetInteractable()
    {
        interactable = true;
        affectedByAbility = false;
    }

    public delegate void delegateFunction();
    public delegateFunction RemoveFromListFunction;
    public void BaseRemoveFromList()
    {
        if (FolderManager.Instance.allFolders.Contains(this))
        {
            FolderManager.Instance.allFolders.Remove(this);
        }

        if (CompareTag("Malware"))
        {
            FolderManager.Instance.malwareFolders.Remove(this);
        }
        else if (CompareTag("Personal"))
        {
            FolderManager.Instance.personalFolders.Remove(this);
        }
        if (FolderManager.Instance.NonSpiderFolderList.Contains(this))
        {
            FolderManager.Instance.NonSpiderFolderList.Remove(this);
        }
        if (movingFolder)
        {
            FolderManager.Instance.MovingFolderList.Remove(this);
        }
        if (!rareFolder)
        {
            FolderManager.Instance.NonRareFolderList.Remove(gameObject);
        }
        RemoveFromListFunction -= BaseRemoveFromList;
    }

    public void RemoveFromTrappedList()
    {
        FolderManager.Instance.TrappedFolderList.Remove(this);
        RemoveFromListFunction -= RemoveFromTrappedList;
    }

    public delegate void delegateFromOtherScript(FolderClass folderClass);
    public delegateFromOtherScript OtherScriptRemoveFromListFunction;

    protected virtual void OnDestroy()
    {
        RemoveFromListFunction();

        OtherScriptRemoveFromListFunction?.Invoke(this);

        folderSelectCoroutine -= FolderSelectedCoroutine;
        isMouseOver = false;
        if (gameCursor != null)
        {
            gameCursor.UpdateSprite(GameCursor.CursorState.NotSelected);

            if (gameCursor.selectedFolder == this)
            {
                gameCursor.selectedFolder = null;
                gameCursor.holdingFolder = false;
            }
        }
    }
}