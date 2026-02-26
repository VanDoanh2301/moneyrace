using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairBuilding : MonoBehaviour
{
    [SerializeField] private int numberOfStaircase;
    [SerializeField] private GameObject staircase;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < numberOfStaircase; i++)
        {
            GameObject newStaircase = Instantiate(staircase, new Vector3(gameObject.transform.position.x + (-2f * i), gameObject.transform.position.y + (1f * i), gameObject.transform.position.z), Quaternion.identity);
            newStaircase.transform.SetParent(gameObject.transform);
        }
    }
}
