using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerInputManager : MonoBehaviour {

    public bool mouseCliked;
    public float speed;
    public Transform knife;

    public GameObject knfe,facturedknife;
	
	void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        { 
            GetComponent<Animator>().enabled = true;
        }
        if (Input.GetMouseButtonUp(0))
        { 
             GetComponent<Animator>().enabled = false;
            knife.rotation = Quaternion.Euler(0, 0, -45f);//Quaternion.Slerp(knife.rotation, Quaternion.Euler(0, 0, -45f), speed * Time.deltaTime);
        }
    }
}
