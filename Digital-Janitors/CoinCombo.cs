using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CoinCombo : MonoBehaviour
{
    #region Singleton

    public static CoinCombo instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    [Header("Score")]
    public int comboScore;
    public int totalComboScore;
    public int comboLimit;
    public Image comboVisual;

    [Header("Increment")]
    public int currencyGain;
    public int incrementNum;
    public int incrementLimit;

    [Header("Visuals")]
    public TextMeshPro comboScoreText;
    public GameObject comboNumTextPrefab;
    public Transform comboNumTextHolder;
    public List<GameObject> comboPrefabList = new List<GameObject>();
    public Color white;
    public Color gold;
    public Color red;

    public SpawnFolders spawnFolders;

    private void Start()
    {
        comboScore = 0;
        currencyGain = 1;

        // Visual
        comboVisual.fillAmount = 0;
        comboScoreText.text = "" + currencyGain;
    }

    public void CheckCombo(bool sortedCorrectly)
    {
        if (sortedCorrectly)
        {
            // Increments Combo Score
            comboScore++;
            totalComboScore++;
            if (comboScore < comboLimit)
            {
                StartCoroutine(ComboNumTextCoroutine(comboNumTextPrefab, white));
            }
            // Combo Completed
            if (comboScore == comboLimit)
            {
                StartCoroutine(ComboNumTextCoroutine(comboNumTextPrefab, gold));
                DataManager.Instance.currency += currencyGain;
                StartCoroutine(TextAnimationCoroutine(comboScoreProfitText, "+", 1, comboScoreText.gameObject.transform.position));
                StartCoroutine(TextAnimationCoroutine(profitText, "+", currencyGain, profitText.gameObject.transform.position));
                Money.instance.CurrencyChanged();
                comboScore = 0;
                currencyGain++;
                incrementNum++;
                comboScoreText.text = "" + currencyGain;

                if (incrementNum == incrementLimit)
                {
                    comboLimit++;
                    incrementNum = 0;
                }
            }
        }
        else
        {
            if (spawnFolders != null)
            {
                // Increases the chance of a rare folder spawning
                spawnFolders.IncrementRareChance();
            }
            // Resets the Combo Score
            StartCoroutine(ComboNumResetCoroutine(comboNumTextPrefab));
            comboScore = 0;
            totalComboScore = 0;
            currencyGain = 1;
            comboScoreText.text = "" + currencyGain;
        }
        // Calculates the percentage to fill the Coin Combo Visual
        comboVisual.fillAmount = (float)comboScore/comboLimit;
    }

    #region Profit Visuals

    [Header("Profit Visuals")]
    public TextMeshPro comboScoreProfitText;
    public TextMeshPro profitText;
    public GameObject profitTextPrefab;

    /// <summary>
    /// Destroys all text visuals but the last one
    /// </summary>
    void DeleteAllButLastPrefab()
    {
        for (int i = 0; i < comboPrefabList.Count; i++)
        {
            if (i != comboPrefabList.Count - 1)
            {
                Destroy(comboPrefabList[i]);
            }
        }
    }

    void ClearComboPrefabList()
    {
        foreach (GameObject obj in comboPrefabList)
        {
            Destroy(obj);
        }
    }

    /// <summary>
    /// Visual for gaining currency from using the DevConsole
    /// </summary>
    /// <returns></returns>
    public IEnumerator DevConsoleCoroutine()
    {
        GameObject obj = Instantiate(profitTextPrefab);
        obj.transform.SetParent(profitText.transform, false);
        obj.transform.position = profitText.transform.position;

        TextMeshPro textOBJ = obj.GetComponent<TextMeshPro>();
        textOBJ.text = "+1";
        float f = 0;
        while (f > -.99)
        {
            // Moves Downwards
            Vector3 pos = textOBJ.gameObject.transform.position;
            textOBJ.gameObject.transform.position = new Vector3(pos.x, pos.y - .05f, pos.z);
            // Text dissipates
            f -= .1f;
            textOBJ.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, f);
            yield return new WaitForSeconds(.075f);
        }
        textOBJ.text = "";
        textOBJ.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
        Destroy(obj);
    }

    /// <summary>
    /// Visual for gaining currency from a successful Coin Combo
    /// </summary>
    /// <param name="textOBJ"></param>
    /// <param name="sign"></param>
    /// <param name="amount"></param>
    /// <param name="startPos"></param>
    /// <returns></returns>
    IEnumerator TextAnimationCoroutine(TextMeshPro textOBJ, string sign, int amount, Vector3 startPos)
    {
        textOBJ.text = sign + amount;
        float f = 0;
        while (f > -.99)
        {
            // Moves Downwards
            Vector3 pos = textOBJ.gameObject.transform.position;
            textOBJ.gameObject.transform.position = new Vector3(pos.x, pos.y - .05f, pos.z);
            // Text dissipates
            f -= .1f;
            textOBJ.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, f);
            yield return new WaitForSeconds(.075f);
        }
        textOBJ.text = "";
        textOBJ.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
        textOBJ.transform.position = startPos;
    }

    /// <summary>
    /// Visual for sorting a folder correctly and increasing the Combo Score
    /// </summary>
    /// <param name="numTextPrefab"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    IEnumerator ComboNumTextCoroutine(GameObject numTextPrefab, Color color)
    {
        GameObject obj = Instantiate(numTextPrefab);
        comboPrefabList.Add(obj);
        DeleteAllButLastPrefab();
        obj.transform.SetParent(transform, false);
        obj.transform.localScale = new Vector3(1,1,1);
        obj.transform.position = comboNumTextHolder.position;
        TextMeshProUGUI numText = obj.GetComponent<TextMeshProUGUI>();
        numText.text = totalComboScore + "x";
        numText.color = color;
        numText.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
        float f = 0;
        while (f > -.99 && obj != null)
        {
            // Moves Downwards
            Vector3 pos = numText.gameObject.transform.position;
            numText.gameObject.transform.position = new Vector3(pos.x, pos.y - .03f, pos.z);
            // Text dissipates
            f -= .05f;
            numText.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, f);
            yield return new WaitForSeconds(.025f);
        }
        Destroy(obj);
    }

    /// <summary>
    /// Visual for sorting a folder incorrectly reseting the Combo Score to 0
    /// </summary>
    /// <param name="numTextPrefab"></param>
    /// <returns></returns>
    IEnumerator ComboNumResetCoroutine(GameObject numTextPrefab)
    {
        ClearComboPrefabList();
        GameObject obj = Instantiate(numTextPrefab);
        obj.transform.SetParent(transform, false);
        TextMeshProUGUI numText = obj.GetComponent<TextMeshProUGUI>();
        int num = totalComboScore;
        numText.text = num + "x";
        numText.color = red;
        numText.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
        float f = 0;
        while (f > -.99)
        {
            if (num != 0)
            {
                num--;
            }
            numText.text = num + "x";
            // Moves Downwards
            Vector3 pos = numText.gameObject.transform.position;
            numText.gameObject.transform.position = new Vector3(pos.x, pos.y - .03f, pos.z);
            // Text dissipates
            f -= .05f;
            numText.fontMaterial.SetFloat(ShaderUtilities.ID_FaceDilate, f);
            yield return new WaitForSeconds(.025f);
        }
        Destroy(obj);
    }
    #endregion

}