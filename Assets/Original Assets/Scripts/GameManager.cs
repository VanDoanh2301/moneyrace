using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject splineComputer;
    public SplineComputer spline;

    public static float totalGemAmount;
    public float currentGemCollected = 0f;

    public float gemWithIncome;
    public float gemWithStackMoney;
    public float gemByCompleteMap;
    public float gemByStar;

    public bool gainPower = false;

    public GameObject player;

    public GameObject canvas;
    public bool hasWon = false;
    public bool gameOver = false;
    public bool startLosing = true;
    public GameObject powerDisplay;
    public Text powerDisplayText;
    public Text levelNoDisplay;

    public Image secondStar;
    public Image thirdStar;

    public Image fillDistanceBar;
    public GameObject cameraFollowPoint;

    public int levelNo;

    public float totalNumberOfStack;

    public GameObject stackPos;
    public float increamentBlockSpeed;

    public GameObject[] dataObstacle;

    //[System.Serializable]
    //public class Level
    //{
    //    //public Item[] listItem;
    //    public GameObject level;
    //}

    //[System.Serializable]
    //public class Item
    //{
    //    public int idObj;
    //    public float posX;
    //    public float posZ;
    //    public int[] listOfChildrenId;
    //}
    public List<GameObject> dataLevels;

   
    private void Awake()
    {
        instance = this;

        Time.timeScale = 0;
        levelNo = PlayerPrefs.GetInt("Level_Number", 0);
        if (levelNo >= dataLevels.Count)
        {
            levelNo = 0;
        }
        totalGemAmount = PlayerPrefs.GetFloat("Total_Gem", 0);
    }

    // Start is called before the first frame update
    void Start()
    {
        //SplineComputer spline = splineComputer.GetComponent<SplineComputer>();
        //for (int i = 0; i < dataLevels[LevelNo].pointLocation.Count; i++)
        //{
        //    spline.SetPointPosition(i, dataLevels[LevelNo].pointLocation[i]);
        //}
        //foreach (var data in dataLevels)
        //{
        //    data.level.SetActive(false);
        //}

        //dataLevels[levelNo].level.SetActive(true);

        levelNoDisplay.text = string.Format("Level " + "{0:0}", levelNo + 1);

        ObstacleSpawn();

        //Setup money stack value through upgrade
        foreach (var stack in GameObject.FindGameObjectsWithTag("Uncollected"))
        {
            stack.GetComponent<MoneyStackValue>().moneyValue += stack.GetComponent<MoneyStackValue>().moneyValue * MenuManager.instance.moneyStackMod;
        }
    }

    private void Update()
    {
        SpeedCalculation();

        LetUsStartTheGame();

        WinScreenPopup();

        LoseCondition();
        LoseScreenPopup();

        fillDistanceBar.GetComponent<Image>().fillAmount = 1 - (cameraFollowPoint.transform.position.x / spline.CalculateLength() + 0.227217f);

        currentGemCollected = gemWithStackMoney + gemByCompleteMap + gemByStar + gemWithIncome;
    }

    void LetUsStartTheGame()
    {
        if (Input.GetMouseButtonDown(0) && canvas.transform.GetChild(0).gameObject.activeSelf && !EventSystem.current.IsPointerOverGameObject(0))
        {
            Time.timeScale = 1;
            canvas.transform.GetChild(0).gameObject.SetActive(false);
            canvas.transform.GetChild(1).gameObject.SetActive(true);

            player.GetComponent<Animator>().SetTrigger("Start");
            player.transform.rotation = Quaternion.Euler(0, -90, 0);
        }
    }
    public void SpeedCalculation()
    {
        for (int i = 0; i < stackPos.transform.childCount; i++)
        {
            stackPos.transform.GetChild(i).gameObject.GetComponent<BuildingBlockMoveSpeed>().buildingBlockMoveSpeed = Constants.STARTING_BUILDING_BLOCK_SPEED + (increamentBlockSpeed * i);
        }
    }

    void WinScreenPopup()
    {
        if (hasWon)
        {
            Time.timeScale = 0;
            canvas.transform.GetChild(2).gameObject.SetActive(true);
        }
    }

    void LoseScreenPopup()
    {
        if (gameOver)
        {
            Time.timeScale = 0;
            canvas.transform.GetChild(3).gameObject.SetActive(true);
        }
    }

    void LoseCondition()
    {
        if (player.GetComponent<PlayerPowerController>().moneyAmount < 0 && startLosing)
        {
            startLosing = false;
            StartCoroutine("LoseScreenDelay");
        }
    }

    IEnumerator LoseScreenDelay()
    {
        yield return new WaitForSeconds(1.5f);
        gameOver = true;
    }

    void ObstacleSpawn()
    {
        //foreach (var item in dataLevels[levelNo].listItem)
        //{
        //    GameObject newObstacle = Instantiate(dataObstacle[item.idObj], new Vector3(item.posX, dataObstacle[item.idObj].transform.position.y, item.posZ), dataObstacle[item.idObj].transform.rotation);

        //    if (item.idObj == 4)
        //    {
        //        newObstacle.GetComponent<PowerPortalsSetting>().portalId = item.listOfChildrenId;
        //    }
        //    if (item.idObj == 0)
        //    {
        //        newObstacle.GetComponent<BundleControl>().numberOfMoneyStack = item.listOfChildrenId[0];
        //        newObstacle.GetComponent<BundleControl>().moneyStackId = item.listOfChildrenId[1];
        //    }
        //    if (item.idObj >= 5)
        //    {
        //        newObstacle.transform.localRotation = Quaternion.Euler(0, 90, 0);
        //    }
        //}
        Instantiate(dataLevels[levelNo], dataLevels[levelNo].transform.position, dataLevels[levelNo].transform.rotation);

        StartCoroutine("DelayCountingStack");
    }

    IEnumerator DelayCountingStack()
    {
        yield return new WaitForSecondsRealtime(0.5f);
        totalNumberOfStack = GameObject.FindGameObjectsWithTag("Uncollected").Length;
    }
}
