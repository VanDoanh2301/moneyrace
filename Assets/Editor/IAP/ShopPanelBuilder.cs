using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

using U = BallEditor.UIBuildUtil;
using T = BallEditor.UIThemeUtil;

namespace BallEditor
{
    /// <summary>
    /// Dựng panel Shop IAP (7 gói iap1–iap7) + HUD coin + nút mở Shop, trực tiếp dưới
    /// GameObject "Canvas" của scene Main.unity.
    /// Port từ game Tetra Rush; bỏ ràng buộc UIController_tetris (Ball dùng
    /// <see cref="UIManager"/> nằm trên GameObject riêng, không trên Canvas).
    /// Dùng bộ art sinh từ <see cref="UIArtGenerator"/> (bo góc + gradient).
    /// Chạy lại được nhiều lần (idempotent).
    /// </summary>
    public static class ShopPanelBuilder
    {
        private const string CanvasName = "Canvas";
        private const string ShopPanelName = "ShopPanel";
        private const string ShopButtonName = "ShopButton";
        private const string PointsHudName = "PointsHUD";
        private const string PointsLabelName = "Coins Text";

        // Panel của Ball — đều là con trực tiếp của root "Canvas", khớp các field
        // mainMenuGui / gameplayGui / gameOverGui trên UIManager.
        private const string MainMenuPanelName = "MainMenu";
        private const string GameplayPanelName = "Gameplay";
        private const string GameOverPanelName = "GameOver";

        private const string ShopTitle = "COIN SHOP";

        private const float PanelSideMargin = 40f;
        private const float HeaderHeight = 180f;
        private const float HeaderTop = 30f;
        private const float PointsPillTop = 250f;
        private const float PointsPillHeight = 96f;

        // Canvas 1080x1920: 7 hàng x 190 + 6 x 18 spacing + 2 x 20 padding = 1478px,
        // vừa trong viewport 1500px nên trên 9:16 không phải cuộn; ScrollRect vẫn giữ
        // để an toàn trên màn tỉ lệ khác.
        private const float ScrollTop = 380f;
        private const float ScrollBottom = 40f;
        private const float RowHeight = 190f;
        private const float RowSpacing = 18f;
        private const int RowPadding = 20;

        /// <summary>HUD coin nằm dưới ô Score (Score chiếm y -240..-360 tính từ mép trên).</summary>
        private const float HudTopMargin = 450f;

        /// <summary>
        /// Nút Shop ở góc DƯỚI phải. Góc trên phải không dùng được: hàng
        /// ButtonEfx/ButtonMusic/TopScore đã chiếm, và vì canvas co chiều ngang theo tỉ lệ
        /// màn hình (match height) nên trên máy cao hơn 9:16 nút sẽ đè lên TopScore/Title.
        /// Dải dưới thì trống ở cả MainMenu lẫn GameOver.
        /// </summary>
        private const float ShopButtonBottom = 40f;

        /// <summary>
        /// Trong mỗi hàng: chừa bao nhiêu px bên phải cho khối giá + nút Buy
        /// (Buy 24..204, giá 224..374 tính từ mép phải).
        /// </summary>
        private const float RowRightBlockWidth = 390f;

        // hệ số bo góc: bán kính ≈ 34 / multiplier
        private const float RadiusRow = 0.75f;
        private const float RadiusHeader = 0.85f;
        private const float RadiusPill = 0.70f;
        private const float RadiusBuy = 0.70f;
        private const float RadiusBack = 1.30f;

        // Màu không còn hardcode: mọi mặt phẳng/chữ lấy vai trò từ BallTheme, tự đổi
        // theo bảng màu đang chạy của ColorManager.

        private struct Pack
        {
            public string ProductId;
            public int Points;
            public string DisplayName;
            public string GoldSprite;
            /// <summary>Chỉ là chữ tạm hiện trong Editor; runtime bị IAPButton ghi đè bằng localizedPriceString.</summary>
            public string PricePlaceholder;

            public Pack(string productId, int points, string displayName, string goldSprite, string pricePlaceholder)
            {
                ProductId = productId;
                Points = points;
                DisplayName = displayName;
                GoldSprite = goldSprite;
                PricePlaceholder = pricePlaceholder;
            }
        }

        // Số coin phải khớp IAPManager.GetPointsForProduct. Giá thật đặt trên Google Play Console.
        private static readonly Pack[] Packs =
        {
            new Pack("iap1", 100,  "Starter Stack", "iv_gold1", "$0.50"),
            new Pack("iap2", 200,  "Small Stack",   "iv_gold2", "$1"),
            new Pack("iap3", 400,  "Ball Stack",    "iv_gold3", "$2"),
            new Pack("iap4", 600,  "Big Stack",     "iv_gold4", "$3"),
            new Pack("iap5", 1000, "Mega Stack",    "iv_gold5", "$5"),
            new Pack("iap6", 2000, "Turbo Stack",   "iv_gold4", "$7"),
            new Pack("iap7", 5000, "Ultra Stack",   "iv_gold5", "$10"),
        };

        [MenuItem("Tools/Ball/Build Shop Panel")]
        public static void BuildShopPanel()
        {
            if (!Build()) return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Build Shop Panel", "Đã dựng Shop Panel với " + Packs.Length + " gói coin.", "OK");
        }

        /// <summary>Dựng shop, chưa lưu scene. Trả về false nếu thiếu điều kiện.</summary>
        public static bool Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Build Shop Panel", "Mở scene Assets/BallsAvoid/Scenes/Main.unity trước đã.", "OK");
                return false;
            }

            GameObject canvasRoot = U.FindRootObject(scene, CanvasName);
            if (canvasRoot == null)
            {
                Debug.LogError("[ShopBuilder] Không tìm thấy GameObject '" + CanvasName + "' trong scene.");
                return false;
            }

            Transform canvasTransform = canvasRoot.transform;

            Transform mainMenuPanel = FindDeep(canvasTransform, MainMenuPanelName);
            Transform gameplayPanel = FindDeep(canvasTransform, GameplayPanelName);
            Transform gameOverPanel = FindDeep(canvasTransform, GameOverPanelName);

            if (mainMenuPanel == null || gameplayPanel == null || gameOverPanel == null)
            {
                Debug.LogError("[ShopBuilder] Thiếu panel dưới Canvas. Cần cả '" + MainMenuPanelName
                    + "', '" + GameplayPanelName + "', '" + GameOverPanelName + "'.");
                return false;
            }

            if (!U.EnsureSprites("Build Shop Panel",
                    "coin", "iv_shop", "iv_gold1", "iv_gold2", "iv_gold3", "iv_gold4", "iv_gold5"))
            {
                return false;
            }

            Sprite rounded = AssetDatabase.LoadAssetAtPath<Sprite>(UIArtGenerator.RoundedPath);
            Sprite shopBg = AssetDatabase.LoadAssetAtPath<Sprite>(UIArtGenerator.BallsBgPath);
            Sprite arrow = AssetDatabase.LoadAssetAtPath<Sprite>(UIArtGenerator.ArrowLeftPath);
            if (rounded == null || shopBg == null || arrow == null)
            {
                Debug.LogError("[ShopBuilder] Thiếu art. Chạy Tools/Ball/Generate UI Art trước.");
                return false;
            }

            if (!T.EnsureArt()) return false;

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Rubik-Regular SDF.asset");
            if (font == null) font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                Debug.LogError("[ShopBuilder] Chưa có TMP font asset. Window > TextMeshPro > Import TMP Essential Resources.");
                return false;
            }

            // Dọn bản dựng trước (idempotent) — shop/nút/HUD có thể nằm rải ở nhiều panel.
            U.DestroyIfExists(canvasTransform, ShopPanelName);
            U.DestroyIfExists(gameplayPanel, PointsHudName);
            U.DestroyIfExists(mainMenuPanel, ShopButtonName);
            U.DestroyIfExists(gameOverPanel, ShopButtonName);

            // Bộ đếm coin sống trên Canvas (thay chỗ PointsScorer_tetris của bản gốc).
            CoinScorer scorer = canvasRoot.GetComponent<CoinScorer>();
            if (scorer == null) scorer = canvasRoot.AddComponent<CoinScorer>();

            EnsureIAPManager(scene);

            // ---------- HUD coin trong panel Gameplay ----------

            RectTransform hud = U.NewUI(PointsHudName, gameplayPanel);
            U.Place(hud, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -HudTopMargin), new Vector2(320f, 88f));

            RectTransform hudIcon = U.NewUI("Icon", hud);
            U.Place(hudIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(72f, 72f));
            U.AddImage(hudIcon, "coin", false, true);
            T.Flat(hudIcon.gameObject, ThemedGraphic.ThemeRole.Coin);

            RectTransform hudText = U.NewUI(PointsLabelName, hud);
            U.Place(hudText, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, 0f), new Vector2(216f, 72f));
            TextMeshProUGUI hudLabel = U.AddText(hudText, font, "0", 48f, TextAlignmentOptions.Left, Color.white);
            hudLabel.fontStyle = FontStyles.Bold;
            T.Flat(hudText.gameObject, ThemedGraphic.ThemeRole.Coin);

            PointsHUD pointsHud = hud.gameObject.AddComponent<PointsHUD>();
            U.SetObjectField(pointsHud, "m_PointsText", hudLabel);

            // ---------- Shop Panel ----------

            RectTransform shopRoot = U.NewUI(ShopPanelName, canvasTransform);
            U.Stretch(shopRoot);

            ShopPanel shopPanel = shopRoot.gameObject.AddComponent<ShopPanel>();

            // Nút mở Shop: một cái ở MainMenu, một cái ở GameOver.
            RectTransform menuShopBtn = BuildShopButton(mainMenuPanel, shopPanel);
            RectTransform overShopBtn = BuildShopButton(gameOverPanel, shopPanel);

            U.SetObjectArrayField(shopPanel, "m_HideWhileOpen",
                hud.gameObject, menuShopBtn.gameObject, overShopBtn.gameObject);

            RectTransform panel = U.NewUI("Panel", shopRoot);
            U.Stretch(panel);
            Image panelBg = U.AddImage(panel, null, true, false); // raycast on => chặn click xuyên xuống dưới
            panelBg.sprite = shopBg;
            T.Attach(panel.gameObject, ThemedGraphic.ThemeRole.Surface, true, 1f);

            // Header: thanh bo góc + tiêu đề + nút back
            RectTransform header = U.NewUI("Header", panel);
            StretchTop(header, HeaderTop, HeaderHeight);
            T.ThemedSurface(header, ThemedGraphic.ThemeRole.Header, RadiusHeader, false, true, true);

            RectTransform headerTitle = U.NewUI("Title", header);
            U.Stretch(headerTitle);
            TextMeshProUGUI titleLabel = U.AddText(headerTitle, font, ShopTitle, 62f, TextAlignmentOptions.Center, Color.white);
            T.Flat(headerTitle.gameObject, ThemedGraphic.ThemeRole.TextOnSurface);
            titleLabel.fontStyle = FontStyles.Bold;
            titleLabel.characterSpacing = 4f;

            RectTransform back = U.NewUI("Back", header);
            U.Place(back, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(96f, 96f));
            Image backBg = T.ThemedSurface(back, ThemedGraphic.ThemeRole.Header, RadiusBack, true, true, true);
            Button backButton = U.AddButton(back, backBg);
            T.ApplyButtonColors(backButton);

            RectTransform backIcon = U.NewUI("Icon", back);
            U.Place(backIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(48f, 48f));
            Image backArrow = backIcon.gameObject.AddComponent<Image>();
            backArrow.sprite = arrow;
            backArrow.preserveAspect = true;
            backArrow.raycastTarget = false;
            T.Flat(backIcon.gameObject, ThemedGraphic.ThemeRole.TextOnSurface);

            UnityEventTools.AddVoidPersistentListener(backButton.onClick, shopPanel.Close);

            // Pill số coin: icon + số, canh giữa
            RectTransform pointsPill = U.TopCenter("Coins Pill", panel, PointsPillTop, 420f, PointsPillHeight);
            T.ThemedSurface(pointsPill, ThemedGraphic.ThemeRole.SurfaceAlt, RadiusPill, false, true, true);

            RectTransform pillIcon = U.NewUI("Icon", pointsPill);
            U.Place(pillIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(64f, 64f));
            U.AddImage(pillIcon, "coin", false, true);
            T.Flat(pillIcon.gameObject, ThemedGraphic.ThemeRole.Coin);

            RectTransform pillValue = U.NewUI("Value", pointsPill);
            U.Place(pillValue, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(120f, 0f), new Vector2(260f, 70f));
            TextMeshProUGUI pillLabel = U.AddText(pillValue, font, "0", 48f, TextAlignmentOptions.Left, Color.white);
            pillLabel.fontStyle = FontStyles.Bold;
            T.Flat(pillValue.gameObject, ThemedGraphic.ThemeRole.Coin);
            U.SetObjectField(shopPanel, "m_PointsText", pillLabel);

            // Scroll view — stretch cả hai chiều để không phụ thuộc bề rộng canvas.
            RectTransform scroll = U.NewUI("Scroll View", panel);
            scroll.anchorMin = Vector2.zero;
            scroll.anchorMax = Vector2.one;
            scroll.pivot = new Vector2(0.5f, 0.5f);
            scroll.offsetMin = new Vector2(PanelSideMargin, ScrollBottom);
            scroll.offsetMax = new Vector2(-PanelSideMargin, -ScrollTop);

            ScrollRect scrollRect = scroll.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 40f;

            RectTransform viewport = U.NewUI("Viewport", scroll);
            U.Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = U.NewUI("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = RowSpacing;
            layout.padding = new RectOffset(RowPadding, RowPadding, RowPadding, RowPadding);
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport;
            scrollRect.content = content;

            for (int i = 0; i < Packs.Length; i++)
            {
                BuildPackRow(content, font, rounded, Packs[i], i + 1, shopPanel);
            }

            shopRoot.SetAsLastSibling();
            shopRoot.gameObject.SetActive(false);

            // ---------- Ô text coin trên MainMenu + GameOver ----------
            // Neo tránh chỗ đã có: MainMenu (Title y 340..660, TapToStart -490..-610),
            // GameOver (Best Score 250..550, Final Score -105..305, PlayAgain ~-548).
            AddPanelCoinsLabel(mainMenuPanel, font, 0.58f, 0.65f);
            AddPanelCoinsLabel(gameOverPanel, font, 0.36f, 0.43f);

            float contentHeight = Packs.Length * RowHeight + (Packs.Length - 1) * RowSpacing + RowPadding * 2f;
            Debug.Log(string.Format(
                "[ShopBuilder] Đã dựng Shop Panel: {0} gói, viewport {1:0}px, content {2:0}px. CoinScorer trên '{3}'.",
                Packs.Length, U.RefHeight - ScrollTop - ScrollBottom, contentHeight, canvasRoot.name), shopRoot.gameObject);

            return true;
        }

        // ---------------------------------------------------------------- helpers

        /// <summary>
        /// Neo theo mép trên, giãn hết chiều ngang (trừ <see cref="PanelSideMargin"/> hai bên).
        /// Phải giãn chứ không đặt bề rộng cố định: Canvas của Ball dùng
        /// matchWidthOrHeight = 1 nên bề rộng canvas đổi theo tỉ lệ màn hình
        /// (1080 ở 9:16, 960 ở 1:2, ~886 ở 20:9) — bề rộng cứng sẽ tràn ra ngoài.
        /// </summary>
        private static void StretchTop(RectTransform rect, float top, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(PanelSideMargin, -(top + height));
            rect.offsetMax = new Vector2(-PanelSideMargin, -top);
        }

        /// <summary>Giãn ngang trong hàng: cách mép trái <paramref name="left"/>, mép phải <paramref name="right"/>.</summary>
        private static void StretchInRow(RectTransform rect, float left, float right, float yOffset, float height)
        {
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, yOffset - height * 0.5f);
            rect.offsetMax = new Vector2(-right, yOffset + height * 0.5f);
        }

        /// <summary>Tìm theo tên trong toàn bộ cây con, không chỉ con trực tiếp.</summary>
        private static Transform FindDeep(Transform root, string name)
        {
            if (root.name == name) return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeep(root.GetChild(i), name);
                if (found != null) return found;
            }

            return null;
        }

        private static RectTransform BuildShopButton(Transform parent, ShopPanel shopPanel)
        {
            RectTransform shopBtn = U.NewUI(ShopButtonName, parent);
            U.Place(shopBtn, new Vector2(1f, 0f), new Vector2(1f, 0f),
                new Vector2(-24f, ShopButtonBottom), new Vector2(120f, 120f));

            Button button = U.IconButton(shopBtn, "iv_shop");
            T.ApplyButtonColors(button);

            UnityEventTools.AddVoidPersistentListener(button.onClick, shopPanel.Open);

            return shopBtn;
        }

        /// <summary>Đảm bảo scene có IAPManager để một cú bấm menu là xong.</summary>
        private static void EnsureIAPManager(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.GetComponentInChildren<IAPManager>(true) != null) return;
            }

            var go = new GameObject("IAPManager");
            go.AddComponent<IAPManager>();

            Debug.Log("[ShopBuilder] Đã thêm GameObject 'IAPManager' vào scene '" + scene.name + "'.");
        }

        /// <summary>Thêm (hoặc dựng lại) một dòng text coin vào panel có sẵn.</summary>
        private static TextMeshProUGUI AddPanelCoinsLabel(Transform panel, TMP_FontAsset font,
            float anchorYMin, float anchorYMax)
        {
            U.DestroyIfExists(panel, PointsLabelName);

            RectTransform rect = U.NewUI(PointsLabelName, panel);
            rect.anchorMin = new Vector2(0.1f, anchorYMin);
            rect.anchorMax = new Vector2(0.9f, anchorYMax);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI label = U.AddText(rect, font, "Coins: {0}", 46f, TextAlignmentOptions.Center, Color.white);
            T.Flat(rect.gameObject, ThemedGraphic.ThemeRole.Coin);

            // Panel này bật/tắt bằng SetActive nên cần một PointsHUD riêng để tự refresh.
            PointsHUD hud = panel.gameObject.GetComponent<PointsHUD>();
            if (hud == null) hud = panel.gameObject.AddComponent<PointsHUD>();
            U.SetObjectField(hud, "m_PointsText", label);

            return label;
        }

        private static void BuildPackRow(RectTransform parent, TMP_FontAsset font, Sprite rounded,
            Pack pack, int index, ShopPanel shopPanel)
        {
            RectTransform row = U.NewUI("Inapp" + index, parent);
            row.sizeDelta = new Vector2(0f, RowHeight);
            T.ThemedSurface(row, ThemedGraphic.ThemeRole.Surface, RadiusRow, true, true, true);

            LayoutElement element = row.gameObject.AddComponent<LayoutElement>();
            element.minHeight = RowHeight;
            element.preferredHeight = RowHeight;

            // Bố cục ngang trong hàng rộng 960:
            //   Gold 30..180 | Tên + số coin 200..520 | Giá ..-220 | Buy ..-24

            RectTransform gold = U.NewUI("Gold", row);
            U.Place(gold, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(150f, 150f));
            U.AddImage(gold, pack.GoldSprite, false, true);

            // Tên + số coin giãn ngang, chừa RowRightBlockWidth cho giá + nút Buy, để trên
            // màn hẹp (canvas ~886) chữ không chạy đè lên giá như khi dùng bề rộng cứng.
            RectTransform title = U.NewUI("Title", row);
            StretchInRow(title, 200f, RowRightBlockWidth, 26f, 60f);
            TextMeshProUGUI titleLabel = U.AddText(title, font, pack.DisplayName, 42f, TextAlignmentOptions.Left, Color.white);
            titleLabel.fontStyle = FontStyles.Bold;
            // Rubik không có ký tự Ellipsis nên TMP sẽ tự rơi về Truncate kèm warning;
            // đặt thẳng Truncate cho Console sạch.
            titleLabel.overflowMode = TextOverflowModes.Truncate;
            T.Flat(title.gameObject, ThemedGraphic.ThemeRole.TextOnSurface);

            RectTransform amount = U.NewUI("Amount", row);
            StretchInRow(amount, 200f, RowRightBlockWidth, -30f, 52f);
            U.AddText(amount, font, pack.Points + " Coins", 36f, TextAlignmentOptions.Left, Color.white);
            T.Flat(amount.gameObject, ThemedGraphic.ThemeRole.Coin);

            // Giá: TMP + Button + IAPButton (giá tự cập nhật sau khi IAP init)
            RectTransform priceRect = U.NewUI("TextIap", row);
            U.Place(priceRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-224f, 0f), new Vector2(150f, 70f));
            TextMeshProUGUI priceLabel = U.AddText(priceRect, font, pack.PricePlaceholder, 40f,
                TextAlignmentOptions.Right, Color.white);
            priceLabel.fontStyle = FontStyles.Bold;
            T.Flat(priceRect.gameObject, ThemedGraphic.ThemeRole.Coin);
            priceLabel.raycastTarget = true;
            T.ApplyButtonColors(U.AddButton(priceRect, priceLabel));

            IAPButton iapButton = priceRect.gameObject.AddComponent<IAPButton>();
            iapButton.productId = pack.ProductId;
            iapButton.priceText = priceLabel;

            // Nút Buy: bo góc + gradient xanh, chữ BUY
            RectTransform buy = U.NewUI("Buy", row);
            U.Place(buy, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(180f, 96f));
            Image buyBg = T.ThemedSurface(buy, ThemedGraphic.ThemeRole.Accent, RadiusBuy, true, true, true);
            Button buyButton = U.AddButton(buy, buyBg);
            T.ApplyButtonColors(buyButton);

            RectTransform buyLabelRect = U.NewUI("Label", buy);
            U.Stretch(buyLabelRect);
            TextMeshProUGUI buyLabel = U.AddText(buyLabelRect, font, "BUY", 40f, TextAlignmentOptions.Center, Color.white);
            T.Flat(buyLabelRect.gameObject, ThemedGraphic.ThemeRole.TextOnAccent);
            buyLabel.fontStyle = FontStyles.Bold;
            buyLabel.characterSpacing = 2f;

            UnityAction buyAction = GetBuyAction(shopPanel, pack.Points);
            if (buyAction != null)
            {
                UnityEventTools.AddVoidPersistentListener(buyButton.onClick, buyAction);
            }
            else
            {
                Debug.LogError("[ShopBuilder] Không có hàm mua cho gói " + pack.Points
                    + " coin (" + pack.ProductId + "). Nút BUY sẽ không làm gì.");
            }
        }

        /// <summary>
        /// Chọn hàm mua theo SỐ COIN, không theo index trong mảng <see cref="Packs"/> —
        /// đảo thứ tự Packs sẽ không còn bind sai nút như bản gốc.
        /// </summary>
        private static UnityAction GetBuyAction(ShopPanel shop, int points)
        {
            switch (points)
            {
                case 100: return shop.BuyPoints100;
                case 200: return shop.BuyPoints200;
                case 400: return shop.BuyPoints400;
                case 600: return shop.BuyPoints600;
                case 1000: return shop.BuyPoints1000;
                case 2000: return shop.BuyPoints2000;
                case 5000: return shop.BuyPoints5000;
            }

            return null;
        }
    }
}
