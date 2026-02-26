using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using UnityEngine;

public class StackIncrease : MonoBehaviour
{
    [SerializeField] private GameObject stackPos;
    private float stackCheckpoint = 100f;
    [SerializeField] private GameObject dollarEffect;
    [SerializeField] private GameObject moneyIndicator;
    [SerializeField] private GameObject moneyIndicatorPos;
    [SerializeField] private GameObject playerBlock;

    public bool endingCalculating = false;

    private void Update()
    {
        if (!endingCalculating)
        {
            for (int i = 0; i < stackPos.GetComponent<StackPosController>().moneyStack.Length; i++)
            {
                if (gameObject.GetComponent<PlayerPowerController>().moneyAmount >= stackCheckpoint)
                {
                    stackPos.GetComponent<StackPosController>().moneyStack[i].gameObject.SetActive(true);
                    stackCheckpoint += 100f;
                }
                else
                {
                    stackCheckpoint = 100f;
                    return;
                }
            }
        }
    }

    //When hit another building block
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Uncollected"))
        {
            collision.gameObject.SetActive(false);

            Instantiate(dollarEffect, collision.gameObject.transform.position, Quaternion.identity);

            GameObject newIndicator = Instantiate(moneyIndicator, moneyIndicatorPos.transform.position, Quaternion.identity);
            newIndicator.GetComponent<MoneyIndicatorValue>().impactValue.text = string.Format("+" + "{0:0}", collision.gameObject.GetComponent<MoneyStackValue>().moneyValue);
            newIndicator.transform.SetParent(playerBlock.transform);
            StartCoroutine("DelayIndicatorDisable", newIndicator);

            //Gain some money
            gameObject.GetComponent<PlayerPowerController>().moneyAmount += collision.gameObject.GetComponent<MoneyStackValue>().moneyValue;

            GameManager.instance.gemWithStackMoney += collision.gameObject.GetComponent<MoneyStackValue>().stackValue;

            gameObject.GetComponent<EndingCalculation>().stackCollected++;
        }
    }

    IEnumerator DelayIndicatorDisable(GameObject newIndicator)
    {
        yield return new WaitForSeconds(0.36f);
        Destroy(newIndicator);
    }
}
