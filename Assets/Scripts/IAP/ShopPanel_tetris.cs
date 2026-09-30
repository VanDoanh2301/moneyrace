using UnityEngine;
using TMPro;

#pragma warning disable CS0649

/// <summary>
/// Panel Shop IAP. Viết lại từ ShopScreen của game Chroma thành MonoBehaviour thuần
/// (bỏ UIScreen/UIManager/DOTween) để hợp với cách bật/tắt panel bằng SetActive
/// của <see cref="UIController_tetris"/>.
/// </summary>
public class ShopPanel_tetris : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI m_PointsText;

    [Header("Ẩn đi trong lúc shop đang mở")]
    [SerializeField]
    private GameObject m_PointsHud;
    [SerializeField]
    private GameObject m_ShopButton;

    private string m_PointsTextFormat;

    private void OnEnable()
    {
        PointsWallet.m_OnPointsChanged += OnPointsChanged;

        RefreshPoints();
        RefreshIapPrices();
    }

    private void OnDisable()
    {
        PointsWallet.m_OnPointsChanged -= OnPointsChanged;
    }

    /// <summary>Mở shop (gọi từ nút Shop trên HUD).</summary>
    public void Open()
    {
        gameObject.SetActive(true);

        if (m_PointsHud != null) m_PointsHud.SetActive(false);
        if (m_ShopButton != null) m_ShopButton.SetActive(false);

        RefreshPoints();
        RefreshIapPrices();
    }

    /// <summary>Refresh giá từng gói theo productId (tránh hiện placeholder / giá cũ khi mở shop).</summary>
    private void RefreshIapPrices()
    {
        var buttons = GetComponentsInChildren<IAPButton>(true);
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].RefreshPrice();
    }

    /// <summary>Đóng shop (gọi từ nút Back).</summary>
    public void Close()
    {
        if (m_PointsHud != null) m_PointsHud.SetActive(true);
        if (m_ShopButton != null) m_ShopButton.SetActive(true);

        gameObject.SetActive(false);
    }

    /// <summary>Khôi phục giao dịch (iOS). Trên Google Play, Unity IAP tự khôi phục khi init.</summary>
    public void RestorePurchases()
    {
        if (IAPManager.Instance != null)
            IAPManager.Instance.RestorePurchases();
    }

    // ----- IAP: gọi từ nút Buy trong Shop (ủy quyền cho IAPManager) -----

    /// <summary>Mua gói 100 điểm (iap1 - 0,50 US$).</summary>
    public void BuyPoints100() { if (IAPManager.Instance != null) IAPManager.Instance.BuyPoints100(); }
    /// <summary>Mua gói 200 điểm (iap2 - 1 US$).</summary>
    public void BuyPoints200() { if (IAPManager.Instance != null) IAPManager.Instance.BuyPoints200(); }
    /// <summary>Mua gói 400 điểm (iap3 - 2 US$).</summary>
    public void BuyPoints400() { if (IAPManager.Instance != null) IAPManager.Instance.BuyPoints400(); }
    /// <summary>Mua gói 600 điểm (iap4 - 3 US$).</summary>
    public void BuyPoints600() { if (IAPManager.Instance != null) IAPManager.Instance.BuyPoints600(); }
    /// <summary>Mua gói 1000 điểm (iap5 - 5 US$).</summary>
    public void BuyPoints1000() { if (IAPManager.Instance != null) IAPManager.Instance.BuyPoints1000(); }
    /// <summary>Mua gói 2000 điểm (iap6 - 7 US$).</summary>
    public void BuyPoints2000() { if (IAPManager.Instance != null) IAPManager.Instance.BuyPoints2000(); }
    /// <summary>Mua gói 5000 điểm (iap7 - 10 US$).</summary>
    public void BuyPoints5000() { if (IAPManager.Instance != null) IAPManager.Instance.BuyPoints5000(); }

    private void OnPointsChanged(int points)
    {
        RefreshPoints();
    }

    public void RefreshPoints()
    {
        if (m_PointsText == null) return;

        if (string.IsNullOrEmpty(m_PointsTextFormat))
        {
            m_PointsTextFormat = m_PointsText.text;

            if (string.IsNullOrEmpty(m_PointsTextFormat) || !m_PointsTextFormat.Contains("{0}"))
            {
                m_PointsTextFormat = "{0}";
            }
        }

        m_PointsText.text = string.Format(m_PointsTextFormat, PointsWallet.Points);
    }
}
