using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cửa hàng gói điểm IAP. Thay cho ShopPanel_tetris của game Terra: ở đây panel chạy
/// theo khuôn <see cref="Menu"/> nên vào được stack back-navigation của
/// <see cref="MenuManager"/> và dùng chung animation với các menu khác.
/// </summary>
public class ShopMenu : Menu
{
    [Header("UI References :")]
    [SerializeField] TMP_Text _coinText;
    [SerializeField] Button _backButton;
    [Tooltip("Chỉ có tác dụng trên iOS; Google Play tự khôi phục khi init.")]
    [SerializeField] Button _restoreButton;
    [Tooltip("Dòng thông báo khi mua thất bại.")]
    [SerializeField] TMP_Text _messageText;

    private string _coinFormat;

    public override void SetEnable()
    {
        base.SetEnable();

        _backButton.interactable = true;

        if (_messageText != null)
            _messageText.text = string.Empty;

        RefreshCoins();

        // Menu chỉ bật/tắt Canvas chứ không SetActive GameObject, nên OnEnable của
        // ShopMenu và của từng IAPButton chỉ chạy một lần lúc load scene.
        // Vì vậy phải làm mới giá ở đây, mỗi lần mở shop.
        RefreshIapPrices();

        // IAPManager tạo trong Awake của chính nó nên lúc OnEnable có thể chưa tồn tại.
        // Đăng ký lại ở đây; -= trước += nên gọi nhiều lần vẫn chỉ có một listener.
        SubscribePurchaseFailed();
    }

    private void Start()
    {
        OnButtonPressed(_backButton, BackButtonPressed);

        if (_restoreButton == null) return;

#if UNITY_IOS
        OnButtonPressed(_restoreButton, RestoreButtonPressed);
#else
        _restoreButton.gameObject.SetActive(false);
#endif
    }

    private void OnEnable()
    {
        CoinWallet.OnCoinsChanged += OnCoinsChanged;

        SubscribePurchaseFailed();

        RefreshCoins();
    }

    private void OnDisable()
    {
        CoinWallet.OnCoinsChanged -= OnCoinsChanged;

        if (IAPManager.Instance != null)
            IAPManager.Instance.PurchaseFailed -= OnPurchaseFailed;
    }

    private void SubscribePurchaseFailed()
    {
        if (IAPManager.Instance == null) return;

        IAPManager.Instance.PurchaseFailed -= OnPurchaseFailed;
        IAPManager.Instance.PurchaseFailed += OnPurchaseFailed;
    }

    private void OnCoinsChanged(int coins)
    {
        RefreshCoins();
    }

    private void OnPurchaseFailed(string productId, string reason)
    {
        if (_messageText == null) return;

        _messageText.text = "Mua không thành công. Vui lòng thử lại.";
    }

    private void BackButtonPressed()
    {
        _backButton.interactable = false;
        MenuManager.GetInstance().CloseMenu();
    }

    private void RestoreButtonPressed()
    {
        if (IAPManager.Instance != null)
            IAPManager.Instance.RestorePurchases();
    }

    private void RefreshCoins()
    {
        if (_coinText == null) return;

        if (string.IsNullOrEmpty(_coinFormat))
        {
            _coinFormat = _coinText.text;

            if (string.IsNullOrEmpty(_coinFormat) || !_coinFormat.Contains("{0}"))
                _coinFormat = "{0}";
        }

        _coinText.text = string.Format(_coinFormat, CoinWallet.Coins);
    }

    private void RefreshIapPrices()
    {
        IAPButton[] buttons = GetComponentsInChildren<IAPButton>(true);

        for (int i = 0; i < buttons.Length; i++)
            buttons[i].RefreshPrice();
    }
}
