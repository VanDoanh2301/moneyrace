using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Nút mua một gói IAP. Tự hiển thị giá nội tệ (localizedPriceString) sau khi IAP init xong.
/// </summary>
[RequireComponent(typeof(Button))]
public class IAPButton : MonoBehaviour
{
    [Tooltip("ID sản phẩm: iap1 … iap7 (trùng Google Play Console).")]
    public string productId = "iap1";

    [Header("Optional - hiển thị giá")]
    [Tooltip("Nếu có, sẽ cập nhật text thành giá sau khi IAP init xong.")]
    public TextMeshProUGUI priceText;

    private Button _button;

    /// <summary>Chữ giá tạm đặt sẵn trong scene, dùng lại khi store chưa trả về giá thật.</summary>
    private string _placeholder;

    private bool _subscribed;
    private Coroutine _bindRoutine;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);

        if (priceText != null)
            _placeholder = priceText.text;
    }

    private void OnEnable()
    {
        TrySubscribeAndRefresh();

        // Shop/IAPButton có thể OnEnable trước IAPManager.Awake → phải đợi Instance.
        if (!_subscribed || IAPManager.Instance == null || !IAPManager.Instance.IsInitialized)
        {
            if (_bindRoutine == null)
                _bindRoutine = StartCoroutine(BindWhenReady());
        }
    }

    private void OnDisable()
    {
        if (_bindRoutine != null)
        {
            StopCoroutine(_bindRoutine);
            _bindRoutine = null;
        }

        Unsubscribe();
    }

    private IEnumerator BindWhenReady()
    {
        while (IAPManager.Instance == null)
            yield return null;

        TrySubscribeAndRefresh();

        while (IAPManager.Instance != null && !IAPManager.Instance.IsInitialized)
            yield return null;

        UpdatePriceDisplay();
        _bindRoutine = null;
    }

    private void TrySubscribeAndRefresh()
    {
        if (IAPManager.Instance == null)
            return;

        if (!_subscribed)
        {
            IAPManager.Instance.OnIAPInitialized += UpdatePriceDisplay;
            _subscribed = true;
        }

        if (IAPManager.Instance.IsInitialized)
            UpdatePriceDisplay();
    }

    private void Unsubscribe()
    {
        if (!_subscribed || IAPManager.Instance == null)
        {
            _subscribed = false;
            return;
        }

        IAPManager.Instance.OnIAPInitialized -= UpdatePriceDisplay;
        _subscribed = false;
    }

    private void OnClick()
    {
        if (IAPManager.Instance == null)
        {
            Debug.LogWarning("[IAPButton] IAPManager chưa có trong scene.");
            return;
        }

        IAPManager.Instance.BuyProduct(productId);
    }

    /// <summary>Gọi lại khi mở Shop để chắc chắn giá đúng productId.</summary>
    public void RefreshPrice()
    {
        UpdatePriceDisplay();
    }

    private void UpdatePriceDisplay()
    {
        if (priceText == null || IAPManager.Instance == null)
            return;

        var product = IAPManager.Instance.GetProduct(productId);

        string price = (product != null && product.metadata != null)
            ? product.metadata.localizedPriceString
            : null;

        // Fake Store trong Editor trả về giá 0 (catalog không có giá). Giữ chữ tạm
        // thay vì hiển thị "0" — trên máy thật Google Play sẽ trả giá nội tệ đúng.
        if (!HasRealPrice(price))
        {
            if (!string.IsNullOrEmpty(_placeholder))
                priceText.text = _placeholder;

            return;
        }

        priceText.text = price;
    }

    /// <summary>Chuỗi giá chỉ dùng được khi có ít nhất một chữ số khác 0 ("0", "0.00", "" đều bỏ).</summary>
    private static bool HasRealPrice(string price)
    {
        if (string.IsNullOrEmpty(price))
            return false;

        for (int i = 0; i < price.Length; i++)
        {
            if (price[i] >= '1' && price[i] <= '9')
                return true;
        }

        return false;
    }
}
