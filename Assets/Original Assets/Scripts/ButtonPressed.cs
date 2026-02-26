using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonPressed : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 1.15f;
        if (gameObject.TryGetComponent(out Animator anim))
        {
            anim.enabled = false;
        }
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
        if (gameObject.TryGetComponent(out Animator anim))
        {
            anim.enabled = true;
        }
    }
}
