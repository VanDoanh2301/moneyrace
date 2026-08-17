using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    int TotalRing,RingLeft;
    public Text RingCounter;
    public ParticleSystem WinExplosion1, WinExplosion2;
    public GameObject GameComplete;
    bool GameWin;
    GameObject Level;
    public GameObject[] GameLevels;
    int CurrentLevel;
    public Text LevelIndicator;

    private const int LevelCompleteCoinReward = 20;

    // Start is called before the first frame update

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

    }

    void Start()
    {
        CurrentLevel = PlayerPrefs.GetInt("Level",0);
        Instantiate(GameLevels[CurrentLevel]);
        GameComplete.SetActive(false);
        GameWin = false;
        RingLeft = 0;
        TotalRing = GameObject.FindGameObjectsWithTag("Ring").Length;
        TotalRing = TotalRing - 1;
        Level = GameObject.FindGameObjectWithTag("Level");
        RingCounter.text = RingLeft + "/" + TotalRing;
        LevelIndicator.text = "LEVEL " + (CurrentLevel+1);

    }

    // Update is called once per frame
    void Update()
    {
        if (RingLeft >= TotalRing) {
            if (!GameWin) {
                GameWin = true;
               StartCoroutine(LevelCompleted());
            }
        }
    }

    public void UpdateScore() {
        RingLeft++;
        RingCounter.text = RingLeft + "/" + TotalRing;
    }

    IEnumerator LevelCompleted() {
        WinExplosion1.Play();

        PlayerPrefs.SetInt("Level",(CurrentLevel+1));
        CoinWallet.Add(LevelCompleteCoinReward, true);
        yield return new WaitForSeconds(2f);
        Level.SetActive(false);
        WinExplosion2.Play();

        GameComplete.SetActive(true);
        Adcontrol.instance.ShowInterstitial();
    }


    public void ReloadLevel() {
        SceneManager.LoadScene("Game");

    }

    public void LoadMenu() {
        SceneManager.LoadScene("Menu");
    }
}
