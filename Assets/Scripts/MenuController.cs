using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class MenuController : MonoBehaviour
{
    public Text LevelText;
    public string RateURL, MoreURL;
    // Start is called before the first frame update
    void Start()
    {
        int CurrentLevel = PlayerPrefs.GetInt("Level", 0);
        LevelText.text = " LEVEL " + (CurrentLevel+1);

    }

    public void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public void RateUS()
    {
        Application.OpenURL(RateURL);
    }
   
    public void MoreGames()
    {
        Application.OpenURL(MoreURL);

    }

    public void startLevel()
    {
        StartCoroutine(LoadLevel());
    }

    IEnumerator LoadLevel() {
        yield return new WaitForSeconds(1.0f);
        SceneManager.LoadScene("Game");

    }
}
