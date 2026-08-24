using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class cameramove : MonoBehaviour
{
    public Camera MainCamera;
    public GameObject TargetPosition;
    public int speed = 2;
    bool camera_move_enabled = false;

    public void Update()
    {
            MainCamera.transform.position = Vector3.Lerp(transform.position, TargetPosition.transform.position, speed * Time.deltaTime);
            MainCamera.transform.rotation = Quaternion.Lerp(transform.rotation, TargetPosition.transform.rotation, speed * Time.deltaTime);
    }
}
