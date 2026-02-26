using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    //[SerializeField] private int immuneLevel;
    [SerializeField] private int valueLevel;
    [SerializeField] private int incomeLevel;

    //public Text immuneLevelText;
    public Text valueLevelText;
    public Text incomeLevelText;

    [SerializeField] private List<int> immunePricePerLevel;
    [SerializeField] private List<int> valuePricePerLevel;
    [SerializeField] private List<int> incomePricePerLevel;

    //public Text immunePriceText;
    public Text valuePriceText;
    public Text incomePriceText;

    //public List<float> immunePerLevel;
    public List<float> valuePerLevel;
    public List<int> incomePerLevel;

    public int multiplierValue;

    public float incomePercentage;
    public float moneyStackMod;

    [SerializeField] private GameObject setting;
    private Animator settingAnim;

    private void Awake()
    {
        instance = this;

        valueLevel = PlayerPrefs.GetInt("Value_Level", 1);
        incomeLevel = PlayerPrefs.GetInt("Income_Level", 1);

        moneyStackMod = valuePerLevel[valueLevel - 1];
        incomePercentage = incomePerLevel[incomeLevel - 1]; 
    }

    private void Start()
    {
        settingAnim = setting.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        valueLevelText.text = string.Format("Level " + "{0:0}", valueLevel);
        incomeLevelText.text = string.Format("Level " + "{0:0}", incomeLevel);

        valuePriceText.text = string.Format("{0:0}", valuePricePerLevel[valueLevel]);
        incomePriceText.text = string.Format("{0:0}", incomePricePerLevel[incomeLevel]);
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BuyValueUpgrade()
    {
        if (GameManager.totalGemAmount >= valuePricePerLevel[valueLevel])
        {
            GameManager.totalGemAmount -= valuePricePerLevel[valueLevel];
            moneyStackMod = valuePerLevel[valueLevel];
            valueLevel++;
            PlayerPrefs.SetInt("Value_Level", valueLevel);
        }
    }

    public void BuyIncomeUpgrade()
    {
        if (GameManager.totalGemAmount >= incomePricePerLevel[incomeLevel])
        {
            GameManager.totalGemAmount -= incomePricePerLevel[incomeLevel];
            incomePercentage = incomePerLevel[incomeLevel];
            incomeLevel++;
            PlayerPrefs.SetInt("Income_Level", incomeLevel);
        }
    }

    public void ContinueGame()
    {
        GameManager.instance.levelNo++;
        GameManager.totalGemAmount += GameManager.instance.currentGemCollected * multiplierValue;
        if (GameManager.instance.levelNo > GameManager.instance.dataLevels.Count - 1)
        {
            GameManager.instance.levelNo = 0;
        }
        PlayerPrefs.SetInt("Level_Number", GameManager.instance.levelNo);
        PlayerPrefs.SetFloat("Total_Gem", GameManager.totalGemAmount);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReviveButton()
    {
        GameManager.totalGemAmount += GameManager.instance.currentGemCollected;
        PlayerPrefs.SetFloat("Total_Gem", GameManager.totalGemAmount);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SkipButton()
    {
        GameManager.instance.levelNo++;
        if (GameManager.instance.levelNo > GameManager.instance.dataLevels.Count - 1)
        {
            GameManager.instance.levelNo = 0;
        }
        PlayerPrefs.SetInt("Level_Number", GameManager.instance.levelNo);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ContinueWithoutMultiplierButton()
    {
        GameManager.instance.levelNo++;
        GameManager.totalGemAmount += GameManager.instance.currentGemCollected * multiplierValue;
        if (GameManager.instance.levelNo > GameManager.instance.dataLevels.Count - 1)
        {
            GameManager.instance.levelNo = 0;
        }
        PlayerPrefs.SetInt("Level_Number", GameManager.instance.levelNo);
        PlayerPrefs.SetFloat("Total_Gem", GameManager.totalGemAmount);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void settingButton()
    {
        if (settingAnim.GetBool("isOpen") == false)
        {
            settingAnim.SetBool("isOpen", true);
        }
        else
        {
            settingAnim.SetBool("isOpen", false);
        }
    }
}
