using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BattleUI : MonoBehaviour, IMenuScreen
{
    public GameObject canvasOBJ;
    public bool hasFocus;

    [Header("Combo")]
    public int currentComboCount;
    public TextMeshProUGUI comboText;
    public GameObject multiplierContainer;

    public const float COMBO_COUNT_MAX_SCALE = 1.5f;
    public const float COMBO_COUNT_ANIM_TIME = 0.15f;

    public bool animatingComboTimer;
    public Image comboTimerFill;
    public float currentComboTimePassed;
    public float comboTimerAmount;

    public const float COMBO_TIMER_MIN_VALUE = 1.75f;
    public const float COMBO_TIMER_MAX_VALUE = 3.5f;
    public const float COMBO_TIMER_COYOTE_TIME = 0.15f; // Some leniency after the combo bar has visually drained before the combo count is actually reset

    [Header("Combo Bar Shine")]
    public Image comboTimerShine;
    private bool animatingShine;
    private bool shineExpanding;
    private float timeAtStartShineAnimation;

    public const float COMBO_SHINE_MAX_HEIGHT = -55;
    public const float COMBO_SHINE_MIN_HEIGHT = -350f;
    public const float COMBO_SHINE_DEFAULT_WIDTH = 65f;
    public const float COMBO_SHINE_EXPANDED_WIDTH = 120f;

    public const float COMBO_SHINE_EXPAND_TIME_MULT = 0.35f;
    public const float COMBO_SHINE_CONTRACT_TIME_MULT = 0.65f;

    public const int MEDIUM_COMBO_AMOUNT = 8;
    public const int LARGE_COMBO_AMOUNT = 16;

    [Header("Fire")]
    public GameObject fireVFXParent;
    public const float FIRE_ANIM_TIME = 0.15f;

    [Header("Skill Notifications")]
    public List<SkillNotification> activeSkillNotifications = new List<SkillNotification>();
    public List<SkillNotification> skillNotificationPool = new List<SkillNotification>();

    [Header("Icons")]
    public GameObject attackIconParent;
    public Image attackImage;
    public GameObject dashIconParent;
    public Image dashImage;
    public Animator dashChargeAnim;

    public string gamepadDisplayName;
    public Image confirmIcon;
    public Sprite xboxConfirmSprite;
    public Sprite playstationConfirmSprite;
    public Image cancelIcon;
    public Sprite xboxCancelSprite;
    public Sprite playstationCancelSprite;

    #region Singleton

    public static BattleUI singleton;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else Destroy(this.gameObject);

        comboTimerAmount = COMBO_TIMER_MAX_VALUE;
    }

    #endregion

    public void IncrementComboCount()
    {
        currentComboCount++;

        if (currentComboCount > GameManager.GetBattleStat(GameManager.HIGHEST_COMBO_STRING)) GameManager.SetBattleStat(GameManager.HIGHEST_COMBO_STRING, currentComboCount);

        if (currentComboCount == 1)
        {
            comboTimerShine.gameObject.SetActive(true);
            LeanTween.pause(multiplierContainer);
            LeanTween.moveLocalX(multiplierContainer, -45f, 0.15f);
        }

        comboText.text = "x" + currentComboCount.ToString();

        LeanTween.pause(comboText.gameObject);
        comboText.rectTransform.localScale = new Vector3(COMBO_COUNT_MAX_SCALE, COMBO_COUNT_MAX_SCALE, COMBO_COUNT_MAX_SCALE);
        LeanTween.scale(comboText.gameObject, Vector3.one, COMBO_COUNT_ANIM_TIME);

        animatingShine = false;
        animatingComboTimer = true;
        comboTimerFill.fillAmount = 1;
        comboTimerShine.transform.localPosition = new Vector3(70.5f, COMBO_SHINE_MAX_HEIGHT, 0);
        currentComboTimePassed = 0;

        // Dynamic music - at certain thresholds, changes the music to emphasize the intensity
        if (currentComboCount >= MEDIUM_COMBO_AMOUNT && currentComboCount < LARGE_COMBO_AMOUNT)
        {
            MusicManager.TryChangeSong(1);
            ScaleFireUp();
        }
        if (currentComboCount >= LARGE_COMBO_AMOUNT) MusicManager.TryChangeSong(2);
        

        // Decrease the combo timer amount for every 5 combo counts
        int remainder = currentComboCount % 5;
        if (remainder == 0)
        {
            comboTimerAmount -= 0.25f;
            if (comboTimerAmount < COMBO_TIMER_MIN_VALUE) comboTimerAmount = COMBO_TIMER_MIN_VALUE;
        }
    }

    public void TryPlaySkillNotification(string notificationString, Color color)
    {
        // Check if there are available skill notifications
        if (skillNotificationPool.Count == 0)
        {
            return;
        }

        if (activeSkillNotifications.Count > 0)
        {
            for (int i = 0; i < activeSkillNotifications.Count; i++)
            {
                SkillNotification notificationToMove = activeSkillNotifications[i];
                float targetYPos = 95 - (activeSkillNotifications.Count - i) * 50;
                LeanTween.moveLocalY(notificationToMove.gameObject, targetYPos, 0.15f);
            }
        }

        SkillNotification notification = skillNotificationPool[0];
        skillNotificationPool.Remove(notification);
        activeSkillNotifications.Add(notification);
        notification.SetupNotification(notificationString, color);
        LeanTween.moveLocalX(notification.notificationElementsParent.gameObject, -30f, SkillNotification.ANIM_IN_TIME);
    }

    void Update()
    {
        if (!GameManager.singleton.battleActive || PauseUI.isGamePaused) return;
        if (!hasFocus || !animatingComboTimer) return;
        if (GameManager.InHitstop)
        {
            HandleComboBarShineAnimation();
            return;
        }

        float comboShineFillAmount = Mathf.Lerp(COMBO_SHINE_MAX_HEIGHT, COMBO_SHINE_MIN_HEIGHT, currentComboTimePassed / comboTimerAmount);
        comboTimerShine.transform.localPosition = new Vector3(70.5f, comboShineFillAmount, 0);

        float comboTimerfillAmount = Mathf.Lerp(1, 0, currentComboTimePassed / comboTimerAmount);
        comboTimerFill.fillAmount = comboTimerfillAmount;
        currentComboTimePassed += Time.deltaTime;

        if (currentComboTimePassed >= (comboTimerAmount + COMBO_TIMER_COYOTE_TIME))
        {
            animatingComboTimer = false;
            ComboDropped();
        }
    }

    public void HandleComboBarShineAnimation()
    {
        if (!animatingShine)
        {
            timeAtStartShineAnimation = Time.time;
            comboTimerShine.rectTransform.sizeDelta = new Vector2(COMBO_SHINE_DEFAULT_WIDTH, 6f);
            animatingShine = true;
            shineExpanding = true;
        }

        if (shineExpanding)
        {
            float timePassed = Time.time - timeAtStartShineAnimation;
            float animLength = GameManager.singleton.currentHitstopLength * COMBO_SHINE_EXPAND_TIME_MULT;
            float pComplete = timePassed / animLength;
            float lerpedWidth = Mathf.Lerp(COMBO_SHINE_DEFAULT_WIDTH, COMBO_SHINE_EXPANDED_WIDTH, pComplete);
            comboTimerShine.rectTransform.sizeDelta = new Vector2(lerpedWidth, 6f);

            if (pComplete >= 1)
            {
                timeAtStartShineAnimation = Time.time;
                shineExpanding = false;
            }
        }
        else
        {
            float timePassed = Time.time - timeAtStartShineAnimation;
            float animLength = GameManager.singleton.currentHitstopLength * COMBO_SHINE_CONTRACT_TIME_MULT;
            float pComplete = timePassed / animLength;
            float lerpedWidth = Mathf.Lerp(COMBO_SHINE_EXPANDED_WIDTH, COMBO_SHINE_DEFAULT_WIDTH, pComplete);
            comboTimerShine.rectTransform.sizeDelta = new Vector2(lerpedWidth, 6f);
        }
    }

    public void ComboDropped()
    {
        MusicManager.TryChangeSong(0);
        ScaleFireDown();

        currentComboCount = 0;
        comboTimerFill.fillAmount = 0;
        comboText.text = 0.ToString();
        comboTimerAmount = COMBO_TIMER_MAX_VALUE;
        comboTimerShine.gameObject.SetActive(false);
        LeanTween.moveLocalX(multiplierContainer, -425f, 0.25f);
    }

    public void ScaleFireUp()
    {
        LeanTween.scale(fireVFXParent, Vector3.one, FIRE_ANIM_TIME);
    }

    public void ScaleFireDown()
    {
        LeanTween.scale(fireVFXParent, Vector3.zero, FIRE_ANIM_TIME);
    }

    private Coroutine dashVisualCoroutine;
    public void DashChargeReset(bool hasDashCharge)
    {
        if (dashVisualCoroutine != null) StopCoroutine(dashVisualCoroutine);

        if (!hasDashCharge) dashChargeAnim.Play("Ability Reset UI", 0, 0f);
        dashVisualCoroutine = StartCoroutine(ChangeDashVisualCoroutine(dashImage.color.r, 1f, FIRE_ANIM_TIME));
    }

    public void DashChargeUsed()
    {
        if (dashVisualCoroutine != null) StopCoroutine(dashVisualCoroutine);

        dashVisualCoroutine = StartCoroutine(ChangeDashVisualCoroutine(dashImage.color.r, 0.25f, FIRE_ANIM_TIME));
    }

    /// <summary>
    /// Changes the color of the Dash Icon's BG to show whether it can be used or not
    /// </summary>
    /// <param name="startColorValue"></param>
    /// <param name="endColorValue"></param>
    /// <param name="animTime"></param>
    /// <returns></returns>
    public IEnumerator ChangeDashVisualCoroutine(float startColorValue, float endColorValue, float animTime)
    {
        float timePassed = 0;
        while (timePassed < animTime)
        {
            float newColorValue = Mathf.Lerp(startColorValue, endColorValue, timePassed / animTime);
            dashImage.color = new Color(newColorValue, newColorValue, newColorValue);
            timePassed += Time.deltaTime;
            yield return null;
        }
    }

    public void AttackInputted(bool isDashAttack)
    {
        if (!isDashAttack)
        {
            StartCoroutine(PulseInputIconCoroutine(attackImage.gameObject));
        }
        else
        {
            StartCoroutine(PulseInputIconCoroutine(dashImage.gameObject));
        }        
    }

    private IEnumerator PulseInputIconCoroutine(GameObject iconOBJ)
    {
        LeanTween.scale(iconOBJ.gameObject, Vector3.one * 1.1f, 0.05f);
        yield return new WaitForSeconds(0.05f);
        LeanTween.scale(iconOBJ.gameObject, Vector3.one, 0.05f);
    }

    public void GameOver()
    {
        animatingComboTimer = false;
        currentComboTimePassed = 0;
        ComboDropped();
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

    public void OpenMenuScreen()
    {
        canvasOBJ.SetActive(true);
        hasFocus = true;
        if (InputHandler.singleton.previousGameState == GameState.TITLE_SCREEN || InputHandler.singleton.previousGameState == GameState.RESULTS_SCREEN)
        {
            GameManager.singleton.SetupBattle();
        }
    }

    public void CloseMenuScreen(GameState newGameState)
    {
        canvasOBJ.SetActive(false);
        hasFocus = false;
    }
}