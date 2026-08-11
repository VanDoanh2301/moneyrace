using UnityEngine;
using UnityEngine.Events;

namespace TapTap
{
    /// <summary>
    /// Ví coin bền vững của người chơi (PlayerPrefs).
    /// Dùng chung cho coin nhặt trong ván và coin mua bằng IAP.
    /// </summary>
    public static class CoinWallet
    {
        private const string PPK_COINS = "TapTap_Coins";

        private static bool m_IsLoaded;

        private static int m_Coins;

        /// <summary>Số coin thay đổi (giá trị mới).</summary>
        public static UnityAction<int> m_OnCoinsChanged;

        public static int Coins
        {
            get
            {
                if (!m_IsLoaded)
                {
                    m_Coins = PlayerPrefs.GetInt(PPK_COINS, 0);
                    m_IsLoaded = true;
                }

                return m_Coins;
            }
        }

        /// <param name="flush">
        /// true = ghi thẳng xuống đĩa ngay (dùng cho giao dịch IAP).
        /// false = chỉ ghi vào PlayerPrefs in-memory, Unity tự flush khi pause/quit —
        /// dùng cho coin nhặt trong ván để không gây khựng frame.
        /// </param>
        public static void Add(int amount, bool flush = false)
        {
            if (amount <= 0)
            {
                return;
            }

            Save(Coins + amount, flush);
        }

        /// <summary>Trừ coin nếu đủ. Trả về false nếu không đủ.</summary>
        public static bool TrySpend(int amount)
        {
            if (amount <= 0 || Coins < amount)
            {
                return false;
            }

            Save(Coins - amount, true);

            return true;
        }

        public static void SetCoins(int value)
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
            m_Coins = value;
            m_IsLoaded = true;

            PlayerPrefs.SetInt(PPK_COINS, m_Coins);

            if (flush)
            {
                PlayerPrefs.Save();
            }

            if (m_OnCoinsChanged != null)
            {
                m_OnCoinsChanged.Invoke(m_Coins);
            }
        }
    }
}
