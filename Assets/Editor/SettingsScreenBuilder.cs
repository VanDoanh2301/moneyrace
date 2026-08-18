using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;

using U = RingGameEditor.UIBuildUtil;

namespace RingGameEditor
{
    /// <summary>
    /// Dựng màn Settings (âm lượng + tắt/bật tiếng) vào Canvas của scene hiện tại.
    /// Cùng cấu trúc/convention với ShopScreenBuilder.cs. Chạy lại được nhiều lần (idempotent).
    /// </summary>
    public static class SettingsScreenBuilder
    {
        private const string SettingsRootName = "Settings";

        // Canvas tham chiếu 1080x1920 (portrait) — xem giải thích ở ShopScreenBuilder.
        private const float RefW = 1080f;
        private const float RefH = 1920f;

        private const float PanelSideMargin = 40f;
        private const float HeaderHeight = 180f;
        private const float HeaderTop = 30f;

        private const float ControlsTopGap = 30f;   // khoảng cách từ đáy Header tới Controls
        private const float ControlsBottom = 40f;   // lề dưới của Controls, tính từ đáy panel
        private const float RowContentHeight = 420f; // chiều cao CỐ ĐỊNH của 1 hàng điều khiển (label+control)
        private const float RowSpacing = 40f;

        // Sprite fallback nằm ngoài Assets/Sprites/ (không resolve được qua UIBuildUtil.LoadSprite).
        private const string HandleSpritePath = "Assets/Images/white_circle.png";
        private const string CheckmarkSpritePath = "Assets/Images/Checkmark.png";

        // Nền header/slider/toggle: vẽ bo góc bằng code (RoundedRectGenerator) thay vì kéo giãn ảnh
        // bg_button_iap.png/rounded.png (Image.Type.Simple làm góc bo bị bóp méo khi resize không đều).
        private static readonly Color PanelBlue = new Color32(21, 35, 135, 255);
        private static readonly Color TrackColor = new Color(1f, 1f, 1f, 0.25f);

        [MenuItem("Tools/RingGame/Build Settings Screen")]
        public static void BuildSettingsScreen()
        {
            if (!Build()) return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Build Settings Screen", "Đã dựng màn Settings.", "OK");
        }

        /// <summary>Dựng Settings, chưa lưu scene. Trả về false nếu thiếu điều kiện.</summary>
        internal static bool Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Build Settings Screen", "Mở scene Menu.unity trước đã.", "OK");
                return false;
            }

            GameObject canvasGo = U.FindRootObject(scene, U.CanvasName);
            if (canvasGo == null)
            {
                Debug.LogError("[SettingsBuilder] Không tìm thấy GameObject '" + U.CanvasName + "' trong scene.");
                return false;
            }

            if (!U.EnsureSprites("Build Settings Screen", "Back New", "iv_back"))
            {
                return false;
            }

            Sprite handleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(HandleSpritePath);
            Sprite checkmarkSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CheckmarkSpritePath);
            if (handleSprite == null || checkmarkSprite == null)
            {
                Debug.LogError("[SettingsBuilder] Thiếu sprite: " + HandleSpritePath + " hoặc " + CheckmarkSpritePath);
                EditorUtility.DisplayDialog("Build Settings Screen", "Thiếu sprite handle/checkmark. Xem Console.", "OK");
                return false;
            }

            Font font = U.LoadDefaultFont();
            if (font == null)
            {
                Debug.LogError("[SettingsBuilder] Không tìm được font để dựng UI.");
                return false;
            }

            // SettingsScreen phụ thuộc SoundManager để đọc/ghi âm lượng — đảm bảo có sẵn trong scene.
            AddSoundManagerToScene.EnsureSoundManagerInScene(scene);

            Vector2 refRes = U.GetReferenceResolution(canvasGo);
            float hScale = refRes.x / RefW;
            float vScale = refRes.y / RefH;

            Transform canvas = canvasGo.transform;

            U.DestroyIfExists(canvas, SettingsRootName);

            float contentWidth = refRes.x - PanelSideMargin * hScale * 2f;

            // ---------- Settings: node luôn active, giữ component SettingsScreen ----------

            RectTransform settingsRoot = U.NewUI(SettingsRootName, canvas);
            U.Stretch(settingsRoot);
            SettingsScreen settingsScreen = settingsRoot.gameObject.AddComponent<SettingsScreen>();

            // ---------- Panel (thứ được bật/tắt) ----------

            RectTransform panel = U.NewUI("Settings Panel", settingsRoot);
            U.Stretch(panel);
            U.AddImage(panel, "Back New", true, false); // raycast on => chặn click xuyên xuống gameplay

            // Header: thanh ngang bo góc + tiêu đề + nút back
            RectTransform header = U.TopCenter("Header", panel, HeaderTop * vScale, contentWidth, HeaderHeight * vScale);
            U.AddSlicedImage(header, RoundedRectGenerator.Ensure(), PanelBlue, false);

            RectTransform headerTitle = U.NewUI("Title", header);
            U.Stretch(headerTitle);
            U.AddText(headerTitle, font, "SETTINGS", Mathf.RoundToInt(68f * vScale), TextAnchor.MiddleCenter, Color.white);

            RectTransform back = U.NewUI("Back", header);
            U.Place(back, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(36f * hScale, 0f), new Vector2(88f * vScale, 88f * vScale));
            Button backButton = U.IconButton(back, "iv_back");

            // ---------- Controls: container co giãn lấp đầy phần còn lại dưới Header ----------
            // Cùng kỹ thuật với ShopScreenBuilder: container stretch theo % panel (luôn đúng ở mọi
            // tỉ lệ máy) thay vì tính vị trí cố định theo refRes.y (chiều cao KHAI BÁO, không phải
            // chiều cao thật hiển thị) — tránh 2 hàng bị dồn lên trên, để trống mảng lớn phía dưới.

            float controlsTopPx = (HeaderTop + HeaderHeight + ControlsTopGap) * vScale;
            float controlsBottomPx = ControlsBottom * vScale;

            RectTransform controls = U.NewUI("Controls", panel);
            controls.anchorMin = new Vector2(0.5f, 0f);
            controls.anchorMax = new Vector2(0.5f, 1f);
            controls.pivot = new Vector2(0.5f, 0.5f);
            controls.offsetMin = new Vector2(-contentWidth * 0.5f, controlsBottomPx);
            controls.offsetMax = new Vector2(contentWidth * 0.5f, -controlsTopPx);

            VerticalLayoutGroup controlsLayout = controls.gameObject.AddComponent<VerticalLayoutGroup>();
            controlsLayout.spacing = RowSpacing * vScale;
            controlsLayout.childAlignment = TextAnchor.UpperCenter;
            controlsLayout.childControlWidth = true;
            controlsLayout.childControlHeight = true;
            controlsLayout.childForceExpandWidth = true;
            controlsLayout.childForceExpandHeight = true;

            float rowContentHeightPx = RowContentHeight * vScale;
            Sprite roundedRect = RoundedRectGenerator.Ensure();

            // ---------- Hàng Volume: label + Slider ----------

            RectTransform volumeSlot = BuildFlexSlot("Volume Row", controls, rowContentHeightPx);
            RectTransform volumeContent = BuildCenteredContent(volumeSlot, contentWidth, rowContentHeightPx);

            RectTransform volumeLabel = U.NewUI("Label", volumeContent);
            U.Place(volumeLabel, new Vector2(0f, 1f), new Vector2(0f, 1f),
                Vector2.zero, new Vector2(contentWidth, 90f * vScale));
            U.AddText(volumeLabel, font, "VOLUME", Mathf.RoundToInt(80f * vScale), TextAnchor.MiddleLeft, Color.white);

            RectTransform volumeControl = U.NewUI("Control", volumeContent);
            U.Place(volumeControl, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                Vector2.zero, new Vector2(contentWidth, 90f * vScale));
            Slider volumeSlider = U.AddSlider(volumeControl, roundedRect, roundedRect, handleSprite, 120f * vScale);
            volumeControl.Find("Background").GetComponent<Image>().color = TrackColor;
            volumeControl.Find("Fill Area/Fill").GetComponent<Image>().color = U.GoldText;
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.wholeNumbers = false;
            volumeSlider.value = SoundManager.Volume;

            // ---------- Hàng Mute: label + Toggle ----------

            RectTransform muteSlot = BuildFlexSlot("Mute Row", controls, rowContentHeightPx);
            RectTransform muteContent = BuildCenteredContent(muteSlot, contentWidth, rowContentHeightPx);

            RectTransform muteLabel = U.NewUI("Label", muteContent);
            U.Place(muteLabel, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                Vector2.zero, new Vector2(contentWidth - 220f * hScale, rowContentHeightPx));
            U.AddText(muteLabel, font, "MUTE SOUND", Mathf.RoundToInt(80f * vScale), TextAnchor.MiddleLeft, Color.white);

            RectTransform muteControl = U.NewUI("Control", muteContent);
            U.Place(muteControl, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                Vector2.zero, new Vector2(170f * vScale, 170f * vScale));
            Toggle muteToggle = U.AddToggle(muteControl, roundedRect, checkmarkSprite);
            muteControl.Find("Background").GetComponent<Image>().color = PanelBlue;
            muteToggle.isOn = SoundManager.Muted;

            // ---------- Nối serialized field + sự kiện ----------

            U.SetObjectField(settingsScreen, "m_SettingsPanel", panel.gameObject);
            U.SetObjectField(settingsScreen, "m_VolumeSlider", volumeSlider);
            U.SetObjectField(settingsScreen, "m_MuteToggle", muteToggle);
            // m_SettingsButton không gán ở đây — MenuButtonsBuilder tạo nút "Setting" trong Bottom Bar
            // và tự gán vào field này sau khi builder này chạy xong.

            UnityEventTools.AddVoidPersistentListener(backButton.onClick, settingsScreen.CloseSettingsScreen);

            panel.gameObject.SetActive(false);
            settingsRoot.SetAsLastSibling();

            Debug.Log("[SettingsBuilder] Đã dựng Settings Screen.", settingsRoot.gameObject);

            return true;
        }

        /// <summary>Slot co giãn (LayoutElement) trong VerticalLayoutGroup — sàn = contentHeightPx, giãn thêm để lấp màn hình.</summary>
        private static RectTransform BuildFlexSlot(string name, Transform parent, float minHeightPx)
        {
            RectTransform slot = U.NewUI(name, parent);
            slot.sizeDelta = new Vector2(0f, minHeightPx);

            LayoutElement element = slot.gameObject.AddComponent<LayoutElement>();
            element.minHeight = minHeightPx;
            element.flexibleHeight = 1f;

            return slot;
        }

        /// <summary>Khối nội dung cao CỐ ĐỊNH, canh giữa trong slot đã giãn (không bị kéo to theo slot).</summary>
        private static RectTransform BuildCenteredContent(RectTransform slot, float width, float heightPx)
        {
            RectTransform content = U.NewUI("Content", slot);
            content.anchorMin = new Vector2(0.5f, 0.5f);
            content.anchorMax = new Vector2(0.5f, 0.5f);
            content.pivot = new Vector2(0.5f, 0.5f);
            content.sizeDelta = new Vector2(width, heightPx);
            content.anchoredPosition = Vector2.zero;

            return content;
        }
    }
}
