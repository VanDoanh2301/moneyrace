using DG.Tweening;
using Dreamteck.Splines;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float originalMouseX;
    [SerializeField] private float sidestepSpeed;

    [SerializeField] private GameObject invisibleTrack;
    [SerializeField] private Transform xMax;
    [SerializeField] private Transform xMin;

    // Update is called once per frame
    void Update()
    {
        EnablePlayerMovement();
        MakePlayerFollowTheTrack();
    }

    void EnablePlayerMovement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            originalMouseX = Input.mousePosition.x;
        }

        if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition.x - originalMouseX > 5f)
            {
                gameObject.transform.Translate(Vector3.right * sidestepSpeed * Time.deltaTime);
            }
            else if (Input.mousePosition.x - originalMouseX < -5f)
            {
                gameObject.transform.Translate(Vector3.left * sidestepSpeed * Time.deltaTime);
            }
            originalMouseX = Input.mousePosition.x;
        }


        if (gameObject.transform.position.x > xMax.position.x)
        {
            gameObject.transform.position = xMax.position;
        }
        else if (gameObject.transform.position.x < xMin.position.x)
        {
            gameObject.transform.position = xMin.position;
        }
    } 

    void MakePlayerFollowTheTrack()
    {
        float distance = gameObject.transform.position.x - invisibleTrack.transform.position.x; 
        if (distance <= Constants.INVIS_CUBE_TO_BUILDING_BLOCK_DISTANCE)
        {
            invisibleTrack.GetComponent<SplineFollower>().follow = true;
            PlayerBlockMovement.instance.playerBlockSpeed = 0;
        }
    }
}
