using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Dán một "vai trò màu" lên một Graphic. Khi <see cref="BallTheme"/> đổi palette thì
/// component tự lerp sang màu mới trong <see cref="FadeDuration"/> giây — khớp nhịp với
/// coroutine ChangeColor của ColorManager để UI và nền game đổi màu cùng lúc.
/// </summary>
[ExecuteAlways]
[DisallowMultipleComponent]
public class ThemedGraphic : MonoBehaviour
{
    public enum ThemeRole
    {
        Surface,
        SurfaceAlt,
        Header,
        Accent,
        Scrim,
        Outline,
        Gloss,
        TextOnSurface,
        TextOnAccent,
        TextOnGameBg,
        Coin,
        CoinOnGameBg,
        Muted
    }

    /// <summary>Bằng thời gian lerp của ColorManager.ChangeColor (t chạy tới 1 theo deltaTime).</summary>
    private const float FadeDuration = 1f;

    [SerializeField]
    private ThemeRole role = ThemeRole.Surface;

    [Tooltip("Dùng gradient dọc (Image: UIGradient; TMP: VertexGradient). Tắt = màu phẳng.")]
    [SerializeField]
    private bool useGradient = true;

    [Tooltip("Nhân thêm vào alpha cuối cùng — để làm gloss/outline mờ hơn mà không đổi palette.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float alphaScale = 1f;

    private Graphic m_Graphic;
    private TMP_Text m_Tmp;
    private UIGradient m_Gradient;

    private Color m_FromTop, m_FromBottom;
    private Color m_ToTop, m_ToBottom;
    private float m_FadeT = 1f;

    public ThemeRole Role
    {
        get { return role; }
        set { role = value; ApplyImmediate(); }
    }

    public bool UseGradient
    {
        get { return useGradient; }
        set { useGradient = value; ApplyImmediate(); }
    }

    public float AlphaScale
    {
        get { return alphaScale; }
        set { alphaScale = value; ApplyImmediate(); }
    }

    private void Awake()
    {
        Cache();
    }

    private void OnEnable()
    {
        Cache();

        BallTheme.Changed += OnThemeChanged;

        ApplyImmediate();
    }

    private void OnDisable()
    {
        BallTheme.Changed -= OnThemeChanged;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Để builder/Inspector đổi role là thấy ngay trong Scene view.
        if (!Application.isPlaying)
        {
            Cache();
            ApplyImmediate();
        }
    }
#endif

    private void Cache()
    {
        if (m_Graphic == null) m_Graphic = GetComponent<Graphic>();
        if (m_Tmp == null) m_Tmp = GetComponent<TMP_Text>();
        if (m_Gradient == null) m_Gradient = GetComponent<UIGradient>();
    }

    /// <summary>
    /// Tạo UIGradient nếu cần. Tách khỏi <see cref="Cache"/> vì Unity cấm AddComponent
    /// trong OnValidate — builder gọi hàm này, còn Cache chỉ đọc.
    ///
    /// UIGradient là BaseMeshEffect, KHÔNG chạy trên TMP (TMP không đi qua ModifyMesh),
    /// nên chỉ gắn cho Image/Graphic thường.
    /// </summary>
    public void EnsureGradient()
    {
        Cache();

        if (!useGradient || m_Tmp != null || m_Graphic == null) return;

        if (m_Gradient == null)
        {
            m_Gradient = gameObject.AddComponent<UIGradient>();
        }
    }

    private void OnThemeChanged(BallTheme.Palette palette)
    {
        Color top, bottom;
        Resolve(palette, out top, out bottom);

        if (!Application.isPlaying)
        {
            SetColors(top, bottom);
            return;
        }

        m_FromTop = m_ToTop;
        m_FromBottom = m_ToBottom;
        m_ToTop = top;
        m_ToBottom = bottom;
        m_FadeT = 0f;
    }

    private void Update()
    {
        if (m_FadeT >= 1f) return;

        m_FadeT += Time.unscaledDeltaTime / FadeDuration;
        float t = Mathf.Clamp01(m_FadeT);

        SetColors(Color.Lerp(m_FromTop, m_ToTop, t), Color.Lerp(m_FromBottom, m_ToBottom, t));
    }

    public void ApplyImmediate()
    {
        Color top, bottom;
        Resolve(BallTheme.Current, out top, out bottom);

        m_FromTop = m_ToTop = top;
        m_FromBottom = m_ToBottom = bottom;
        m_FadeT = 1f;

        SetColors(top, bottom);
    }

    private void Resolve(BallTheme.Palette p, out Color top, out Color bottom)
    {
        switch (role)
        {
            case ThemeRole.Surface: top = p.SurfaceTop; bottom = p.SurfaceBottom; break;
            case ThemeRole.SurfaceAlt: top = p.SurfaceBottom; bottom = p.SurfaceTop; break;
            case ThemeRole.Header: top = p.HeaderTop; bottom = p.HeaderBottom; break;
            case ThemeRole.Accent: top = p.AccentTop; bottom = p.AccentBottom; break;
            case ThemeRole.Scrim: top = p.Scrim; bottom = p.Scrim; break;
            case ThemeRole.Outline: top = p.Outline; bottom = p.Outline; break;

            // Gloss: dải trắng mạnh ở đỉnh, tắt dần xuống dưới. Sprite ui_gloss đã có
            // sẵn alpha ramp, gradient ở đây chỉ làm đậm thêm phần đỉnh.
            case ThemeRole.Gloss:
                top = p.Gloss;
                bottom = new Color(p.Gloss.r, p.Gloss.g, p.Gloss.b, 0f);
                break;

            case ThemeRole.TextOnSurface: top = p.OnSurface; bottom = p.OnSurface; break;
            case ThemeRole.TextOnAccent: top = p.OnAccent; bottom = p.OnAccent; break;
            case ThemeRole.TextOnGameBg: top = p.OnGameBg; bottom = p.OnGameBg; break;
            case ThemeRole.Coin: top = p.Coin; bottom = p.Coin; break;
            case ThemeRole.CoinOnGameBg: top = p.CoinOnGameBg; bottom = p.CoinOnGameBg; break;
            case ThemeRole.Muted: top = p.Muted; bottom = p.Muted; break;

            default: top = p.SurfaceTop; bottom = p.SurfaceBottom; break;
        }

        if (alphaScale < 1f)
        {
            top.a *= alphaScale;
            bottom.a *= alphaScale;
        }
    }

    private void SetColors(Color top, Color bottom)
    {
        if (m_Tmp != null)
        {
            m_Tmp.color = top;

            if (useGradient && !Approximately(top, bottom))
            {
                m_Tmp.enableVertexGradient = true;
                m_Tmp.colorGradient = new VertexGradient(top, top, bottom, bottom);
            }
            else
            {
                m_Tmp.enableVertexGradient = false;
            }

            return;
        }

        if (m_Graphic == null) return;

        if (m_Gradient != null && useGradient)
        {
            // UIGradient nhân với vertex color, nên Graphic.color phải là trắng để
            // gradient ra đúng màu; alpha vẫn lấy từ top để fade được cả cụm.
            m_Graphic.color = new Color(1f, 1f, 1f, Mathf.Max(top.a, bottom.a));
            m_Gradient.SetColors(
                new Color(top.r, top.g, top.b, SafeRatio(top.a, Mathf.Max(top.a, bottom.a))),
                new Color(bottom.r, bottom.g, bottom.b, SafeRatio(bottom.a, Mathf.Max(top.a, bottom.a))));
        }
        else
        {
            m_Graphic.color = top;
        }
    }

    private static float SafeRatio(float value, float max)
    {
        return max <= 0.0001f ? 1f : value / max;
    }

    private static bool Approximately(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) < 0.003f
            && Mathf.Abs(a.g - b.g) < 0.003f
            && Mathf.Abs(a.b - b.b) < 0.003f
            && Mathf.Abs(a.a - b.a) < 0.003f;
    }
}
