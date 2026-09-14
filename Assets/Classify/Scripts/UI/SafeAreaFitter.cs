using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeAreaFitter : MonoBehaviour
{
    [SerializeField] RectTransform panelRect;
    private void Awake()
    {
        if (!panelRect) panelRect = GetComponent<RectTransform>();
        Adjust();
    }

    void Adjust()
    {
        Rect safeRect = Screen.safeArea;
        Vector2 anchorMin = safeRect.position;
        Vector2 anchorMax = safeRect.position + safeRect.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        panelRect.anchorMin = anchorMin;
        panelRect.anchorMax = anchorMax;

    }
}
