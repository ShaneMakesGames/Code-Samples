using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class UISliderContainer : MonoBehaviour
{
    public AudioMixer myMixer;
    public int volume;

    public Image sliderImage;
    public Image fillImage;
    public Image bgImage;
    public Shadow myShadow;
    public TextMeshProUGUI myText;
    public Color unselectedColor;
    public TMP_FontAsset selectedFontAsset;
    public TMP_FontAsset unselectedFontAsset;
    public Sprite selectedSprite;
    public Sprite unselectedSprite;

    private bool waitingForScaleUpToFinish;
    private bool scaleDownQueued;
    public float lastTimeAnimChanged;

    public const float ANIM_SCALE_UP_TIME = 0.2f;
    public const float ANIM_SCALE_DOWN_TIME = 0.15f;

    public const float SLIDER_FILL_STARTING_POINT = 3f;
    public const float SLIDER_FILL_MAX_DISTANCE = 188f;

    public const float SLIDER_CURSOR_STARTING_POINT = 0f;
    public const float SLIDER_CURSOR_MAX_DISTANCE = 170f;

    public const float MIXER_MIN_VOLUME = -45f;
    public const float MIXER_MAX_DISTANCE = 55f;

    public void Update()
    {
        if (waitingForScaleUpToFinish)
        {
            if (Time.time - lastTimeAnimChanged < ANIM_SCALE_UP_TIME) return;

            if (scaleDownQueued)
            {
                LeanTween.scale(bgImage.gameObject, Vector3.one, ANIM_SCALE_DOWN_TIME);
                scaleDownQueued = false;
            }
            waitingForScaleUpToFinish = false;
        }
    }


    public void OnDeselect()
    {
        bgImage.color = unselectedColor;
        myShadow.enabled = false;
        myText.font = unselectedFontAsset;
        bgImage.sprite = unselectedSprite;
        if (!waitingForScaleUpToFinish) LeanTween.scale(bgImage.gameObject, Vector3.one, ANIM_SCALE_DOWN_TIME);
        else scaleDownQueued = true;
    }

    public void OnSelect()
    {
        bgImage.color = Color.white;
        myShadow.enabled = true;
        myText.font = selectedFontAsset;
        bgImage.sprite = selectedSprite;
        LeanTween.scale(bgImage.gameObject, new Vector3(1.1f, 1.1f, 1.1f), ANIM_SCALE_UP_TIME);

        waitingForScaleUpToFinish = true;
        lastTimeAnimChanged = Time.time;
    }

    public void MoveSliderCursor(int moveAmount)
    {
        SFXSystem.singleton.PlaySFX("Menu_Tick_1", randomizePitch: true);

        volume += moveAmount;

        if (volume < 0) volume = 0;
        if (volume > 100) volume = 100;


        float volumePercent = volume / 100f;
        if (volumePercent != 0) myMixer.SetFloat("Volume", Mathf.Log10(volumePercent) * 20);
        else myMixer.SetFloat("Volume", -80f);
        SetVisualsFromVolume(volumePercent);
    }

    public void SetVisualsFromVolume(float volumePercent)
    {
        float fillXDistance = volumePercent * SLIDER_FILL_MAX_DISTANCE;
        fillImage.rectTransform.localPosition = new Vector3(SLIDER_CURSOR_STARTING_POINT + fillXDistance, 0, 0);
        float cursorXDistance = volumePercent * SLIDER_CURSOR_MAX_DISTANCE;
        sliderImage.rectTransform.localPosition = new Vector3(SLIDER_CURSOR_STARTING_POINT + cursorXDistance, 0, 0);
    }
}