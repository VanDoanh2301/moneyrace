using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class platform : MonoBehaviour
{

    private float speed;
    public bool stopmovement;
    PlatforMove _PlatforMove;

    private void Start()
    {

        stopmovement = true;
    }
    
    void Update()
    {
        if (stopmovement)
        {
            transform.Translate(0, 0,-speed * Time.deltaTime);
        }

        if (SceneManager.GetActiveScene().buildIndex <= 10)
        {
           speed = 12;

        }
        else if (SceneManager.GetActiveScene().buildIndex <= 20)
        {
            speed = 16;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 30)
        {
            speed = 20;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 50)
        {
            speed = 20;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 60)
        {
            speed = 22;
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 70)
        {
            speed = 25;

        }
        else if (SceneManager.GetActiveScene().buildIndex <= 100)
        {
            speed = 25;
        }

    }
}
