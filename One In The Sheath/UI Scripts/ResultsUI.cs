using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;

public class ResultsUI : MonoBehaviour, IMenuScreen
{
    public GameObject canvasOBJ;

    public GameObject SparkPrefab;

    public VolumeProfile volumeProfile;
    private Tonemapping tonemapping;

    public bool isMenuAnimating;

    [Header("Ranking")]
    public string rankName;
    public Image rankImage;
    public Image rankOutline;

    public const float RANK_ICON_MAX_SCALE = 1.5f;
    public const float RANK_ICON_ANIM_TIME = 0.15f;

    public List<Sprite> RankIcons = new List<Sprite>();
    public List<Color> RankColors = new List<Color>();

    public const int UNRANKED_CUTOFF_AMOUNT = 50000;
    public const int BRONZE_CUTOFF_AMOUNT = 100000;
    public const int SILVER_CUTOFF_AMOUNT = 150000;
    public const int GOLD_CUTOFF_AMOUNT = 200000;
    public const int DIAMOND_CUTOFF_AMOUNT = 250000;
    public const int RUBY_CUTOFF_AMOUNT = 300000;

    [Header("Stats")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI killText;
    public TextMeshProUGUI parryText;

    public List<string> TypingSFXList = new List<string>();

    [Header("Progress Bar")]
    public Image progressFill;
    public int progressIconAnimationIndex;
    public List<float> ProgressIconPositions;
    public List<Image> ProgressRankIcons = new List<Image>();
    public List<Image> ProgressRankFills = new List<Image>();

    public Vector2 SmallRankIconSizeDelta;
    public Vector2 LargeRankIconSizeDelta;

    public Vector2 SmallFillSizeDelta;
    public Vector2 SmallFillVector;
    public Vector2 LargeFillSizeDelta;
    public Vector2 LargeFillVector;

    public const float PROGRESS_FILL_STARTING_POSITION = -509;
    public const float MAX_PROGRESS_FILL_AMOUNT = 510;

    public const float PROGRESS_BAR_TOTAL_ANIM_TIME_QUICK = 0.35f;
    public const float PROGRESS_BAR_TOTAL_ANIM_TIME = 1f;
    public const float PROGRESS_BAR_ICON_ANIM_TIME = 0.25f;

    [Header("Scroll")]
    public Image leftScrollImage;
    public Image animatedScrollBGImage;
    public Image rightScrollImage;
    public Image fullScrollImage;
    public Animator scrollSparkAnim;

    public const float LEFT_SCROLL_START_POSITION = -30f;
    public const float LEFT_SCROLL_END_POSITION = -250f;
    public const float RIGHT_SCROLL_START_POSITION = 30f;
    public const float RIGHT_SCROLL_END_POSITION = 250f;
    public const float SCROLL_ANIM_TIME = 0.35f;

    public TextMeshProUGUI rankText;

    public string gamepadDisplayName;
    public Image confirmIcon;
    public Sprite xboxConfirmSprite;
    public Sprite playstationConfirmSprite;
    public Image cancelIcon;
    public Sprite xboxCancelSprite;
    public Sprite playstationCancelSprite;

    public const float TYPING_ANIM_TIME = 0.05f;
    public const float TIME_BETWEEN_TEXT_ANIMATIONS = 0.05f;

    #region Singleton

    public static ResultsUI singleton;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else Destroy(this.gameObject);

        Initialize();
    }

    #endregion
    
    private void Initialize()
    {
        volumeProfile.TryGet(out tonemapping);

        ProgressIconPositions = new List<float>();
        float scorePercent = (float)UNRANKED_CUTOFF_AMOUNT / RUBY_CUTOFF_AMOUNT;
        ProgressRankIcons[0].transform.localPosition = new Vector3(-250f + (scorePercent * 500f), 4f, 0);
        float calculatedPos = PROGRESS_FILL_STARTING_POSITION + (MAX_PROGRESS_FILL_AMOUNT * scorePercent);
        ProgressIconPositions.Add(calculatedPos);
        scorePercent = (float)BRONZE_CUTOFF_AMOUNT / RUBY_CUTOFF_AMOUNT;
        ProgressRankIcons[1].transform.localPosition = new Vector3(-250f + (scorePercent * 500f), 4f, 0);
        calculatedPos = PROGRESS_FILL_STARTING_POSITION + (MAX_PROGRESS_FILL_AMOUNT * scorePercent);
        ProgressIconPositions.Add(calculatedPos);
        scorePercent = (float)SILVER_CUTOFF_AMOUNT / RUBY_CUTOFF_AMOUNT;
        ProgressRankIcons[2].transform.localPosition = new Vector3(-250f + (scorePercent * 500f), 4f, 0);
        calculatedPos = PROGRESS_FILL_STARTING_POSITION + (MAX_PROGRESS_FILL_AMOUNT * scorePercent);
        ProgressIconPositions.Add(calculatedPos);
        scorePercent = (float)GOLD_CUTOFF_AMOUNT / RUBY_CUTOFF_AMOUNT;
        ProgressRankIcons[3].transform.localPosition = new Vector3(-250f + (scorePercent * 500f), 4f, 0);
        calculatedPos = PROGRESS_FILL_STARTING_POSITION + (MAX_PROGRESS_FILL_AMOUNT * scorePercent);
        ProgressIconPositions.Add(calculatedPos);
        scorePercent = (float)DIAMOND_CUTOFF_AMOUNT / RUBY_CUTOFF_AMOUNT;
        ProgressRankIcons[4].transform.localPosition = new Vector3(-250f + (scorePercent * 500f), 4f, 0);
        calculatedPos = PROGRESS_FILL_STARTING_POSITION + (MAX_PROGRESS_FILL_AMOUNT * scorePercent);
        ProgressIconPositions.Add(calculatedPos);
    }

    private IEnumerator OpenResultsScreenCoroutine()
    {
        isMenuAnimating = true;
        rankImage.sprite = RankIcons[0];

        yield return new WaitForSeconds(0.5f);

        int score = GameManager.singleton.score;
        Sprite rankSprite = GetRankIconFromScore(score);
        rankName = rankSprite.name.ToUpperInvariant();

        string printString = score.ToString("n0");
        StartCoroutine(TypingCoroutine(scoreText, printString));
        yield return new WaitForSeconds(printString.Length * TYPING_ANIM_TIME + TIME_BETWEEN_TEXT_ANIMATIONS);

        printString = GameManager.GetBattleStat(GameManager.WAVE_COUNT_STRING).ToString();
        StartCoroutine(TypingCoroutine(waveText, printString));
        yield return new WaitForSeconds(printString.Length * TYPING_ANIM_TIME + TIME_BETWEEN_TEXT_ANIMATIONS);

        printString = GameManager.GetBattleStat(GameManager.HIGHEST_COMBO_STRING).ToString();
        StartCoroutine(TypingCoroutine(comboText, printString));
        yield return new WaitForSeconds(printString.Length * TYPING_ANIM_TIME + TIME_BETWEEN_TEXT_ANIMATIONS);

        printString = GameManager.GetBattleStat(GameManager.KILLS_STRING).ToString();
        StartCoroutine(TypingCoroutine(killText, printString));
        yield return new WaitForSeconds(printString.Length * TYPING_ANIM_TIME + TIME_BETWEEN_TEXT_ANIMATIONS);

        printString = GameManager.GetBattleStat(GameManager.PARRIES_STRING).ToString();
        StartCoroutine(TypingCoroutine(parryText, printString));
        yield return new WaitForSeconds(printString.Length * TYPING_ANIM_TIME + TIME_BETWEEN_TEXT_ANIMATIONS);

        float animTime = score < UNRANKED_CUTOFF_AMOUNT ? PROGRESS_BAR_TOTAL_ANIM_TIME_QUICK : PROGRESS_BAR_TOTAL_ANIM_TIME;
        StartCoroutine(AnimateProgressBarCoroutine(animTime));
        yield return new WaitForSeconds(animTime + TIME_BETWEEN_TEXT_ANIMATIONS);
        StartCoroutine(AnimateScrollsCoroutine());
    }

    private Sprite GetRankIconFromScore(int score)
    {
        if (score < UNRANKED_CUTOFF_AMOUNT) return RankIcons[0];
        if (score < BRONZE_CUTOFF_AMOUNT) return RankIcons[1];
        if (score < SILVER_CUTOFF_AMOUNT) return RankIcons[2];
        if (score < GOLD_CUTOFF_AMOUNT) return RankIcons[3];
        if (score < DIAMOND_CUTOFF_AMOUNT) return RankIcons[4];
        if (score >= DIAMOND_CUTOFF_AMOUNT) return RankIcons[5];

        return RankIcons[0];
    }

    private IEnumerator TypingCoroutine(TextMeshProUGUI _text, string stringToUse)
    {
        for (int i = 0; i < stringToUse.Length; i++)
        {
            _text.text += stringToUse[i];
            SFXSystem.singleton.PlayRandomSFX(TypingSFXList, randomizePitch:true);
            yield return new WaitForSeconds(TYPING_ANIM_TIME);
        }
    }

    private IEnumerator AnimateProgressBarCoroutine(float animTime)
    {
        float scorePercent = Mathf.Clamp((float)GameManager.singleton.score / RUBY_CUTOFF_AMOUNT, 0f, 1f);
        float targetPos = PROGRESS_FILL_STARTING_POSITION + (MAX_PROGRESS_FILL_AMOUNT * scorePercent);

        float timePassed = 0;
        while (timePassed < animTime)
        {
            timePassed += Time.deltaTime;
            float lerpedPos = Mathf.Lerp(PROGRESS_FILL_STARTING_POSITION, targetPos, timePassed / animTime);
            progressFill.transform.localPosition = new Vector3(lerpedPos, 0, 0);
            CheckForProgressIconAnimation(lerpedPos);
            yield return null;
        }
    }

    private void CheckForProgressIconAnimation(float lerpedPos)
    {
        bool animOccured = false;

        if (progressIconAnimationIndex == 0 && lerpedPos >= ProgressIconPositions[0])
        {
            StartCoroutine(AnimateProgressRankIconCoroutine(ProgressRankIcons[0], ProgressRankFills[0]));
            progressIconAnimationIndex++;
            animOccured = true;
        }
        if (progressIconAnimationIndex == 1 && lerpedPos >= ProgressIconPositions[1])
        {
            StartCoroutine(AnimateProgressRankIconCoroutine(ProgressRankIcons[1], ProgressRankFills[1]));
            progressIconAnimationIndex++;
            animOccured = true;
        }
        if (progressIconAnimationIndex == 2 && lerpedPos >= ProgressIconPositions[2])
        {
            StartCoroutine(AnimateProgressRankIconCoroutine(ProgressRankIcons[2], ProgressRankFills[2]));
            progressIconAnimationIndex++;
            animOccured = true;
        }
        if (progressIconAnimationIndex == 3 && lerpedPos >= ProgressIconPositions[3])
        {
            StartCoroutine(AnimateProgressRankIconCoroutine(ProgressRankIcons[3], ProgressRankFills[3]));
            progressIconAnimationIndex++;
            animOccured = true;
        }
        if (progressIconAnimationIndex == 4 && lerpedPos >= ProgressIconPositions[4])
        {
            StartCoroutine(AnimateProgressRankIconCoroutine(ProgressRankIcons[4], ProgressRankFills[4]));
            progressIconAnimationIndex++;
            animOccured = true;
        }

        if (!animOccured) return;
        
        rankImage.sprite = RankIcons[progressIconAnimationIndex];

        GameObject sparkOBJ = Instantiate(SparkPrefab, rankImage.transform);
        var main = sparkOBJ.GetComponent<ParticleSystem>().main;
        main.startColor = RankColors[progressIconAnimationIndex - 1];
        Destroy(sparkOBJ, 2f);

        GameObject sparkOBJ2 = Instantiate(SparkPrefab, ProgressRankIcons[progressIconAnimationIndex - 1].transform);
        var main2 = sparkOBJ2.GetComponent<ParticleSystem>().main;
        main2.startColor = RankColors[progressIconAnimationIndex - 1];
        Destroy(sparkOBJ2, 2f);

        string sfxString = "Score " + progressIconAnimationIndex;
        if (progressIconAnimationIndex == 5) sfxString = "High Score";
        SFXSystem.singleton.PlaySFX(sfxString);

        LeanTween.pause(rankImage.gameObject);
        LeanTween.pause(rankOutline.gameObject);
        rankImage.rectTransform.localScale = new Vector3(RANK_ICON_MAX_SCALE, RANK_ICON_MAX_SCALE, RANK_ICON_MAX_SCALE);
        rankOutline.rectTransform.localScale = new Vector3(RANK_ICON_MAX_SCALE, RANK_ICON_MAX_SCALE, RANK_ICON_MAX_SCALE);
        LeanTween.scale(rankImage.gameObject, Vector3.one, RANK_ICON_ANIM_TIME);
        LeanTween.scale(rankOutline.gameObject, Vector3.one, RANK_ICON_ANIM_TIME);
    }

    private IEnumerator AnimateProgressRankIconCoroutine(Image rankIcon, Image fillImage)
    {
        float timePassed = 0;
        while (timePassed < PROGRESS_BAR_ICON_ANIM_TIME)
        {
            timePassed += Time.deltaTime;

            Vector2 lerpedRankSizeDelta = Vector2.Lerp(SmallRankIconSizeDelta, LargeRankIconSizeDelta, timePassed / PROGRESS_BAR_ICON_ANIM_TIME);
            rankIcon.rectTransform.sizeDelta = lerpedRankSizeDelta;
            
            Vector2 lerpedFillSizeDelta = Vector2.Lerp(SmallFillSizeDelta, LargeFillSizeDelta, timePassed / PROGRESS_BAR_ICON_ANIM_TIME);
            Vector2 lerpedFillVector = Vector2.Lerp(SmallFillVector, LargeFillVector, timePassed / PROGRESS_BAR_ICON_ANIM_TIME);
            fillImage.rectTransform.sizeDelta = lerpedFillSizeDelta;
            fillImage.rectTransform.localPosition = lerpedFillVector;

            yield return null;
        }
    }

    private IEnumerator AnimateScrollsCoroutine()
    {
        scrollSparkAnim.Play("Ability Reset UI", 0, 0f);
        SFXSystem.singleton.PlaySFX("Scoring Sound Effects_Insert 5");
        yield return new WaitForSeconds(0.1f);

        LeanTween.moveLocalX(leftScrollImage.gameObject, LEFT_SCROLL_END_POSITION, SCROLL_ANIM_TIME);
        LeanTween.moveLocalX(rightScrollImage.gameObject, RIGHT_SCROLL_END_POSITION, SCROLL_ANIM_TIME);
        LeanTween.scaleX(animatedScrollBGImage.gameObject, 1f, SCROLL_ANIM_TIME);
        yield return new WaitForSeconds(SCROLL_ANIM_TIME);
        fullScrollImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(TIME_BETWEEN_TEXT_ANIMATIONS);
        StartCoroutine(TypingCoroutine(rankText, rankName));
        yield return new WaitForSeconds(rankName.Length * TYPING_ANIM_TIME);
        isMenuAnimating = false;
    }

    public void HandleInput(Gamepad gamepad)
    {
        if (gamepad.buttonSouth.wasPressedThisFrame)
        {
            OnConfirm();
            return;
        }
        if (gamepad.buttonEast.wasPressedThisFrame)
        {
            OnCancel();
            return;
        }
    }

    public void OnConfirm()
    {
        TryUpdateInputIcons();

        if (isMenuAnimating)
        {
            SkipAnimationCompletely();
            SFXSystem.singleton.PlaySFX("UI_Confirm");
            return;
        }

        SFXSystem.singleton.PlaySFX("UI_Confirm");
        InputHandler.SetGameState(GameState.BATTLE, playFadeAnimation: true);
    }

    public void OnCancel()
    {
        TryUpdateInputIcons();

        if (isMenuAnimating)
        {
            SkipAnimationCompletely();
            SFXSystem.singleton.PlaySFX("UI_Cancel");
            return;
        }

        SFXSystem.singleton.PlaySFX("UI_Cancel");
        InputHandler.SetGameState(GameState.TITLE_SCREEN, playFadeAnimation: true);
    }

    public void TryUpdateInputIcons()
    {
        if (gamepadDisplayName == InputHandler.singleton.gamepadDisplayName) return;

        gamepadDisplayName = InputHandler.singleton.gamepadDisplayName;
        if (gamepadDisplayName.Contains("Xbox"))
        {
            confirmIcon.sprite = xboxConfirmSprite;
            cancelIcon.sprite = xboxCancelSprite;
        }
        else
        {
            confirmIcon.sprite = playstationConfirmSprite;
            cancelIcon.sprite = playstationCancelSprite;
        }
    }

    public void SkipAnimationCompletely()
    {
        StopAllCoroutines();

        int score = GameManager.singleton.score;
        Sprite rankSprite = GetRankIconFromScore(score);
        rankImage.sprite = rankSprite;
        rankName = rankSprite.name.ToUpperInvariant();

        scoreText.text = score.ToString("n0");
        waveText.text = GameManager.GetBattleStat(GameManager.WAVE_COUNT_STRING).ToString();
        comboText.text = GameManager.GetBattleStat(GameManager.HIGHEST_COMBO_STRING).ToString();
        killText.text = GameManager.GetBattleStat(GameManager.KILLS_STRING).ToString();
        parryText.text = GameManager.GetBattleStat(GameManager.PARRIES_STRING).ToString();


        float scorePercent = Mathf.Clamp((float)GameManager.singleton.score / RUBY_CUTOFF_AMOUNT, 0f, 1f);
        float targetPos = PROGRESS_FILL_STARTING_POSITION + (MAX_PROGRESS_FILL_AMOUNT * scorePercent);
        progressFill.transform.localPosition = new Vector3(targetPos, 0, 0);
        CheckForProgressIconAnimation(targetPos);

        LeanTween.moveLocalX(leftScrollImage.gameObject, LEFT_SCROLL_END_POSITION, 0);
        LeanTween.moveLocalX(rightScrollImage.gameObject, RIGHT_SCROLL_END_POSITION, 0);
        animatedScrollBGImage.rectTransform.localScale = Vector3.one;
        fullScrollImage.gameObject.SetActive(true);
        rankText.text = rankName;
        isMenuAnimating = false;
    }

    public void OpenMenuScreen()
    {
        canvasOBJ.SetActive(true);
        TryUpdateInputIcons();

        tonemapping.active = false;
        EnemyManager.singleton.CleanUpLeftoverEnemies();
        StartCoroutine(OpenResultsScreenCoroutine());
    }

    public void CloseMenuScreen(GameState newGameState)
    {
        canvasOBJ.SetActive(false);

        ResetResultsScreenStats();
        tonemapping.active = true;
    }

    private void ResetResultsScreenStats()
    {
        progressIconAnimationIndex = 0;

        scoreText.text = "";
        scoreText.text = "";
        waveText.text = "";
        comboText.text = "";
        killText.text = "";
        parryText.text = "";
        rankText.text = "";

        progressFill.rectTransform.localPosition = new Vector3(PROGRESS_FILL_STARTING_POSITION, 0, 0);

        for (int i = 0; i < ProgressRankIcons.Count; i++)
        {
            Image rankIcon = ProgressRankIcons[i];
            Image iconFill = ProgressRankFills[i];

            rankIcon.rectTransform.sizeDelta = SmallRankIconSizeDelta;

            iconFill.rectTransform.sizeDelta = SmallFillSizeDelta;
            iconFill.rectTransform.localPosition = SmallFillVector;
        }

        fullScrollImage.gameObject.SetActive(false);
        LeanTween.moveLocalX(leftScrollImage.gameObject, LEFT_SCROLL_START_POSITION, 0);
        LeanTween.moveLocalX(rightScrollImage.gameObject, RIGHT_SCROLL_START_POSITION, 0);
        animatedScrollBGImage.rectTransform.localScale = new Vector3(0, 1, 1);
    }
}
