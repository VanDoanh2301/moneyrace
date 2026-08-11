using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TapTap
{
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

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClick);

            if (priceText != null)
            {
                _placeholder = priceText.text;
            }
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

            string price = (product != null && product.metadata != null)
                ? product.metadata.localizedPriceString
                : null;

            // Fake Store trong Editor trả về giá 0 (catalog không có giá). Giữ chữ tạm
            // thay vì hiển thị "0" — trên máy thật Google Play sẽ trả giá nội tệ đúng.
            if (!HasRealPrice(price))
            {
                if (!string.IsNullOrEmpty(_placeholder))
                {
                    priceText.text = _placeholder;
                }

                return;
            }

            priceText.text = price;
        }

        /// <summary>Chuỗi giá chỉ dùng được khi có ít nhất một chữ số khác 0 ("0", "0.00", "" đều bỏ).</summary>
        private static bool HasRealPrice(string price)
        {
            if (string.IsNullOrEmpty(price)) return false;

            for (int i = 0; i < price.Length; i++)
            {
                if (price[i] >= '1' && price[i] <= '9') return true;
            }

            return false;
        }
    }
}
