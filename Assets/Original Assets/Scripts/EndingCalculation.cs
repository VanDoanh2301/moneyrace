using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingCalculation : MonoBehaviour
{
    public float stackCollected;
    [SerializeField] private GameObject invisibleTrack;
    [SerializeField] private GameObject playerController;

    // Start is called before the first frame update
    void Start()
    {
        stackCollected = 0;
    }

    // Update is called once per frame
    void Update()
    {
        CalculateGemByStar();
        CalculateGemByCompleteMap();
        CalculateGemWithIncome();
    }

    void CalculateGemByStar()
    {
        float starPercentage = stackCollected / GameManager.instance.totalNumberOfStack;

        if (starPercentage <= 0.3f)
        {
            GameManager.instance.gemByStar = 100;
        }
        else if (starPercentage > 0.3f && starPercentage < 0.6f)
        {
            GameManager.instance.secondStar.gameObject.SetActive(true);
            GameManager.instance.gemByStar = 200;
        }
        else if (starPercentage >= 0.6f)
        {
            GameManager.instance.secondStar.gameObject.SetActive(true);
            GameManager.instance.thirdStar.gameObject.SetActive(true);
            GameManager.instance.gemByStar = 300;
        }
    }

    void CalculateGemByCompleteMap()
    {
        float stackMoneyAmount = gameObject.GetComponent<PlayerPowerController>().moneyAmount;
        float stackMultiplier = 0f;

        if (stackMoneyAmount < 1500)
        {
            stackMultiplier = 1;
        }
        else if (stackMoneyAmount >= 1500 && stackMoneyAmount < 2500)
        {
            stackMultiplier = 2;
        }
        else if (stackMoneyAmount >= 2500 && stackMoneyAmount < 3500)
        {
            stackMultiplier = 3;
        }
        else if (stackMoneyAmount >= 3500 && stackMoneyAmount < 4500)
        {
            stackMultiplier = 4;
        }
        else if (stackMoneyAmount >=4500 && stackMoneyAmount < 5500)
        {
            stackMultiplier = 5;
        }
        else if (stackMoneyAmount >= 5500 && stackMoneyAmount < 6500)
        {
            stackMultiplier = 6;    
        }
        else if (stackMoneyAmount >= 6500 && stackMoneyAmount < 7500)
        {
            stackMultiplier = 7;
        }
        else if (stackMoneyAmount >= 7500 && stackMoneyAmount < 8500)
        {
            stackMultiplier = 8;
        }
        else if (stackMoneyAmount >= 8500 && stackMoneyAmount < 9500)
        {
            stackMultiplier = 9;
        }
        else
        {
            stackMultiplier = 10;
        }


        GameManager.instance.gemByCompleteMap = GameManager.instance.gemWithStackMoney * stackMultiplier;
    }

    void CalculateGemWithIncome()
    {
        GameManager.instance.gemWithIncome = (GameManager.instance.gemByCompleteMap + GameManager.instance.gemByStar + GameManager.instance.gemWithStackMoney) / 100 * MenuManager.instance.incomePercentage;
    }

    private void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Stair"))
        {
            invisibleTrack.GetComponent<SplineFollower>().follow = false;
            playerController.GetComponent<control>().enabled = false;
        }
    }
}
