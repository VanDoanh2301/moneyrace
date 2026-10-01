using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

// Unity IAP 5.x đánh dấu API "coded IAP" cũ (UnityPurchasing.Initialize / IDetailedStoreListener)
// là [Obsolete] nhưng vẫn hoạt động đầy đủ. Tắt cảnh báo để Console sạch.
#pragma warning disable CS0618

/// <summary>
/// Quản lý In-App Purchase (Google Play Billing qua Unity Purchasing).
/// Port từ game Terra. Điểm mua được đẩy qua <see cref="ScoreRewardService"/>
/// nên vừa cộng vào tổng điểm vừa quy ra coin — cùng một đường với điểm chơi được.
///
/// Danh mục sản phẩm đọc từ <see cref="IAPCatalog"/>.
/// </summary>
public class IAPManager : MonoBehaviour, IDetailedStoreListener
{
    public static IAPManager Instance { get; private set; }

    [Header("Local validation (receipt) – bật nếu dùng CrossPlatformValidator")]
    public bool isLocalValidation;

    [Header("Loại sản phẩm")]
    [Tooltip("false = NonConsumable (mỗi tài khoản chỉ mua được 1 lần).\n" +
             "true = Consumable (mua lại nhiều lần) – CHỈ bật khi đã đổi loại sản phẩm tương ứng trên Google Play Console.")]
    public bool pointPacksAreConsumable = true;

    [Header("Debug - tự log danh sách product sau khi init")]
    [Tooltip("Số giây sau khi khởi động sẽ tự log giá trị IAP lấy được (0 = tắt).")]
    [SerializeField] private float _autoLogAfterSeconds = 0f;

    private IStoreController _storeController;
    private IExtensionProvider _extensionProvider;
    private bool _isInitialized;

    public bool IsInitialized => _isInitialized;

    /// <summary>Mua thành công (productId).</summary>
    public event Action<string> OnPurchaseSuccess;
    /// <summary>Mua thất bại (productId, reason).</summary>
    public event Action<string, string> PurchaseFailed;
    /// <summary>Khởi tạo xong.</summary>
    public event Action OnIAPInitialized;
    /// <summary>Khởi tạo thất bại.</summary>
    public event Action<string> OnIAPInitializeFailed;

    private void Awake()
    {
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
            Instance = null;
    }

    private void Start()
    {
        if (_autoLogAfterSeconds > 0f)
            StartCoroutine(AutoLogFetchedValuesAfterDelay());
    }

    private IEnumerator AutoLogFetchedValuesAfterDelay()
    {
        yield return new WaitForSeconds(_autoLogAfterSeconds);

        if (_isInitialized)
            LogFetchedProductValues();
        else
            Debug.Log("[IAP] IAP chưa init xong, không có giá trị để log.");
    }

    private void InitializePurchasing()
    {
        if (_isInitialized) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        ProductType type = pointPacksAreConsumable ? ProductType.Consumable : ProductType.NonConsumable;

        for (int i = 0; i < IAPCatalog.Packs.Length; i++)
            builder.AddProduct(IAPCatalog.Packs[i].ProductId, type);

        Debug.Log($"[IAP] Initialize – đăng ký {IAPCatalog.Packs.Length} product ({type}).");
        UnityPurchasing.Initialize(this, builder);
    }

    #region IDetailedStoreListener
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        _storeController = controller;
        _extensionProvider = extensions;
        _isInitialized = true;

        Debug.Log("[IAP] Initialized.");

        OnIAPInitialized?.Invoke();
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

        if (isLocalValidation)
        {
            // Có thể thêm CrossPlatformValidator (Unity IAP Security) để validate receipt trước khi grant.
            // if (!IsPurchaseValid(product)) return PurchaseProcessingResult.Pending;
        }

        int points = IAPCatalog.GetPoints(id);

        if (points > 0)
        {
            // Giao dịch tiền thật => ghi xuống đĩa ngay.
            ScoreRewardService.GrantScore(points, true);

            Debug.Log($"[IAP] Cộng {points} điểm + {ScoreRewardService.ToCoins(points)} coin cho {id}.");
        }

        PlayerPrefs.SetInt("IAP_" + id, 1);
        PlayerPrefs.Save();

        OnPurchaseSuccess?.Invoke(id);

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
    #endregion

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
        {
            _storeController.InitiatePurchase(product);
        }
        else
        {
            Debug.LogWarning($"[IAP] Product không tồn tại hoặc không khả dụng: {productId}");

            PurchaseFailed?.Invoke(productId, "Product not available");
        }
    }

    /// <summary>Khôi phục mua (quan trọng trên iOS; Google Play tự khôi phục khi init).</summary>
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
    }

    /// <summary>Lấy Product theo id (để hiển thị giá/tên trong <see cref="IAPButton"/>).</summary>
    public Product GetProduct(string productId)
    {
        if (!_isInitialized || _storeController == null) return null;

        return _storeController.products.WithID(productId);
    }

    /// <summary>Log các giá trị đã lấy được từ store (giá, tên, mô tả).</summary>
    public void LogFetchedProductValues()
    {
        if (!_isInitialized || _storeController == null)
        {
            Debug.Log("[IAP] LogFetchedProductValues: chưa init hoặc chưa có store.");
            return;
        }

        var all = _storeController.products.all;

        Debug.Log($"[IAP] ===== Giá trị lấy từ store – tổng {all.Length} product =====");

        for (int i = 0; i < all.Length; i++)
        {
            Product product = all[i];
            ProductMetadata meta = product.metadata;

            Debug.Log($"[IAP] [{i + 1}] id={product.definition.id}\n" +
                      $"     title={(meta != null ? meta.localizedTitle : "(chưa có)")}\n" +
                      $"     price={(meta != null ? meta.localizedPriceString : "(chưa có)")}\n" +
                      $"     availableToPurchase={product.availableToPurchase}");
        }

        Debug.Log("[IAP] ===== Hết danh sách =====");
    }

    /// <summary>Đã mua gói theo productId chưa (vd: iap1, iap2...).</summary>
    public static bool HasPurchased(string productId)
    {
        return PlayerPrefs.GetInt("IAP_" + productId, 0) == 1;
    }
}
