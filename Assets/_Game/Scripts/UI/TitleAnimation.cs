using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleAnimation : MonoBehaviour
{
    [SerializeField] Transform _title;
    [SerializeField] Transform _tutorial;

    [Space]
    public float _duration = .2f;
    public Vector3 _to;

    Vector3 _defaultPos;
    RectTransform _titleRect;
    CanvasGroup _cg;

    private void Awake()
    {
        _titleRect = _title.gameObject.GetComponent<RectTransform>();
        _defaultPos = _titleRect.anchoredPosition;

        _cg = _tutorial.gameObject.GetComponent<CanvasGroup>();
        if (!_cg) _cg = _tutorial.gameObject.AddComponent<CanvasGroup>();
    }

    public void SetEnable()
    {
        LeanTween.move(_titleRect, _defaultPos + _to, _duration).setEase(LeanTweenType.easeOutQuad).setLoopPingPong();

        _cg.alpha = 0f;
        LeanTween.alphaCanvas(_cg, 1f, _duration * .5f).setLoopPingPong();
    }

    public void SetDisable()
    {
        LeanTween.cancel(_title.gameObject);
        _cg.alpha = 0f;

        LeanTween.cancel(_tutorial.gameObject);
        if (_titleRect) _titleRect.anchoredPosition = _defaultPos;
    }
}
