using UnityEngine;
using TMPro;

#pragma warning disable CS0649

namespace TapTap
{
    /// <summary>
    /// Hiển thị số coin của ví bền vững (<see cref="CoinWallet"/>) trên HUD.
    /// </summary>
    public class CoinHUD : Entity, IEnableEvent, IDisableEvent
    {
        [SerializeField]
        private TextMeshProUGUI m_CoinsText;

        private string m_CoinsTextFormat;

        private void Start()
        {
            Refresh();
        }

        public void OnEnableEvent()
        {
            CoinWallet.m_OnCoinsChanged += OnCoinsChanged;
        }

        public void OnDisableEvent()
        {
            CoinWallet.m_OnCoinsChanged -= OnCoinsChanged;
        }

        private void OnCoinsChanged(int coins)
        {
            Refresh();
        }

        private void Refresh()
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
}
