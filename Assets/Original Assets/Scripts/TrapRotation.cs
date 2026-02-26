using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapRotation : MonoBehaviour
{
    [SerializeField] private float rotateSpeed;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.unscaledDeltaTime);
    }
}
