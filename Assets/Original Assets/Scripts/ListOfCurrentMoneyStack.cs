using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ListOfCurrentMoneyStack : MonoBehaviour
{
    public List<GameObject> listOfMoneyStack;
    [SerializeField] private GameObject stackPos;
    [SerializeField] private GameObject playerController;
    [SerializeField] private GameObject moneyPath;

    private void Update()
    {
        for (int i = 0; i < moneyPath.transform.childCount; i++)
        {
            if (gameObject.transform.position.x < moneyPath.transform.GetChild(i).position.x)
            {
                moneyPath.transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Stair"))
        {
            for (int i = 0; i < stackPos.GetComponent<StackPosController>().moneyStack.Length; i++)
            {
                if (stackPos.GetComponent<StackPosController>().moneyStack[i].activeSelf)
                {
                    listOfMoneyStack.Add(stackPos.GetComponent<StackPosController>().moneyStack[i]);
                }
            }
            playerController.GetComponent<control>().enabled = false;
        }

        if (other.gameObject.CompareTag("Staircase"))
        {
            gameObject.GetComponent<StackIncrease>().endingCalculating = true;

            if (listOfMoneyStack.Count < 10)
            {
                for (int i = 0; i < listOfMoneyStack.Count; i++)
                {
                    listOfMoneyStack[listOfMoneyStack.Count - 1].gameObject.SetActive(false);
                    listOfMoneyStack.Remove(listOfMoneyStack[listOfMoneyStack.Count - 1].gameObject);
                }
            }
            else
            {
                for (int i = 0; i < other.gameObject.GetComponent<StaircaseValue>().moneyStackNeeded; i++)
                {
                    Debug.Log(listOfMoneyStack[listOfMoneyStack.Count - 1].gameObject);
                    listOfMoneyStack[listOfMoneyStack.Count - 1].SetActive(false);
                    listOfMoneyStack.Remove(listOfMoneyStack[listOfMoneyStack.Count - 1].gameObject);
                }
                if (listOfMoneyStack.Count < 5)
                {
                    for (int i = 0; i < listOfMoneyStack.Count; i++)
                    {
                        listOfMoneyStack[listOfMoneyStack.Count - 1].SetActive(false);
                        listOfMoneyStack.Remove(listOfMoneyStack[listOfMoneyStack.Count - 1].gameObject);
                    }
                }
            }
        }
    }
}
