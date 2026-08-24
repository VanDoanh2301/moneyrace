using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailScript : MonoBehaviour
{
    public GraterInputManager animationn;
    public AIScript AIanimationn;

    private void Start()
    {
        animationn = FindObjectOfType<GraterInputManager>();
        AIanimationn = FindObjectOfType<AIScript>();
    }

    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            animationn.FailAnimation();
            Debug.Log("fal");
            platform[] platform = FindObjectsOfType<platform>();
            for(int i = 0; i < platform.Length; i++)
            {
                platform[i].stopmovement = false;
            }
        }

        if (collision.gameObject.tag == "Ai")
        {
            AIanimationn.AIFailAnimation();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Ai")
        {
            AIanimationn.AIFailAnimation1();
        }
    }
}
