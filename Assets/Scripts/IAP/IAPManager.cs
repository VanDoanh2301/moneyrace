using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
using TMPro;

// Unity IAP 5.x đánh dấu API "coded IAP" cũ (UnityPurchasing.Initialize / IDetailedStoreListener)
// là [Obsolete] nhưng vẫn hoạt động đầy đủ. Tắt cảnh báo để Console sạch.
#pragma warning disable CS0618

/// <summary>
/// Quản lý In-App Purchase (Google Play Billing qua Unity Purchasing).
/// Port từ game Chroma; điểm mua được cộng vào <see cref="PointsWallet"/> —
/// cùng một số dư với điểm kiếm được khi chơi.
/// </summary>
public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance { get; private set; }

    [Header("Local validation (receipt) – bật nếu dùng CrossPlatformValidator")]
    public bool isLocalValidation;

    // Thang giá (đặt trên Google Play Console, code chỉ quy đổi ra điểm):
    // iap1=0,50$ →100 | iap2=1$ →200 | iap3=2$ →400 | iap4=3$ →600 | iap5=5$ →1000 | iap6=7$ →2000 | iap7=10$ →5000
    [Header("Gói điểm - iap1=0,50$ (100) | iap2=1$ (200) | iap3=2$ (400) | iap4=3$ (600) | iap5=5$ (1000) | iap6=7$ (2000) | iap7=10$ (5000)")]
    public string productIap1 = "iap1";
    public string productIap2 = "iap2";
    public string productIap3 = "iap3";
    public string productIap4 = "iap4";
    public string productIap5 = "iap5";
    public string productIap6 = "iap6";
    public string productIap7 = "iap7";

    [Header("Loại sản phẩm")]
    [Tooltip("false = NonConsumable (sản phẩm 'tính phí một lần' trên Play Console, mỗi tài khoản chỉ mua được 1 lần).\n" +
             "true = Consumable (mua lại nhiều lần) – CHỈ bật khi đã đổi loại sản phẩm tương ứng trên Google Play Console.")]
    public bool pointPacksAreConsumable = true;

    [Header("Optional UI - hiển thị số điểm sau khi mua / init")]
    [Tooltip("Hiển thị số điểm hiện tại (vd: Điểm: 500).")]
    public TextMeshProUGUI pointsStatusText;

    private IStoreController _storeController;
    private IExtensionProvider _extensionProvider;
    private bool _isInitialized;

    public bool IsInitialized { get { return _isInitialized; } }

    /// <summary>Mua thành công (productId)</summary>
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

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        if (autoLogAfterSeconds > 0f)
        {
            StartCoroutine(AutoLogFetchedValuesAfterDelay());
        }
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

        ProductType type = pointPacksAreConsumable ? ProductType.Consumable : ProductType.NonConsumable;

        builder.AddProduct(productIap1, type);
        builder.AddProduct(productIap2, type);
        builder.AddProduct(productIap3, type);
        builder.AddProduct(productIap4, type);
        builder.AddProduct(productIap5, type);
        builder.AddProduct(productIap6, type);
        builder.AddProduct(productIap7, type);

        Debug.Log("[IAP] Gọi UnityPurchasing.Initialize – đăng ký iap1–iap7 (" + type + ").");
        UnityPurchasing.Initialize(this, builder);
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _storeController = controller;
        _extensionProvider = extensions;
        _isInitialized = true;

        Debug.Log("[IAP] Initialized.");

        if (OnIAPInitialized != null) OnIAPInitialized.Invoke();

        LogFetchedProductValues();
        UpdateUIPoints();
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogWarning(string.Format("[IAP] Init failed: {0}", error));

        if (OnIAPInitializeFailed != null) OnIAPInitializeFailed.Invoke(error.ToString());
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        Debug.LogWarning(string.Format("[IAP] Init failed: {0} - {1}", error, message));

        if (OnIAPInitializeFailed != null) OnIAPInitializeFailed.Invoke(string.Format("{0}: {1}", error, message));
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        var product = args.purchasedProduct;
        string id = product.definition.id;

        Debug.Log(string.Format("[IAP] Purchase OK: {0}", id));

        if (isLocalValidation)
        {
            // Có thể thêm CrossPlatformValidator (Unity IAP Security) để validate receipt trước khi grant.
            // if (!IsPurchaseValid(product)) return PurchaseProcessingResult.Pending;
        }

        int points = GetPointsForProduct(id);
        if (points > 0)
        {
            PointsWallet.Add(points, true); // giao dịch tiền thật => ghi xuống đĩa ngay
            UpdateUIPoints();
        }

        PlayerPrefs.SetInt("IAP_" + id, 1);
        PlayerPrefs.Save();

        if (OnPurchaseSuccess != null) OnPurchaseSuccess.Invoke(id);

        return PurchaseProcessingResult.Complete;
    }

    /// <summary>Số điểm tương ứng với một product id (0 nếu không phải gói điểm).</summary>
    public int GetPointsForProduct(string productId)
    {
        if (productId == productIap1) return 100;
        if (productId == productIap2) return 200;
        if (productId == productIap3) return 400;
        if (productId == productIap4) return 600;
        if (productId == productIap5) return 1000;
        if (productId == productIap6) return 2000;
        if (productId == productIap7) return 5000;

        return 0;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.LogWarning(string.Format("[IAP] Purchase failed: {0} - {1}", product.definition.id, failureReason));

        if (PurchaseFailed != null) PurchaseFailed.Invoke(product.definition.id, failureReason.ToString());
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
    {
        Debug.LogWarning(string.Format("[IAP] Purchase failed: {0} - {1} {2}",
            product.definition.id, failureDescription.reason, failureDescription.message));

        if (PurchaseFailed != null) PurchaseFailed.Invoke(product.definition.id, failureDescription.message);
    }

    /// <summary>Cập nhật UI số điểm.</summary>
    public void UpdateUIPoints()
    {
        if (pointsStatusText == null) return;

        pointsStatusText.text = "Points: " + PointsWallet.Points;
    }

    /// <summary>Gọi mua theo product ID.</summary>
    public void BuyProduct(string productId)
    {
        if (!_isInitialized || _storeController == null)
        {
            Debug.LogWarning("[IAP] Chưa khởi tạo. Đợi hoặc kiểm tra kết nối.");

            if (PurchaseFailed != null) PurchaseFailed.Invoke(productId, "Not initialized");
            return;
        }

        Product product = _storeController.products.WithID(productId);
        if (product != null && product.availableToPurchase)
        {
            _storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.LogWarning(string.Format("[IAP] Product không tồn tại hoặc không khả dụng: {0}", productId));

            if (PurchaseFailed != null) PurchaseFailed.Invoke(productId, "Product not available");
        }
    }

    /// <summary>iap1 = 0,50 US$ → 100 điểm.</summary>
    public void BuyPoints100() { BuyProduct(productIap1); }
    /// <summary>iap2 = 1 US$ → 200 điểm.</summary>
    public void BuyPoints200() { BuyProduct(productIap2); }
    /// <summary>iap3 = 2 US$ → 400 điểm.</summary>
    public void BuyPoints400() { BuyProduct(productIap3); }
    /// <summary>iap4 = 3 US$ → 600 điểm.</summary>
    public void BuyPoints600() { BuyProduct(productIap4); }
    /// <summary>iap5 = 5 US$ → 1000 điểm.</summary>
    public void BuyPoints1000() { BuyProduct(productIap5); }
    /// <summary>iap6 = 7 US$ → 2000 điểm.</summary>
    public void BuyPoints2000() { BuyProduct(productIap6); }
    /// <summary>iap7 = 10 US$ → 5000 điểm.</summary>
    public void BuyPoints5000() { BuyProduct(productIap7); }

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
        Debug.Log(string.Format("[IAP] Restore finished: {0} - {1}", success, message));

        if (success)
            UpdateUIPoints();
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

        Debug.Log(string.Format("[IAP] ========== CÁC GIÁ TRỊ ĐÃ LẤY ĐƯỢC (từ store) – Tổng: {0} product ==========", all.Length));

        for (int i = 0; i < all.Length; i++)
        {
            var product = all[i];
            var meta = product.metadata;

            Debug.Log(string.Format("[IAP] [{0}] id={1}\n     title={2}\n     description={3}\n     price={4}\n     availableToPurchase={5}",
                i + 1,
                product.definition.id,
                meta != null ? meta.localizedTitle : "(chưa có)",
                meta != null ? meta.localizedDescription : "(chưa có)",
                meta != null ? meta.localizedPriceString : "(chưa có)",
                product.availableToPurchase));
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

    public static int GetPoints()
    {
        return PointsWallet.Points;
    }

    /// <summary>Đã mua gói theo productId chưa (vd: iap1, iap2...).</summary>
    public static bool HasPurchased(string productId)
    {
        return PlayerPrefs.GetInt("IAP_" + productId, 0) == 1;
    }
}
