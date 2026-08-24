using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class colorChange : MonoBehaviour
{
    public Material aiPlatformColor;

    public void Start()
    {
        if (MenuScript.mul == 0)
        {
            gameObject.transform.gameObject.SetActive(false);
        }
    }

    public void OnTriggerEnter(Collider ditched)
    {
        if (MenuScript.mul == 1&& ditched.gameObject.name== "Board_Model")
        {
           // Debug.Log("mugging");
            ditched.gameObject.GetComponent<MeshRenderer>().material = aiPlatformColor;
           // Destroy(this.gameObject);
        }
    }
}
