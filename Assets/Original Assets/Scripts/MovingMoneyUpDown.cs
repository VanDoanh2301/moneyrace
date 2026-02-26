using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class MovingMoneyUpDown : MonoBehaviour
{
    [SerializeField] private float moveSpeed = -0.2f;
    private bool canShift = true;

    void Update()
    {
        
        if (gameObject.transform.position.y <= 0.1 || gameObject.transform.position.y >= 0.4f)
        {
            if (canShift)
            {
                moveSpeed = -moveSpeed;
                canShift = false;
            }
        }
        if (gameObject.transform.position.y > 0.1 && gameObject.transform.position.y < 0.4f)
        {
            canShift = true;
        }
        gameObject.transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
    }

}
