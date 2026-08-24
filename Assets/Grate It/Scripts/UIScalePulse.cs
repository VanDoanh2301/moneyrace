using UnityEngine;

/// <summary>
/// Hiệu ứng phóng to/thu nhỏ nhẹ theo hình sin, dùng cho logo/UI trên Main Menu.
/// </summary>
public class UIScalePulse : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.06f;
    [SerializeField] private float speed = 1.5f;

    private Vector3 _baseScale;
    private float _seed;

    private void Awake()
    {
        _baseScale = transform.localScale;
        _seed = Random.Range(0f, Mathf.PI * 2f);
    }

    private void OnEnable()
    {
        transform.localScale = _baseScale;
    }

    private void Update()
    {
        float scale = 1f + Mathf.Sin(Time.unscaledTime * speed + _seed) * amplitude;
        transform.localScale = _baseScale * scale;
    }
}
