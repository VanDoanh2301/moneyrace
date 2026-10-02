using UnityEngine;
using TMPro;

#pragma warning disable CS0649

/// <summary>
/// Panel Shop IAP. Port từ ShopPanel_tetris của game Tetra Rush — MonoBehaviour thuần
/// (không UIScreen/DOTween) để hợp với cách bật/tắt panel bằng SetActive của
/// <see cref="UIManager"/>.
/// Khác bản gốc ở chỗ ẩn/hiện một MẢNG object khi mở shop, vì Ball có 2 nút Shop
/// (MainMenu + GameOver) chứ không phải một.
/// </summary>
public class ShopPanel : MonoBehaviour
{
    /// <summary>
    /// Shop đang mở hay không. <see cref="UIManager.Update"/> cần cờ này: ở GameState.MENU
    /// mọi cú bấm không trúng Button đều start game, mà nền shop chỉ là Image (không Button)
    /// nên bấm vùng trống trong shop sẽ start game sau lưng shop.
    /// </summary>
    public static bool IsOpen { get; private set; }

    [SerializeField]
    private TextMeshProUGUI m_PointsText;

    [Header("Ẩn đi trong lúc shop đang mở (HUD coin, các nút Shop)")]
    [SerializeField]
    private GameObject[] m_HideWhileOpen;

    private string m_PointsTextFormat;

    private void OnEnable()
    {
        PointsWallet.m_OnPointsChanged += OnPointsChanged;

        IsOpen = true;

        RefreshPoints();
        RefreshIapPrices();
    }

    private void OnDisable()
    {
        PointsWallet.m_OnPointsChanged -= OnPointsChanged;

        IsOpen = false;
    }

    /// <summary>Mở shop (gọi từ nút Shop trên MainMenu / GameOver).</summary>
    public void Open()
    {
        // Set thẳng ở đây chứ không chỉ dựa vào OnEnable: OnEnable không chạy ở edit mode,
        // và cờ này phải đúng ngay cả khi panel đã active sẵn.
        IsOpen = true;

        SetHiddenObjectsActive(false);

        gameObject.SetActive(true);

        RefreshPoints();
        RefreshIapPrices();
    }

    /// <summary>Đóng shop (gọi từ nút Back).</summary>
    public void Close()
    {
        IsOpen = false;

        SetHiddenObjectsActive(true);

        gameObject.SetActive(false);
    }

    private void SetHiddenObjectsActive(bool value)
    {
        if (m_HideWhileOpen == null) return;

        for (int i = 0; i < m_HideWhileOpen.Length; i++)
        {
            if (m_HideWhileOpen[i] != null)
                m_HideWhileOpen[i].SetActive(value);
        }
    }

    /// <summary>Refresh giá từng gói theo productId (tránh hiện placeholder / giá cũ khi mở shop).</summary>
    private void RefreshIapPrices()
    {
        var buttons = GetComponentsInChildren<IAPButton>(true);
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].RefreshPrice();
    }

    /// <summary>Khôi phục giao dịch (iOS). Trên Google Play, Unity IAP tự khôi phục khi init.</summary>
    public void RestorePurchases()
    {
        if (IAPManager.Instance != null)
            IAPManager.Instance.RestorePurchases();
    }

    // ----- IAP: gọi từ nút Buy trong Shop (ủy quyền cho IAPManager) -----

    /// <summary>Mua gói 100 coin (iap1 - 0,50 US$).</summary>
    public void BuyPoints100() { if (IAPManager.Instance != null) IAPManager.Instance.BuyPoints100(); }
    /// <summary>Mua gói 200 coin (iap2 - 1 US$).</summary>
    public void BuyPoints200() { if (IAPManager.Instance != null) IAPManager.Instance.BuyPoints200(); }
    /// <summary>Mua gói 400 coin (iap3 - 2 US$).</summary>
    public void BuyPoints400() { if (IAPManager.Instance != null) IAPManager.Instance.BuyPoints400(); }
    /// <summary>Mua gói 600 coin (iap4 - 3 US$).</summary>
    public void BuyPoints600() { if (IAPManager.Instance != null) IAPManager.Instance.BuyPoints600(); }
    /// <summary>Mua gói 1000 coin (iap5 - 5 US$).</summary>
    public void BuyPoints1000() { if (IAPManager.Instance != null) IAPManager.Instance.BuyPoints1000(); }
    /// <summary>Mua gói 2000 coin (iap6 - 7 US$).</summary>
    public void BuyPoints2000() { if (IAPManager.Instance != null) IAPManager.Instance.BuyPoints2000(); }
    /// <summary>Mua gói 5000 coin (iap7 - 10 US$).</summary>
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
