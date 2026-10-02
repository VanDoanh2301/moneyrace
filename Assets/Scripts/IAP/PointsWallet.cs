using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Ví coin bền vững của người chơi (PlayerPrefs).
/// Dùng chung cho coin kiếm được khi chơi và coin mua bằng IAP — chỉ MỘT số dư duy nhất.
/// </summary>
public static class PointsWallet
{
    private const string PPK_POINTS = "Coins_ball";

    private static bool m_IsLoaded;

    private static int m_Points;

    /// <summary>Số điểm thay đổi (giá trị mới).</summary>
    public static UnityAction<int> m_OnPointsChanged;

    public static int Points
    {
        get
        {
            if (!m_IsLoaded)
            {
                m_Points = PlayerPrefs.GetInt(PPK_POINTS, 0);
                m_IsLoaded = true;
            }

            return m_Points;
        }
    }

    /// <param name="flush">
    /// true = ghi thẳng xuống đĩa ngay (dùng cho giao dịch IAP).
    /// false = chỉ ghi vào PlayerPrefs in-memory, gọi Flush() một lần khi kết thúc lượt.
    /// </param>
    public static void Add(int amount, bool flush = false)
    {
        if (amount <= 0)
        {
            return;
        }

        Save(Points + amount, flush);
    }

    /// <summary>Trừ điểm nếu đủ. Trả về false nếu không đủ.</summary>
    public static bool TrySpend(int amount)
    {
        if (amount <= 0 || Points < amount)
        {
            return false;
        }

        Save(Points - amount, true);

        return true;
    }

    public static void SetPoints(int value)
    {
        Save(value < 0 ? 0 : value, true);
    }

    /// <summary>Ép ghi xuống đĩa.</summary>
    public static void Flush()
    {
        PlayerPrefs.Save();
    }

    private static void Save(int value, bool flush)
    {
        m_Points = value;
        m_IsLoaded = true;

        PlayerPrefs.SetInt(PPK_POINTS, m_Points);

        if (flush)
        {
            PlayerPrefs.Save();
        }

        if (m_OnPointsChanged != null)
        {
            m_OnPointsChanged.Invoke(m_Points);
        }
    }
}
