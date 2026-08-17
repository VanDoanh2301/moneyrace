using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;

using U = RingGameEditor.UIBuildUtil;

namespace RingGameEditor
{
    /// <summary>
    /// Dựng ảnh nền Menu và thay 2 nút "Share"/"Rate Us" trong Bottom Bar bằng "Shop"/"Setting".
    /// Phụ thuộc Shop/Settings đã dựng (tự gọi ShopScreenBuilder/SettingsScreenBuilder nếu thiếu).
    /// Chạy lại được nhiều lần (idempotent).
    /// </summary>
    public static class MenuButtonsBuilder
    {
        private const string BackgroundName = "Background";
        private const string BottomBarName = "Bottom Bar";

        // Sprite/màu nút tròn nền của Rate Us/Share gốc — nằm ngoài Assets/Sprites/ nên không
        // resolve được qua UIBuildUtil.LoadSprite, phải load thẳng theo đường dẫn.
        private const string CircleSpritePath = "Assets/Images/Panels@2x-assets/Circles/BigSize_Stroke_30px.png";
        private static readonly Color CircleColor = new Color(0.198f, 0.198f, 0.198f, 0.792f);

        // Vị trí/kích thước copy nguyên từ 2 nút Rate Us/Share gốc trong Menu.unity.
        private static readonly Vector2 SlotSize = new Vector2(85.31f, 85.31f);
        private const float SlotAnchoredX = 100f;
        private const float SlotAnchoredY = 0.0012f;

        [MenuItem("Tools/RingGame/Build Menu Buttons")]
        public static void BuildMenuButtons()
        {
            if (!Build()) return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Build Menu Buttons", "Đã dựng nền Menu và nút Shop/Setting.", "OK");
        }

        internal static bool Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Build Menu Buttons", "Mở scene Menu.unity trước đã.", "OK");
                return false;
            }

            GameObject canvasGo = U.FindRootObject(scene, U.CanvasName);
            if (canvasGo == null)
            {
                Debug.LogError("[MenuButtonsBuilder] Không tìm thấy GameObject '" + U.CanvasName + "' trong scene.");
                return false;
            }

            Transform canvas = canvasGo.transform;

            Transform bottomBar = canvas.Find(BottomBarName);
            if (bottomBar == null)
            {
                Debug.LogError("[MenuButtonsBuilder] Không tìm thấy '" + BottomBarName + "' trong Canvas.");
                return false;
            }

            if (!U.EnsureSprites("Build Menu Buttons", "Back New", "iv_shop"))
            {
                return false;
            }

            Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CircleSpritePath);
            if (circleSprite == null)
            {
                Debug.LogError("[MenuButtonsBuilder] Thiếu sprite: " + CircleSpritePath);
                EditorUtility.DisplayDialog("Build Menu Buttons", "Thiếu sprite nền nút tròn. Xem Console.", "OK");
                return false;
            }

            Font font = U.LoadDefaultFont();
            if (font == null)
            {
                Debug.LogError("[MenuButtonsBuilder] Không tìm được font để dựng UI.");
                return false;
            }

            // Đảm bảo Shop/Settings đã tồn tại — build sẵn nếu chưa có, để nút vừa tạo có nơi trỏ tới.
            ShopScreen shopScreen = canvas.Find("Shop") == null ? null : canvas.Find("Shop").GetComponent<ShopScreen>();
            if (shopScreen == null)
            {
                if (!ShopScreenBuilder.Build())
                {
                    Debug.LogError("[MenuButtonsBuilder] Không dựng được Shop Screen (phụ thuộc bắt buộc).");
                    return false;
                }

                shopScreen = canvas.Find("Shop").GetComponent<ShopScreen>();
            }

            SettingsScreen settingsScreen = canvas.Find("Settings") == null ? null : canvas.Find("Settings").GetComponent<SettingsScreen>();
            if (settingsScreen == null)
            {
                if (!SettingsScreenBuilder.Build())
                {
                    Debug.LogError("[MenuButtonsBuilder] Không dựng được Settings Screen (phụ thuộc bắt buộc).");
                    return false;
                }

                settingsScreen = canvas.Find("Settings").GetComponent<SettingsScreen>();
            }

            // ---------- Dọn rác từ bản build cũ ----------
            // "Shop Button" đứng riêng ở góc trên phải là tàn dư của bản build trước khi
            // ShopScreenBuilder ngừng tự tạo nó — dọn luôn ở đây cho chắc (idempotent).
            U.DestroyIfExists(canvas, "Shop Button");

            // ---------- Đổi tên tiêu đề ----------

            Transform titleT = canvas.Find("Text");
            if (titleT != null)
            {
                Text titleText = titleT.GetComponent<Text>();
                if (titleText != null) titleText.text = "Untangle Rings";
            }

            // ---------- Nền Menu ----------

            U.DestroyIfExists(canvas, BackgroundName);

            RectTransform background = U.NewUI(BackgroundName, canvas);
            U.Stretch(background);
            U.AddImage(background, "Back New", false, false); // raycast off => không chặn click nút Menu
            background.SetAsFirstSibling(); // render sau lưng mọi thứ khác trong Canvas

            // ---------- Xoá 2 nút cũ + dựng lại (idempotent) ----------

            U.DestroyIfExists(bottomBar, "Rate Us");
            U.DestroyIfExists(bottomBar, "Share");
            U.DestroyIfExists(bottomBar, "Shop");
            U.DestroyIfExists(bottomBar, "Setting");
            U.DestroyIfExists(canvas, "Coin HUD"); // dọn bản Coin HUD cũ ở góc trên trái (nếu còn)

            AudioSource clickSfx = FindClickSfx();

            // ---------- Nút Shop: đúng vị trí Rate Us cũ (phải) ----------

            RectTransform shopSlot = U.NewUI("Shop", bottomBar);
            U.Place(shopSlot, new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-SlotAnchoredX, SlotAnchoredY), SlotSize);
            Image shopBackdrop = U.AddImage(shopSlot, circleSprite, true, false);
            shopBackdrop.color = CircleColor;

            RectTransform shopIcon = U.NewUI("Icon", shopSlot);
            U.Place(shopIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(52f, 52f));
            U.AddImage(shopIcon, "iv_shop", false, true);

            Button shopButton = U.AddButton(shopSlot, shopBackdrop);
            UnityEventTools.AddVoidPersistentListener(shopButton.onClick, shopScreen.ShowShopScreen);
            if (clickSfx != null) UnityEventTools.AddVoidPersistentListener(shopButton.onClick, clickSfx.Play);

            U.SetObjectField(shopScreen, "m_ShopButton", shopSlot.gameObject);

            // ---------- Coin HUD: ngay dưới icon Shop, không chặn click nút Shop ----------

            RectTransform coinHud = U.NewUI("Coin HUD", shopSlot);
            U.Place(coinHud, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f),
                new Vector2(0f, -6f), new Vector2(90f, 26f));

            RectTransform coinIcon = U.NewUI("Icon", coinHud);
            U.Place(coinIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(4f, 0f), new Vector2(18f, 18f));
            U.AddImage(coinIcon, "coin", false, true);

            RectTransform coinText = U.NewUI("Coins Text", coinHud);
            U.Place(coinText, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(24f, 0f), new Vector2(64f, 24f));
            Text coinLabel = U.AddText(coinText, font, "{0}", 20, TextAnchor.MiddleLeft, U.GoldText);

            CoinHUD coinHudComp = coinHud.gameObject.AddComponent<CoinHUD>();
            U.SetObjectField(coinHudComp, "m_CoinsText", coinLabel);

            // ---------- Nút Setting: đúng vị trí Share cũ (trái) ----------
            // Không có sẵn icon bánh răng trong project (đã kiểm tra Assets/Images, Assets/Sprites) và
            // Unity AI asset-generation không khả dụng trong phiên Editor này (GetModels trả về rỗng) —
            // dùng chữ "SET" thay icon, giống cách IAPButton dùng Text cho nút giá thay vì icon.

            RectTransform settingSlot = U.NewUI("Setting", bottomBar);
            U.Place(settingSlot, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(SlotAnchoredX, SlotAnchoredY), SlotSize);
            Image settingBackdrop = U.AddImage(settingSlot, circleSprite, true, false);
            settingBackdrop.color = CircleColor;

            RectTransform settingLabel = U.NewUI("Icon", settingSlot);
            U.Stretch(settingLabel);
            U.AddText(settingLabel, font, "SET", 30, TextAnchor.MiddleCenter, Color.white);

            Button settingButton = U.AddButton(settingSlot, settingBackdrop);
            UnityEventTools.AddVoidPersistentListener(settingButton.onClick, settingsScreen.ShowSettingsScreen);
            if (clickSfx != null) UnityEventTools.AddVoidPersistentListener(settingButton.onClick, clickSfx.Play);

            U.SetObjectField(settingsScreen, "m_SettingsButton", settingSlot.gameObject);

            Debug.Log("[MenuButtonsBuilder] Đã dựng nền Menu + nút Shop/Setting trong Bottom Bar.", bottomBar.gameObject);

            return true;
        }

        /// <summary>Tìm AudioSource dùng cho tiếng click nút (tuỳ chọn — không có thì bỏ qua, không lỗi).</summary>
        private static AudioSource FindClickSfx()
        {
            GameObject audioGo = GameObject.Find("Audio");
            if (audioGo == null) return null;

            return audioGo.GetComponent<AudioSource>();
        }
    }
}
