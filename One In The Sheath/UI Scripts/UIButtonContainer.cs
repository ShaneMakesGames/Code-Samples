using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.TextCore.Text;

public enum ButtonType
{
    PLAY,
    SETTINGS,
    QUIT,
    RESUME
}

public class UIButtonContainer : MonoBehaviour
{
    public ButtonType myButtonType;
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
        myText.fontSize = 40;
        string capitalLetter = myText.text[0].ToString();
        string restOfTheLetters = myText.text.Substring(1);
        myText.text = capitalLetter + restOfTheLetters.ToLowerInvariant();
        bgImage.sprite = unselectedSprite;
        if (!waitingForScaleUpToFinish) LeanTween.scale(bgImage.gameObject, Vector3.one, ANIM_SCALE_DOWN_TIME);
        else scaleDownQueued = true;
    }

    public void OnSelect()
    {
        bgImage.color = Color.white;
        myShadow.enabled = true;
        myText.font = selectedFontAsset;
        myText.fontSize = 48;
        myText.text = myText.text.ToUpperInvariant();
        bgImage.sprite = selectedSprite;
        LeanTween.scale(bgImage.gameObject, new Vector3(1.1f,1.1f,1.1f), ANIM_SCALE_UP_TIME);

        waitingForScaleUpToFinish = true;
        lastTimeAnimChanged = Time.time;
    }
}