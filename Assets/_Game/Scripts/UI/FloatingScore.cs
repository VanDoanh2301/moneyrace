using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingScore : MonoBehaviour
{
    [SerializeField] float _tweenDuration = 1f;
    [SerializeField] TextMesh _textMesh;
    [SerializeField] Color _defaultColor;
    [SerializeField] Color _fadedColor;

    private void DoTweening()
    {
        LeanTween.value(gameObject, FadedColor, _defaultColor, _fadedColor, _tweenDuration);

        LeanTween.moveLocalY(gameObject, transform.position.y + 2f, _tweenDuration);
    }

    private void FadedColor(Color val)
    {
        _textMesh.color = val;
    }

    public void Spawn(Vector3 pos, int score)
    {
        transform.position = pos;

        _textMesh.text = $"+{score}";

        DoTweening();
    }
}
