using UnityEngine;
using TMPro;

#pragma warning disable CS0649

namespace TapTap
{
    /// <summary>
    /// Màn Shop IAP. Component này nằm trên một node LUÔN ACTIVE ("Shop"),
    /// còn panel thật (<see cref="m_ShopPanel"/>) mới là thứ được bật/tắt.
    /// </summary>
    public class ShopScreen : Entity, IInstance, IEnableEvent, IDisableEvent
    {
        [SerializeField]
        private GameObject m_ShopPanel;

        [SerializeField]
        private GameObject m_ShopButton;

        [SerializeField]
        private TextMeshProUGUI m_CoinsText;

        private string m_CoinsTextFormat;

        public bool IsOpened
        {
            get { return m_ShopPanel != null && m_ShopPanel.activeSelf; }
        }

        private void Start()
        {
            Close(); // im lặng: CloseShopScreen() sẽ kêu tiếng nút, không hợp lúc mới vào game
        }

        public void OnEnableEvent()
        {
            CoinWallet.m_OnCoinsChanged += OnCoinsChanged;
        }

        public void OnDisableEvent()
        {
            CoinWallet.m_OnCoinsChanged -= OnCoinsChanged;
        }

        public void ShowShopScreen()
        {
            SoundManager.PlayButton();

            if (m_ShopPanel != null) m_ShopPanel.SetActive(true);
            if (m_ShopButton != null) m_ShopButton.SetActive(false);

            RefreshCoins();
        }

        public void CloseShopScreen()
        {
            SoundManager.PlayButton();

            Close();
        }

        private void Close()
        {
            if (m_ShopPanel != null) m_ShopPanel.SetActive(false);
            if (m_ShopButton != null) m_ShopButton.SetActive(true);
        }

        public void ToggleShopScreen()
        {
            if (IsOpened)
                CloseShopScreen();
            else
                ShowShopScreen();
        }

        /// <summary>Khôi phục giao dịch (iOS). Trên Google Play, Unity IAP tự khôi phục khi init.</summary>
        public void RestorePurchases()
        {
            if (IAPManager.Instance != null)
                IAPManager.Instance.RestorePurchases();
        }

        // ----- IAP: gọi từ nút Buy trong Shop (ủy quyền cho IAPManager) -----

        /// <summary>Mua gói 100 coin (iap1 - 0,50 US$).</summary>
        public void BuyCoins100() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins100(); }
        /// <summary>Mua gói 200 coin (iap2 - 1 US$).</summary>
        public void BuyCoins200() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins200(); }
        /// <summary>Mua gói 400 coin (iap3 - 2 US$).</summary>
        public void BuyCoins400() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins400(); }
        /// <summary>Mua gói 600 coin (iap4 - 3 US$).</summary>
        public void BuyCoins600() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins600(); }
        /// <summary>Mua gói 1000 coin (iap5 - 5 US$).</summary>
        public void BuyCoins1000() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins1000(); }
        /// <summary>Mua gói 2000 coin (iap6 - 7 US$).</summary>
        public void BuyCoins2000() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins2000(); }
        /// <summary>Mua gói 5000 coin (iap7 - 10 US$).</summary>
        public void BuyCoins5000() { if (IAPManager.Instance != null) IAPManager.Instance.BuyCoins5000(); }

        private void OnCoinsChanged(int coins)
        {
            RefreshCoins();
        }

        private void RefreshCoins()
        {
            if (m_CoinsText == null) return;

            if (string.IsNullOrEmpty(m_CoinsTextFormat))
            {
                m_CoinsTextFormat = m_CoinsText.text;

                if (string.IsNullOrEmpty(m_CoinsTextFormat) || !m_CoinsTextFormat.Contains("{0}"))
                {
                    m_CoinsTextFormat = "Coins: {0}";
                }
            }

            m_CoinsText.text = string.Format(m_CoinsTextFormat, CoinWallet.Coins);
        }
    }
}
