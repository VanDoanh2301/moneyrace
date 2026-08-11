using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using TMPro;
using TapTap;

namespace TapTapEditor
{
    /// <summary>
    /// Dựng màn Shop IAP (7 gói iap1–iap7) vào Canvas của scene hiện tại,
    /// bằng bộ sprite copy từ moneyrace-skybranch. Chạy lại được nhiều lần (idempotent).
    /// </summary>
    public static class ShopScreenBuilder
    {
        private const string SpriteFolder = "Assets/Sprites/";

        private const string CanvasName = "Canvas";

        private const string ShopRootName = "Shop";
        private const string ShopButtonName = "Shop Button";
        private const string CoinHudName = "Coin HUD";

        // Canvas tham chiếu 1080 x 1920.
        private const float RefWidth = 1080f;
        private const float RefHeight = 1920f;

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

        private static readonly Color GoldText = new Color(1f, 0.85f, 0.32f, 1f);

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
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Build Shop Screen", "Mở scene Game.unity trước đã.", "OK");
                return;
            }

            GameObject canvasGo = FindRootObject(scene, CanvasName);
            if (canvasGo == null)
            {
                Debug.LogError("[ShopBuilder] Không tìm thấy GameObject '" + CanvasName + "' trong scene.");
                EditorUtility.DisplayDialog("Build Shop Screen", "Không tìm thấy '" + CanvasName + "' trong scene.", "OK");
                return;
            }

            if (!EnsureSprites()) return;

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            if (font == null)
            {
                Debug.LogError("[ShopBuilder] Chưa có TMP default font asset. Vào Window > TextMeshPro > Import TMP Essential Resources.");
                EditorUtility.DisplayDialog("Build Shop Screen", "Thiếu TMP Essential Resources.", "OK");
                return;
            }

            Transform canvas = canvasGo.transform;

            DestroyIfExists(canvas, ShopRootName);
            DestroyIfExists(canvas, ShopButtonName);
            DestroyIfExists(canvas, CoinHudName);

            float contentWidth = RefWidth - PanelSideMargin * 2f; // 1000

            // ---------- HUD: ví coin (góc trên trái, ngoài shop) ----------

            RectTransform hud = NewUI(CoinHudName, canvas);
            Place(hud, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(320f, 88f));

            RectTransform hudIcon = NewUI("Icon", hud);
            Place(hudIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(72f, 72f));
            AddImage(hudIcon, "coin", false, true);

            RectTransform hudText = NewUI("Coins Text", hud);
            Place(hudText, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, 0f), new Vector2(216f, 72f));
            TextMeshProUGUI hudLabel = AddText(hudText, font, "{0}", 48f, TextAlignmentOptions.Left, GoldText);

            CoinHUD coinHud = hud.gameObject.AddComponent<CoinHUD>();
            SetEntityName(coinHud, "Coin HUD");
            SetObjectField(coinHud, "m_CoinsText", hudLabel);

            // ---------- Nút mở shop (góc trên phải) ----------

            RectTransform shopBtn = NewUI(ShopButtonName, canvas);
            Place(shopBtn, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(120f, 120f));
            Image shopBtnImage = AddImage(shopBtn, "iv_shop", true, true);
            Button shopBtnButton = AddButton(shopBtn, shopBtnImage);

            // ---------- Shop: node luôn active, giữ component ShopScreen ----------

            RectTransform shopRoot = NewUI(ShopRootName, canvas);
            Stretch(shopRoot);
            ShopScreen shopScreen = shopRoot.gameObject.AddComponent<ShopScreen>();
            SetEntityName(shopScreen, "Shop Screen");

            // ---------- Panel shop (thứ được bật/tắt) ----------

            RectTransform panel = NewUI("Shop Panel", shopRoot);
            Stretch(panel);
            AddImage(panel, "bg_iap", true, false); // raycast on => chặn click xuyên xuống gameplay

            // Header: thanh ngang bo góc + tiêu đề + nút back
            RectTransform header = NewUI("Header", panel);
            Place(header, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -HeaderTop), new Vector2(contentWidth, HeaderHeight));
            AddImage(header, HeaderSprite, false, false);

            if (ShowHeaderTitle)
            {
                RectTransform headerTitle = NewUI("Title", header);
                Stretch(headerTitle);
                AddText(headerTitle, font, "SHOP", 68f, TextAlignmentOptions.Center, Color.white);
            }

            RectTransform back = NewUI("Back", header);
            Place(back, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(36f, 0f), new Vector2(88f, 88f));
            Image backImage = AddImage(back, "iv_back", true, true);
            Button backButton = AddButton(back, backImage);

            // Pill số coin: icon + số, canh giữa
            RectTransform coinsPill = NewUI("Coins Pill", panel);
            Place(coinsPill, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0f, -CoinsPillTop), new Vector2(420f, CoinsPillHeight));
            AddImage(coinsPill, "bg_button_iap", false, false);

            RectTransform coinsIcon = NewUI("Icon", coinsPill);
            Place(coinsIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(64f, 64f));
            AddImage(coinsIcon, "coin", false, true);

            RectTransform coinsValue = NewUI("Value", coinsPill);
            Place(coinsValue, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(120f, 0f), new Vector2(260f, 70f));
            TextMeshProUGUI coinsLabelText = AddText(coinsValue, font, "{0}", 48f, TextAlignmentOptions.Left, GoldText);

            // Scroll view
            float scrollHeight = RefHeight - ScrollTop - ScrollBottom;
            float scrollCenterFromBottom = RefHeight - ScrollTop - scrollHeight * 0.5f;

            RectTransform scroll = NewUI("Scroll View", panel);
            scroll.anchorMin = new Vector2(0.5f, 0f);
            scroll.anchorMax = new Vector2(0.5f, 1f);
            scroll.pivot = new Vector2(0.5f, 0.5f);
            scroll.sizeDelta = new Vector2(contentWidth, -(ScrollTop + ScrollBottom));
            scroll.anchoredPosition = new Vector2(0f, scrollCenterFromBottom - RefHeight * 0.5f);

            ScrollRect scrollRect = scroll.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            scrollRect.scrollSensitivity = 40f;

            RectTransform viewport = NewUI("Viewport", scroll);
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = NewUI("Content", viewport);
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

            float rowWidth = contentWidth - RowPadding * 2f; // 960

            for (int i = 0; i < Packs.Length; i++)
            {
                BuildPackRow(content, font, Packs[i], i + 1, shopScreen, rowWidth);
            }

            // ---------- Nối serialized field + sự kiện ----------

            SetObjectField(shopScreen, "m_ShopPanel", panel.gameObject);
            SetObjectField(shopScreen, "m_ShopButton", shopBtn.gameObject);
            SetObjectField(shopScreen, "m_CoinsText", coinsLabelText);

            UnityEventTools.AddVoidPersistentListener(shopBtnButton.onClick, shopScreen.ShowShopScreen);
            UnityEventTools.AddVoidPersistentListener(backButton.onClick, shopScreen.CloseShopScreen);

            panel.gameObject.SetActive(false);
            shopRoot.SetAsLastSibling();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            float contentHeight = Packs.Length * RowHeight + (Packs.Length - 1) * RowSpacing + RowPadding * 2f;
            Debug.Log(string.Format(
                "[ShopBuilder] Đã dựng Shop Screen: {0} gói, viewport {1:0}px, content {2:0}px.",
                Packs.Length, scrollHeight, contentHeight), shopRoot.gameObject);

            EditorUtility.DisplayDialog("Build Shop Screen", "Đã dựng Shop Screen với " + Packs.Length + " gói coin.", "OK");
        }

        private static void BuildPackRow(RectTransform parent, TMP_FontAsset font, Pack pack, int index,
            ShopScreen shopScreen, float rowWidth)
        {
            RectTransform row = NewUI("Inapp" + index, parent);
            row.sizeDelta = new Vector2(0f, RowHeight);
            AddImage(row, "bg_button_iap", true, false);

            LayoutElement element = row.gameObject.AddComponent<LayoutElement>();
            element.minHeight = RowHeight;
            element.preferredHeight = RowHeight;

            // Bố cục ngang trong hàng rộng 960:
            //   Gold 30..180 | Title 210..550 | Price 570..750 | Buy 766..934

            RectTransform gold = NewUI("Gold", row);
            Place(gold, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(150f, 150f));
            AddImage(gold, pack.GoldSprite, false, true);

            RectTransform title = NewUI("Title", row);
            Place(title, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(210f, 0f), new Vector2(340f, 90f));
            AddText(title, font, pack.Coins + " Coins", 50f, TextAlignmentOptions.Left, Color.white);

            // Giá: TMP + Button + IAPButton (giá tự cập nhật sau khi IAP init)
            RectTransform priceRect = NewUI("TextIap", row);
            Place(priceRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-210f, 0f), new Vector2(180f, 80f));
            TextMeshProUGUI priceLabel = AddText(priceRect, font, pack.PricePlaceholder, 42f, TextAlignmentOptions.Right, GoldText);
            priceLabel.raycastTarget = true;
            AddButton(priceRect, priceLabel);

            IAPButton iapButton = priceRect.gameObject.AddComponent<IAPButton>();
            iapButton.productId = pack.ProductId;
            iapButton.priceText = priceLabel;

            // Nút Buy (iv_buy 270x144 => giữ tỉ lệ ở 168x90)
            RectTransform buy = NewUI("Buy", row);
            Place(buy, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-26f, 0f), new Vector2(168f, 90f));
            Image buyImage = AddImage(buy, "iv_buy", true, true);
            Button buyButton = AddButton(buy, buyImage);

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

        // ----- Helpers -----

        private static bool EnsureSprites()
        {
            string[] needed =
            {
                "bg_iap", "bg_button_iap", "iv_buy", "iv_back", "iv_shop", "coin",
                "iv_gold1", "iv_gold2", "iv_gold3", "iv_gold4", "iv_gold5", HeaderSprite
            };

            bool ok = true;

            for (int i = 0; i < needed.Length; i++)
            {
                if (LoadSprite(needed[i]) == null)
                {
                    Debug.LogError("[ShopBuilder] Thiếu sprite: " + SpriteFolder + needed[i]
                        + ".png (hoặc file chưa import ở Texture Type = Sprite (2D and UI)).");
                    ok = false;
                }
            }

            if (!ok)
            {
                EditorUtility.DisplayDialog("Build Shop Screen", "Thiếu sprite trong Assets/Sprites. Xem Console.", "OK");
            }

            return ok;
        }

        private static Sprite LoadSprite(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpriteFolder + name + ".png");
        }

        private static GameObject FindRootObject(UnityEngine.SceneManagement.Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == name) return root;
            }

            return null;
        }

        private static void DestroyIfExists(Transform parent, string name)
        {
            Transform found = parent.Find(name);
            if (found != null)
            {
                Object.DestroyImmediate(found.gameObject);
            }
        }

        private static RectTransform NewUI(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = parent.gameObject.layer;
            go.transform.SetParent(parent, false);

            return (RectTransform)go.transform;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>Đặt rect ở một điểm neo cố định. anchor = điểm neo, pivot quyết định gốc của anchoredPosition.</summary>
        private static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
        }

        private static Image AddImage(RectTransform rect, string spriteName, bool raycastTarget, bool preserveAspect)
        {
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = LoadSprite(spriteName);
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            image.raycastTarget = raycastTarget;

            return image;
        }

        private static TextMeshProUGUI AddText(RectTransform rect, TMP_FontAsset font, string text, float size,
            TextAlignmentOptions alignment, Color color)
        {
            TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.color = color;

            return label;
        }

        private static Button AddButton(RectTransform rect, Graphic target)
        {
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = target;
            button.transition = Selectable.Transition.ColorTint;

            return button;
        }

        /// <summary>m_EntityName là private + [ReadOnlyAttribute] nên phải set qua SerializedObject.</summary>
        private static void SetEntityName(Entity entity, string entityName)
        {
            SetStringField(entity, "m_EntityName", entityName);
        }

        private static void SetStringField(Object owner, string fieldName, string value)
        {
            SerializedObject so = new SerializedObject(owner);
            SerializedProperty property = so.FindProperty(fieldName);

            if (property == null)
            {
                Debug.LogWarning("[ShopBuilder] Không tìm thấy field '" + fieldName + "' trên " + owner.GetType().Name);
                return;
            }

            property.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetObjectField(Object owner, string fieldName, Object value)
        {
            SerializedObject so = new SerializedObject(owner);
            SerializedProperty property = so.FindProperty(fieldName);

            if (property == null)
            {
                Debug.LogWarning("[ShopBuilder] Không tìm thấy field '" + fieldName + "' trên " + owner.GetType().Name);
                return;
            }

            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
