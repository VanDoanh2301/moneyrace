using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingBlockMoveSpeed : MonoBehaviour
{
    public float buildingBlockMoveSpeed;
    [SerializeField] private float zOffset;
    [SerializeField] private GameObject playerController;

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.DOMoveZ(playerController.transform.position.z + zOffset, buildingBlockMoveSpeed);
    }
}
