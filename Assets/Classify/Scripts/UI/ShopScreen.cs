using UnityEngine;
using TMPro;

#pragma warning disable CS0649

/// <summary>
/// Màn Shop IAP. Tự bật/tắt như một <see cref="UIScreen"/> bình thường qua <see cref="UIManager"/>.
/// </summary>
public class ShopScreen : UIScreen
{
    [SerializeField]
    private UIManager uiManager;

    [SerializeField]
    private TextMeshProUGUI m_CoinsText;

    private string m_CoinsTextFormat;

    private void OnEnable()
    {
        CoinWallet.m_OnCoinsChanged += OnCoinsChanged;
    }

    private void OnDisable()
    {
        CoinWallet.m_OnCoinsChanged -= OnCoinsChanged;
    }

    public override void Show(bool animate = true)
    {
        base.Show(animate);
        RefreshCoins();
    }

    public void ShowShopScreen()
    {
        uiManager.ShowScreen(Screens.Shop);
    }

    public void CloseShopScreen()
    {
        uiManager.ShowScreen(Screens.MainMenu);
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
                m_CoinsTextFormat = "{0}";
            }
        }

        m_CoinsText.text = string.Format(m_CoinsTextFormat, CoinWallet.Coins);
    }
}
