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

        // Nền pill Coin HUD: vẽ bo góc bằng code (RoundedRectGenerator), đồng bộ tông xanh navy
        // với Shop/Settings.
        private static readonly Color CoinPillColor = new Color(0.082f, 0.137f, 0.529f, 0.85f);

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

            // ---------- Canvas chính LUÔN ở Overlay ----------
            // Screen Space - Overlay là bắt buộc để UI (title, nút, panel Shop/Settings...) không bao giờ
            // bị 3D che — thử đổi nguyên Canvas này sang Screen Space - Camera ở bản trước đã làm MẤT
            // TOÀN BỘ UI (không chỉ bị che), nên revert lại đây cho chắc (idempotent, không hại gì nếu
            // Canvas vốn đã là Overlay).
            ResetCanvasToOverlay(canvas);
            ReenablePreview3D();

            // Dọn "Background" cũ còn sót lại BÊN TRONG Canvas Overlay (từ bản build lỗi trước) — nếu để
            // sót, nó sẽ lại đè lên preview 3D y hệt bug ban đầu, dù Background Canvas riêng đã đúng.
            U.DestroyIfExists(canvas, BackgroundName);

            // ---------- Nền Menu: Canvas RIÊNG, Screen Space - Camera, nằm sau preview 3D ----------
            // Không thể đặt ảnh nền full-screen vào Canvas Overlay ở trên (nó sẽ luôn vẽ đè lên preview
            // 3D của MenuPrefab, bất kể sibling order). Giải pháp: một Canvas riêng, độc lập, dùng
            // Screen Space - Camera với Plane Distance nằm SAU MenuPrefab (~10-11 đơn vị) — Canvas Overlay
            // ở trên vẫn luôn vẽ đè lên MỌI THỨ (kể cả Canvas Camera này), nên UI tương tác không đổi.
            BuildBackgroundCanvas(scene);

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
            U.AddSlicedImage(coinHud, RoundedRectGenerator.Ensure(), CoinPillColor, false);

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
            // Icon bánh răng trắng được vẽ bằng code (IconGenerator) — Unity AI asset-generation
            // không khả dụng trong phiên Editor này (GetModels trả về rỗng).

            RectTransform settingSlot = U.NewUI("Setting", bottomBar);
            U.Place(settingSlot, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(SlotAnchoredX, SlotAnchoredY), SlotSize);
            Image settingBackdrop = U.AddImage(settingSlot, circleSprite, true, false);
            settingBackdrop.color = CircleColor;

            RectTransform settingIcon = U.NewUI("Icon", settingSlot);
            U.Place(settingIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(48f, 48f));
            U.AddImage(settingIcon, IconGenerator.EnsureSettingsIcon(), false, true);

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

        /// <summary>Đảm bảo Canvas chính là Screen Space - Overlay (an toàn dù đã là Overlay sẵn).</summary>
        private static void ResetCanvasToOverlay(Transform canvas)
        {
            Canvas canvasComp = canvas.GetComponent<Canvas>();
            if (canvasComp == null) return;

            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasComp.worldCamera = null;
        }

        /// <summary>Tìm Main Camera theo Camera.main, dự phòng bằng tên GameObject nếu tag chưa gán.</summary>
        private static Camera FindMainCamera()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null) return mainCamera;

            GameObject camGo = GameObject.Find("Main Camera");
            return camGo != null ? camGo.GetComponent<Camera>() : null;
        }

        /// <summary>
        /// Dựng Canvas riêng (root level, ngoài "Canvas" chính) chỉ chứa ảnh nền, ở Screen Space - Camera
        /// với Plane Distance nằm sau MenuPrefab (~10-11 đơn vị từ camera) để preview 3D vẽ đè lên nó.
        /// Canvas chính (Overlay) vẫn luôn vẽ đè lên Canvas này, nên UI tương tác không bị ảnh hưởng.
        /// </summary>
        private static void BuildBackgroundCanvas(UnityEngine.SceneManagement.Scene scene)
        {
            Camera mainCamera = FindMainCamera();
            if (mainCamera == null)
            {
                Debug.LogWarning("[MenuButtonsBuilder] Không tìm thấy Main Camera — bỏ qua dựng Background Canvas.");
                return;
            }

            GameObject existing = U.FindRootObject(scene, "Background Canvas");
            if (existing != null) Object.DestroyImmediate(existing);

            GameObject bgCanvasGo = new GameObject("Background Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0) bgCanvasGo.layer = uiLayer;

            Canvas bgCanvas = bgCanvasGo.GetComponent<Canvas>();
            bgCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            bgCanvas.worldCamera = mainCamera;
            // MenuPrefab (nhẫn/đường ray) cách camera ~10-11 đơn vị — 50 chừa dư nhiều, vẫn nhỏ hơn
            // nhiều so với far clip (1000) của camera.
            bgCanvas.planeDistance = 50f;

            CanvasScaler bgScaler = bgCanvasGo.GetComponent<CanvasScaler>();
            bgScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            bgScaler.referenceResolution = new Vector2(800f, 600f);
            bgScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            bgScaler.matchWidthOrHeight = 0f;

            RectTransform background = U.NewUI(BackgroundName, bgCanvasGo.transform);
            U.Stretch(background);
            U.AddImage(background, "Back New", false, false); // raycast off => không chặn click nút Menu
        }

        /// <summary>
        /// "Test" (mesh đường ray/wire dưới MenuPrefab) đang bị tắt sẵn trong scene — bật lại để khớp
        /// hình ảnh preview gốc (nhẫn + đường ray + bi), không chỉ còn mỗi nhẫn.
        /// </summary>
        private static void ReenablePreview3D()
        {
            GameObject menuPrefab = GameObject.Find("MenuPrefab");
            if (menuPrefab == null) return;

            Transform wire = menuPrefab.transform.Find("Test");
            if (wire != null && !wire.gameObject.activeSelf)
            {
                wire.gameObject.SetActive(true);
                Debug.Log("[MenuButtonsBuilder] Đã bật lại 'MenuPrefab/Test' (mesh đường ray) — trước đó bị tắt trong scene.");
            }
        }
    }
}
