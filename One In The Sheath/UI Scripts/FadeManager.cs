using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeManager : MonoBehaviour
{
    public bool isFading;
    public Image fadeImage;

    public const float FADE_TIME = 0.5f;

    #region Singleton

    public static FadeManager singleton;

    private void Awake()
    {
        if (singleton == null)
        {
            singleton = this;
        }
        else Destroy(this.gameObject);
    }

    #endregion

    public void InitiateFade(bool fadingOut)
    {
        if (isFading) return;

        if (!fadingOut) StartCoroutine(FadeWithColorCoroutine(1, 0));
        else StartCoroutine(FadeWithColorCoroutine(0, 1));
    }

    private IEnumerator FadeWithColorCoroutine(float startAlpha, float endAlpha)
    {
        isFading = true;
        float timePassed = 0;

        Color startColor = fadeImage.color;
        startColor.a = startAlpha;
        fadeImage.color = startColor;

        while (timePassed < FADE_TIME)
        {
            float lerpAmount = Mathf.Lerp(startAlpha, endAlpha, timePassed / FADE_TIME);
            Color c = fadeImage.color;
            c.a = lerpAmount;
            fadeImage.color = c;

            timePassed += Time.deltaTime;
            yield return null;
        }

        Color endColor = fadeImage.color;
        endColor.a = endAlpha;
        fadeImage.color = endColor;
        isFading = false;
    }
}