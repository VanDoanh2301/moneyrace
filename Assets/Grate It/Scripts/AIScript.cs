using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIScript : MonoBehaviour
{
    public bool mouseCliked;
    public bool aistop, play;
    public Animator AIgrateranimation;
    public Rigidbody AIrgBody;

    public void Start()
    {
        AIgrateranimation = GetComponent<Animator>();
        aistop = false;
        play = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            play = true;
        }

        if (aistop)
        {
            AIgrateranimation.SetTrigger("AIMouseUp");
            //AIgrateranimation.SetBool("Idle", false);
        }
        else if (!aistop && play)
        {
            AIgrateranimation.SetTrigger("AIMouseDown");
            //GetComponent<Animator>().enabled = true;
        }
    }

    public void AIFailAnimation()
    {
        AIgrateranimation.SetTrigger("AIMouseUp");
        //AIgrateranimation.SetTrigger("AIFail");
        //AIrgBody.constraints = RigidbodyConstraints.None;
        //AIrgBody.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        //AIrgBody.AddForce(500, 0, 0);
        //AIrgBody.useGravity = true;
    }

    public void AIFailAnimation1()
    {
        AIgrateranimation.SetTrigger("AIMouseDown");
    }
}
