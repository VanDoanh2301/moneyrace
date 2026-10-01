using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PlayerInput : MonoBehaviour
{
    [field: SerializeField]
    public UnityEvent OnJump { get; set; }

    private void Update()
    {
#if UNITY_EDITOR
        GetInput();
#endif

#if UNITY_ANDROID
        GetMobileInput();
#endif
    }

    private void GetInput()
    {
        if (Application.isEditor && !EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButtonDown(0))
                OnJump?.Invoke();
        }
    }

    private void GetMobileInput()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                OnJump?.Invoke();
        }
    }
}