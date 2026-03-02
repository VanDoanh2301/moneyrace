using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gắn vào từng nút "Buy" trong màn Shop (InApp).
/// Chọn loại sản phẩm trong Inspector, bấm sẽ gọi IAPManager mua đúng gói.
/// </summary>
[RequireComponent(typeof(Button))]
public class ShopBuyButton : MonoBehaviour
{
    public enum ShopProduct
    {
        Coins100 = 0,   // iap1 - 0,30 US$
        Coins200 = 1,   // iap2 - 0,49 US$
        Coins400 = 2,   // iap3 - 0,99 US$
        Coins600 = 3,   // iap4 - 1,99 US$
        Coins1000 = 4,  // iap5 - 2,99 US$
        Coins2000 = 5,  // iap6 - 4,99 US$
        Coins5000 = 6   // iap7 - 9,99 US$
    }

    [Tooltip("Chọn gói cần mua khi bấm nút này.")]
    public ShopProduct product = ShopProduct.Coins100;

    [Header("Optional - hiển thị giá")]
    [Tooltip("Nếu gán, sẽ hiển thị giá từ store (vd: $3.99) sau khi IAP init xong.")]
    public TextMeshProUGUI priceText;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnBuyClick);
    }

    private void OnEnable()
    {
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.OnIAPInitialized += UpdatePriceDisplay;
            if (IAPManager.Instance.IsInitialized)
                UpdatePriceDisplay();
        }
    }

    private void OnDisable()
    {
        if (IAPManager.Instance != null)
            IAPManager.Instance.OnIAPInitialized -= UpdatePriceDisplay;
    }

    private void OnBuyClick()
    {
        if (IAPManager.Instance == null)
        {
            Debug.LogWarning("[ShopBuyButton] IAPManager chưa có trong scene.");
            return;
        }
        string productId = GetProductId();
        IAPManager.Instance.BuyProduct(productId);
    }

    private string GetProductId()
    {
        if (IAPManager.Instance == null) return "iap1";
        switch (product)
        {
            case ShopProduct.Coins100:  return IAPManager.Instance.productIap1;
            case ShopProduct.Coins200:  return IAPManager.Instance.productIap2;
            case ShopProduct.Coins400:  return IAPManager.Instance.productIap3;
            case ShopProduct.Coins600:  return IAPManager.Instance.productIap4;
            case ShopProduct.Coins1000: return IAPManager.Instance.productIap5;
            case ShopProduct.Coins2000: return IAPManager.Instance.productIap6;
            case ShopProduct.Coins5000: return IAPManager.Instance.productIap7;
            default: return IAPManager.Instance.productIap1;
        }
    }

    private void UpdatePriceDisplay()
    {
        if (priceText == null || IAPManager.Instance == null) return;
        string productId = GetProductId();
        var p = IAPManager.Instance.GetProduct(productId);
        priceText.text = p?.metadata?.localizedPriceString ?? productId;
    }
}
