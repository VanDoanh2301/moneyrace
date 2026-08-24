using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraterInputManager : MonoBehaviour
{
    public bool mouseCliked;
    public float speed;
    public Rigidbody rgBody;

    public Animator grateranimation;

    private void Start()
    {
        grateranimation = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            grateranimation.SetTrigger("MouseDown");            
        }

        if (Input.GetMouseButtonUp(0))
        {
            grateranimation.SetTrigger("MouseUp");
        }
    }

    public void FailAnimation()
    {
        rgBody.gameObject.GetComponent<MeshCollider>().isTrigger = false;
        grateranimation.SetTrigger("Fail");
        rgBody.constraints = RigidbodyConstraints.None;
        rgBody.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        rgBody.AddForce(-500, 0, 0);
        rgBody.useGravity = true;
        FindObjectOfType<GraterGameManager>().LevelFail();
        FindObjectOfType<GraterGameManager>().gameover();
    }
}
