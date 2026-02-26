using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyPathGen : MonoBehaviour
{
    [SerializeField] private int numberOfMoneyOnPath;
    [SerializeField] private GameObject money;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfMoneyOnPath; i++)
        {

            GameObject newMoneyStack = Instantiate(money, new Vector3(gameObject.transform.position.x + (-0.2f * i), gameObject.transform.position.y + (0.1f * i), gameObject.transform.position.z), Quaternion.identity);
            newMoneyStack.transform.SetParent(gameObject.transform);
        }
    }
}
