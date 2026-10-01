using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerRenderer : MonoBehaviour
{
    public UnityEvent OnInvisible;

    Camera _viewCam;

    private void Start()
    {
        _viewCam = Camera.main;
    }

    private void Update()
    {
        Vector2 screenPos = _viewCam.WorldToScreenPoint(transform.position);

        bool becomeInvisible = screenPos.x < -50f || screenPos.x > Screen.width + 50f || screenPos.y < 100f;

        if (becomeInvisible)
        {
            OnInvisible?.Invoke();
        }
    }
}
