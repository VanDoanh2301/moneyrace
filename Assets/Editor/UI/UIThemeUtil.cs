using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace BallEditor
{
    /// <summary>
    /// Hàm dựng mặt phẳng "có theme" dùng chung cho <see cref="UIThemeBuilder"/> và
    /// ShopPanelBuilder: nền bo góc + gradient + highlight + viền sáng, và chuyển
    /// Text thường sang TMP.
    /// </summary>
    public static class UIThemeUtil
    {
        public const string GlossName = "Gloss";
        public const string OutlineName = "Outline";
        public const string FontPath = "Assets/Fonts/Rubik-Regular SDF.asset";

        public static Sprite Rounded { get { return Load(UIArtGenerator.RoundedPath); } }
        public static Sprite Gloss { get { return Load(UIArtGenerator.GlossPath); } }
        public static Sprite Outline { get { return Load(UIArtGenerator.OutlinePath); } }
        public static Sprite BallsBg { get { return Load(UIArtGenerator.BallsBgPath); } }
        public static Sprite ArrowLeft { get { return Load(UIArtGenerator.ArrowLeftPath); } }

        private static Sprite Load(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>Báo thiếu art một lần, có hướng dẫn chạy menu nào.</summary>
        public static bool EnsureArt()
        {
            if (Rounded != null && Gloss != null && Outline != null && BallsBg != null && ArrowLeft != null)
            {
                return true;
            }

            Debug.LogError("[UITheme] Thiếu art. Chạy Tools/Ball/Generate UI Art trước.");

            return false;
        }

        public static TMP_FontAsset Font
        {
            get
            {
                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

                return font != null ? font : TMP_Settings.defaultFontAsset;
            }
        }

        // ------------------------------------------------------------- mặt phẳng

        /// <summary>
        /// Nền bo góc 9-slice, gradient theo vai trò, kèm highlight + viền sáng.
        /// Chạy lại được: xoá Gloss/Outline cũ trước khi dựng.
        /// </summary>
        public static Image ThemedSurface(RectTransform rect, ThemedGraphic.ThemeRole role,
            float radiusMultiplier, bool raycastTarget, bool addGloss, bool addOutline)
        {
            Image image = rect.gameObject.GetComponent<Image>();
            if (image == null) image = rect.gameObject.AddComponent<Image>();

            image.sprite = Rounded;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = radiusMultiplier;
            image.raycastTarget = raycastTarget;

            ThemedGraphic themed = Attach(rect.gameObject, role, true, 1f);
            themed.ApplyImmediate();

            UIBuildUtil.DestroyIfExists(rect, GlossName);
            UIBuildUtil.DestroyIfExists(rect, OutlineName);

            // Viền dựng trước, highlight sau => highlight nằm trên cùng.
            if (addOutline) Overlay(rect, OutlineName, Outline, radiusMultiplier,
                ThemedGraphic.ThemeRole.Outline, 1f);

            if (addGloss) Overlay(rect, GlossName, Gloss, radiusMultiplier,
                ThemedGraphic.ThemeRole.Gloss, 0.26f);

            return image;
        }

        /// <summary>Lớp phủ full-rect, không chặn chuột, cùng bán kính bo với nền.</summary>
        private static void Overlay(RectTransform parent, string name, Sprite sprite,
            float radiusMultiplier, ThemedGraphic.ThemeRole role, float alphaScale)
        {
            if (sprite == null) return;

            RectTransform rect = UIBuildUtil.NewUI(name, parent);
            UIBuildUtil.Stretch(rect);

            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = radiusMultiplier;
            image.raycastTarget = false;

            Attach(rect.gameObject, role, true, alphaScale).ApplyImmediate();

            // Luôn nằm trên nền, nhưng dưới nội dung được thêm sau.
            rect.SetSiblingIndex(0);
        }

        /// <summary>Gắn (hoặc cập nhật) ThemedGraphic.</summary>
        public static ThemedGraphic Attach(GameObject go, ThemedGraphic.ThemeRole role,
            bool useGradient, float alphaScale)
        {
            ThemedGraphic themed = go.GetComponent<ThemedGraphic>();
            if (themed == null) themed = go.AddComponent<ThemedGraphic>();

            themed.UseGradient = useGradient;
            themed.AlphaScale = alphaScale;
            themed.EnsureGradient();
            themed.Role = role;

            return themed;
        }

        /// <summary>Màu phẳng (không gradient) — dùng cho chữ và lớp phủ scrim.</summary>
        public static ThemedGraphic Flat(GameObject go, ThemedGraphic.ThemeRole role)
        {
            return Attach(go, role, false, 1f);
        }

        // ------------------------------------------------------------- Text -> TMP

        /// <summary>
        /// Thay <see cref="Text"/> bằng <see cref="TextMeshProUGUI"/>, giữ nội dung / cỡ /
        /// canh lề. Trả về TMP mới, hoặc TMP đã có sẵn nếu đã chuyển rồi (idempotent).
        /// Màu do ThemedGraphic quyết định nên không chép sang.
        /// </summary>
        public static TextMeshProUGUI ConvertToTmp(GameObject go)
        {
            TextMeshProUGUI existing = go.GetComponent<TextMeshProUGUI>();
            if (existing != null)
            {
                existing.font = Font;
                return existing;
            }

            Text legacy = go.GetComponent<Text>();
            if (legacy == null) return null;

            string content = legacy.text;
            float size = legacy.fontSize;
            FontStyles style = ToTmpStyle(legacy.fontStyle);
            TextAlignmentOptions align = ToTmpAlignment(legacy.alignment);
            bool raycast = legacy.raycastTarget;
            bool wrap = legacy.horizontalOverflow == HorizontalWrapMode.Wrap;

            // Một GameObject chỉ giữ được một Graphic nên phải bỏ Text trước khi thêm TMP.
            Object.DestroyImmediate(legacy);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = Font;
            tmp.text = content;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = align;
            tmp.raycastTarget = raycast;
            tmp.textWrappingMode = wrap ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
            tmp.overflowMode = TextOverflowModes.Overflow;

            return tmp;
        }

        private static FontStyles ToTmpStyle(FontStyle style)
        {
            switch (style)
            {
                case FontStyle.Bold: return FontStyles.Bold;
                case FontStyle.Italic: return FontStyles.Italic;
                case FontStyle.BoldAndItalic: return FontStyles.Bold | FontStyles.Italic;
                default: return FontStyles.Normal;
            }
        }

        private static TextAlignmentOptions ToTmpAlignment(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }

        /// <summary>ColorBlock chuẩn cho mọi nút: giữ trắng để gradient quyết định màu.</summary>
        public static void ApplyButtonColors(Button button)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.transition = Selectable.Transition.ColorTint;
        }
    }
}
