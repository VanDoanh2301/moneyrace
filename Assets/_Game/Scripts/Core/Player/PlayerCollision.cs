using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [field: SerializeField]
    public bool CanJump { get; set; }

    private void Start()
    {
        CanJump = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        CanJump = true;
        transform.parent = collision.transform;
    }

    private void OnCollisionExit(Collision collision)
    {
        CanJump = false;
        transform.parent = null;
    }
}
