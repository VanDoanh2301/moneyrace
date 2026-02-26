using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBlockMovement : MonoBehaviour
{
    public static PlayerBlockMovement instance;
    public float playerBlockSpeed;

    private void Awake()
    {
        instance = this;
    }

    //Keep the block moving to meet a condition
    void Update()
    {
        KeepMoving();
    }

    public void KeepMoving()
    {
        gameObject.transform.Translate(Vector3.forward * playerBlockSpeed * Time.deltaTime);
    }
}
