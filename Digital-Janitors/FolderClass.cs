using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class FolderClass : MonoBehaviour
{
    #region Variables
    [Header("Bools")]
    public bool isSelected; // Whether the folder is being dragged
    public bool abilityProof;   // Whether a folder can be affected by an ability
    public bool abilityDrag;    // Whether a folder is being moved by an ability
    public bool affectedByAbility;  // Whether a folder's behavior is being stopped by an ability

    protected GameCursor gameCursor;
    protected FolderClass folderClass;

    public enum FolderTypes
    {
        defaultFolder,
        encryptedFolder
    }
    public enum AnimatedFolder
    {
        folderAnim,
        noFolderAnim
    }
    
    [Header("Folder Specs")]
    public FolderTypes folderType;
    public AnimatedFolder animatedFolder;

    public EncryptedFolder encryptFolderRef;

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
                if (folderType != FolderTypes.encryptedFolder || encryptFolderRef != null && !encryptFolderRef.isEncrypted)
                {
                    foreach (SpriteRenderer spr in RendererList)
                    {
                        spr.sortingOrder++;
                    }
                    fileName.sortingOrder++;
                }
                OffsetFunction();
            }
            else
            {
                if (folderType != FolderTypes.encryptedFolder || encryptFolderRef != null && !encryptFolderRef.isEncrypted)
                {
                    foreach (SpriteRenderer spr in RendererList)
                    {
                        spr.sortingOrder--;
                    }
                    fileName.sortingOrder--;
                }
            }
        }
    }

    [Header("TMPRO")]
    public TextMeshPro fileName;
    public string[] fileNameList;

    [Header("Animation")]
    public Animator folderAnim;
    public Animator highlightAnim;
    #endregion

    #region Folder Animation

    private void Awake()
    {
        folderClass = this;
        gameCursor = ReferenceManager.instance.GetGameCursor();

        switch (folderType)
        {
            case FolderTypes.defaultFolder:
                RandomAssignFolderType();
                runDefault = true;
                StartCoroutine(DefaultFolderCoroutine());
                break;
            case FolderTypes.encryptedFolder:
                AssignEncryptedFileName();
                runDefault = true;
                StartCoroutine(DefaultFolderCoroutine());
                break;
            default:
                RandomAssignFolderType();
                break;
        }
        switch (animatedFolder)
        {
            case AnimatedFolder.folderAnim:
                StartFolderAnim();
                break;
            case AnimatedFolder.noFolderAnim:
                ChangeHighlightVisual("NoHighlight");
                break;
            default:
                StartFolderAnim();
                break;
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

    protected void StartFolderAnim()
    {
        ChangeHighlightVisual("NoHighlight");
        StartCoroutine(AnimationCoroutine());
    }

    void ChangeAnimation()
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

    public bool runDefault = true;
    protected IEnumerator DefaultFolderCoroutine()
    {
        while (runDefault)
        {
            // Drops folder if paused
            if (PauseMenu.isPaused)
            {
                DropFolder();
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
            // Drops folder if a pop up is active
            if (PopUp.popUpActive)
            {
                DropFolder();
            }
            // Folder is dropped if Left Mouse is let go of
            else if (Input.GetMouseButtonUp(0))
            {
                DropFolder();
            }
            yield return null;
        }
    }

    void DropFolder()
    {
        if (highlightState.ToString() != "NoHighlight")
        {
            ChangeHighlightVisual("NoHighlight");
        }
        if (!isSelected)
        {
            isSelected = false;
        }
        if (gameCursor.holdingFolder && gameCursor.selectedFolder == this)
        {
            gameCursor.holdingFolder = false;
            gameCursor.selectedFolder = null;
        }
    }

    protected IEnumerator FolderSelectedCoroutine()
    {
        // Normal mouse click selection & dragging
        while (isSelected)
        {
            if (highlightState.ToString() != "HoldingFolder")
            {
                ChangeHighlightVisual("HoldingFolder");
            }
            Vector2 pos = ReferenceManager.instance.cursorOBJ.transform.position;
            pos.x += xOffset;
            pos.y += yOffset;
            transform.position = pos;
            yield return new WaitForEndOfFrame();
        }
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
            xOffset = transform.position.x - ReferenceManager.instance.cursorOBJ.transform.position.x;
            yOffset = transform.position.y - ReferenceManager.instance.cursorOBJ.transform.position.y;
            yield return null;
        }
    }

    #endregion

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

    public void OnDestroy()
    {
        isMouseOver = false;
        gameCursor.UpdateSprite(GameCursor.CursorState.NotSelected);
        FolderManager.instance.allFolders.Remove(gameObject);
        if (CompareTag("Malware"))
        {
            FolderManager.instance.malwareFolders.Remove(gameObject);
        }
        else if (CompareTag("Personal"))
        {
            FolderManager.instance.personalFolders.Remove(gameObject);
        }
        if (gameCursor.selectedFolder == this)
        {
            gameCursor.selectedFolder = null;
            gameCursor.holdingFolder = false;
        }
    }
}