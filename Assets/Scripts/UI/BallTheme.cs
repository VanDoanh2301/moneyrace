using System;
using UnityEngine;

/// <summary>
/// Dẫn xuất bảng màu UI từ bộ màu đang chạy của <see cref="ColorManager"/>.
///
/// Bảng màu của game dao động rất mạnh về độ sáng (backgroundMain từ #F7E967 vàng chói
/// tới #36B1BF teal; Ground1 từ #FFCB05 tới #2B3A42 xám đen), nên không thể hardcode màu
/// chữ. Ở đây giữ hue + saturation của game rồi ÉP luminance về dải đọc được, và chọn
/// màu chữ theo luminance nền thực tế.
/// </summary>
public static class BallTheme
{
    public struct Palette
    {
        public Color SurfaceTop;
        public Color SurfaceBottom;
        public Color HeaderTop;
        public Color HeaderBottom;
        public Color AccentTop;
        public Color AccentBottom;
        public Color Scrim;
        public Color OnSurface;
        public Color OnAccent;
        public Color OnGameBg;
        public Color Coin;
        public Color CoinOnGameBg;
        public Color Muted;
        public Color Outline;
        public Color Gloss;
    }

    /// <summary>Chữ sáng / chữ tối — hai cực dùng cho mọi nền.</summary>
    private static readonly Color TextLight = new Color(0.961f, 0.961f, 0.973f, 1f); // #F5F5F8
    private static readonly Color TextDark = new Color(0.102f, 0.102f, 0.118f, 1f);  // #1A1A1E

    /// <summary>Vàng coin gốc của game (Final Score / PlayAgain đang dùng #FFD200).</summary>
    private static readonly Color CoinGold = new Color(1f, 0.824f, 0f, 1f);

    private static bool m_HasPalette;
    private static Palette m_Current;

    /// <summary>Phát mỗi khi palette đổi. UI nghe qua ThemedGraphic.</summary>
    public static event Action<Palette> Changed;

    public static Palette Current
    {
        get
        {
            if (!m_HasPalette)
            {
                // Mặc định = bộ 0 của game, để edit mode và frame đầu (trước
                // ColorManager.Start) vẫn ra đúng tông thay vì đen/trắng trơ.
                m_Current = Derive(
                    Hex(0xFFCB05),  // Ground1
                    Hex(0xE9F1DF),  // Ground2
                    Hex(0x36B1BF)); // backgroundMain
                m_HasPalette = true;
            }

            return m_Current;
        }
    }

    public static void Apply(ColorList set)
    {
        if (set == null) return;

        Apply(Derive(set.Ground1, set.Ground2, set.backgroundMain));
    }

    public static void Apply(Palette palette)
    {
        m_Current = palette;
        m_HasPalette = true;

        if (Changed != null)
        {
            Changed(palette);
        }
    }

    // ------------------------------------------------------------------ dẫn xuất

    private static Palette Derive(Color ground1, Color ground2, Color backgroundMain)
    {
        Palette p = new Palette();

        // Mặt phẳng nền: tối, giữ hue của "đất" trong bộ màu.
        p.SurfaceTop = WithLuminance(ground1, 0.16f);
        p.SurfaceBottom = WithLuminance(ground2, 0.09f);

        p.HeaderTop = WithLuminance(ground1, 0.26f);
        p.HeaderBottom = WithLuminance(ground2, 0.16f);

        // Accent = backgroundMain, màu "phải ăn" — màu hero của bộ.
        p.AccentTop = WithLuminance(backgroundMain, 0.62f);
        p.AccentBottom = WithLuminance(backgroundMain, 0.38f);

        Color scrim = WithLuminance(ground2, 0.05f);
        scrim.a = 0.72f;
        p.Scrim = scrim;

        p.OnSurface = BestTextOn(p.SurfaceTop);
        p.OnAccent = BestTextOn(p.AccentTop);
        p.OnGameBg = BestTextOn(backgroundMain);

        p.Coin = Mix(CoinGold, p.AccentTop, 0.25f);

        // Trên MainMenu không có panel: số coin và icon cúp nằm thẳng trên cảnh game,
        // nên màu vàng coin phải được đẩy tới khi đủ tương phản với backgroundMain.
        p.CoinOnGameBg = EnsureContrast(p.Coin, backgroundMain);
        p.Muted = Color.Lerp(p.OnSurface, p.SurfaceTop, 0.45f);

        p.Outline = new Color(1f, 1f, 1f, 0.22f);
        p.Gloss = new Color(1f, 1f, 1f, 1f);

        return p;
    }

    // ------------------------------------------------------------------ helper màu

    /// <summary>Luminance cảm nhận (hệ số Rec.709).</summary>
    public static float Luminance(Color c)
    {
        return 0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b;
    }

    /// <summary>
    /// Giữ hue, ép về luminance mục tiêu. Nền càng tối thì saturation phải đẩy lên
    /// một chút, nếu không mọi bộ màu đều ra xám như nhau và UI mất chất riêng.
    /// </summary>
    public static Color WithLuminance(Color c, float target)
    {
        float h, s, v;
        Color.RGBToHSV(c, out h, out s, out v);

        // Màu gần như xám (ví dụ #FFFFFF của bộ 3) thì cho mượn một chút sắc
        // để panel không bị xám chết.
        if (s < 0.08f) s = 0.08f;

        if (target < 0.3f)
        {
            s = Mathf.Clamp01(s * 1.25f + 0.08f);
        }

        // Dò nhị phân theo V vì luminance không tuyến tính với V khi s > 0.
        float lo = 0f;
        float hi = 1f;
        Color result = c;

        for (int i = 0; i < 12; i++)
        {
            float mid = (lo + hi) * 0.5f;
            result = Color.HSVToRGB(h, s, mid);

            if (Luminance(result) < target) lo = mid;
            else hi = mid;
        }

        // Màu bão hoà mạnh có trần luminance thấp (đỏ thuần chỉ tới ~0.21 vì hệ số R
        // là 0.2126), nên chỉ chỉnh V là không với tới các mốc sáng. Khi đó phải nhả
        // saturation về phía trắng — nếu không, hai mốc khác nhau sẽ cùng kẹt ở V=1
        // và gradient biến mất.
        if (Luminance(result) < target - 0.005f)
        {
            float sLo = 0f;      // càng ít sat càng sáng
            float sHi = s;

            for (int i = 0; i < 12; i++)
            {
                float mid = (sLo + sHi) * 0.5f;
                result = Color.HSVToRGB(h, mid, 1f);

                if (Luminance(result) < target) sHi = mid;
                else sLo = mid;
            }
        }

        // Alpha LUÔN đặc. Các màu trong ColorList được serialize với a = 0 (ColorManager
        // tự ghi đè alpha = 1 khi lerp material nên trong game không lộ); giữ nguyên a
        // của nguồn sẽ cho ra panel trong suốt nhìn xuyên thấy cảnh game.
        result.a = 1f;

        return result;
    }

    /// <summary>Ngưỡng tương phản WCAG AA cho chữ lớn/đậm.</summary>
    public const float MinContrast = 4.5f;

    /// <summary>
    /// Chọn chữ sáng hay tối cho nền đã cho. Nền có luminance trung bình (đỏ #EA2E49,
    /// cam #FF6517…) không bên nào đạt ngưỡng với hai màu chữ mặc định, nên đẩy tiếp
    /// màu đã chọn về cực (trắng tinh / đen tuyền) vừa đủ để qua ngưỡng.
    /// </summary>
    public static Color BestTextOn(Color background)
    {
        float toLight = Contrast(TextLight, background);
        float toDark = Contrast(TextDark, background);

        bool useLight = toLight >= toDark;

        Color chosen = useLight ? TextLight : TextDark;
        if ((useLight ? toLight : toDark) >= MinContrast)
        {
            return chosen;
        }

        return EnsureContrast(chosen, background);
    }

    /// <summary>
    /// Đẩy <paramref name="foreground"/> về phía trắng hoặc đen vừa đủ để đạt
    /// <see cref="MinContrast"/> trên nền đã cho, giữ lại nhiều sắc gốc nhất có thể.
    /// </summary>
    public static Color EnsureContrast(Color foreground, Color background)
    {
        if (Contrast(foreground, background) >= MinContrast)
        {
            return foreground;
        }

        // Chọn cực theo tương phản THỰC TẾ, không theo luminance của nền: với đỏ
        // #EA2E49 (luminance ~0.35, "có vẻ tối") thì đen cho 4.98 còn trắng chỉ 4.22.
        Color extreme = Contrast(Color.black, background) >= Contrast(Color.white, background)
            ? Color.black
            : Color.white;

        float lo = 0f;
        float hi = 1f;
        Color result = extreme;

        for (int i = 0; i < 12; i++)
        {
            float mid = (lo + hi) * 0.5f;
            Color candidate = Color.Lerp(foreground, extreme, mid);

            if (Contrast(candidate, background) < MinContrast) lo = mid;
            else { hi = mid; result = candidate; }
        }

        result.a = foreground.a;

        return result;
    }

    /// <summary>Tỉ lệ tương phản WCAG (1..21).</summary>
    public static float Contrast(Color a, Color b)
    {
        float la = RelativeLuminance(a);
        float lb = RelativeLuminance(b);

        float hi = Mathf.Max(la, lb);
        float lo = Mathf.Min(la, lb);

        return (hi + 0.05f) / (lo + 0.05f);
    }

    private static float RelativeLuminance(Color c)
    {
        return 0.2126f * Linear(c.r) + 0.7152f * Linear(c.g) + 0.0722f * Linear(c.b);
    }

    private static float Linear(float channel)
    {
        return channel <= 0.03928f
            ? channel / 12.92f
            : Mathf.Pow((channel + 0.055f) / 1.055f, 2.4f);
    }

    public static Color Mix(Color a, Color b, float t)
    {
        return Color.Lerp(a, b, t);
    }

    public static Color Hex(uint rgb)
    {
        return new Color(
            ((rgb >> 16) & 0xFF) / 255f,
            ((rgb >> 8) & 0xFF) / 255f,
            (rgb & 0xFF) / 255f,
            1f);
    }
}
