using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlatforMove : MonoBehaviour
{
    public Transform startpostion;
    public GameObject startpostion2;
    public GameObject floar;

    public float instatiationSpeed;
    void Start()
    {
        
        startpostion2 = GameObject.FindGameObjectWithTag("AiObjectSpawnPoint");

        if (SceneManager.GetActiveScene().buildIndex <= 10)
        {
            InvokeRepeating("vehicledelay", 0.1f, 10);

        }
        else if (SceneManager.GetActiveScene().buildIndex <= 20)
        {
            InvokeRepeating("vehicledelay", 0.1f, 8);
        }
        else if (SceneManager.GetActiveScene().buildIndex <= 30)
        {
            InvokeRepeating("vehicledelay", 0.1f, 6);

        }
        else if (SceneManager.GetActiveScene().buildIndex <= 50)
        {
            InvokeRepeating("vehicledelay", 0.1f, 6);

        }
        else if (SceneManager.GetActiveScene().buildIndex <= 60)
        {
            InvokeRepeating("vehicledelay", 0.1f, 7);

        }
        else if (SceneManager.GetActiveScene().buildIndex <= 70)
        {
            InvokeRepeating("vehicledelay", 0.1f, 4);


        }
        else if (SceneManager.GetActiveScene().buildIndex <= 100)
        {
            InvokeRepeating("vehicledelay", 0.1f, 4);

        }

    }
   

    public void vehicledelay()
    {
        GameObject floarinstance1, floarinstance;
        floarinstance = Instantiate(floar,startpostion.position, Quaternion.identity) ;
        if (MenuScript.mul == 1)
        {

            floarinstance1 = Instantiate(floar, startpostion2.transform.position, Quaternion.identity);
            for (int i = 0; i <4; i++)
            {
                floarinstance1.transform.GetChild(i).gameObject.name = "MyAiFloor";
                floarinstance1.transform.GetChild(i).gameObject.tag = "Avoid";

                floarinstance1.transform.GetChild(i).GetComponent<BoxCollider>().size = new Vector3(1, 30, 1.8f);
            }
            //for (int i = 5; i <= 9; i++)
            //{

            //    floarinstance1.transform.GetChild(i).transform.GetChild(0).transform.GetChild(0).GetComponent<MeshRenderer>().material = FindObjectOfType<GraterGameManager>().AiBoard;
            //    if (floarinstance1.transform.GetChild(i).transform.GetChild(0).childCount == 2)
            //    {
            //        floarinstance1.transform.GetChild(i).transform.GetChild(0).transform.GetChild(1).GetComponent<MeshRenderer>().material = FindObjectOfType<GraterGameManager>().AiBoard;
            //    }
            //}
        }
    }
    public void tomakecancelinvoke()
    {
        CancelInvoke();
    }
}
