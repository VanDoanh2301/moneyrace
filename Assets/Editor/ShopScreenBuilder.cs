using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;

using U = RingGameEditor.UIBuildUtil;

namespace RingGameEditor
{
    /// <summary>
    /// Dựng màn Shop IAP (7 gói iap1–iap7) vào Canvas của scene hiện tại,
    /// bằng bộ sprite copy từ moneyrace-skybranch/TapTapGame. Chạy lại được nhiều lần (idempotent).
    /// </summary>
    public static class ShopScreenBuilder
    {
        private const string ShopRootName = "Shop";

        // Các số đo dưới đây tuned cho canvas tham chiếu 1080x1920 (portrait). Ở Build(), chúng
        // được nhân với hScale/vScale đọc từ CanvasScaler thật của scene, để layout không vỡ nếu
        // Canvas trong scene đang dùng reference resolution khác (RingGame mặc định 800x600).
        private const float RefW = 1080f;
        private const float RefH = 1920f;

        private const float PanelSideMargin = 40f;          // lề trái/phải của nội dung shop
        private const float HeaderHeight = 180f;
        private const float HeaderTop = 30f;
        private const float CoinsPillTop = 250f;
        private const float CoinsPillHeight = 96f;
        private const float CoinsPillWidth = 420f;
        private const float ScrollTop = 400f;               // tính từ mép trên panel
        private const float ScrollBottom = 50f;
        private const float RowHeight = 200f;
        private const float RowSpacing = 18f;
        private const int RowPadding = 20;

        /// <summary>Ảnh nền thanh header (thanh bo góc, không dùng header_iap vì tỉ lệ/chữ in sẵn không hợp).</summary>
        private const string HeaderSprite = "bg_button_iap";

        /// <summary>Đặt false nếu ảnh header đã in sẵn chữ tiêu đề.</summary>
        private const bool ShowHeaderTitle = true;

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

        // Số coin phải khớp IAPManager.GetCoinsForProduct.
        // Giá thật đặt trên Google Play Console; ở đây chỉ là chữ hiển thị tạm.
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

        [MenuItem("Tools/RingGame/Build Shop Screen")]
        public static void BuildShopScreen()
        {
            if (!Build()) return;

            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            EditorUtility.DisplayDialog("Build Shop Screen", "Đã dựng Shop Screen với " + Packs.Length + " gói coin.", "OK");
        }

        /// <summary>Dựng shop, chưa lưu scene. Trả về false nếu thiếu điều kiện.</summary>
        internal static bool Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Build Shop Screen", "Mở scene Menu.unity trước đã.", "OK");
                return false;
            }

            GameObject canvasGo = U.FindRootObject(scene, U.CanvasName);
            if (canvasGo == null)
            {
                Debug.LogError("[ShopBuilder] Không tìm thấy GameObject '" + U.CanvasName + "' trong scene.");
                return false;
            }

            if (!U.EnsureSprites("Build Shop Screen",
                    "Back New", "bg_button_iap", "iv_buy", "iv_back", "coin",
                    "iv_gold1", "iv_gold2", "iv_gold3", "iv_gold4", "iv_gold5", HeaderSprite))
            {
                return false;
            }

            Font font = U.LoadDefaultFont();
            if (font == null)
            {
                Debug.LogError("[ShopBuilder] Không tìm được font để dựng UI.");
                return false;
            }

            Vector2 refRes = U.GetReferenceResolution(canvasGo);
            float hScale = refRes.x / RefW;
            float vScale = refRes.y / RefH;

            Transform canvas = canvasGo.transform;

            U.DestroyIfExists(canvas, ShopRootName);

            float contentWidth = refRes.x - PanelSideMargin * hScale * 2f;

            // Coin HUD không còn dựng ở đây — MenuButtonsBuilder dựng nó làm con của
            // Bottom Bar/Shop (hiện coin ngay dưới icon Shop) sau khi builder này chạy xong.

            // ---------- Shop: node luôn active, giữ component ShopScreen ----------
            // Nút mở shop (m_ShopButton) không còn dựng ở đây — MenuButtonsBuilder sẽ tạo nút
            // "Shop" trong Bottom Bar và tự gán vào field này sau khi builder này chạy xong.

            RectTransform shopRoot = U.NewUI(ShopRootName, canvas);
            U.Stretch(shopRoot);
            ShopScreen shopScreen = shopRoot.gameObject.AddComponent<ShopScreen>();

            // ---------- Panel shop (thứ được bật/tắt) ----------

            RectTransform panel = U.NewUI("Shop Panel", shopRoot);
            U.Stretch(panel);
            U.AddImage(panel, "Back New", true, false); // raycast on => chặn click xuyên xuống gameplay

            // Header: thanh ngang bo góc + tiêu đề + nút back
            RectTransform header = U.TopCenter("Header", panel, HeaderTop * vScale, contentWidth, HeaderHeight * vScale);
            U.AddImage(header, HeaderSprite, false, false);

            if (ShowHeaderTitle)
            {
                RectTransform headerTitle = U.NewUI("Title", header);
                U.Stretch(headerTitle);
                U.AddText(headerTitle, font, "SHOP", Mathf.RoundToInt(68f * vScale), TextAnchor.MiddleCenter, Color.white);
            }

            RectTransform back = U.NewUI("Back", header);
            U.Place(back, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(36f * hScale, 0f), new Vector2(88f * vScale, 88f * vScale));
            Button backButton = U.IconButton(back, "iv_back");

            // Pill số coin: icon + số, canh giữa
            RectTransform coinsPill = U.TopCenter("Coins Pill", panel, CoinsPillTop * vScale, CoinsPillWidth * hScale, CoinsPillHeight * vScale);
            U.AddImage(coinsPill, "bg_button_iap", false, false);

            RectTransform coinsIcon = U.NewUI("Icon", coinsPill);
            U.Place(coinsIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(40f * hScale, 0f), new Vector2(64f * vScale, 64f * vScale));
            U.AddImage(coinsIcon, "coin", false, true);

            RectTransform coinsValue = U.NewUI("Value", coinsPill);
            U.Place(coinsValue, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(120f * hScale, 0f), new Vector2(260f * hScale, 70f * vScale));
            Text coinsLabelText = U.AddText(coinsValue, font, "{0}", Mathf.RoundToInt(48f * vScale), TextAnchor.MiddleLeft, U.GoldText);

            // Scroll view
            float scrollTopPx = ScrollTop * vScale;
            float scrollBottomPx = ScrollBottom * vScale;
            float scrollHeight = refRes.y - scrollTopPx - scrollBottomPx;
            float scrollCenterFromBottom = refRes.y - scrollTopPx - scrollHeight * 0.5f;

            RectTransform scroll = U.NewUI("Scroll View", panel);
            scroll.anchorMin = new Vector2(0.5f, 0f);
            scroll.anchorMax = new Vector2(0.5f, 1f);
            scroll.pivot = new Vector2(0.5f, 0.5f);
            scroll.sizeDelta = new Vector2(contentWidth, -(scrollTopPx + scrollBottomPx));
            scroll.anchoredPosition = new Vector2(0f, scrollCenterFromBottom - refRes.y * 0.5f);

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

            float rowSpacingPx = RowSpacing * vScale;
            int rowPaddingPx = Mathf.RoundToInt(RowPadding * vScale);

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = rowSpacingPx;
            layout.padding = new RectOffset(rowPaddingPx, rowPaddingPx, rowPaddingPx, rowPaddingPx);
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

            // ---------- 7 hàng gói coin ----------

            float rowHeightPx = RowHeight * vScale;

            for (int i = 0; i < Packs.Length; i++)
            {
                BuildPackRow(content, font, Packs[i], i + 1, shopScreen, rowHeightPx, hScale, vScale);
            }

            // ---------- Nối serialized field + sự kiện ----------

            U.SetObjectField(shopScreen, "m_ShopPanel", panel.gameObject);
            U.SetObjectField(shopScreen, "m_CoinsText", coinsLabelText);

            UnityEventTools.AddVoidPersistentListener(backButton.onClick, shopScreen.CloseShopScreen);

            panel.gameObject.SetActive(false);
            shopRoot.SetAsLastSibling();

            float contentHeight = Packs.Length * rowHeightPx + (Packs.Length - 1) * rowSpacingPx + rowPaddingPx * 2f;
            Debug.Log(string.Format(
                "[ShopBuilder] Đã dựng Shop Screen: {0} gói, canvas ref {1}x{2}, viewport {3:0}px, content {4:0}px.",
                Packs.Length, refRes.x, refRes.y, scrollHeight, contentHeight), shopRoot.gameObject);

            return true;
        }

        private static void BuildPackRow(RectTransform parent, Font font, Pack pack, int index, ShopScreen shopScreen,
            float rowHeightPx, float hScale, float vScale)
        {
            RectTransform row = U.NewUI("Inapp" + index, parent);
            row.sizeDelta = new Vector2(0f, rowHeightPx);
            U.AddImage(row, "bg_button_iap", true, false);

            LayoutElement element = row.gameObject.AddComponent<LayoutElement>();
            element.minHeight = rowHeightPx;
            element.preferredHeight = rowHeightPx;

            // Bố cục ngang trong hàng (x/width theo hScale, y/height theo vScale):
            //   Gold 30..180 | Title 210..550 | Price 570..750 | Buy 766..934

            RectTransform gold = U.NewUI("Gold", row);
            U.Place(gold, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(30f * hScale, 0f), new Vector2(150f * vScale, 150f * vScale));
            U.AddImage(gold, pack.GoldSprite, false, true);

            RectTransform title = U.NewUI("Title", row);
            U.Place(title, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(210f * hScale, 0f), new Vector2(340f * hScale, 90f * vScale));
            U.AddText(title, font, pack.Coins + " Coins", Mathf.RoundToInt(50f * vScale), TextAnchor.MiddleLeft, Color.white);

            // Giá: Text + Button + IAPButton (giá tự cập nhật sau khi IAP init)
            RectTransform priceRect = U.NewUI("TextIap", row);
            U.Place(priceRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-210f * hScale, 0f), new Vector2(180f * hScale, 80f * vScale));
            Text priceLabel = U.AddText(priceRect, font, pack.PricePlaceholder, Mathf.RoundToInt(42f * vScale),
                TextAnchor.MiddleRight, U.GoldText);
            priceLabel.raycastTarget = true;
            U.AddButton(priceRect, priceLabel);

            IAPButton iapButton = priceRect.gameObject.AddComponent<IAPButton>();
            iapButton.productId = pack.ProductId;
            iapButton.priceText = priceLabel;

            // Nút Buy (iv_buy 270x144 => giữ tỉ lệ, preserveAspect=true tự canh giữa trong rect)
            RectTransform buy = U.NewUI("Buy", row);
            U.Place(buy, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(-26f * hScale, 0f), new Vector2(168f * hScale, 90f * vScale));
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
