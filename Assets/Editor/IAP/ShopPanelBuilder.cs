using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

using U = TetrisEditor.UIBuildUtil;

namespace TetrisEditor
{
    /// <summary>
    /// Dựng panel Shop IAP (7 gói iap1–iap7) + HUD điểm + nút mở Shop, trực tiếp dưới
    /// GameObject "Canvas" của scene game.unity, và nối các ô text điểm vào
    /// <see cref="UIController_tetris"/>.
    /// Dùng bộ art sinh từ <see cref="UIArtGenerator"/> (bo góc + gradient) để đồng bộ
    /// tông màu với màn home, chỉ giữ lại icon vàng và icon coin của bộ IAP gốc.
    /// Chạy lại được nhiều lần (idempotent).
    /// </summary>
    public static class ShopPanelBuilder
    {
        private const string CanvasName = "Canvas";
        private const string ShopPanelName = "ShopPanel";
        private const string ShopButtonName = "ShopButton";
        private const string PointsHudName = "PointsHUD";
        private const string PointsLabelName = "Points Text";

        private const string StartPanelName = "startGameUI";
        private const string EndSuccessPanelName = "EndGameUI";
        private const string EndFailPanelName = "EndGameFailUI";

        private const string ShopTitle = "POINT SHOP";

        private const float PanelSideMargin = 40f;
        private const float HeaderHeight = 180f;
        private const float HeaderTop = 30f;
        private const float PointsPillTop = 250f;
        private const float PointsPillHeight = 96f;
        private const float ScrollTop = 400f;
        private const float ScrollBottom = 50f;
        private const float RowHeight = 200f;
        private const float RowSpacing = 18f;
        private const int RowPadding = 20;

        /// <summary>Màn 2340px cao nên đẩy HUD xuống dưới notch.</summary>
        private const float HudTopMargin = 120f;

        // hệ số bo góc: bán kính ≈ 34 / multiplier
        private const float RadiusRow = 0.75f;
        private const float RadiusHeader = 0.85f;
        private const float RadiusPill = 0.70f;
        private const float RadiusBuy = 0.70f;
        private const float RadiusBack = 1.30f;

        private static Color Hex(uint rgb, float a)
        {
            return new Color(((rgb >> 16) & 0xFF) / 255f, ((rgb >> 8) & 0xFF) / 255f, (rgb & 0xFF) / 255f, a);
        }

        private static readonly Color RowTop = Hex(0x323B78, 0.94f);
        private static readonly Color RowBottom = Hex(0x1E2449, 0.94f);
        private static readonly Color HeaderTopCol = Hex(0x3E4A9C, 1f);
        private static readonly Color HeaderBottomCol = Hex(0x242B60, 1f);
        private static readonly Color PillTop = Hex(0x232A55, 0.95f);
        private static readonly Color PillBottom = Hex(0x161A36, 0.95f);
        private static readonly Color BuyTop = Hex(0x4BE38C, 1f);
        private static readonly Color BuyBottom = Hex(0x1C9E62, 1f);
        private static readonly Color BackTop = Hex(0x323B78, 1f);
        private static readonly Color BackBottom = Hex(0x1E2449, 1f);

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

        // Số điểm phải khớp IAPManager.GetPointsForProduct. Giá thật đặt trên Google Play Console.
        private static readonly Pack[] Packs =
        {
            new Pack("iap1", 100,  "Starter Stack", "iv_gold1", "$0.50"),
            new Pack("iap2", 200,  "Small Stack",   "iv_gold2", "$1"),
            new Pack("iap3", 400,  "Block Stack",   "iv_gold3", "$2"),
            new Pack("iap4", 600,  "Big Stack",     "iv_gold4", "$3"),
            new Pack("iap5", 1000, "Mega Stack",    "iv_gold5", "$5"),
            new Pack("iap6", 2000, "Turbo Stack",   "iv_gold4", "$7"),
            new Pack("iap7", 5000, "Ultra Stack",   "iv_gold5", "$10"),
        };

        [MenuItem("Tools/Tetris/Build Shop Panel")]
        public static void BuildShopPanel()
        {
            if (!Build()) return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Build Shop Panel", "Đã dựng Shop Panel với " + Packs.Length + " gói điểm.", "OK");
        }

        /// <summary>Dựng shop, chưa lưu scene. Trả về false nếu thiếu điều kiện.</summary>
        public static bool Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Build Shop Panel", "Mở scene Assets/Scenes/game.unity trước đã.", "OK");
                return false;
            }

            GameObject canvasRoot = U.FindRootObject(scene, CanvasName);
            if (canvasRoot == null)
            {
                Debug.LogError("[ShopBuilder] Không tìm thấy GameObject '" + CanvasName + "' trong scene.");
                return false;
            }

            UIController_tetris uiController = canvasRoot.GetComponent<UIController_tetris>();
            if (uiController == null)
            {
                Debug.LogError("[ShopBuilder] GameObject '" + CanvasName + "' không có UIController_tetris.");
                return false;
            }

            // chỉ còn dùng icon vàng + icon coin của bộ IAP gốc
            if (!U.EnsureSprites("Build Shop Panel",
                    "coin", "iv_gold1", "iv_gold2", "iv_gold3", "iv_gold4", "iv_gold5"))
            {
                return false;
            }

            Sprite rounded = AssetDatabase.LoadAssetAtPath<Sprite>(UIArtGenerator.RoundedPath);
            Sprite shopBg = AssetDatabase.LoadAssetAtPath<Sprite>(UIArtGenerator.HomeBgPath);
            Sprite arrow = AssetDatabase.LoadAssetAtPath<Sprite>(UIArtGenerator.ArrowLeftPath);
            if (rounded == null || shopBg == null || arrow == null)
            {
                Debug.LogError("[ShopBuilder] Thiếu art. Chạy Tools/Tetris/Generate UI Art trước.");
                return false;
            }

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Rubik-Regular SDF.asset");
            if (font == null) font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                Debug.LogError("[ShopBuilder] Chưa có TMP font asset. Window > TextMeshPro > Import TMP Essential Resources.");
                return false;
            }

            Transform canvasTransform = canvasRoot.transform;

            U.DestroyIfExists(canvasTransform, ShopPanelName);
            U.DestroyIfExists(canvasTransform, ShopButtonName);
            U.DestroyIfExists(canvasTransform, PointsHudName);

            // Bộ đếm điểm sống trên Canvas cùng UIController_tetris.
            PointsScorer_tetris scorer = canvasRoot.GetComponent<PointsScorer_tetris>();
            if (scorer == null) scorer = canvasRoot.AddComponent<PointsScorer_tetris>();

            float contentWidth = U.RefWidth - PanelSideMargin * 2f; // 1000

            // ---------- HUD điểm + nút mở Shop (luôn hiện trong lúc chơi) ----------

            RectTransform hud = U.NewUI(PointsHudName, canvasTransform);
            U.Place(hud, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -HudTopMargin), new Vector2(320f, 88f));

            RectTransform hudIcon = U.NewUI("Icon", hud);
            U.Place(hudIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(72f, 72f));
            U.AddImage(hudIcon, "coin", false, true);

            RectTransform hudText = U.NewUI(PointsLabelName, hud);
            U.Place(hudText, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, 0f), new Vector2(216f, 72f));
            TextMeshProUGUI hudLabel = U.AddText(hudText, font, "0", 48f, TextAlignmentOptions.Left, U.GoldText);
            hudLabel.fontStyle = FontStyles.Bold;

            PointsHUD pointsHud = hud.gameObject.AddComponent<PointsHUD>();
            U.SetObjectField(pointsHud, "m_PointsText", hudLabel);

            RectTransform shopBtn = U.NewUI(ShopButtonName, canvasTransform);
            U.Place(shopBtn, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -HudTopMargin), new Vector2(120f, 120f));
            Button shopBtnButton = U.IconButton(shopBtn, "iv_shop");

            // ---------- Shop Panel ----------

            RectTransform shopRoot = U.NewUI(ShopPanelName, canvasTransform);
            U.Stretch(shopRoot);

            ShopPanel_tetris shopPanel = shopRoot.gameObject.AddComponent<ShopPanel_tetris>();
            U.SetObjectField(shopPanel, "m_PointsHud", hud.gameObject);
            U.SetObjectField(shopPanel, "m_ShopButton", shopBtn.gameObject);

            UnityEventTools.AddVoidPersistentListener(shopBtnButton.onClick, shopPanel.Open);

            RectTransform panel = U.NewUI("Panel", shopRoot);
            U.Stretch(panel);
            Image panelBg = U.AddImage(panel, null, true, false); // raycast on => chặn click xuyên xuống gameplay
            panelBg.sprite = shopBg;
            panelBg.color = new Color(0.78f, 0.78f, 0.88f, 1f);

            // Header: thanh bo góc + tiêu đề + nút back
            RectTransform header = U.TopCenter("Header", panel, HeaderTop, contentWidth, HeaderHeight);
            RoundedFill(header, rounded, HeaderTopCol, HeaderBottomCol, RadiusHeader, false);

            RectTransform headerTitle = U.NewUI("Title", header);
            U.Stretch(headerTitle);
            TextMeshProUGUI titleLabel = U.AddText(headerTitle, font, ShopTitle, 62f, TextAlignmentOptions.Center, Color.white);
            titleLabel.fontStyle = FontStyles.Bold;
            titleLabel.characterSpacing = 4f;

            RectTransform back = U.NewUI("Back", header);
            U.Place(back, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(96f, 96f));
            Image backBg = RoundedFill(back, rounded, BackTop, BackBottom, RadiusBack, true);
            Button backButton = U.AddButton(back, backBg);
            ApplyButtonColors(backButton);

            RectTransform backIcon = U.NewUI("Icon", back);
            U.Place(backIcon, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(48f, 48f));
            Image backArrow = backIcon.gameObject.AddComponent<Image>();
            backArrow.sprite = arrow;
            backArrow.preserveAspect = true;
            backArrow.raycastTarget = false;

            UnityEventTools.AddVoidPersistentListener(backButton.onClick, shopPanel.Close);

            // Pill số điểm: icon + số, canh giữa
            RectTransform pointsPill = U.TopCenter("Points Pill", panel, PointsPillTop, 420f, PointsPillHeight);
            RoundedFill(pointsPill, rounded, PillTop, PillBottom, RadiusPill, false);

            RectTransform pillIcon = U.NewUI("Icon", pointsPill);
            U.Place(pillIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(64f, 64f));
            U.AddImage(pillIcon, "coin", false, true);

            RectTransform pillValue = U.NewUI("Value", pointsPill);
            U.Place(pillValue, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(120f, 0f), new Vector2(260f, 70f));
            TextMeshProUGUI pillLabel = U.AddText(pillValue, font, "0", 48f, TextAlignmentOptions.Left, U.GoldText);
            pillLabel.fontStyle = FontStyles.Bold;
            U.SetObjectField(shopPanel, "m_PointsText", pillLabel);

            // Scroll view
            float scrollHeight = U.RefHeight - ScrollTop - ScrollBottom;
            float scrollCenterFromBottom = U.RefHeight - ScrollTop - scrollHeight * 0.5f;

            RectTransform scroll = U.NewUI("Scroll View", panel);
            scroll.anchorMin = new Vector2(0.5f, 0f);
            scroll.anchorMax = new Vector2(0.5f, 1f);
            scroll.pivot = new Vector2(0.5f, 0.5f);
            scroll.sizeDelta = new Vector2(contentWidth, -(ScrollTop + ScrollBottom));
            scroll.anchoredPosition = new Vector2(0f, scrollCenterFromBottom - U.RefHeight * 0.5f);

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

            // ---------- Ô text điểm trên 3 panel có sẵn ----------

            TextMeshProUGUI startPoints = AddPanelPointsLabel(canvasTransform, StartPanelName, font);
            TextMeshProUGUI successPoints = AddPanelPointsLabel(canvasTransform, EndSuccessPanelName, font);
            TextMeshProUGUI failPoints = AddPanelPointsLabel(canvasTransform, EndFailPanelName, font);

            U.SetObjectField(uiController, "scorer", scorer);
            U.SetObjectField(uiController, "startPointsText", startPoints);
            U.SetObjectField(uiController, "endSuccessPointsText", successPoints);
            U.SetObjectField(uiController, "endFailPointsText", failPoints);

            float contentHeight = Packs.Length * RowHeight + (Packs.Length - 1) * RowSpacing + RowPadding * 2f;
            Debug.Log(string.Format(
                "[ShopBuilder] Đã dựng Shop Panel: {0} gói, viewport {1:0}px, content {2:0}px.",
                Packs.Length, scrollHeight, contentHeight), shopRoot.gameObject);

            return true;
        }

        // ---------------------------------------------------------------- helpers

        /// <summary>Nền bo góc 9-slice + gradient dọc.</summary>
        private static Image RoundedFill(RectTransform rect, Sprite rounded, Color top, Color bottom,
            float radiusMultiplier, bool raycastTarget)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = rounded;
            image.type = Image.Type.Sliced;
            image.pixelsPerUnitMultiplier = radiusMultiplier;
            image.color = Color.white;
            image.raycastTarget = raycastTarget;

            UIGradient gradient = rect.gameObject.AddComponent<UIGradient>();
            gradient.SetColors(top, bottom);

            return image;
        }

        private static void ApplyButtonColors(Button button)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }

        /// <summary>
        /// Thêm (hoặc dựng lại) một dòng text điểm vào panel có sẵn, ở khoảng trống
        /// giữa tiêu đề (neo y 0.70–0.80) và nút đầu tiên (neo y 0.50–0.60).
        /// </summary>
        private static TextMeshProUGUI AddPanelPointsLabel(Transform canvasTransform, string panelName, TMP_FontAsset font)
        {
            Transform panel = canvasTransform.Find(panelName);
            if (panel == null)
            {
                Debug.LogWarning("[ShopBuilder] Không tìm thấy panel '" + panelName + "' trong Canvas.");
                return null;
            }

            U.DestroyIfExists(panel, PointsLabelName);

            RectTransform rect = U.NewUI(PointsLabelName, panel);
            rect.anchorMin = new Vector2(0.1f, 0.61f);
            rect.anchorMax = new Vector2(0.9f, 0.68f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            return U.AddText(rect, font, "Points: 0", 46f, TextAlignmentOptions.Center, U.GoldText);
        }

        private static void BuildPackRow(RectTransform parent, TMP_FontAsset font, Sprite rounded,
            Pack pack, int index, ShopPanel_tetris shopPanel)
        {
            RectTransform row = U.NewUI("Inapp" + index, parent);
            row.sizeDelta = new Vector2(0f, RowHeight);
            RoundedFill(row, rounded, RowTop, RowBottom, RadiusRow, true);

            LayoutElement element = row.gameObject.AddComponent<LayoutElement>();
            element.minHeight = RowHeight;
            element.preferredHeight = RowHeight;

            // Bố cục ngang trong hàng rộng 960:
            //   Gold 30..180 | Tên + số điểm 200..520 | Giá ..-220 | Buy ..-24

            RectTransform gold = U.NewUI("Gold", row);
            U.Place(gold, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(150f, 150f));
            U.AddImage(gold, pack.GoldSprite, false, true);

            RectTransform title = U.NewUI("Title", row);
            U.Place(title, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(200f, 26f), new Vector2(330f, 60f));
            TextMeshProUGUI titleLabel = U.AddText(title, font, pack.DisplayName, 42f, TextAlignmentOptions.Left, Color.white);
            titleLabel.fontStyle = FontStyles.Bold;

            RectTransform amount = U.NewUI("Amount", row);
            U.Place(amount, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(200f, -30f), new Vector2(330f, 52f));
            U.AddText(amount, font, pack.Points + " Points", 36f, TextAlignmentOptions.Left, U.GoldText);

            // Giá: TMP + Button + IAPButton (giá tự cập nhật sau khi IAP init)
            RectTransform priceRect = U.NewUI("TextIap", row);
            U.Place(priceRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-224f, 0f), new Vector2(150f, 70f));
            TextMeshProUGUI priceLabel = U.AddText(priceRect, font, pack.PricePlaceholder, 40f,
                TextAlignmentOptions.Right, U.GoldText);
            priceLabel.fontStyle = FontStyles.Bold;
            priceLabel.raycastTarget = true;
            U.AddButton(priceRect, priceLabel);

            IAPButton iapButton = priceRect.gameObject.AddComponent<IAPButton>();
            iapButton.productId = pack.ProductId;
            iapButton.priceText = priceLabel;

            // Nút Buy: bo góc + gradient xanh, chữ BUY
            RectTransform buy = U.NewUI("Buy", row);
            U.Place(buy, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(180f, 96f));
            Image buyBg = RoundedFill(buy, rounded, BuyTop, BuyBottom, RadiusBuy, true);
            Button buyButton = U.AddButton(buy, buyBg);
            ApplyButtonColors(buyButton);

            RectTransform buyLabelRect = U.NewUI("Label", buy);
            U.Stretch(buyLabelRect);
            TextMeshProUGUI buyLabel = U.AddText(buyLabelRect, font, "BUY", 40f, TextAlignmentOptions.Center, Color.white);
            buyLabel.fontStyle = FontStyles.Bold;
            buyLabel.characterSpacing = 2f;

            UnityAction buyAction = GetBuyAction(shopPanel, index);
            if (buyAction != null)
            {
                UnityEventTools.AddVoidPersistentListener(buyButton.onClick, buyAction);
            }
        }

        private static UnityAction GetBuyAction(ShopPanel_tetris shop, int index)
        {
            switch (index)
            {
                case 1: return shop.BuyPoints100;
                case 2: return shop.BuyPoints200;
                case 3: return shop.BuyPoints400;
                case 4: return shop.BuyPoints600;
                case 5: return shop.BuyPoints1000;
                case 6: return shop.BuyPoints2000;
                case 7: return shop.BuyPoints5000;
            }

            return null;
        }
    }
}
