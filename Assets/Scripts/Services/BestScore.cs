using UnityEngine;
using UnityEngine.Events;

namespace TapTap
{
    /// <summary>
    /// Điểm cao nhất, lưu bền vững qua PlayerPrefs. Cùng kiểu với <see cref="CoinWallet"/>.
    /// </summary>
    public static class BestScore
    {
        private const string PPK_BEST = "TapTap_BestScore";

        private static bool m_IsLoaded;

        private static int m_Value;

        /// <summary>Kỷ lục mới được thiết lập (giá trị mới).</summary>
        public static UnityAction<int> m_OnBestScoreChanged;

        public static int Value
        {
            get
            {
                if (!m_IsLoaded)
                {
                    m_Value = PlayerPrefs.GetInt(PPK_BEST, 0);
                    m_IsLoaded = true;
                }

                return m_Value;
            }
        }

        /// <summary>Ghi nhận điểm cuối ván. Trả về true nếu đó là kỷ lục mới.</summary>
        public static bool TrySet(int score)
        {
            if (score <= Value)
            {
                return false;
            }

            m_Value = score;
            m_IsLoaded = true;

            PlayerPrefs.SetInt(PPK_BEST, m_Value);
            PlayerPrefs.Save();

            if (m_OnBestScoreChanged != null)
            {
                m_OnBestScoreChanged.Invoke(m_Value);
            }

            return true;
        }

        public static void Clear()
        {
            m_Value = 0;
            m_IsLoaded = true;

            PlayerPrefs.SetInt(PPK_BEST, 0);
            PlayerPrefs.Save();

            if (m_OnBestScoreChanged != null)
            {
                m_OnBestScoreChanged.Invoke(0);
            }
        }
    }
}
