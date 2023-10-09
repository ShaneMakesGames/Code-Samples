using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using FMODUnity;
using FMOD.Studio;

public class DialogueManager : MonoBehaviour
{
    public static bool isActive;

    public const string DIALOGUE_PATH = "Dialogue/MasterDialogueSheet";

    public const string audioEventPath = "event:/Voices/";
    EventInstance dialogueAudioInstance;

    public static Dictionary<string, Dictionary<string, Conversation>> dictAllPooledDialogueByCharacter;
    public static Dictionary<string, Dictionary<string, Conversation>> dictAllUnpooledDialogueByCharacter;
    public static Dictionary<string, Queue<Conversation>> dictAllPrioritizedDialogueByCharacter;

    public string currentCharacterRef;

    private bool canInput = true;
    public const float INPUT_LOCKOUT_TIME = 0.1f;
    public const float LONGER_INPUT_LOCKOUT_TIME = 0.25f;
    bool dialogueStarted;
    int currentBranchIndex;
    bool dialogueBeingTyped;
    public bool skipTyping;
    public bool tutorialDialogue;

    public const float TYPING_TIME = 0.025f;

    Conversation currentConversation;

    [Header("Input Glyphs")]
    public static Dictionary<InputType, Dictionary<string, int>> dictAllGlyphIDByInputType;

    [Header("Dialogue Components")]
    public GameObject dialogueContainer;
    public TextMeshProUGUI characterText;
    public TextMeshProUGUI dialogueText;
    public GameObject portraitParent;
    public Image portraitImage;
    public Image confirmIcon;

    public string activeConversationRef;

    public const string DANDI = "dandi";
    public const string DESTINY = "destiny";
    public const string ULFHILDA = "ulfhilda";

    public List<string> allCharacterRefs = new List<string>();

    #region Singleton

    public static DialogueManager singleton;

    private void Awake()
    {
        RuntimeManager.LoadBank("Dialogue/Dialogue_Dandi");

        if (singleton == null)
        {
            singleton = this;
            DontDestroyOnLoad(this.gameObject);
            SetupGlyphIDs();
            LoadAllDialogue();
        }
        else Destroy(this.gameObject);
    }

    #endregion

    /// <summary>
    /// Sets up the glyphs for Keyboard & Mouse and controller input icons 
    /// </summary>
    private void SetupGlyphIDs()
    {
        Dictionary<string, int> xboxGlyphs = new Dictionary<string, int>()
        {
            { "light_attack", 23 },
            { "heavy_attack", 24 },
            { "interact", 14 },
            { "dash_1", 22 },
            { "dash_2", 1 },
        };

        Dictionary<string, int> playstationGlyphs = new Dictionary<string, int>()
        {
            { "light_attack", 27 },
            { "heavy_attack", 28 },
            { "interact", 17 },
            { "dash_1", 26 },
            { "dash_2", 5 },
        };

        Dictionary<string, int> keyboardGlyphs = new Dictionary<string, int>()
        {
            { "light_attack", 19 },
            { "heavy_attack", 20 },
            { "interact", 10 },
            { "dash_1", 18 },
            { "dash_2", 18 },
        };

        dictAllGlyphIDByInputType = new Dictionary<InputType, Dictionary<string, int>>()
        {
            { InputType.XInputController, xboxGlyphs },
            { InputType.DualShockGamepad, playstationGlyphs },
            { InputType.Mouse, keyboardGlyphs },
        };
    }

    public static string GetGlyphIDFromAction(string actionID)
    {
        InputType inputType = BattleManager.singleton.inputHandler.currentInputType;
        if (!dictAllGlyphIDByInputType[inputType].ContainsKey(actionID.ToLowerInvariant())) return null;

        return "<sprite="  + dictAllGlyphIDByInputType[inputType][actionID.ToLowerInvariant()] + ">";
    }

    private void LoadAllDialogue()
    {
        dictAllPooledDialogueByCharacter = new Dictionary<string, Dictionary<string, Conversation>>();
        dictAllUnpooledDialogueByCharacter = new Dictionary<string, Dictionary<string, Conversation>>();
        dictAllPrioritizedDialogueByCharacter = new Dictionary<string, Queue<Conversation>>();

        for (int i = 0; i < allCharacterRefs.Count; i++)
        {
            dictAllPooledDialogueByCharacter.Add(allCharacterRefs[i], new Dictionary<string, Conversation>());
            dictAllUnpooledDialogueByCharacter.Add(allCharacterRefs[i], new Dictionary<string, Conversation>());
            dictAllPrioritizedDialogueByCharacter.Add(allCharacterRefs[i], new Queue<Conversation>());
        }

        var loadedDialogue = Resources.Load<TextAsset>(DIALOGUE_PATH);
        string[] columns = loadedDialogue.text.Split('\n');

        for (int i = 0; i < columns.Length; i++)
        {
            if (i == 0) continue; // Skips the first column because it's just headers

            string[] unparsedRow = columns[i].Split('\t');

            // Pulls data from text file
            string conversationRef = unparsedRow[0];
            string characterRef = unparsedRow[2];
            string scriptOnDialogEnd = unparsedRow[8];


            // Dialogue has already been seen
            if (StoryProgress.encounteredDialogue.ContainsKey(conversationRef)) continue;

            bool priorityDialogue;
            if (!string.IsNullOrEmpty(unparsedRow[5])) priorityDialogue = bool.Parse(unparsedRow[5]);
            else priorityDialogue = false;

            Conversation conversation = GetConversationIfOneExists(characterRef, conversationRef, priorityDialogue);
            conversation.isPriorityDialogue = priorityDialogue;
            conversation.conversationRef = conversationRef;
            conversation.characterRef = characterRef;
            if (!string.IsNullOrEmpty(scriptOnDialogEnd)) conversation.scriptOnDialogueEnd = scriptOnDialogEnd;

            ConversationBranch conversationBranch = new ConversationBranch();

            if (!string.IsNullOrEmpty(unparsedRow[7]))
            {
                conversation.audioRef = unparsedRow[7];
            }

            for (int x = 0; x < unparsedRow.Length; x++)
            {
                string readValue = unparsedRow[x];
                if (string.IsNullOrEmpty(readValue)) continue;
                switch (x)
                {
                    case 1: // BranchRef
                        conversationBranch.branchRef = readValue.ToLowerInvariant();
                        break;
                    case 2: // CharacterRef
                        conversationBranch.characterRef = readValue.ToLowerInvariant();
                        break;
                    case 3: // Dialogue
                        conversationBranch.dialogue = readValue;
                        break;
                    case 4: // PortraitRef
                        conversationBranch.portraitRef = readValue.ToLowerInvariant();
                        break;
                    case 6: // Speaker
                        conversationBranch.speaker = readValue;
                        break;
                }
            }

            conversation.conversationBranches.Add(conversationBranch);
            AddConversationToAppropriateDictionary(characterRef, conversationRef, conversation, priorityDialogue);
        }

        AddConversationsToPriorityQueueFromSave();
    }

    private Conversation GetConversationIfOneExists(string charRef, string convoRef, bool priority)
    {
        Conversation c = new Conversation();
        if (priority)
        {
            // If the character dictionary does not exist, make one
            if (!dictAllUnpooledDialogueByCharacter.ContainsKey(charRef))
            {
                dictAllUnpooledDialogueByCharacter.Add(charRef, new Dictionary<string, Conversation>());
            }

            // Return existing conversation, otherwise return new conversation
            if (dictAllUnpooledDialogueByCharacter[charRef].ContainsKey(convoRef))
            {
                return dictAllUnpooledDialogueByCharacter[charRef][convoRef];
            }
            else
            {
                return c;
            }
        }
        else
        {
            // If the character dictionary does not exist, make one
            if (!dictAllPooledDialogueByCharacter.ContainsKey(charRef))
            {
                dictAllPooledDialogueByCharacter.Add(charRef, new Dictionary<string, Conversation>());
            }

            // Return existing conversation, otherwise return new conversation
            if (dictAllPooledDialogueByCharacter[charRef].ContainsKey(convoRef))
            {
                return dictAllPooledDialogueByCharacter[charRef][convoRef];
            }
            else
            {
                return c;
            }
        }
    }

    private void AddConversationToAppropriateDictionary(string charRef, string convoRef, Conversation c, bool priority)
    {
        if (priority)
        {
            if (dictAllUnpooledDialogueByCharacter[charRef].ContainsKey(convoRef))
            {
                dictAllUnpooledDialogueByCharacter[charRef][convoRef] = c;
            }
            else
            {
                dictAllUnpooledDialogueByCharacter[charRef].Add(convoRef, c);
            }
        }
        else
        {
            if (dictAllPooledDialogueByCharacter[charRef].ContainsKey(convoRef))
            {
                dictAllPooledDialogueByCharacter[charRef][convoRef] = c;
            }
            else
            {
                dictAllPooledDialogueByCharacter[charRef].Add(convoRef, c);
            }
        }
    }

    private void AddConversationsToPriorityQueueFromSave()
    {
        foreach (KeyValuePair<string, Conversation> kvp in StoryProgress.queuedDialogue)
        {
            Conversation c = kvp.Value;
            AddConversationToPriorityQueue(c.characterRef, kvp.Key);
        }
    }

    public static void AddConversationToPriorityQueue(string characterRef, string dialogueRef)
    {
        Conversation c = dictAllUnpooledDialogueByCharacter[characterRef][dialogueRef];
        dictAllUnpooledDialogueByCharacter[characterRef].Remove(dialogueRef);

        if (dictAllPrioritizedDialogueByCharacter.ContainsKey(characterRef))
        {
            dictAllPrioritizedDialogueByCharacter[characterRef].Enqueue(c);
        }
        else
        {
            Queue<Conversation> newQueue = new Queue<Conversation>();
            newQueue.Enqueue(c);
            dictAllPrioritizedDialogueByCharacter.Add(characterRef, newQueue);
        }

        if (StoryProgress.queuedDialogue.ContainsKey(c.conversationRef)) return;
        StoryProgress.queuedDialogue.Add(c.conversationRef, c);
    }

    public Conversation GetConversationForCharacter(string characterRef)
    {
        // Get prioritized dialogue if any
        if (dictAllPrioritizedDialogueByCharacter.ContainsKey(characterRef) && dictAllPrioritizedDialogueByCharacter[characterRef].Count > 0)
        {
            return dictAllPrioritizedDialogueByCharacter[characterRef].Dequeue();
        }

        if (!dictAllPooledDialogueByCharacter.ContainsKey(characterRef)) return null;
        // Otherwise, choose dialogue randomly from the pool
        int randomIndex = Random.Range(0, dictAllPooledDialogueByCharacter[characterRef].Values.Count);
        return dictAllPooledDialogueByCharacter[characterRef].ElementAt(randomIndex).Value;
    }

    public void SetupDialogue()
    {
        if (!canInput) return;
        if (dialogueStarted) return;

        isActive = true;
        dialogueStarted = true;
        dialogueContainer.SetActive(true);

        confirmIcon.sprite = SpriteManager.GetInputSprite(BattleManager.singleton.inputHandler.currentInputType, "confirm");

        if (currentCharacterRef == DANDI)
        {
            if (StoryProgress.currentBartenderDialogue != null) currentConversation = StoryProgress.currentBartenderDialogue;
        }

        if (currentConversation == null)
        {
            currentConversation = GetConversationForCharacter(currentCharacterRef);
            StoryProgress.encounteredDialogue.Add(currentConversation.conversationRef, currentConversation);

            if (currentCharacterRef == DANDI) StoryProgress.currentBartenderDialogue = currentConversation;
        }

        //Create audio event instance from file path based on currentConversation
        if (!string.IsNullOrEmpty(currentConversation.audioRef)) dialogueAudioInstance = RuntimeManager.CreateInstance(audioEventPath + currentConversation.audioRef);

        ConversationBranch cb = currentConversation.conversationBranches[currentBranchIndex];
        if (!string.IsNullOrEmpty(cb.portraitRef))
        {
            Sprite poraitSprite = SpriteManager.GetSprite(cb.portraitRef + SpriteManager.PORTRAIT);
            if (poraitSprite != null)
            {
                portraitParent.SetActive(true);
                portraitImage.enabled = true;
                portraitImage.sprite = poraitSprite;
            }
        }
        else portraitImage.enabled = false;

        characterText.text = cb.speaker;
        string dialogue = ParseDialogueForVariables(cb.dialogue);
        StartCoroutine(TypeOutDialogueCoroutine(dialogue));
        RuntimeManager.WaitForAllSampleLoading();
        if (!string.IsNullOrEmpty(currentConversation.audioRef)) dialogueAudioInstance.start();
        StartCoroutine(InputLockoutCoroutine(INPUT_LOCKOUT_TIME));
    }

    public void SetupNonNPCDialogue()
    {
        if (!canInput) return;
        if (dialogueStarted) return;
        if (currentConversation == null) return;

        Time.timeScale = 0;
        isActive = true;
        dialogueStarted = true;
        dialogueContainer.SetActive(true);

        confirmIcon.sprite = SpriteManager.GetInputSprite(BattleManager.singleton.inputHandler.currentInputType, "confirm");

        if (!string.IsNullOrEmpty(currentConversation.audioRef)) dialogueAudioInstance = RuntimeManager.CreateInstance(audioEventPath + currentConversation.audioRef);

        ConversationBranch cb = currentConversation.conversationBranches[currentBranchIndex];
        characterText.text = cb.speaker;
        string dialogue = ParseDialogueForVariables(cb.dialogue);
        StartCoroutine(TypeOutDialogueCoroutine(dialogue));
        if (!string.IsNullOrEmpty(currentConversation.audioRef)) dialogueAudioInstance.start();
        StartCoroutine(InputLockoutCoroutine(INPUT_LOCKOUT_TIME));
    }

    public IEnumerator DelayedSetupDialogueCoroutine(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        SetupNonNPCDialogue();
    }

    public void HandleDialogue()
    {
        if (!canInput) return;

        if (dialogueBeingTyped)
        {
            skipTyping = true;
            StartCoroutine(InputLockoutCoroutine(INPUT_LOCKOUT_TIME));
            return;
        }
        else
        {
            currentBranchIndex++;
            //Trigger the next sustain point cue in the FMOD event instance
            if (!string.IsNullOrEmpty(currentConversation.audioRef)) dialogueAudioInstance.setPaused(true);
            if (!string.IsNullOrEmpty(currentConversation.audioRef)) dialogueAudioInstance.keyOff();
            if (!string.IsNullOrEmpty(currentConversation.audioRef)) dialogueAudioInstance.setPaused(false);

            if (currentConversation.conversationBranches.Count > currentBranchIndex)
            {
                ConversationBranch cb = currentConversation.conversationBranches[currentBranchIndex];
                if (!string.IsNullOrEmpty(cb.portraitRef))
                {
                    Sprite poraitSprite = SpriteManager.GetSprite(cb.portraitRef + SpriteManager.PORTRAIT);
                    if (poraitSprite != null)
                    {
                        portraitParent.SetActive(true);
                        portraitImage.enabled = true;
                        portraitImage.sprite = poraitSprite;
                    }
                }
                else portraitImage.enabled = false;
                characterText.text = cb.speaker;
                string dialogue = ParseDialogueForVariables(cb.dialogue);
                StartCoroutine(TypeOutDialogueCoroutine(dialogue));
                StartCoroutine(InputLockoutCoroutine(INPUT_LOCKOUT_TIME));
            }
            else
            {
                if (!string.IsNullOrEmpty(currentConversation.audioRef)) dialogueAudioInstance.release();
                portraitParent.SetActive(false);
                ClearDialogue();
                StartCoroutine(InputLockoutCoroutine(LONGER_INPUT_LOCKOUT_TIME));
                Time.timeScale = 1;
                tutorialDialogue = false;
            }
        }
    }

    public static string ParseDialogueForVariables(string text)
    {
        if (!text.Contains("*")) return text;

        text = text.Replace("*light_attack", GetGlyphIDFromAction("light_attack"));
        text = text.Replace("*heavy_attack", GetGlyphIDFromAction("heavy_attack"));
        text = text.Replace("*interact", GetGlyphIDFromAction("interact"));
        text = text.Replace("*dash_1", GetGlyphIDFromAction("dash_1"));
        text = text.Replace("*dash_2", GetGlyphIDFromAction("dash_2"));

        return text;
    }

    public static void TrySetCurrentDialogue(string charRef, string convoRef, bool _tutorialDialogue, bool playDialogueWhenReady)
    {
        if (!isActive)
        {
            SetCurrentDialogue(charRef, convoRef, _tutorialDialogue, playDialogueWhenReady);
        }
        else
        {
            singleton.StartCoroutine(singleton.SetDialogueOnceCurrentDialogueHasEndedCoroutine(charRef, convoRef, _tutorialDialogue, playDialogueWhenReady));
        }
    }

    private IEnumerator SetDialogueOnceCurrentDialogueHasEndedCoroutine(string charRef, string convoRef, bool _tutorialDialogue, bool playDialogueWhenReady)
    {
        while (isActive)
        {
            yield return null;
        }

        SetCurrentDialogue(charRef, convoRef, _tutorialDialogue, playDialogueWhenReady);
    }

    private static void SetCurrentDialogue(string charRef, string convoRef, bool _tutorialDialogue, bool playDialogueWhenReady)
    {
        if (dictAllPooledDialogueByCharacter.ContainsKey(charRef))
        {
            if (dictAllPooledDialogueByCharacter[charRef].ContainsKey(convoRef))
            {
                singleton.currentConversation = dictAllPooledDialogueByCharacter[charRef][convoRef];
            }
        }
        else if (dictAllUnpooledDialogueByCharacter.ContainsKey(charRef))
        {
            if (dictAllUnpooledDialogueByCharacter[charRef].ContainsKey(convoRef))
            {
                singleton.currentConversation = dictAllUnpooledDialogueByCharacter[charRef][convoRef];
            }
        }

        if (singleton.currentConversation != null)
        {
            singleton.activeConversationRef = singleton.currentConversation.conversationRef;
            singleton.tutorialDialogue = _tutorialDialogue;
        }

        if (playDialogueWhenReady)
        {
            singleton.StartCoroutine(singleton.DelayedSetupDialogueCoroutine(0.75f));
        }
    }

    public void ClearDialogue()
    {
        if (!string.IsNullOrEmpty(currentConversation.scriptOnDialogueEnd))
        {
            if (!currentConversation.scriptHasRun) RunScriptFromDialogue(currentConversation);
        }

        if (StoryProgress.queuedDialogue.ContainsKey(currentConversation.conversationRef))
        {
            StoryProgress.queuedDialogue.Remove(currentConversation.conversationRef);
        }

        if (tutorialDialogue)
        {
            currentConversation = null;
            activeConversationRef = null;
        }

        currentBranchIndex = 0;
        dialogueStarted = false;
        isActive = false;
        dialogueContainer.SetActive(false);
    }

    public static void ClearCurrentDialogueData()
    {
        singleton.currentConversation = null;
        singleton.currentBranchIndex = 0;
        singleton.dialogueStarted = false;
        isActive = false;
        singleton.dialogueContainer.SetActive(false);
    }

    private IEnumerator TypeOutDialogueCoroutine(string dialogue)
    {
        dialogueBeingTyped = true;
        dialogueText.text = "";
        string typedDialogue = "";

        for (int i = 0; i < dialogue.Length; i++)
        {
            if (skipTyping)
            {
                dialogueText.text = dialogue;
                skipTyping = false;
                break;
            }

            if (dialogue[i] == '<') // Instantly adds the glyph to the text
            {
                string textToAddInstantly = "";
                int loopAmount = dialogue.Length - i;
                int increaseBy = -1;

                for (int x = 0; x < loopAmount; x++)
                {
                    char c = dialogue[i + x];
                    textToAddInstantly += c;
                    increaseBy++;

                    if (c == '>') break;
                }

                typedDialogue += textToAddInstantly;
                dialogueText.text = typedDialogue;
                i += increaseBy;
                continue;
            }

            typedDialogue += dialogue[i];
            dialogueText.text = typedDialogue;
            yield return new WaitForSecondsRealtime(TYPING_TIME);
        }
        dialogueBeingTyped = false;
    }

    private IEnumerator InputLockoutCoroutine(float timeToWait)
    {
        canInput = false;
        yield return new WaitForSecondsRealtime(timeToWait);
        canInput = true;
    }

    public void RunScriptFromDialogue(Conversation conversation)
    {
        string addZeroIfNecessary = "";
        string conversationRef = "";
        switch (conversation.scriptOnDialogueEnd.ToLowerInvariant())
        {
            case "get_axe":
                StoryProgress.SetProgress(StoryProgressEvent.FIRST_BARTENDER_CONVERSATION, StoryProgressValue.COMPLETE);
                GiveAxeToPlayer();
                OpenSceneChangerIfPossible();
                break;
            case "destiny_dialogue":
                StoryProgress.destinyDialogueIndex++;
                addZeroIfNecessary = "_";
                if (StoryProgress.destinyDialogueIndex < 10) addZeroIfNecessary += "0";
                conversationRef = DESTINY + addZeroIfNecessary + StoryProgress.destinyDialogueIndex;
                AddConversationToPriorityQueue(DESTINY, conversationRef);
                AddConversationToPriorityQueue(DANDI, DANDI + "_0" + (4 + StoryProgress.destinyDialogueIndex));
                BattleManager.GetPlayerHealth().TakeDamage(100, "Destiny");
                GameUI.singleton.StartCoroutine(GameUI.singleton.WarningVisualCoroutine(3));
                currentCharacterRef = null;
                break;
            case "ulfhilda_dialogue":
                BossCinematicController.singleton.StartBossFight();
                StoryProgress.ulfhildaDialogueIndex++;
                addZeroIfNecessary = "_";
                if (StoryProgress.ulfhildaDialogueIndex < 10) addZeroIfNecessary += "0";
                conversationRef = ULFHILDA + addZeroIfNecessary + StoryProgress.ulfhildaDialogueIndex;
                AddConversationToPriorityQueue(ULFHILDA, conversationRef);
                break;
        }
        conversation.scriptHasRun = true;
    }

    public static void ResetAllEncounteredDialogue()
    {
        foreach (KeyValuePair<string, Conversation> kvp in StoryProgress.encounteredDialogue)
        {
            Conversation c = kvp.Value;
            c.scriptHasRun = false;
            if (c.isPriorityDialogue)
            {
                if (dictAllUnpooledDialogueByCharacter[c.characterRef].ContainsKey(c.conversationRef)) continue;

                dictAllUnpooledDialogueByCharacter[c.characterRef].Add(c.conversationRef, c);

            }
            else
            {
                if (dictAllPooledDialogueByCharacter[c.characterRef].ContainsKey(c.conversationRef)) continue;

                dictAllPooledDialogueByCharacter[c.characterRef].Add(c.conversationRef, c);
            }
        }

        foreach (KeyValuePair<string, Conversation> kvp in StoryProgress.queuedDialogue)
        {
            dictAllUnpooledDialogueByCharacter[kvp.Value.characterRef].Add(kvp.Value.conversationRef, kvp.Value);
        }

        dictAllPrioritizedDialogueByCharacter = new Dictionary<string, Queue<Conversation>>();
        for (int i = 0; i < singleton.allCharacterRefs.Count; i++)
        {
            dictAllPrioritizedDialogueByCharacter.Add(singleton.allCharacterRefs[i], new Queue<Conversation>());
        }
    }

    private void GiveAxeToPlayer()
    {
        BattleManager.singleton.currentEquippedWeapon.DisableModels();
        BattleManager.EquipWeapon(WeaponType.BATTLE_AXE);
    }

    private void OpenSceneChangerIfPossible()
    {
        if (BattleManager.singleton.currentRoom.sceneChangerOBJ == null) return;
        BattleManager.singleton.currentRoom.sceneChangerOBJ.SetActive(true);
    }
}