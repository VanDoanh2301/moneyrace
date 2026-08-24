using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrakeKnife : MonoBehaviour
{
    public GameObject fracterdknife;

    private void Start()
    {
      fracterdknife = FindObjectOfType<PlayerInputManager>().facturedknife;
       
    }
    platform[] pl;
    public void OnTriggerEnter(Collider collision)
    {

        if (collision.gameObject.tag == "Player")
        {

            pl = FindObjectsOfType<platform>();
            for (int i = 0; i < pl.Length; i++)
            {
                pl[i].stopmovement = false;
            }
            Destroy(collision.gameObject);
            fracterdknife.SetActive(true);
            for (int i = 0; i < fracterdknife.transform.childCount; i++)
            {
                if (fracterdknife.transform.GetChild(i).GetComponent<Rigidbody>())
                {
                    fracterdknife.transform.GetChild(i).GetComponent<Rigidbody>().isKinematic = false;
                    Invoke("levelfailui", 2f);
                }
            }
            collision.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    public void levelfailui()
    { 
       FindObjectOfType<GameManager>().LevelFail();
        FindObjectOfType<GameManager>().gameover();
    }
    

}
