using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace TetrisEditor
{
    /// <summary>
    /// Áp bộ style mới cho giao diện: nút bo góc + gradient, nền home, font Rubik, tên game.
    /// Chạy lại được nhiều lần (idempotent).
    /// </summary>
    public static class UIStyleBuilder
    {
        /// <summary>Tên game mới.</summary>
        public const string GameName = "Tetra Rush";
        public const string GameTitleDisplay = "TETRA RUSH";

        private const string FontPath = "Assets/Fonts/Rubik-Regular SDF.asset";

        private struct ButtonStyle
        {
            public Color Top;
            public Color Bottom;

            public ButtonStyle(uint top, uint bottom)
            {
                Top = Hex(top);
                Bottom = Hex(bottom);
            }
        }

        private static Color Hex(uint rgb)
        {
            return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, 1f);
        }

        /// <summary>Màu gradient theo vai trò của từng nút (tra theo tên GameObject).</summary>
        private static bool TryGetStyle(string buttonName, out ButtonStyle style)
        {
            switch (buttonName)
            {
                case "StartGameBTN":     style = new ButtonStyle(0x4BE38C, 0x1C9E62); return true;  // xanh lá
                case "NextLevelBTN":     style = new ButtonStyle(0x5FB3FF, 0x2C6AD4); return true;  // xanh dương
                case "RestartLevelBTN":  style = new ButtonStyle(0xFFC15E, 0xEE8324); return true;  // hổ phách
                case "HighestLevelsBTN": style = new ButtonStyle(0xB49CFF, 0x7A57E0); return true;  // tím
                case "ExitGameBTN":      style = new ButtonStyle(0xFF7D7D, 0xD03A3A); return true;  // đỏ
            }

            style = default(ButtonStyle);
            return false;
        }

        private static readonly string[] Panels = { "startGameUI", "EndGameUI", "EndGameFailUI" };

        [MenuItem("Tools/Tetris/Restyle UI")]
        public static void RestyleMenu()
        {
            if (!Restyle()) return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Restyle UI", "Đã áp style mới và đổi tên game thành \"" + GameName + "\".", "OK");
        }

        public static bool Restyle()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError("[UIStyle] Mở scene Assets/Scenes/game.unity trước đã.");
                return false;
            }

            GameObject canvasRoot = UIBuildUtil.FindRootObject(scene, "Canvas");
            if (canvasRoot == null)
            {
                Debug.LogError("[UIStyle] Không tìm thấy GameObject 'Canvas'.");
                return false;
            }

            Sprite rounded = AssetDatabase.LoadAssetAtPath<Sprite>(UIArtGenerator.RoundedPath);
            Sprite homeBg = AssetDatabase.LoadAssetAtPath<Sprite>(UIArtGenerator.HomeBgPath);
            if (rounded == null || homeBg == null)
            {
                Debug.LogError("[UIStyle] Chưa có art. Chạy Tools/Tetris/Generate UI Art trước.");
                return false;
            }

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null) font = TMP_Settings.defaultFontAsset;

            Transform canvas = canvasRoot.transform;
            int styledButtons = 0;

            for (int i = 0; i < Panels.Length; i++)
            {
                Transform panel = canvas.Find(Panels[i]);
                if (panel == null)
                {
                    Debug.LogWarning("[UIStyle] Không tìm thấy panel '" + Panels[i] + "'.");
                    continue;
                }

                StyleBackground(panel, homeBg, Panels[i] == "startGameUI");
                styledButtons += StyleButtons(panel, rounded, font);
                StyleLabels(panel, font);
            }

            StyleTitle(canvas, font);
            StyleCounters(canvas, font);

            // Trong Editor cả 3 panel đều bật nên chữ chồng lên nhau; lúc chạy
            // UIController_tetris.Start() vẫn tự tắt 2 panel kết thúc như cũ.
            SetActive(canvas, "EndGameUI", false);
            SetActive(canvas, "EndGameFailUI", false);
            SetActive(canvas, "startGameUI", true);

            PlayerSettings.productName = GameName;

            Debug.Log("[UIStyle] Đã style " + styledButtons + " nút, đổi productName = \"" + GameName + "\".");
            return true;
        }

        // ---------------------------------------------------------------- nền

        private static void StyleBackground(Transform panel, Sprite homeBg, bool isHome)
        {
            Transform bg = panel.Find("BG");
            if (bg == null) return;

            Image image = bg.GetComponent<Image>();
            if (image == null) return;

            image.sprite = homeBg;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            // 2 panel kết thúc dùng lại cùng nền nhưng tối hơn để thấy rõ là lớp phủ
            image.color = isHome ? Color.white : new Color(0.62f, 0.62f, 0.74f, 0.97f);
        }

        // ---------------------------------------------------------------- nút

        private static int StyleButtons(Transform panel, Sprite rounded, TMP_FontAsset font)
        {
            int count = 0;
            Button[] buttons = panel.GetComponentsInChildren<Button>(true);

            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];

                ButtonStyle style;
                if (!TryGetStyle(button.name, out style)) continue;

                Image image = button.GetComponent<Image>();
                if (image == null) continue;

                image.sprite = rounded;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = 0.55f; // bo góc to hơn so với ảnh gốc 128px
                image.color = Color.white;             // màu do UIGradient lo

                UIGradient gradient = button.GetComponent<UIGradient>();
                if (gradient == null) gradient = button.gameObject.AddComponent<UIGradient>();
                gradient.SetColors(style.Top, style.Bottom);

                // ColorTint mặc định quá nhạt trên nền trắng -> chỉnh lại cho rõ khi bấm
                ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(1.0f, 1.0f, 1.0f, 1f);
                colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
                colors.selectedColor = Color.white;
                colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
                colors.fadeDuration = 0.08f;
                button.colors = colors;

                StyleButtonLabel(button.transform, font);
                count++;
            }

            return count;
        }

        private static void StyleButtonLabel(Transform button, TMP_FontAsset font)
        {
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label == null) return;

            if (font != null) label.font = font;
            label.color = Color.white;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.fontSizeMin = 18f;
            label.fontSizeMax = 64f;
        }

        private static void StyleLabels(Transform panel, TMP_FontAsset font)
        {
            if (font == null) return;

            TextMeshProUGUI[] labels = panel.GetComponentsInChildren<TextMeshProUGUI>(true);
            for (int i = 0; i < labels.Length; i++)
                labels[i].font = font;
        }

        // ---------------------------------------------------------------- chữ

        private static void StyleTitle(Transform canvas, TMP_FontAsset font)
        {
            Transform title = canvas.Find("startGameUI/Text (TMP)");
            if (title == null) return;

            TextMeshProUGUI text = title.GetComponent<TextMeshProUGUI>();
            if (text == null) return;

            if (font != null) text.font = font;
            text.text = GameTitleDisplay;
            text.fontStyle = FontStyles.Bold;
            text.characterSpacing = 6f;
            text.enableAutoSizing = true;
            text.fontSizeMin = 30f;
            text.fontSizeMax = 110f;

            text.enableVertexGradient = true;
            text.colorGradient = new VertexGradient(
                Hex(0xFFFFFF), Hex(0xFFFFFF),
                Hex(0x8FD8FF), Hex(0x8FD8FF));
        }

        /// <summary>
        /// HUD và pill trong shop dùng chính chuỗi trong scene làm format string.
        /// Để "0" thay vì "{0}" thì trong Editor nhìn đúng, còn lúc chạy vẫn tự
        /// quay về "{0}" (vì chuỗi không chứa "{0}").
        /// </summary>
        private static void StyleCounters(Transform canvas, TMP_FontAsset font)
        {
            SetCounter(canvas.Find("PointsHUD/Points Text"), font, 48f);
            SetCounter(canvas.Find("ShopPanel/Panel/Points Pill/Value"), font, 48f);
        }

        private static void SetCounter(Transform target, TMP_FontAsset font, float size)
        {
            if (target == null) return;

            TextMeshProUGUI text = target.GetComponent<TextMeshProUGUI>();
            if (text == null) return;

            if (font != null) text.font = font;
            text.text = "0";
            text.fontStyle = FontStyles.Bold;
            text.fontSize = size;
        }

        private static void SetActive(Transform canvas, string child, bool active)
        {
            Transform t = canvas.Find(child);
            if (t != null) t.gameObject.SetActive(active);
        }
    }
}
