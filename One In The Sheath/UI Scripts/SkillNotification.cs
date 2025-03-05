using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillNotification : MonoBehaviour
{
    public Transform notificationElementsParent;
    public Image notificationBG;
    public TextMeshProUGUI notificationText;
    public Image starFill;
    public Image starOutline;

    private bool updatedMaterial;
    public Material defaultMat;
    public Material glowMat;

    public List<ParticleSystem> blueParticleFXList = new List<ParticleSystem>();
    public List<ParticleSystem> orangeParticleFXList = new List<ParticleSystem>();

    public TMP_FontAsset blueFont;
    public TMP_FontAsset orangeFont;

    public List<Image> elementsToFade = new List<Image>();

    public bool isActive;
    private float timeSinceActive;

    public const string SLASH_KILL = "Slash Kill";
    public const string DASH_KILL = "Dash Kill";
    public const string PARRY = "Parry";

    public const float LIFETIME = 0.5f;

    public const float ANIM_IN_TIME = 0.25f;
    public const float ANIM_OUT_TIME = 0.2f;

    public void SetupNotification(string _notificationString, Color _skillColor)
    {
        timeSinceActive = 0;
        isActive = true;

        notificationBG.color = Color.white;
        starFill.color = Color.white;

        notificationText.text = _notificationString;
        if (_skillColor == Color.cyan) notificationText.font = blueFont;
        else notificationText.font = orangeFont;
        starOutline.color = _skillColor;

        PlayParticleSystem(_skillColor == Color.cyan);
    }

    private void Update()
    {
        if (!isActive) return;
        if (GameManager.InHitstop) return;
        
        
        timeSinceActive += Time.deltaTime;

        if (timeSinceActive > 0.1f && !updatedMaterial)
        {
            updatedMaterial = true;
            starOutline.material = glowMat;
        }

        if (timeSinceActive >= LIFETIME)
        {
            isActive = false;
            BattleUI.singleton.activeSkillNotifications.Remove(this);
            StartCoroutine(AnimateTextClearCoroutine());
            StartCoroutine(FadeImagesOutCoroutine());
        }   
    }

    private IEnumerator AnimateTextClearCoroutine()
    {
        string originalString = notificationText.text;
        for (int i = 0; i < originalString.Length; i++)
        {
            string updatedString = notificationText.text;
            updatedString = updatedString.Remove(updatedString.Length - 1);
            notificationText.text = updatedString;

            yield return new WaitForSeconds(0.015f);
        }
    }

    private IEnumerator FadeImagesOutCoroutine()
    {
        LeanTween.moveLocalY(gameObject, transform.localPosition.y - 40, ANIM_OUT_TIME);

        float timePassed = 0;
        while (timePassed < ANIM_OUT_TIME)
        {
            timePassed += Time.deltaTime;

            for (int i = 0; i < elementsToFade.Count; i++)
            {
                Color c = elementsToFade[i].color;
                c.a = Mathf.Lerp(1, 0, timePassed / ANIM_OUT_TIME);
                elementsToFade[i].color = c;
            }

            yield return null;
        }

        // Resets position
        transform.localPosition = new Vector3(100, 95, 0);
        notificationElementsParent.localPosition = new Vector3(-450, 0, 0);

        updatedMaterial = false;
        starOutline.material = defaultMat;

        BattleUI.singleton.skillNotificationPool.Add(this);
    }

    public void PlayParticleSystem(bool isBlue)
    {
        if (isBlue)
        {
            for (int i = 0; i < blueParticleFXList.Count; i++)
            {
                blueParticleFXList[i].Play();
            }
        }
        else
        {
            for (int i = 0; i < orangeParticleFXList.Count; i++)
            {
                orangeParticleFXList[i].Play();
            }
        }
    }
}