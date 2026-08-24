using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KinematicOFF : MonoBehaviour
{
    public Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "cuttingobjects")
        {
            other.gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }
    }
}
