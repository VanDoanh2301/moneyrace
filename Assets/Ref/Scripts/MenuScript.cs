using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    public static MenuScript instance;
    public PlatforMove PlatformMover;
    public static int mul;
    public GraterGameManager GM;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        GM = GetComponent<GraterGameManager>();
        PlatformMover = GetComponent<PlatforMove>();
    }

    public void singlePlayer()
    {
        mul = 0;
        SceneManager.LoadScene(PlayerPrefs.GetString("level", "1"));
    }

    public void MultiPlayer()
    {
        mul = 1;
        SceneManager.LoadScene(PlayerPrefs.GetString("level", "1"));
    }

    public void PrivacyPolicy()
    {
        Application.OpenURL("https://unconditionalgames.s3.ap-south-1.amazonaws.com/UnConditionalPrivacyPolicy.txt");
    }

}
