using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;
using TapTap;

using U = TapTapEditor.UIBuildUtil;

namespace TapTapEditor
{
    /// <summary>
    /// Dựng Main Menu + bảng Pause vào Canvas của scene hiện tại. Chạy lại được nhiều lần.
    /// </summary>
    public static class MenuBuilder
    {
        private const string MenuRootName = "Main Menu";
        private const string PauseRootName = "Pause";
        private const string PauseButtonName = "Pause Button";

        private const string ShopRootName = "Shop";
        private const string ShopButtonName = "Shop Button";

        [MenuItem("Tools/TapTap/Build Menu + Pause")]
        public static void BuildMenus()
        {
            if (Build()) SaveAndReport();
        }

        [MenuItem("Tools/TapTap/Build All UI", priority = -100)]
        public static void BuildAll()
        {
            // Shop trước, menu sau: menu cần trỏ tới nút Shop do ShopScreenBuilder tạo ra.
            if (!ShopScreenBuilder.Build()) return;
            if (!Build()) return;
            if (!GameHudBuilder.Build()) return;
            if (!SoundManagerSetup.Build()) return;

            SaveAndReport();
        }

        private static void SaveAndReport()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Build UI", "Đã dựng xong UI trong scene " + scene.name + ".", "OK");
        }

        internal static bool Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Build Menu", "Mở scene Game.unity trước đã.", "OK");
                return false;
            }

            GameObject canvasGo = U.FindRootObject(scene, U.CanvasName);
            if (canvasGo == null)
            {
                Debug.LogError("[MenuBuilder] Không tìm thấy GameObject '" + U.CanvasName + "' trong scene.");
                return false;
            }

            if (!U.EnsureSprites("Build Menu",
                    "bg_iap", "bg_button_iap", "sunburst", "rounded",
                    "play", "restart", "ic_home", "sound_on", "sound_off", "music_on", "music_off"))
            {
                return false;
            }

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                Debug.LogError("[MenuBuilder] Chưa có TMP default font asset. Window > TextMeshPro > Import TMP Essential Resources.");
                return false;
            }

            Transform canvas = canvasGo.transform;

            U.DestroyIfExists(canvas, MenuRootName);
            U.DestroyIfExists(canvas, PauseRootName);
            U.DestroyIfExists(canvas, PauseButtonName);

            // ---------- Nút Pause (góc trên phải, chỉ hiện khi đang chơi) ----------

            RectTransform pauseBtn = U.NewUI(PauseButtonName, canvas);
            U.Place(pauseBtn, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(110f, 110f));
            Image pauseBtnImage = U.AddImage(pauseBtn, "rounded", true, true);
            Button pauseBtnButton = U.AddButton(pauseBtn, pauseBtnImage);

            RectTransform pauseGlyph = U.NewUI("Glyph", pauseBtn);
            U.Stretch(pauseGlyph);
            U.AddText(pauseGlyph, font, "II", 56f, TextAlignmentOptions.Center, Color.white);

            // ---------- Pause ----------

            RectTransform pauseRoot = U.NewUI(PauseRootName, canvas);
            U.Stretch(pauseRoot);
            PauseScreen pauseScreen = pauseRoot.gameObject.AddComponent<PauseScreen>();
            U.SetEntityName(pauseScreen, "Pause Screen");

            RectTransform pausePanel = U.NewUI("Pause Panel", pauseRoot);
            U.Stretch(pausePanel);
            // Nền mờ chặn click: màu đặc, không cần sprite nào.
            U.AddSolidImage(pausePanel, new Color(0f, 0f, 0f, 0.75f), true);

            RectTransform card = U.NewUI("Card", pausePanel);
            U.Place(card, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800f, 1000f));
            U.AddImage(card, "bg_button_iap", false, false);

            U.AddText(U.TopCenter("Title", card, 60f, 700f, 110f), font, "PAUSED", 88f,
                TextAlignmentOptions.Center, Color.white);

            RectTransform resume = U.TopCenter("Resume", card, 280f, 200f, 200f);
            Button resumeButton = U.IconButton(resume, "play");

            RectTransform restart = U.NewUI("Restart", card);
            U.Place(restart, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-130f, -640f), new Vector2(150f, 150f));
            Button restartButton = U.IconButton(restart, "restart");

            RectTransform home = U.NewUI("Home", card);
            U.Place(home, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(130f, -640f), new Vector2(150f, 150f));
            Button homeButton = U.IconButton(home, "ic_home");

            BuildAudioToggle(card, "Sound Toggle", new Vector2(-130f, -840f),
                AudioToggleButton.Channel.Sound, "sound_on", "sound_off");
            BuildAudioToggle(card, "Music Toggle", new Vector2(130f, -840f),
                AudioToggleButton.Channel.Music, "music_on", "music_off");

            U.SetObjectField(pauseScreen, "m_PausePanel", pausePanel.gameObject);
            U.SetObjectField(pauseScreen, "m_PauseButton", pauseBtn.gameObject);

            UnityEventTools.AddVoidPersistentListener(pauseBtnButton.onClick, pauseScreen.Pause);
            UnityEventTools.AddVoidPersistentListener(resumeButton.onClick, pauseScreen.Resume);
            UnityEventTools.AddVoidPersistentListener(restartButton.onClick, pauseScreen.Restart);
            UnityEventTools.AddVoidPersistentListener(homeButton.onClick, pauseScreen.GoHome);

            pausePanel.gameObject.SetActive(false);

            // ---------- Main Menu ----------

            RectTransform menuRoot = U.NewUI(MenuRootName, canvas);
            U.Stretch(menuRoot);
            MainMenuScreen menuScreen = menuRoot.gameObject.AddComponent<MainMenuScreen>();
            U.SetEntityName(menuScreen, "Main Menu Screen");

            RectTransform menuPanel = U.NewUI("Menu Panel", menuRoot);
            U.Stretch(menuPanel);
            U.AddImage(menuPanel, "bg_iap", true, false);

            RectTransform sunburst = U.TopCenter("Sunburst", menuPanel, 180f, 900f, 900f);
            Image sunburstImage = U.AddImage(sunburst, "sunburst", false, true);
            sunburstImage.color = new Color(1f, 1f, 1f, 0.22f);

            U.AddText(U.TopCenter("Title", menuPanel, 480f, 900f, 200f), font, "TAP TAP", 150f,
                TextAlignmentOptions.Center, Color.white);

            TextMeshProUGUI bestText = U.AddText(U.TopCenter("Best Score", menuPanel, 760f, 700f, 90f), font,
                "BEST {0}", 62f, TextAlignmentOptions.Center, U.GoldText);

            RectTransform play = U.TopCenter("Play", menuPanel, 1080f, 260f, 260f);
            Button playButton = U.IconButton(play, "play");

            U.AddText(U.TopCenter("Play Label", menuPanel, 1370f, 600f, 90f), font, "PLAY", 64f,
                TextAlignmentOptions.Center, Color.white);

            BuildAudioToggle(menuPanel, "Sound Toggle", new Vector2(-110f, -1650f),
                AudioToggleButton.Channel.Sound, "sound_on", "sound_off");
            BuildAudioToggle(menuPanel, "Music Toggle", new Vector2(110f, -1650f),
                AudioToggleButton.Channel.Music, "music_on", "music_off");

            U.SetObjectField(menuScreen, "m_MenuPanel", menuPanel.gameObject);
            U.SetObjectField(menuScreen, "m_BestScoreText", bestText);
            U.SetObjectArrayField(menuScreen, "m_GameplayOnly", pauseBtn.gameObject);

            // Nút Shop (do ShopScreenBuilder tạo) chỉ hiện ở menu.
            Transform shopButton = canvas.Find(ShopButtonName);
            if (shopButton != null)
            {
                U.SetObjectArrayField(menuScreen, "m_MenuOnly", shopButton.gameObject);
            }
            else
            {
                Debug.LogWarning("[MenuBuilder] Chưa có '" + ShopButtonName
                    + "'. Chạy Tools/TapTap/Build Shop Screen trước, hoặc dùng Build All UI.");
            }

            UnityEventTools.AddVoidPersistentListener(playButton.onClick, menuScreen.Play);

            // ---------- Thứ tự vẽ: Menu < Pause < Shop ----------

            menuRoot.SetAsLastSibling();
            pauseRoot.SetAsLastSibling();

            Transform shopRoot = canvas.Find(ShopRootName);
            if (shopRoot != null) shopRoot.SetAsLastSibling();

            Debug.Log("[MenuBuilder] Đã dựng Main Menu + Pause.", menuRoot.gameObject);

            return true;
        }

        /// <param name="position">Vị trí so với mép trên của <paramref name="parent"/> (y âm là đi xuống).</param>
        private static void BuildAudioToggle(RectTransform parent, string name, Vector2 position,
            AudioToggleButton.Channel channel, string onSprite, string offSprite)
        {
            RectTransform rect = U.NewUI(name, parent);
            U.Place(rect, new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), position, new Vector2(130f, 130f));

            Image icon = U.AddImage(rect, onSprite, true, true);
            U.AddButton(rect, icon);

            AudioToggleButton toggle = rect.gameObject.AddComponent<AudioToggleButton>();
            U.SetEnumField(toggle, "m_Channel", (int)channel);
            U.SetObjectField(toggle, "m_Icon", icon);
            U.SetObjectField(toggle, "m_OnSprite", U.LoadSprite(onSprite));
            U.SetObjectField(toggle, "m_OffSprite", U.LoadSprite(offSprite));
        }
    }
}
