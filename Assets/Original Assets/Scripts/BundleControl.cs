using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BundleControl : MonoBehaviour
{
    public int numberOfMoneyStack;
    public int moneyStackId;
    [SerializeField] private List<GameObject> listOfMoneyStack;

    private void Start()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Destroy(gameObject.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < numberOfMoneyStack; i++)
        {
            GameObject newMoneyStack = Instantiate(listOfMoneyStack[moneyStackId], new Vector3(gameObject.transform.position.x + (-0.3f * i), listOfMoneyStack[moneyStackId].gameObject.transform.position.y, gameObject.transform.position.z), listOfMoneyStack[moneyStackId].transform.rotation);
            newMoneyStack.transform.SetParent(gameObject.transform);
        }
    }
}
