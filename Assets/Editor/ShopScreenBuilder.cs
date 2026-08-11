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
    /// Dựng màn Shop IAP (7 gói iap1–iap7) vào Canvas của scene hiện tại,
    /// bằng bộ sprite copy từ moneyrace-skybranch. Chạy lại được nhiều lần (idempotent).
    /// </summary>
    public static class ShopScreenBuilder
    {
        private const string ShopRootName = "Shop";
        private const string ShopButtonName = "Shop Button";
        private const string CoinHudName = "Coin HUD";

        private const float PanelSideMargin = 40f;          // lề trái/phải của nội dung shop
        private const float HeaderHeight = 180f;
        private const float HeaderTop = 30f;
        private const float CoinsPillTop = 250f;
        private const float CoinsPillHeight = 96f;
        private const float ScrollTop = 400f;               // tính từ mép trên panel
        private const float ScrollBottom = 50f;
        private const float RowHeight = 200f;
        private const float RowSpacing = 18f;
        private const int RowPadding = 20;

        /// <summary>
        /// Ảnh nền thanh header. Mặc định dùng <c>bg_button_iap</c> (thanh bo góc, 1328x352).
        /// <para>
        /// KHÔNG dùng <c>header_iap</c> làm header full-width: ảnh đó 1472x992 (tỉ lệ 1.48),
        /// ép xuống một thanh ngang sẽ bẹp dúm, và nó đã in sẵn chữ "SHOP" + tên game khác
        /// ("SKY BRANCH RUN") nên sẽ chồng chữ và sai thương hiệu.
        /// Nếu sau này có banner riêng đúng tỉ lệ ngang, đổi hằng này và tắt ShowHeaderTitle.
        /// </para>
        /// </summary>
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

        [MenuItem("Tools/TapTap/Build Shop Screen")]
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
                EditorUtility.DisplayDialog("Build Shop Screen", "Mở scene Game.unity trước đã.", "OK");
                return false;
            }

            GameObject canvasGo = U.FindRootObject(scene, U.CanvasName);
            if (canvasGo == null)
            {
                Debug.LogError("[ShopBuilder] Không tìm thấy GameObject '" + U.CanvasName + "' trong scene.");
                return false;
            }

            if (!U.EnsureSprites("Build Shop Screen",
                    "bg_iap", "bg_button_iap", "iv_buy", "iv_back", "iv_shop", "coin",
                    "iv_gold1", "iv_gold2", "iv_gold3", "iv_gold4", "iv_gold5", HeaderSprite))
            {
                return false;
            }

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                Debug.LogError("[ShopBuilder] Chưa có TMP default font asset. Window > TextMeshPro > Import TMP Essential Resources.");
                return false;
            }

            Transform canvas = canvasGo.transform;

            U.DestroyIfExists(canvas, ShopRootName);
            U.DestroyIfExists(canvas, ShopButtonName);
            U.DestroyIfExists(canvas, CoinHudName);

            float contentWidth = U.RefWidth - PanelSideMargin * 2f; // 1000

            // ---------- HUD: ví coin (góc trên trái, ngoài shop) ----------

            RectTransform hud = U.NewUI(CoinHudName, canvas);
            U.Place(hud, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(320f, 88f));

            RectTransform hudIcon = U.NewUI("Icon", hud);
            U.Place(hudIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(72f, 72f));
            U.AddImage(hudIcon, "coin", false, true);

            RectTransform hudText = U.NewUI("Coins Text", hud);
            U.Place(hudText, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, 0f), new Vector2(216f, 72f));
            TextMeshProUGUI hudLabel = U.AddText(hudText, font, "{0}", 48f, TextAlignmentOptions.Left, U.GoldText);

            CoinHUD coinHud = hud.gameObject.AddComponent<CoinHUD>();
            U.SetEntityName(coinHud, "Coin HUD");
            U.SetObjectField(coinHud, "m_CoinsText", hudLabel);

            // ---------- Nút mở shop (góc trên phải, chỉ hiện ở menu) ----------

            RectTransform shopBtn = U.NewUI(ShopButtonName, canvas);
            U.Place(shopBtn, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(120f, 120f));
            Button shopBtnButton = U.IconButton(shopBtn, "iv_shop");

            // ---------- Shop: node luôn active, giữ component ShopScreen ----------

            RectTransform shopRoot = U.NewUI(ShopRootName, canvas);
            U.Stretch(shopRoot);
            ShopScreen shopScreen = shopRoot.gameObject.AddComponent<ShopScreen>();
            U.SetEntityName(shopScreen, "Shop Screen");

            // ---------- Panel shop (thứ được bật/tắt) ----------

            RectTransform panel = U.NewUI("Shop Panel", shopRoot);
            U.Stretch(panel);
            U.AddImage(panel, "bg_iap", true, false); // raycast on => chặn click xuyên xuống gameplay

            // Header: thanh ngang bo góc + tiêu đề + nút back
            RectTransform header = U.TopCenter("Header", panel, HeaderTop, contentWidth, HeaderHeight);
            U.AddImage(header, HeaderSprite, false, false);

            if (ShowHeaderTitle)
            {
                RectTransform headerTitle = U.NewUI("Title", header);
                U.Stretch(headerTitle);
                U.AddText(headerTitle, font, "SHOP", 68f, TextAlignmentOptions.Center, Color.white);
            }

            RectTransform back = U.NewUI("Back", header);
            U.Place(back, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(88f, 88f));
            Button backButton = U.IconButton(back, "iv_back");

            // Pill số coin: icon + số, canh giữa
            RectTransform coinsPill = U.TopCenter("Coins Pill", panel, CoinsPillTop, 420f, CoinsPillHeight);
            U.AddImage(coinsPill, "bg_button_iap", false, false);

            RectTransform coinsIcon = U.NewUI("Icon", coinsPill);
            U.Place(coinsIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(64f, 64f));
            U.AddImage(coinsIcon, "coin", false, true);

            RectTransform coinsValue = U.NewUI("Value", coinsPill);
            U.Place(coinsValue, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(120f, 0f), new Vector2(260f, 70f));
            TextMeshProUGUI coinsLabelText = U.AddText(coinsValue, font, "{0}", 48f, TextAlignmentOptions.Left, U.GoldText);

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

            // ---------- 7 hàng gói coin ----------

            for (int i = 0; i < Packs.Length; i++)
            {
                BuildPackRow(content, font, Packs[i], i + 1, shopScreen);
            }

            // ---------- Nối serialized field + sự kiện ----------

            U.SetObjectField(shopScreen, "m_ShopPanel", panel.gameObject);
            U.SetObjectField(shopScreen, "m_ShopButton", shopBtn.gameObject);
            U.SetObjectField(shopScreen, "m_CoinsText", coinsLabelText);

            UnityEventTools.AddVoidPersistentListener(shopBtnButton.onClick, shopScreen.ShowShopScreen);
            UnityEventTools.AddVoidPersistentListener(backButton.onClick, shopScreen.CloseShopScreen);

            panel.gameObject.SetActive(false);
            shopRoot.SetAsLastSibling();

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
