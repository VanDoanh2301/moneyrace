using UnityEngine;

/// <summary>
/// Nối <see cref="ColorManager.OnPaletteChanged"/> vào <see cref="BallTheme"/>.
/// Đặt trên Canvas. Tách riêng khỏi ColorManager để ColorManager không phải biết gì về UI.
/// </summary>
[DisallowMultipleComponent]
public class BallThemeDriver : MonoBehaviour
{
    private ColorManager m_Bound;

    private void OnEnable()
    {
        Bind();
    }

    private void Start()
    {
        // ColorManager.Start() gọi ChangeColors() ngay, có thể chạy trước OnEnable của
        // component này tuỳ thứ tự khởi tạo — nên đồng bộ lại một lần ở đây.
        Bind();
        Sync();
    }

    private void Update()
    {
        // Instance được gán ở Awake; nếu lúc OnEnable chưa có thì bắt lại.
        if (m_Bound == null)
        {
            Bind();
            Sync();
        }
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void Bind()
    {
        ColorManager manager = ColorManager.Instance;
        if (manager == null || manager == m_Bound) return;

        Unbind();

        m_Bound = manager;
        m_Bound.OnPaletteChanged += OnPaletteChanged;
    }

    private void Unbind()
    {
        if (m_Bound == null) return;

        m_Bound.OnPaletteChanged -= OnPaletteChanged;
        m_Bound = null;
    }

    private void Sync()
    {
        if (m_Bound != null && m_Bound.CurrentSet != null)
        {
            BallTheme.Apply(m_Bound.CurrentSet);
        }
    }

    private void OnPaletteChanged(ColorList set)
    {
        BallTheme.Apply(set);
    }
}
