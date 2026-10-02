using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

using U = BallEditor.UIBuildUtil;
using T = BallEditor.UIThemeUtil;

namespace BallEditor
{
    /// <summary>
    /// Dựng lại theme cho UI gốc của Ball (MainMenu / Gameplay / PauseMenu / GameOver):
    /// chuyển Text sang TMP, gắn vai trò màu, thêm gradient + highlight + viền sáng.
    ///
    /// Viết mới chứ không port UIStyleBuilder của Tetra Rush — bản đó khoá cứng vào tên
    /// panel/nút của Tetris, chỉ xử lý TMP, và còn ghi đè PlayerSettings.productName.
    /// Chạy lại được nhiều lần (idempotent).
    /// </summary>
    public static class UIThemeBuilder
    {
        private const string CanvasName = "Canvas";

        private const float RadiusButton = 0.95f;
        private const float RadiusIcon = 1.30f;

        /// <summary>Khoảng cách tâm giữa các nút trong hàng Pause.</summary>
        private const float PauseButtonSpacing = 220f;

        private const string IconName = "Icon";

        /// <summary>Icon thụt vào trong tấm nền, để lộ viền bo góc quanh nó.</summary>
        private const float IconPadding = 26f;

        private const string CoinsPillName = "Coins Pill";

        /// <summary>Pill nền cho số coin trên MainMenu (giữ vàng coin đọc được).</summary>
        private static readonly Vector2 CoinsPillSize = new Vector2(420f, 104f);

        [MenuItem("Tools/Ball/Restyle UI")]
        public static void RestyleMenu()
        {
            if (!Restyle()) return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Restyle UI", "Đã dựng lại theme cho UI.", "OK");
        }

        public static bool Restyle()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                Debug.LogError("[UITheme] Mở scene Assets/BallsAvoid/Scenes/Main.unity trước đã.");
                return false;
            }

            GameObject canvasRoot = U.FindRootObject(scene, CanvasName);
            if (canvasRoot == null)
            {
                Debug.LogError("[UITheme] Không tìm thấy GameObject 'Canvas'.");
                return false;
            }

            if (!T.EnsureArt()) return false;
            if (T.Font == null)
            {
                Debug.LogError("[UITheme] Chưa có TMP font asset.");
                return false;
            }

            Transform canvas = canvasRoot.transform;

            Transform mainMenu = canvas.Find("MainMenu");
            Transform gameplay = canvas.Find("Gameplay");
            Transform pauseMenu = canvas.Find("PauseMenu");
            Transform gameOver = canvas.Find("GameOver");

            if (mainMenu == null || gameplay == null || pauseMenu == null || gameOver == null)
            {
                Debug.LogError("[UITheme] Thiếu panel dưới Canvas (cần MainMenu/Gameplay/PauseMenu/GameOver).");
                return false;
            }

            EnsureDriver(canvasRoot);

            StyleMainMenu(mainMenu);
            StyleGameplay(gameplay);
            StylePauseMenu(pauseMenu);
            StyleGameOver(gameOver);

            RewireScoreManager(scene, canvas);

            Debug.Log("[UITheme] Đã dựng lại theme: MainMenu, Gameplay, PauseMenu, GameOver.", canvasRoot);

            return true;
        }

        // ---------------------------------------------------------------- panel

        private static void StyleMainMenu(Transform panel)
        {
            // MainMenu không có nền panel: chữ nằm thẳng trên cảnh game, nên phải tương
            // phản với backgroundMain chứ không phải với một mặt phẳng UI.
            Text(panel, "Title", ThemedGraphic.ThemeRole.TextOnGameBg, true);
            Text(panel, "TapToStart", ThemedGraphic.ThemeRole.TextOnGameBg, false);
            Text(panel, "Top/TopScore", ThemedGraphic.ThemeRole.TextOnGameBg, false);

            Icon(panel, "Top/TopScore/Icon", ThemedGraphic.ThemeRole.CoinOnGameBg);

            IconButton(panel, "Top/ButtonEfx", false);
            IconButton(panel, "Top/ButtonMusic", false);
            IconButton(panel, "ShopButton", true);

            CoinsLabel(panel, true);
        }

        private static void StyleGameplay(Transform panel)
        {
            Text(panel, "Score", ThemedGraphic.ThemeRole.TextOnGameBg, true);
            IconButton(panel, "PauseButton", true);

            // HUD coin do ShopPanelBuilder dựng; chỉ gắn vai trò màu nếu đã có.
            Text(panel, "PointsHUD/Coins Text", ThemedGraphic.ThemeRole.Coin, true);
            Icon(panel, "PointsHUD/Icon", ThemedGraphic.ThemeRole.Coin);

            // Thanh tiến trình đổi màu: rãnh theo Muted, ruột theo Accent.
            // BarLineFill là con của BarLine (không phải của Bar) — sai path thì
            // Transform.Find trả null và bị bỏ qua im lặng.
            // Rãnh dùng SurfaceAlt (tối) chứ không Muted: Muted là xám trung tính cùng
            // độ sáng với Accent nên ở bộ màu olive hai phần nhìn bệt vào nhau.
            Graphic(panel, "Bar/BarLine", ThemedGraphic.ThemeRole.SurfaceAlt, false);
            Graphic(panel, "Bar/BarLine/BarLineFill", ThemedGraphic.ThemeRole.Accent, false);
        }

        private static void StylePauseMenu(Transform panel)
        {
            Scrim(panel);

            Text(panel, "Title", ThemedGraphic.ThemeRole.TextOnSurface, true);

            CenterPauseButtons(panel);

            IconButton(panel, "Buttons/ButtonHome", true);
            IconButton(panel, "Buttons/ButtonRestart", true);
            IconButton(panel, "Buttons/ButtonPlay", true);
        }

        private static void StyleGameOver(Transform panel)
        {
            Scrim(panel);

            Text(panel, "Final Score", ThemedGraphic.ThemeRole.Coin, true);
            Text(panel, "Best Score", ThemedGraphic.ThemeRole.TextOnSurface, false);

            // 'Restart Button' là một Image alpha=0 phủ gần hết panel chỉ để bắt click.
            // Dựng nền thật cho nó sẽ che mất Final Score, nên giữ vô hình và chỉ
            // tạo nút nhìn thấy được quanh chữ PLAY AGAIN.
            StylePlayAgain(panel);

            IconButton(panel, "ShopButton", true);
            CoinsLabel(panel, false);
        }

        /// <summary>Biến dòng chữ PLAY AGAIN thành nút có nền accent + gloss + viền.</summary>
        private static void StylePlayAgain(Transform panel)
        {
            Transform restart = panel.Find("Restart Button");
            if (restart == null) return;

            Image hit = restart.GetComponent<Image>();
            if (hit != null)
            {
                // giữ vùng bắt click nhưng không vẽ gì
                hit.color = new Color(1f, 1f, 1f, 0f);
                hit.raycastTarget = true;

                ThemedGraphic stray = restart.GetComponent<ThemedGraphic>();
                if (stray != null) Object.DestroyImmediate(stray);
            }

            // Nút này mới là thứ người chơi bấm, nhưng Graphic của nó trong suốt nên
            // ColorTint không hiện gì — phải trỏ targetGraphic vào tấm nền thấy được,
            // nếu không bấm PLAY AGAIN sẽ không có phản hồi.
            Button restartButton = restart.GetComponent<Button>();

            Transform label = restart.Find("PlayAgain");
            if (label == null) return;

            RectTransform labelRect = (RectTransform)label;

            // Nền nằm sau chữ, cùng rect, không chặn click (click do 'Restart Button' lo).
            U.DestroyIfExists(restart, "PlayAgain BG");

            RectTransform bg = U.NewUI("PlayAgain BG", restart);
            bg.anchorMin = labelRect.anchorMin;
            bg.anchorMax = labelRect.anchorMax;
            bg.pivot = labelRect.pivot;
            bg.anchoredPosition = labelRect.anchoredPosition;
            bg.sizeDelta = labelRect.sizeDelta + new Vector2(120f, 60f);
            bg.SetSiblingIndex(labelRect.GetSiblingIndex());

            Image bgImage = T.ThemedSurface(bg, ThemedGraphic.ThemeRole.Accent, RadiusButton, false, true, true);

            if (restartButton != null)
            {
                T.ApplyButtonColors(restartButton);
                restartButton.targetGraphic = bgImage;
            }

            TextMeshProUGUI tmp = T.ConvertToTmp(label.gameObject);
            if (tmp != null)
            {
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;

                // "PLAY AGAIN" ở cỡ 80 bị ngắt thành 2 dòng và tràn khỏi nền.
                // Ép một dòng, cho tự co cỡ nếu màn hẹp.
                tmp.textWrappingMode = TextWrappingModes.NoWrap;
                tmp.enableAutoSizing = true;
                tmp.fontSizeMin = 36f;
                tmp.fontSizeMax = 80f;

                T.Flat(label.gameObject, ThemedGraphic.ThemeRole.TextOnAccent);
            }
        }

        // ---------------------------------------------------------------- helper

        private static void EnsureDriver(GameObject canvasRoot)
        {
            if (canvasRoot.GetComponent<BallThemeDriver>() == null)
            {
                canvasRoot.AddComponent<BallThemeDriver>();
            }
        }

        /// <summary>Lớp phủ nền của PauseMenu / GameOver: thay đen cứng bằng scrim theo theme.</summary>
        private static void Scrim(Transform panel)
        {
            Image image = panel.GetComponent<Image>();
            if (image == null) return;

            image.sprite = null;
            image.type = Image.Type.Simple;
            image.raycastTarget = true;

            T.Flat(panel.gameObject, ThemedGraphic.ThemeRole.Scrim).ApplyImmediate();
        }

        /// <summary>
        /// Tìm theo path và BÁO khi không thấy. Trước đây các helper lặng lẽ return khi
        /// Find trả null, nên một path sai ("Bar/BarLineFill" trong khi thật ra là
        /// "Bar/BarLine/BarLineFill") làm phần tử đó không được theme mà không ai biết.
        /// </summary>
        private static Transform Resolve(Transform panel, string path)
        {
            Transform t = panel.Find(path);

            if (t == null)
            {
                Debug.LogWarning("[UITheme] Không tìm thấy '" + path + "' dưới '"
                    + panel.name + "' — bỏ qua phần tử này.", panel);
            }

            return t;
        }

        private static TextMeshProUGUI Text(Transform panel, string path,
            ThemedGraphic.ThemeRole role, bool bold)
        {
            Transform t = Resolve(panel, path);
            if (t == null) return null;

            TextMeshProUGUI tmp = T.ConvertToTmp(t.gameObject);
            if (tmp == null) return null;

            if (bold) tmp.fontStyle |= FontStyles.Bold;

            // Không dùng tmp.outlineWidth: nó ghi vào material, mà TMP vừa AddComponent
            // trong edit mode chưa có material -> NullReferenceException. Tính đọc được
            // đã do BallTheme.BestTextOn bảo đảm (contrast >= 4.5 trên cả 5 bộ màu).
            T.Flat(t.gameObject, role).ApplyImmediate();

            return tmp;
        }

        private static void Icon(Transform panel, string path, ThemedGraphic.ThemeRole role)
        {
            Transform t = Resolve(panel, path);
            if (t == null) return;
            if (t.GetComponent<Graphic>() == null) return;

            T.Flat(t.gameObject, role).ApplyImmediate();
        }

        private static void Graphic(Transform panel, string path, ThemedGraphic.ThemeRole role, bool gradient)
        {
            Transform t = Resolve(panel, path);
            if (t == null) return;
            if (t.GetComponent<Graphic>() == null) return;

            T.Attach(t.gameObject, role, gradient, 1f).ApplyImmediate();
        }

        /// <summary>
        /// Nút icon: giữ nguyên sprite icon, và (nếu <paramref name="withPlate"/>) thêm
        /// một nền bo góc phía SAU icon để có gradient + gloss + viền.
        ///
        /// ButtonEfx/ButtonMusic KHÔNG dùng nền: rect của chúng vốn đã thò khỏi mép trái
        /// canvas (neo trái, pos.x=80 nhưng rộng 216 nên nửa bề rộng 108 > 80) và chồng
        /// lên nhau 76px — sprite có viền trong suốt nên trước giờ không lộ. Đặt tấm nền
        /// đục phía sau sẽ phơi cả hai lỗi đó ra.
        /// </summary>
        private static void IconButton(Transform panel, string path, bool withPlate)
        {
            Transform t = Resolve(panel, path);
            if (t == null) return;

            Button button = t.GetComponent<Button>();
            Image image = t.GetComponent<Image>();
            if (image == null) return;

            RectTransform rect = (RectTransform)t;

            // Bản dựng cũ đặt tấm nền làm con tên "BG" — sai, xem chú thích bên dưới.
            U.DestroyIfExists(rect, "BG");

            if (!withPlate)
            {
                U.DestroyIfExists(rect, IconName);

                image.raycastTarget = true;

                // Không có nền thì icon nằm thẳng trên cảnh game -> phải tương phản với nó.
                T.Flat(t.gameObject, ThemedGraphic.ThemeRole.TextOnGameBg).ApplyImmediate();

                if (button != null)
                {
                    T.ApplyButtonColors(button);
                    button.targetGraphic = image;
                }

                return;
            }

            // uGUI vẽ Image của CHA trước rồi mới tới con, nên tấm nền đặt làm con sẽ
            // luôn đè mất icon. Vì vậy: Image của chính nút trở thành tấm nền, còn icon
            // chuyển xuống một con vẽ sau cùng.
            //
            // Lần chạy thứ hai, Image của nút đã là sprite nền rồi — phải lấy lại sprite
            // gốc từ con "Icon" đã dựng, nếu không icon sẽ biến thành hình chữ nhật bo góc.
            Transform existingIcon = rect.Find(IconName);
            Image existingIconImage = existingIcon != null ? existingIcon.GetComponent<Image>() : null;

            Sprite iconSprite = existingIconImage != null ? existingIconImage.sprite : image.sprite;

            Image plate = T.ThemedSurface(rect, ThemedGraphic.ThemeRole.Header,
                RadiusIcon, true, true, true);

            U.DestroyIfExists(rect, IconName);

            RectTransform icon = U.NewUI(IconName, rect);
            U.Stretch(icon);
            icon.offsetMin = new Vector2(IconPadding, IconPadding);
            icon.offsetMax = new Vector2(-IconPadding, -IconPadding);
            icon.SetAsLastSibling();

            Image iconImage = icon.gameObject.AddComponent<Image>();
            iconImage.sprite = iconSprite;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;

            T.Flat(icon.gameObject, ThemedGraphic.ThemeRole.TextOnSurface).ApplyImmediate();

            if (button != null)
            {
                T.ApplyButtonColors(button);
                button.targetGraphic = plate;
            }
        }

        /// <summary>
        /// Căn giữa lại hàng nút Pause. Bản gốc neo ba nút bằng toạ độ tuyệt đối tính từ
        /// mép TRÁI (520 / 730 / 930), chỉnh cho canvas rộng 1080. Vì CanvasScaler dùng
        /// matchWidthOrHeight = 1 nên canvas hẹp lại trên màn cao hơn 9:16 (960 ở 1:2,
        /// ~886 ở 20:9), cả hàng trôi sang phải và ButtonPlay chạy hẳn ra ngoài màn —
        /// không bấm được. Neo theo tâm thì đúng ở mọi tỉ lệ.
        /// </summary>
        private static void CenterPauseButtons(Transform panel)
        {
            Transform row = panel.Find("Buttons");
            if (row == null) return;

            string[] names = { "ButtonHome", "ButtonRestart", "ButtonPlay" };
            float[] offsets = { -PauseButtonSpacing, 0f, PauseButtonSpacing };

            for (int i = 0; i < names.Length; i++)
            {
                Transform t = row.Find(names[i]);
                if (t == null) continue;

                RectTransform rect = (RectTransform)t;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(offsets[i], 0f);
            }
        }

        /// <summary>
        /// Dòng "Coins: {0}" do ShopPanelBuilder dựng.
        ///
        /// MainMenu không có panel nên chữ nằm thẳng trên cảnh game. Nếu chỉ ép màu chữ
        /// cho đủ tương phản thì vàng coin bị dìm thành nâu olive (#3B3B0E) và mất hẳn
        /// chất "vàng". Nên thay vào đó dựng một pill nền tối phía sau — giống Coins Pill
        /// trong shop — để giữ vàng tươi mà vẫn đọc rõ trên mọi bộ màu.
        /// </summary>
        private static void CoinsLabel(Transform panel, bool overGameBackground)
        {
            Transform t = panel.Find("Coins Text");
            if (t == null) return;

            RectTransform label = t as RectTransform;
            if (label == null || t.GetComponent<TMP_Text>() == null) return;

            U.DestroyIfExists(panel, CoinsPillName);

            if (overGameBackground)
            {
                // Neo theo tâm với kích thước cố định: rect của chữ neo 0.1–0.9 nên bề
                // rộng đổi theo canvas, không đặt pill cố định bằng offset được.
                float anchorY = (label.anchorMin.y + label.anchorMax.y) * 0.5f;

                RectTransform pill = U.NewUI(CoinsPillName, panel);
                pill.anchorMin = new Vector2(0.5f, anchorY);
                pill.anchorMax = new Vector2(0.5f, anchorY);
                pill.pivot = new Vector2(0.5f, 0.5f);
                pill.anchoredPosition = Vector2.zero;
                pill.sizeDelta = CoinsPillSize;
                pill.SetSiblingIndex(label.GetSiblingIndex());

                T.ThemedSurface(pill, ThemedGraphic.ThemeRole.SurfaceAlt, RadiusButton, false, true, true);
            }

            T.Flat(t.gameObject, ThemedGraphic.ThemeRole.Coin).ApplyImmediate();
        }

        /// <summary>
        /// Nối lại 4 ref của ScoreManager — đổi field từ Text sang TMP_Text làm mất
        /// ref serialize, và sẽ hỏng âm thầm (score không hiện, không báo lỗi).
        /// </summary>
        private static void RewireScoreManager(UnityEngine.SceneManagement.Scene scene, Transform canvas)
        {
            ScoreManager manager = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                manager = root.GetComponentInChildren<ScoreManager>(true);
                if (manager != null) break;
            }

            if (manager == null)
            {
                Debug.LogWarning("[UITheme] Không tìm thấy ScoreManager để nối lại ref.");
                return;
            }

            Bind(manager, "currentScoreLabel", canvas, "Gameplay/Score");
            Bind(manager, "highScoreLabel", canvas, "MainMenu/Top/TopScore");
            Bind(manager, "currentScoreGameOverLabel", canvas, "GameOver/Final Score");
            Bind(manager, "highScoreGameOverLabel", canvas, "GameOver/Best Score");
        }

        private static void Bind(ScoreManager manager, string field, Transform canvas, string path)
        {
            Transform t = canvas.Find(path);
            TMP_Text label = t != null ? t.GetComponent<TMP_Text>() : null;

            if (label == null)
            {
                Debug.LogWarning("[UITheme] Không nối được ScoreManager." + field + " -> '" + path + "'.");
                return;
            }

            U.SetObjectField(manager, field, label);
        }
    }
}
