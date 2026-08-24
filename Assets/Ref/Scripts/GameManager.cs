using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public AudioSource AudioSource;
    public AudioClip CompleteSound;
    public AudioClip FailSound;
    public AudioClip KnifeCutSound;
    public GameObject levelcomplete,levelfail,ingame,playerRank,aiRank,lfPRank,lfAiRank;

    public Text leveltext,scoretext;

    public Image fill1, ailevelfill, aiFillBar, aiImage, playerImage;
    public Text AiWinpercentage,playerwinper;
    public float score;
    public float aiscore;
    public Text AiscoreText;

    public GameObject particale;
    public Material AiBoard;

    public float speed;
    public GameController gmcontrol;

	void Start ()
    {

        RenderSettings.fogStartDistance = 35;
        RenderSettings.fogEndDistance = 90;
        gmcontrol = FindObjectOfType<GameController>();
        //  RenderSettings.fogColor = Random.ColorHSV(0,360,24,25,89,90,99,100);
        GameObject.Find("player").transform.position = new Vector3(0, -0.7f, 0);
        GameObject.FindGameObjectWithTag("AIsetup").transform.position = new Vector3(-10.5f, 0, -12.2f);
        leveltext.text ="LEVEL"+(SceneManager.GetActiveScene().buildIndex-1).ToString();
        
        if (MenuScript.mul == 0)
        {
            GameObject.FindGameObjectWithTag("AiObjectSpawnPoint").SetActive(false);
            GameObject.FindGameObjectWithTag("AIsetup").SetActive(false);
            GameObject.FindGameObjectWithTag("AiPlayer").SetActive(false);
        }
        if (MenuScript.mul == 1)
        {
            Camera.main.transform.position = new Vector3(1f, 18.7f, -19.4f);
            Camera.main.fieldOfView = 78;
            aiFillBar.gameObject.SetActive(true);
            AiscoreText.gameObject.SetActive(true);
        }
    }

    int i = 0;
    public platform[] pl;
    public platform[] pl2;

    void Update ()
    {
        scoretext.text = score.ToString();
        if (MenuScript.mul == 1)
        {
            AiscoreText.text = aiscore.ToString();
        }
   
        if (fill1.fillAmount == 1 && i==0)
        {
            FindObjectOfType<cameramove>().enabled=true;
            gameover();
            Debug.Log("levelcomplete");
            Invoke("LevelComplete", 3f);
            pl = FindObjectsOfType<platform>();
            FindObjectOfType<PlatforMove>().tomakecancelinvoke();

            for (int i = 0; i < pl.Length; i++)
            {
                pl[i].stopmovement = false;
            }
            Instantiate(particale,new Vector3(100,25,61),Quaternion.Euler(23,29,-4));
            i = 1;
        }
        if (ailevelfill.fillAmount==1 && i==0 && FindObjectOfType<AiPlayer>())
        {
            LevelFail();
            Debug.Log("levelfail");
            FindObjectOfType<PlatforMove>().tomakecancelinvoke();

            pl2 = FindObjectsOfType<platform>();
            for (int i = 0; i < pl2.Length; i++)
            {
                pl2[i].stopmovement = false;
            }
            i = 1;
        }
	}

    public void LevelComplete()
    {
        AudioSource.PlayOneShot(CompleteSound);
        levelcomplete.SetActive(true);
        ingame.SetActive(false);
        
        int level = int.Parse(SceneManager.GetActiveScene().name);
        PlayerPrefs.SetString("level", (level + 1).ToString());
        
        if (MenuScript.mul == 1)
        {
            playerwinper.text = (int)((score / gmcontrol.totalscore) * 100) + "%";
            AiWinpercentage.text = (int)((aiscore / gmcontrol.totalscore) * 100) + "%";
            GameObject.FindGameObjectWithTag("AiPlayer").SetActive(false);
            aiImage.gameObject.SetActive(true);
            playerImage.gameObject.SetActive(true);
        }
        else
        {
            playerRank.SetActive(false);
            aiRank.SetActive(false);
        }
    }
    public void LevelFail()
    {
        AudioSource.PlayOneShot(FailSound);
        levelfail.SetActive(true);
        ingame.SetActive(false);
        if (MenuScript.mul == 1)
        {
            playerwinper.text = (int)((score / gmcontrol.totalscore) * 100) + "%";
            AiWinpercentage.text = (int)((aiscore / gmcontrol.totalscore) * 100) + "%";
            aiImage.gameObject.SetActive(true);
            playerImage.gameObject.SetActive(true);
        }
        else {
            lfAiRank.SetActive(false);
            lfPRank.SetActive(false);
        }
    }
    public void RestartButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void NextButton()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void gameover()
    {
        FindObjectOfType<PlayerInputManager>().enabled = false;
        FindObjectOfType<PlayerInputManager>().GetComponent<Animator>().enabled = false;
        FindObjectOfType<GameController>().GetComponent<BoxCollider>().enabled = false;
        FindObjectOfType<BrakeKnife>().GetComponent<BoxCollider>().enabled = false;
        FindObjectOfType<PlatforMove>().enabled = false;
        FindObjectOfType<platform>().enabled = false;
    }

   public void Home()
    {
        SceneManager.LoadScene(1);
    }
}
