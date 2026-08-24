using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;

using U = UIBuildUtil;

/// <summary>
/// Dựng màn Shop IAP (7 gói iap1–iap7) vào Canvas Main Menu,
/// dùng icon/sprite import từ moneyrace-hyper_jump. Chạy lại được nhiều lần (idempotent).
/// </summary>
public static class ShopScreenBuilder
{
    private const string ShopRootName = "Shop";
    /// <summary>Nút Shop hồng dưới Multi player (Panel/ShopButton) — không dùng icon góc.</summary>
    private const string ShopButtonName = "ShopButton";
    private const string LegacyIconShopButtonName = "Shop Button";
    private const string CoinHudName = "Coin HUD";
    private const string MenuPanelName = "Panel";

    private const float PanelSideMargin = 40f;
    private const float HeaderHeight = 160f;
    private const float HeaderTop = 30f;
    private const float CoinsPillTop = 220f;
    private const float CoinsPillHeight = 88f;
    private const float ScrollTop = 340f;
    private const float ScrollBottom = 40f;
    private const float RowHeight = 170f;
    private const float RowSpacing = 14f;
    private const int RowPadding = 16;
    private const bool ShowHeaderTitle = true;

    private struct Pack
    {
        public string ProductId;
        public int Coins;
        public string GoldSprite;
        public string PricePlaceholder;

        public Pack(string productId, int coins, string goldSprite, string pricePlaceholder)
        {
            ProductId = productId;
            Coins = coins;
            GoldSprite = goldSprite;
            PricePlaceholder = pricePlaceholder;
        }
    }

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

    [MenuItem("Tools/GraceGame/Build Shop Screen")]
    public static void BuildShopScreen()
    {
        if (!Build()) return;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        EditorUtility.DisplayDialog("Build Shop Screen", "Đã dựng Shop Screen với " + Packs.Length + " gói coin.", "OK");
    }

    public static bool Build()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            EditorUtility.DisplayDialog("Build Shop Screen", "Mở scene Main Menu.unity trước đã.", "OK");
            return false;
        }

        GameObject canvasGo = U.FindRootObject(scene, U.CanvasName);
        if (canvasGo == null)
        {
            Debug.LogError("[ShopBuilder] Không tìm thấy GameObject '" + U.CanvasName + "' trong scene.");
            return false;
        }

        Transform menuHost = canvasGo.transform.Find(MenuPanelName);
        if (menuHost == null)
        {
            menuHost = canvasGo.transform;
            Debug.LogWarning("[ShopBuilder] Không thấy '" + MenuPanelName + "', gắn Shop Button / Coin HUD trực tiếp lên Canvas.");
        }

        if (!U.EnsureSprites("Build Shop Screen",
                "iv_buy", "iv_back", "iv_money",
                "iv_gold1", "iv_gold2", "iv_gold3", "iv_gold4", "iv_gold5"))
        {
            return false;
        }

        if (U.LoadSpriteAt(U.ShopBackgroundPath) == null)
        {
            EditorUtility.DisplayDialog("Build Shop Screen", "Thiếu nền: " + U.ShopBackgroundPath, "OK");
            return false;
        }

        if (U.LoadSpriteAt(U.CapsuleSpritePath) == null)
        {
            EditorUtility.DisplayDialog("Build Shop Screen", "Thiếu capsule: " + U.CapsuleSpritePath, "OK");
            return false;
        }

        Font font = U.DefaultFont;
        if (font == null)
        {
            Debug.LogError("[ShopBuilder] Không tải được font LegacyRuntime.ttf builtin.");
            return false;
        }

        Transform canvas = canvasGo.transform;

        U.DestroyIfExists(canvas, ShopRootName);
        // Xóa icon shop góc (bản hyperjump), GIỮ nguyên Panel/ShopButton hồng
        U.DestroyIfExists(canvas, LegacyIconShopButtonName);
        U.DestroyIfExists(menuHost, LegacyIconShopButtonName);
        U.DestroyIfExists(menuHost, CoinHudName);
        U.DestroyIfExists(canvas, CoinHudName);

        var panel = canvas.Find(MenuPanelName);
        Transform shopBtnTransform = panel != null ? panel.Find(ShopButtonName) : null;
        if (shopBtnTransform == null)
            shopBtnTransform = canvas.Find(ShopButtonName);

        if (shopBtnTransform == null)
        {
            Debug.LogError("[ShopBuilder] Không tìm thấy '" + ShopButtonName + "' dưới Panel. Tạo nút Shop hồng trước.");
            EditorUtility.DisplayDialog("Build Shop Screen",
                "Thiếu nút Panel/ShopButton. Nút hồng này mới là nút mở Shop.", "OK");
            return false;
        }

        Button shopBtnButton = shopBtnTransform.GetComponent<Button>();
        if (shopBtnButton == null)
        {
            Debug.LogError("[ShopBuilder] '" + ShopButtonName + "' không có component Button.");
            return false;
        }

        // Xóa listener cũ rồi gắn lại ShowShopScreen sau khi tạo ShopScreen
        var soBtn = new SerializedObject(shopBtnButton);
        soBtn.FindProperty("m_OnClick.m_PersistentCalls.m_Calls").ClearArray();
        soBtn.ApplyModifiedPropertiesWithoutUndo();

        float contentWidth = U.RefWidth - PanelSideMargin * 2f;

        // ---------- HUD coin (góc trên trái Canvas) ----------

        RectTransform hud = U.NewUI(CoinHudName, canvas);
        U.Place(hud, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(280f, 80f));

        RectTransform hudIcon = U.NewUI("Icon", hud);
        U.Place(hudIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(8f, 0f), new Vector2(64f, 64f));
        U.AddImage(hudIcon, "iv_money", false, true);

        RectTransform hudText = U.NewUI("Coins Text", hud);
        U.Place(hudText, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(84f, 0f), new Vector2(180f, 64f));
        Text hudLabel = U.AddText(hudText, font, "{0}", 42f, TextAnchor.MiddleLeft, U.GoldText);

        CoinHUD coinHud = hud.gameObject.AddComponent<CoinHUD>();
        U.SetObjectField(coinHud, "m_CoinsText", hudLabel);

        // ---------- Shop root ----------

        RectTransform shopRoot = U.NewUI(ShopRootName, canvas);
        U.Stretch(shopRoot);
        ShopScreen shopScreen = shopRoot.gameObject.AddComponent<ShopScreen>();

        RectTransform panelRt = U.NewUI("Shop Panel", shopRoot);
        U.Stretch(panelRt);
        // Nền Shop = image 96
        Image panelBg = U.AddImageFromPath(panelRt, U.ShopBackgroundPath, true, false);
        panelBg.type = Image.Type.Simple;
        panelBg.preserveAspect = false;
        panelBg.color = Color.white;

        // Header / Coins pill: capsule + viền trắng (giống ShopButton)
        RectTransform header = U.TopCenter("Header", panelRt, HeaderTop, contentWidth, HeaderHeight);
        U.StyleCapsuleRow(header, new Color(0.15f, 0.45f, 0.95f, 0.92f), 8f);

        if (ShowHeaderTitle)
        {
            RectTransform headerTitle = U.NewUI("Title", header);
            U.Stretch(headerTitle);
            U.AddText(headerTitle, font, "SHOP", 60f, TextAnchor.MiddleCenter, Color.white);
            headerTitle.SetAsLastSibling();
        }

        RectTransform back = U.NewUI("Back", header);
        U.Place(back, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(28f, 0f), new Vector2(72f, 72f));
        Button backButton = U.IconButton(back, "iv_back");
        back.SetAsLastSibling();

        RectTransform coinsPill = U.TopCenter("Coins Pill", panelRt, CoinsPillTop, 380f, CoinsPillHeight);
        U.StyleCapsuleRow(coinsPill, new Color(1f, 0.55f, 0.12f, 0.95f), 7f);

        RectTransform coinsIcon = U.NewUI("Icon", coinsPill);
        U.Place(coinsIcon, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(32f, 0f), new Vector2(56f, 56f));
        U.AddImage(coinsIcon, "iv_money", false, true);
        coinsIcon.SetAsLastSibling();

        RectTransform coinsValue = U.NewUI("Value", coinsPill);
        U.Place(coinsValue, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(100f, 0f), new Vector2(240f, 64f));
        Text coinsLabelText = U.AddText(coinsValue, font, "{0}", 42f, TextAnchor.MiddleLeft, Color.white);
        coinsValue.SetAsLastSibling();

        float scrollHeight = U.RefHeight - ScrollTop - ScrollBottom;
        float scrollCenterFromBottom = U.RefHeight - ScrollTop - scrollHeight * 0.5f;

        RectTransform scroll = U.NewUI("Scroll View", panelRt);
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

        U.SetObjectField(shopScreen, "m_ShopPanel", panelRt.gameObject);
        U.SetObjectField(shopScreen, "m_ShopButton", shopBtnTransform.gameObject);
        U.SetObjectField(shopScreen, "m_CoinsText", coinsLabelText);

        UnityEventTools.AddVoidPersistentListener(shopBtnButton.onClick, shopScreen.ShowShopScreen);
        UnityEventTools.AddVoidPersistentListener(backButton.onClick, shopScreen.CloseShopScreen);

        panelRt.gameObject.SetActive(false);
        shopRoot.SetAsLastSibling();

        float contentHeight = Packs.Length * RowHeight + (Packs.Length - 1) * RowSpacing + RowPadding * 2f;
        Debug.Log(string.Format(
            "[ShopBuilder] Đã dựng Shop Screen: {0} gói, viewport {1:0}px, content {2:0}px.",
            Packs.Length, scrollHeight, contentHeight), shopRoot.gameObject);

        return true;
    }

    // Màu fill các hàng IAP — tông Fruit Race, viền trắng bên ngoài
    private static readonly Color[] PackRowColors =
    {
        new Color(1.00f, 0.45f, 0.15f, 0.95f), // cam
        new Color(0.20f, 0.75f, 0.35f, 0.95f), // xanh lá
        new Color(0.20f, 0.55f, 1.00f, 0.95f), // xanh dương
        new Color(0.85f, 0.30f, 0.85f, 0.95f), // tím hồng
        new Color(1.00f, 0.75f, 0.10f, 0.95f), // vàng
        new Color(0.15f, 0.80f, 0.85f, 0.95f), // cyan
        new Color(1.00f, 0.25f, 0.45f, 0.95f), // hồng đỏ
    };

    private static void BuildPackRow(RectTransform parent, Font font, Pack pack, int index, ShopScreen shopScreen)
    {
        RectTransform row = U.NewUI("Inapp" + index, parent);
        row.sizeDelta = new Vector2(0f, RowHeight);

        Color fill = PackRowColors[(index - 1) % PackRowColors.Length];
        U.StyleCapsuleRow(row, fill, 8f);

        LayoutElement element = row.gameObject.AddComponent<LayoutElement>();
        element.minHeight = RowHeight;
        element.preferredHeight = RowHeight;

        RectTransform gold = U.NewUI("Gold", row);
        U.Place(gold, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(24f, 0f), new Vector2(120f, 120f));
        U.AddImage(gold, pack.GoldSprite, false, true);
        gold.SetAsLastSibling();

        RectTransform title = U.NewUI("Title", row);
        U.Place(title, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(160f, 0f), new Vector2(280f, 70f));
        U.AddText(title, font, pack.Coins + " Coins", 40f, TextAnchor.MiddleLeft, Color.white);
        title.SetAsLastSibling();

        RectTransform priceRect = U.NewUI("TextIap", row);
        U.Place(priceRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-170f, 0f), new Vector2(150f, 70f));
        Text priceLabel = U.AddText(priceRect, font, pack.PricePlaceholder, 36f,
            TextAnchor.MiddleRight, new Color(1f, 0.95f, 0.4f, 1f));
        priceLabel.raycastTarget = true;
        U.AddButton(priceRect, priceLabel);
        priceRect.SetAsLastSibling();

        IAPButton iapButton = priceRect.gameObject.AddComponent<IAPButton>();
        iapButton.productId = pack.ProductId;
        iapButton.priceText = priceLabel;

        RectTransform buy = U.NewUI("Buy", row);
        U.Place(buy, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-18f, 0f), new Vector2(140f, 74f));
        Button buyButton = U.IconButton(buy, "iv_buy");
        buy.SetAsLastSibling();

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
