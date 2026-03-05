using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System;


#if EASY_MOBILE
using EasyMobile;
#endif

public class UIManager_Branches : MonoBehaviour
{
    [Header("Object References")]
    public GameManager_Branches gameManager;
    public GameObject header;
    public Text score;
    public Text bestScore;
    public Text coinText;
    public Text title;
    public GameObject tapToStart;
    public GameObject characterSelectBtn;
    public GameObject menuButtons;
    public GameObject dailyRewardBtn;
    public Text dailyRewardBtnText;
    public GameObject rewardUI;
    public GameObject shopScreen;
    public GameObject shopBtn;

    Animator scoreAnimator;
    Animator dailyRewardAnimator;
    public GameObject panel_loading;


    void OnEnable()
    {
        GameManager_Branches.GameState_BRChanged += GameManager_GameState_BRChanged;
        ScoreManager_BR.ScoreUpdated += OnScoreUpdated;
    }

    void OnDisable()
    {
        GameManager_Branches.GameState_BRChanged -= GameManager_GameState_BRChanged;
        ScoreManager_BR.ScoreUpdated -= OnScoreUpdated;
    }

    // Use this for initialization
    void Start()
    {
        scoreAnimator = score.GetComponent<Animator>();
        dailyRewardAnimator = dailyRewardBtn.GetComponent<Animator>();

        Reset();
        ShowStartUI();
    }

    // Update is called once per frame
    void Update()
    {
        score.text = ScoreManager_BR.Instance.Score.ToString();
        bestScore.text = ScoreManager_BR.Instance.HighScore.ToString();
        coinText.text = CoinManager_BR.Instance.Coins.ToString();

        if (!DailyRewardController_BR.Instance.disable && dailyRewardBtn.gameObject.activeSelf)
        {
            if (DailyRewardController_BR.Instance.CanRewardNow())
            {
                dailyRewardBtnText.text = "GRAB YOUR REWARD!";
                dailyRewardAnimator.SetTrigger("activate");
            }
            else
            {
                TimeSpan timeToReward = DailyRewardController_BR.Instance.TimeUntilReward;
                dailyRewardBtnText.text = string.Format("REWARD IN {0:00}:{1:00}:{2:00}", timeToReward.Hours, timeToReward.Minutes, timeToReward.Seconds);
                dailyRewardAnimator.SetTrigger("deactivate");
            }
        }

        
    }

    void GameManager_GameState_BRChanged(GameState_BR newState, GameState_BR oldState)
    {
        if (newState == GameState_BR.Playing)
        {              
            ShowGameUI();
        }
        else if (newState == GameState_BR.PreGameOver)
        {
            // Before game over, i.e. game potentially will be recovered
        }
        else if (newState == GameState_BR.GameOver)
        {
            Invoke("ShowGameOverUI", 0.5f);
        }
    }

    void OnScoreUpdated(int newScore)
    {
        scoreAnimator.Play("NewScore");
    }

    void Reset()
    {

        header.SetActive(false);
        title.gameObject.SetActive(false);
        score.gameObject.SetActive(false);
        tapToStart.SetActive(false);
        characterSelectBtn.SetActive(false);
        menuButtons.SetActive(false);
        dailyRewardBtn.SetActive(false);


    }

    public void StartGame()
    {
        gameManager.StartGame();
    }

    public void EndGame()
    {
        gameManager.GameOver();
    }

    public void RestartGame()
    {
        gameManager.RestartGame(0.2f);
    }

    public void ShowStartUI()
    {
    

        header.SetActive(true);
        title.gameObject.SetActive(true);
        tapToStart.SetActive(true);
        characterSelectBtn.SetActive(true);

        // If first launch: show "WatchForCoins" and "DailyReward" buttons if the conditions are met
        if (GameManager_Branches.GameCount == 0)
        {
            ShowDailyRewardBtn();
        }
    }

    public void ShowGameUI()
    {
        header.SetActive(true);
        title.gameObject.SetActive(false);
        score.gameObject.SetActive(true);
        tapToStart.SetActive(false);
        characterSelectBtn.SetActive(false);
        dailyRewardBtn.SetActive(false);
    }

    public void ShowGameOverUI()
    {
        header.SetActive(true);
        title.gameObject.SetActive(false);
        score.gameObject.SetActive(true);
        tapToStart.SetActive(false);
        menuButtons.SetActive(true);

        // Show "WatchForCoins" and "DailyReward" buttons if the conditions are met
        ShowDailyRewardBtn();
        //AdManager.instance.show_ads_ingames();
    }

    void ShowWatchForCoinsBtn()
    {
        // Only show "watch for coins button" if a rewarded ad is loaded and premium features are enabled
        #if EASY_MOBILE
        if (IsPremiumFeaturesEnabled() && AdDisplayer.Instance.CanShowRewardedAd() && AdDisplayer.Instance.watchAdToEarnCoins)
        {
        watchForCoinsBtn.SetActive(true);
        watchForCoinsBtn.GetComponent<Animator>().SetTrigger("activate");
        }
        else
        {
        watchForCoinsBtn.SetActive(false);
        }
        #endif
    }

    void ShowDailyRewardBtn()
    {
        // Not showing the daily reward button if the feature is disabled
        if (!DailyRewardController_BR.Instance.disable)
        {
            dailyRewardBtn.SetActive(true);
        }
    }

   
   

    void OnCompleteRewardedAdToEarnCoins()
    {
        #if EASY_MOBILE
        // Unsubscribe
        AdDisplayer.CompleteRewardedAdToEarnCoins -= OnCompleteRewardedAdToEarnCoins;

        // Give the coins!
        ShowRewardUI(AdDisplayer.Instance.rewardedCoins);
        #endif
    }

    public void GrabDailyReward()
    {
        if (DailyRewardController_BR.Instance.CanRewardNow())
        {
            int reward = DailyRewardController_BR.Instance.GetRandomReward();

            // Round the number and make it mutiplies of 5 only.
            int roundedReward = (reward / 5) * 5;

            // Show the reward UI
            ShowRewardUI(roundedReward);

            // Update next time for the reward
            DailyRewardController_BR.Instance.ResetNextRewardTime();
        }
    }

    public void ShowRewardUI(int reward)
    {
        rewardUI.SetActive(true);
        rewardUI.GetComponent<RewardUIController_BR>().Reward(reward);
    }

    public void HideRewardUI()
    {
        rewardUI.GetComponent<RewardUIController_BR>().Close();
    }

    public void exitGame() {
        panel_loading.SetActive(true);
        SceneManager.LoadSceneAsync(0 , LoadSceneMode.Single);  
    }

    public void ShowShopScreen()
    {
        if (shopScreen != null) shopScreen.SetActive(true);
        if (shopBtn != null) shopBtn.SetActive(false);
    }

    public void CloseShopScreen()
    {
        if (shopScreen != null) shopScreen.SetActive(false);
        if (shopBtn != null) shopBtn.SetActive(true);
    }

    // ----- IAP: gọi từ nút Buy trong Shop (ủy quyền cho IAPManager) -----
    /// <summary>Mua gói 100 coin (iap1 - 0,30 US$).</summary>
    public void BuyCoins100() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins100(); }
    /// <summary>Mua gói 200 coin (iap2 - 0,49 US$).</summary>
    public void BuyCoins200() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins200(); }
    /// <summary>Mua gói 400 coin (iap3 - 0,99 US$).</summary>
    public void BuyCoins400() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins400(); }
    /// <summary>Mua gói 600 coin (iap4 - 1,99 US$).</summary>
    public void BuyCoins600() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins600(); }
    /// <summary>Mua gói 1000 coin (iap5 - 2,99 US$).</summary>
    public void BuyCoins1000() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins1000(); }
    /// <summary>Mua gói 2000 coin (iap6 - 4,99 US$).</summary>
    public void BuyCoins2000() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins2000(); }
    /// <summary>Mua gói 5000 coin (iap7 - 9,99 US$).</summary>
    public void BuyCoins5000() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins5000(); }

    public void ShowCharacterSelectionScene()
    {
        SceneManager.LoadScene("CharacterSelection");
    }

  
}
