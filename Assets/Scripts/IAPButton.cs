using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class IAPButton : MonoBehaviour
{
    [Tooltip("ID sản phẩm: iap1, iap2, iap3, iap4, iap5 (trùng Google Play Console).")]
    public string productId = "iap1";

    [Header("Optional - hiển thị giá")]
    [Tooltip("Nếu có, sẽ cập nhật text thành giá sau khi IAP init xong.")]
    public TextMeshProUGUI priceText;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
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

    private void OnClick()
    {
        if (IAPManager.Instance == null)
        {
            Debug.LogWarning("[IAPButton] IAPManager chưa có trong scene.");
            return;
        }
        IAPManager.Instance.BuyProduct(productId);
    }

    private void UpdatePriceDisplay()
    {
        if (priceText == null || IAPManager.Instance == null) return;
        var product = IAPManager.Instance.GetProduct(productId);
        priceText.text = product?.metadata?.localizedPriceString ?? productId;
    }
}
