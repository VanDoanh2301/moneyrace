using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using TMPro;

public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance { get; private set; }

    [Header("Local validation (receipt) – bật nếu dùng CrossPlatformValidator")]
    public bool isLocalValidation;

    [Header("Purchase Settings")]
    [Tooltip("Bật true: gói coin có thể mua lại nhiều lần (Consumable). Tắt: mỗi gói chỉ mua 1 lần.")]
    public bool allowRepeatPurchase = true;

    [Header("Coin Packs - iap1=0,30$ (100) | iap2=0,49$ (200) | iap3=0,99$ (400) | iap4=1,99$ (600) | iap5=2,99$ (1000) | iap6=4,99$ (2000) | iap7=9,99$ (5000)")]
    public string productIap1 = "iap1";
    public string productIap2 = "iap2";
    public string productIap3 = "iap3";
    public string productIap4 = "iap4";
    public string productIap5 = "iap5";
    public string productIap6 = "iap6";
    public string productIap7 = "iap7";

    [Header("Optional UI - hiển thị số coin sau khi mua / init")]
    [Tooltip("Hiển thị số coin hiện tại (vd: Coins: 500).")]
    public TextMeshProUGUI coinsStatusText;

    /// <summary>Đã mua gỡ quảng cáo (PlayerPrefs).</summary>

    private IStoreController _storeController;
    private IExtensionProvider _extensionProvider;
    private bool _isInitialized;

    public bool IsInitialized => _isInitialized;

    /// <summary>Mua thành công (productId, receipt)</summary>
    public event Action<string> OnPurchaseSuccess;
    /// <summary>Mua thất bại (productId, reason)</summary>
    public event Action<string, string> PurchaseFailed;
    /// <summary>Khởi tạo xong</summary>
    public event Action OnIAPInitialized;
    /// <summary>Khởi tạo thất bại</summary>
    public event Action<string> OnIAPInitializeFailed;

    [Header("Test - tự log khi game khởi động")]
    [Tooltip("Sau số giây này sẽ tự gọi LogFetchedProductValues() để test (0 = tắt).")]
    [SerializeField] private float autoLogAfterSeconds = 3f;

    private void Awake()
    {
        Debug.Log("[IAP] IAPManager khởi động – bắt đầu init để bắt log khi game chạy.");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializePurchasing();
    }

    private void Start()
    {
        if (autoLogAfterSeconds > 0f)
            StartCoroutine(AutoLogFetchedValuesAfterDelay());
    }

    private IEnumerator AutoLogFetchedValuesAfterDelay()
    {
        yield return new WaitForSeconds(autoLogAfterSeconds);
        Debug.Log("[IAP] (Game start - log test) Tự động gọi log giá trị IAP đã lấy được.");
        if (_isInitialized)
            LogFetchedProductValues();
        else
            Debug.Log("[IAP] (Game start - log test) IAP chưa init xong, không có giá trị để log.");
    }

    private void InitializePurchasing()
    {
        if (_isInitialized) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        // iap1–iap7: coin pack — Consumable khi allowRepeatPurchase = true
        var productType = allowRepeatPurchase ? ProductType.Consumable : ProductType.NonConsumable;
        builder.AddProduct(productIap1, productType);
        builder.AddProduct(productIap2, productType);
        builder.AddProduct(productIap3, productType);
        builder.AddProduct(productIap4, productType);
        builder.AddProduct(productIap5, productType);
        builder.AddProduct(productIap6, productType);
        builder.AddProduct(productIap7, productType);

        Debug.Log("[IAP] Gọi UnityPurchasing.Initialize – đăng ký iap1–iap7.");
        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _storeController = controller;
        _extensionProvider = extensions;
        _isInitialized = true;
        Debug.Log("[IAP] Initialized.");
        OnIAPInitialized?.Invoke();

        LogFetchedProductValues();
        UpdateUICoins();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogWarning($"[IAP] Init failed: {error}");
        OnIAPInitializeFailed?.Invoke(error.ToString());
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogWarning($"[IAP] Init failed: {error} - {message}");
        OnIAPInitializeFailed?.Invoke($"{error}: {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        var product = args.purchasedProduct;
        string id = product.definition.id;
        Debug.Log($"[IAP] Purchase OK: {id}");
        OnPurchaseSuccess?.Invoke(id);

        if (isLocalValidation)
        {
            // Có thể thêm CrossPlatformValidator (Unity IAP Security) để validate receipt trước khi grant.
            // if (!IsPurchaseValid(product)) return PurchaseProcessingResult.Pending;
        }

        if (id == productIap1) { AddCoins(100); UpdateUICoins(); }
        else if (id == productIap2) { AddCoins(200); UpdateUICoins(); }
        else if (id == productIap3) { AddCoins(400); UpdateUICoins(); }
        else if (id == productIap4) { AddCoins(600); UpdateUICoins(); }
        else if (id == productIap5) { AddCoins(1000); UpdateUICoins(); }
        else if (id == productIap6) { AddCoins(2000); UpdateUICoins(); }
        else if (id == productIap7) { AddCoins(5000); UpdateUICoins(); }

        if (!allowRepeatPurchase)
        {
            PlayerPrefs.SetInt("IAP_" + id, 1);
            PlayerPrefs.Save();
        }
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogWarning($"[IAP] Purchase failed: {product.definition.id} - {failureReason}");
        PurchaseFailed?.Invoke(product.definition.id, failureReason.ToString());
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogWarning($"[IAP] Purchase failed: {product.definition.id} - {failureDescription.reason} {failureDescription.message}");
        PurchaseFailed?.Invoke(product.definition.id, failureDescription.message);
    }

    /// <summary>Cập nhật UI số coin.</summary>
    public void UpdateUICoins()
    {
        if (coinsStatusText == null) return;
        coinsStatusText.text = "Coins: " + GetCoins();
    }

    /// <summary>Gọi mua theo product ID.</summary>
    public void BuyProduct(string productId)
    {
        if (!_isInitialized || _storeController == null)
        {
            Debug.LogWarning("[IAP] Chưa khởi tạo. Đợi hoặc kiểm tra kết nối.");
            PurchaseFailed?.Invoke(productId, "Not initialized");
            return;
        }

        Product product = _storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
            _storeController.InitiatePurchase(product);
        else
        {
            Debug.LogWarning($"[IAP] Product không tồn tại hoặc không khả dụng: {productId}");
            PurchaseFailed?.Invoke(productId, "Product not available");
        }
    }

    /// <summary>iap1 = 0,30 US$ → 100 coin.</summary>
    public void BuyCoins100() => BuyProduct(productIap1);
    /// <summary>iap2 = 0,49 US$ → 200 coin.</summary>
    public void BuyCoins200() => BuyProduct(productIap2);
    /// <summary>iap3 = 0,99 US$ → 400 coin.</summary>
    public void BuyCoins400() => BuyProduct(productIap3);
    /// <summary>iap4 = 1,99 US$ → 600 coin.</summary>
    public void BuyCoins600() => BuyProduct(productIap4);
    /// <summary>iap5 = 2,99 US$ → 1000 coin.</summary>
    public void BuyCoins1000() => BuyProduct(productIap5);
    /// <summary>iap6 = 4,99 US$ → 2000 coin.</summary>
    public void BuyCoins2000() => BuyProduct(productIap6);
    /// <summary>iap7 = 9,99 US$ → 5000 coin.</summary>
    public void BuyCoins5000() => BuyProduct(productIap7);

    /// <summary>Khôi phục mua (quan trọng trên iOS).</summary>
    public void RestorePurchases()
    {
        if (!_isInitialized || _extensionProvider == null) return;
        var apple = _extensionProvider.GetExtension<IAppleExtensions>();
        if (apple != null)
            apple.RestoreTransactions(OnRestoreFinished);
        else
            Debug.Log("[IAP] Restore chỉ hỗ trợ trên Apple.");
    }

    private void OnRestoreFinished(bool success, string message)
    {
        Debug.Log($"[IAP] Restore finished: {success} - {message}");
        if (success)
            UpdateUICoins();
    }

    /// <summary>Lấy Product theo id (để hiển thị giá/tên trong IAPButton).</summary>
    public Product GetProduct(string productId)
    {
        if (!_isInitialized || _storeController == null) return null;
        return _storeController.products.WithID(productId);
    }

    /// <summary>Log ra các giá trị đã lấy được từ store (giá, tên, mô tả).</summary>
    public void LogFetchedProductValues()
    {
        if (!_isInitialized || _storeController == null) return;
        var all = _storeController.products.all;
        Debug.Log($"[IAP] ========== CÁC GIÁ TRỊ ĐÃ LẤY ĐƯỢC (từ store) – Tổng: {all.Length} product ==========");
        for (int i = 0; i < all.Length; i++)
        {
            var product = all[i];
            var meta = product.metadata;
            Debug.Log($"[IAP] [{i + 1}] id={product.definition.id}\n" +
                $"     title={meta?.localizedTitle ?? "(chưa có)"}\n" +
                $"     description={meta?.localizedDescription ?? "(chưa có)"}\n" +
                $"     price={meta?.localizedPriceString ?? "(chưa có)"}\n" +
                $"     availableToPurchase={product.availableToPurchase}");
        }
        Debug.Log("[IAP] ========== HẾT DANH SÁCH GIÁ TRỊ IAP ==========");
    }

    /// <summary>Log tất cả product IAP đã lấy từ store (để test).</summary>
    public void LogAllProducts()
    {
        if (!_isInitialized || _storeController == null)
        {
            Debug.Log("[IAP] LogAllProducts: Chưa init hoặc chưa có store.");
            return;
        }
        LogFetchedProductValues();
    }

    private void AddCoins(int amount)
    {
        int current = PlayerPrefs.GetInt("Coins", 0);
        PlayerPrefs.SetInt("Coins", current + amount);
    }

    public static int GetCoins()
    {
        return PlayerPrefs.GetInt("Coins", 0);
    }

    /// <summary>Đã mua gói theo productId chưa (vd: iap1, iap2...).</summary>
    public static bool HasPurchased(string productId)
    {
        if (Instance != null && Instance.allowRepeatPurchase)
            return false;
        return PlayerPrefs.GetInt("IAP_" + productId, 0) == 1;
    }
}