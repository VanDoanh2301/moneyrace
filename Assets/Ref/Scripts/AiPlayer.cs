using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiPlayer : MonoBehaviour
{
    public bool mouseCliked;
    public float speed;
    public Transform knife;

    public GameObject knfe, facturedknife;
    public bool aistop,play;
    public void Start()
    {
        aistop = false;
        play = false;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
           // GetComponent<Animator>().enabled = true;
            play = true;
        }

        if (aistop)
        {
            GetComponent<Animator>().enabled = false;
        }
        else if(!aistop && play)
        {
            GetComponent<Animator>().enabled = true;
        }
    }
}
