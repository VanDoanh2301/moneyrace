using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;

using U = ClassifyEditor.UIBuildUtil;

namespace ClassifyEditor
{
    /// <summary>
    /// Dựng màn Shop IAP (7 gói iap1–iap7) làm một <see cref="UIScreen"/> mới bên trong
    /// GameObject "UI" của scene, cộng với nút mở Shop + Coin HUD trên MainMenuScreen.
    /// Port từ TapTapGame (ShopScreenBuilder.cs); chạy lại được nhiều lần (idempotent).
    /// </summary>
    public static class ShopScreenBuilder
    {
        private const string ShopScreenName = "ShopScreen";
        private const string ShopButtonName = "Shop Button";
        private const string CoinHudName = "Coin HUD";
        private const string MainMenuScreenName = "MainMenuScreen";

        private const float PanelSideMargin = 40f;
        private const float HeaderHeight = 180f;
        private const float HeaderTop = 30f;
        private const float CoinsPillTop = 250f;
        private const float CoinsPillHeight = 96f;
        private const float ScrollTop = 400f;
        private const float ScrollBottom = 50f;
        private const float RowHeight = 200f;
        private const float RowSpacing = 18f;
        private const int RowPadding = 20;

        /// <summary>Ảnh nền thanh header/hàng: thanh bo góc, tỉ lệ gần ngang (1328x352).</summary>
        private const string HeaderSprite = "bg_button_iap";

        private const string FontPath = "Assets/Classify/Fonts/Rubik-Regular SDF.asset";

        private struct Pack
        {
            public string ProductId;
            public int Coins;
            public string GoldSprite;
            /// <summary>Chỉ là chữ tạm hiện trong Editor; runtime bị IAPButton ghi đè bằng localizedPriceString.</summary>
            public string PricePlaceholder;

            public Pack(string productId, int coins, string goldSprite, string pricePlaceholder)
            {
                ProductId = productId;
                Coins = coins;
                GoldSprite = goldSprite;
                PricePlaceholder = pricePlaceholder;
            }
        }

        // Số coin phải khớp IAPManager.GetCoinsForProduct. Giá thật đặt trên Google Play Console.
        private static readonly Pack[] Packs =
        {
            new Pack("iap1", 100,  "iv_gold1", "$0.50"),
            new Pack("iap2", 200,  "iv_gold2", "$1"),
            new Pack("iap3", 400,  "iv_gold3", "$2"),
            new Pack("iap4", 600,  "iv_gold4", "$3"),
            new Pack("iap5", 1000, "iv_gold5", "$5"),
            new Pack("iap6", 2000, "iv_gold4", "$7"),
            new Pack("iap7", 5000, "iv_gold5", "$10"),
        };

        [MenuItem("Tools/Classify/Build Shop Screen")]
        public static void BuildShopScreen()
        {
            if (!Build()) return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Build Shop Screen", "Đã dựng Shop Screen với " + Packs.Length + " gói coin.", "OK");
        }

        /// <summary>Dựng shop, chưa lưu scene. Trả về false nếu thiếu điều kiện.</summary>
        public static bool Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Build Shop Screen", "Mở scene MainScene.unity trước đã.", "OK");
                return false;
            }

            GameObject uiRoot = U.FindRootObject(scene, U.UIRootName);
            if (uiRoot == null)
            {
                Debug.LogError("[ShopBuilder] Không tìm thấy GameObject '" + U.UIRootName + "' trong scene.");
                return false;
            }

            UIManager uiManager = uiRoot.GetComponent<UIManager>();
            if (uiManager == null)
            {
                Debug.LogError("[ShopBuilder] GameObject '" + U.UIRootName + "' không có UIManager.");
                return false;
            }

            Transform mainMenuTransform = uiRoot.transform.Find(MainMenuScreenName);
            if (mainMenuTransform == null)
            {
                Debug.LogError("[ShopBuilder] Không tìm thấy '" + MainMenuScreenName + "' trong '" + U.UIRootName + "'.");
                return false;
            }

            if (!U.EnsureSprites("Build Shop Screen",
                    "bg_iap", "bg_button_iap", "iv_buy", "iv_back", "iv_shop", "coin",
                    "iv_gold1", "iv_gold2", "iv_gold3", "iv_gold4", "iv_gold5", HeaderSprite))
            {
                return false;
            }

            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null) font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                Debug.LogError("[ShopBuilder] Chưa có TMP font asset. Window > TextMeshPro > Import TMP Essential Resources.");
                return false;
            }

            Transform uiTransform = uiRoot.transform;

            U.DestroyIfExists(uiTransform, ShopScreenName);
            U.DestroyIfExists(mainMenuTransform, ShopButtonName);
            U.DestroyIfExists(mainMenuTransform, CoinHudName);

            float contentWidth = U.RefWidth - PanelSideMargin * 2f; // 1000

            // ---------- Shop Screen: UIScreen mới, canvas riêng, tắt sẵn ----------

            RectTransform shopRoot = U.NewUI(ShopScreenName, uiTransform);
            U.Stretch(shopRoot);

            Canvas shopCanvas = shopRoot.gameObject.AddComponent<Canvas>();
            shopRoot.gameObject.AddComponent<GraphicRaycaster>();
            shopCanvas.enabled = false; // giống các UIScreen khác: chỉ MainMenu bật sẵn lúc build

            ShopScreen shopScreen = shopRoot.gameObject.AddComponent<ShopScreen>();
            shopScreen.screenId = Screens.Shop;
            U.SetObjectField(shopScreen, "canvas", shopCanvas);
            U.SetObjectField(shopScreen, "uiManager", uiManager);

            RectTransform panel = U.NewUI("Panel", shopRoot);
            U.Stretch(panel);
            U.AddImage(panel, "bg_iap", true, false); // raycast on => chặn click xuyên xuống gameplay/menu

            // Header: thanh ngang bo góc + tiêu đề + nút back
            RectTransform header = U.TopCenter("Header", panel, HeaderTop, contentWidth, HeaderHeight);
            U.AddImage(header, HeaderSprite, false, false);

            RectTransform headerTitle = U.NewUI("Title", header);
            U.Stretch(headerTitle);
            U.AddText(headerTitle, font, "SHOP", 68f, TextAlignmentOptions.Center, Color.white);

            RectTransform back = U.NewUI("Back", header);
            U.Place(back, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(88f, 88f));
            Button backButton = U.IconButton(back, "iv_back");
            UnityEventTools.AddVoidPersistentListener(backButton.onClick, shopScreen.CloseShopScreen);

            // Pill số coin: icon + số, canh giữa
            RectTransform coinsPill = U.TopCenter("Coins Pill", panel, CoinsPillTop, 420f, CoinsPillHeight);
            U.AddImage(coinsPill, "bg_button_iap", false, false);

            RectTransform coinsIcon = U.NewUI("Icon", coinsPill);
            U.Place(coinsIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(64f, 64f));
            U.AddImage(coinsIcon, "coin", false, true);

            RectTransform coinsValue = U.NewUI("Value", coinsPill);
            U.Place(coinsValue, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(120f, 0f), new Vector2(260f, 70f));
            TextMeshProUGUI coinsLabelText = U.AddText(coinsValue, font, "{0}", 48f, TextAlignmentOptions.Left, U.GoldText);
            U.SetObjectField(shopScreen, "m_CoinsText", coinsLabelText);

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
                BuildPackRow(content, font, Packs[i], i + 1, shopScreen);
            }

            shopRoot.SetAsLastSibling();

            // ---------- Coin HUD + nút mở Shop, đặt trên MainMenuScreen (tự ẩn hiện theo canvas MainMenu) ----------

            RectTransform hud = U.NewUI(CoinHudName, mainMenuTransform);
            U.Place(hud, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(320f, 88f));

            RectTransform hudIcon = U.NewUI("Icon", hud);
            U.Place(hudIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(72f, 72f));
            U.AddImage(hudIcon, "coin", false, true);

            RectTransform hudText = U.NewUI("Coins Text", hud);
            U.Place(hudText, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, 0f), new Vector2(216f, 72f));
            TextMeshProUGUI hudLabel = U.AddText(hudText, font, "{0}", 48f, TextAlignmentOptions.Left, U.GoldText);

            CoinHUD coinHud = hud.gameObject.AddComponent<CoinHUD>();
            U.SetObjectField(coinHud, "m_CoinsText", hudLabel);

            RectTransform shopBtn = U.NewUI(ShopButtonName, mainMenuTransform);
            U.Place(shopBtn, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(120f, 120f));
            Button shopBtnButton = U.IconButton(shopBtn, "iv_shop");
            UnityEventTools.AddVoidPersistentListener(shopBtnButton.onClick, shopScreen.ShowShopScreen);

            // ---------- Đăng ký Shop Screen vào UIManager.screens ----------

            U.AppendToObjectArrayField(uiManager, "screens", shopScreen);

            float contentHeight = Packs.Length * RowHeight + (Packs.Length - 1) * RowSpacing + RowPadding * 2f;
            Debug.Log(string.Format(
                "[ShopBuilder] Đã dựng Shop Screen: {0} gói, viewport {1:0}px, content {2:0}px.",
                Packs.Length, scrollHeight, contentHeight), shopRoot.gameObject);

            return true;
        }

        private static void BuildPackRow(RectTransform parent, TMP_FontAsset font, Pack pack, int index, ShopScreen shopScreen)
        {
            RectTransform row = U.NewUI("Inapp" + index, parent);
            row.sizeDelta = new Vector2(0f, RowHeight);
            U.AddImage(row, "bg_button_iap", true, false);

            LayoutElement element = row.gameObject.AddComponent<LayoutElement>();
            element.minHeight = RowHeight;
            element.preferredHeight = RowHeight;

            // Bố cục ngang trong hàng rộng 960:
            //   Gold 30..180 | Title 210..550 | Price 570..750 | Buy 766..934

            RectTransform gold = U.NewUI("Gold", row);
            U.Place(gold, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(150f, 150f));
            U.AddImage(gold, pack.GoldSprite, false, true);

            RectTransform title = U.NewUI("Title", row);
            U.Place(title, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(210f, 0f), new Vector2(340f, 90f));
            U.AddText(title, font, pack.Coins + " Coins", 50f, TextAlignmentOptions.Left, Color.white);

            // Giá: TMP + Button + IAPButton (giá tự cập nhật sau khi IAP init)
            RectTransform priceRect = U.NewUI("TextIap", row);
            U.Place(priceRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-210f, 0f), new Vector2(180f, 80f));
            TextMeshProUGUI priceLabel = U.AddText(priceRect, font, pack.PricePlaceholder, 42f,
                TextAlignmentOptions.Right, U.GoldText);
            priceLabel.raycastTarget = true;
            U.AddButton(priceRect, priceLabel);

            IAPButton iapButton = priceRect.gameObject.AddComponent<IAPButton>();
            iapButton.productId = pack.ProductId;
            iapButton.priceText = priceLabel;

            // Nút Buy (iv_buy 270x144 => giữ tỉ lệ ở 168x90)
            RectTransform buy = U.NewUI("Buy", row);
            U.Place(buy, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-26f, 0f), new Vector2(168f, 90f));
            Button buyButton = U.IconButton(buy, "iv_buy");

            UnityAction buyAction = GetBuyAction(shopScreen, index);
            if (buyAction != null)
            {
                UnityEventTools.AddVoidPersistentListener(buyButton.onClick, buyAction);
            }
        }

        private static UnityAction GetBuyAction(ShopScreen shop, int index)
        {
            switch (index)
            {
                case 1: return shop.BuyCoins100;
                case 2: return shop.BuyCoins200;
                case 3: return shop.BuyCoins400;
                case 4: return shop.BuyCoins600;
                case 5: return shop.BuyCoins1000;
                case 6: return shop.BuyCoins2000;
                case 7: return shop.BuyCoins5000;
            }

            return null;
        }
    }
}
