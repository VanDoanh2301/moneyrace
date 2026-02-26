using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointerMovement : MonoBehaviour
{
    public GameObject startPos;
    public GameObject targetPos;

    public float speed;

    private bool moveFarEnough = true;

    private Vector2 pointerPos;

    // Start is called before the first frame update
    void Start()
    {
        speed = -speed;
        gameObject.GetComponent<RectTransform>().anchoredPosition = startPos.GetComponent<RectTransform>().anchoredPosition;
    }

    // Update is called once per frame
    void Update()
    {
        pointerPos = gameObject.GetComponent<RectTransform>().anchoredPosition;

        StartCoroutine("DelayStartRunning");
        
        if (pointerPos.x >= -360 && pointerPos.x <= -107)
        {
            MenuManager.instance.multiplierValue = 2;
        }
        else if (pointerPos.x > -107 &&  pointerPos.x <= 100)
        {
            MenuManager.instance.multiplierValue = 5;
        }
        else if (pointerPos.x > 100 && pointerPos.x <= 240)
        {
            MenuManager.instance.multiplierValue = 3;
        }
        else
        {
            MenuManager.instance.multiplierValue = 2;
        }
    }

    IEnumerator DelayStartRunning()
    {
        yield return new WaitForSecondsRealtime(0.8f);
        gameObject.GetComponent<RectTransform>().anchoredPosition += Vector2.right * speed;
        if (pointerPos.x >= targetPos.GetComponent<RectTransform>().anchoredPosition.x || pointerPos.x <= startPos.GetComponent<RectTransform>().anchoredPosition.x)
        {
            if (pointerPos.x >= targetPos.GetComponent<RectTransform>().anchoredPosition.x)
            {
                pointerPos.x = targetPos.GetComponent<RectTransform>().anchoredPosition.x;
            }
            else if (pointerPos.x <= startPos.GetComponent<RectTransform>().anchoredPosition.x)
            {
                pointerPos.x = startPos.GetComponent<RectTransform>().anchoredPosition.x;
            }

            if (moveFarEnough)
            {
                speed = -speed;
                moveFarEnough = false;
                StartCoroutine("DelayRedirection");
            }
        }
    }
    IEnumerator DelayRedirection()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        moveFarEnough = true;
    }
}
