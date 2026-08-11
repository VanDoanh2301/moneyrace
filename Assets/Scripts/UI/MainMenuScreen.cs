using UnityEngine;
using TMPro;

#pragma warning disable CS0649

namespace TapTap
{
    /// <summary>
    /// Màn hình chính. Component nằm trên node LUÔN ACTIVE, panel mới là thứ được bật/tắt.
    /// Mở lúc vào game; bấm PLAY thì ẩn đi để lộ gameplay ("TAP TO PLAY").
    /// </summary>
    public class MainMenuScreen : Entity, IInstance, IEnableEvent, IDisableEvent
    {
        [SerializeField]
        private GameObject m_MenuPanel;

        [Tooltip("Các object chỉ hiện khi đang chơi (nút pause…). Ẩn khi menu mở.")]
        [SerializeField]
        private GameObject[] m_GameplayOnly;

        [Tooltip("Các object chỉ hiện khi ở menu (nút shop…). Ẩn khi vào chơi.")]
        [SerializeField]
        private GameObject[] m_MenuOnly;

        [SerializeField]
        private TextMeshProUGUI m_BestScoreText;

        private string m_BestScoreFormat;

        private GameLogic GameLogic
        {
            get { return Main.Get<GameLogic>(); }
        }

        public bool IsOpened
        {
            get { return m_MenuPanel != null && m_MenuPanel.activeSelf; }
        }

        private void Start()
        {
            Show();
        }

        public void OnEnableEvent()
        {
            BestScore.m_OnBestScoreChanged += OnBestScoreChanged;
        }

        public void OnDisableEvent()
        {
            BestScore.m_OnBestScoreChanged -= OnBestScoreChanged;
        }

        public void Show()
        {
            if (m_MenuPanel != null) m_MenuPanel.SetActive(true);

            SetActive(m_GameplayOnly, false);
            SetActive(m_MenuOnly, true);

            RefreshBestScore();
        }

        /// <summary>Nút PLAY: đóng menu, ván mới bắt đầu khi người chơi chạm màn hình.</summary>
        public void Play()
        {
            SoundManager.PlayButton();

            if (m_MenuPanel != null) m_MenuPanel.SetActive(false);

            SetActive(m_GameplayOnly, true);
            SetActive(m_MenuOnly, false);

            if (GameLogic != null)
            {
                GameLogic.SetPaused(false);
                GameLogic.DoReset();
            }
        }

        /// <summary>Về màn hình chính (gọi từ Pause hoặc Game Over).</summary>
        public void GoHome()
        {
            SoundManager.PlayButton();

            if (GameLogic != null)
            {
                GameLogic.SetPaused(false);
                GameLogic.DoReset();
            }

            Show();
        }

        private static void SetActive(GameObject[] objects, bool active)
        {
            if (objects == null) return;

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    objects[i].SetActive(active);
                }
            }
        }

        private void OnBestScoreChanged(int value)
        {
            RefreshBestScore();
        }

        private void RefreshBestScore()
        {
            if (m_BestScoreText == null) return;

            if (string.IsNullOrEmpty(m_BestScoreFormat))
            {
                m_BestScoreFormat = m_BestScoreText.text;

                if (string.IsNullOrEmpty(m_BestScoreFormat) || !m_BestScoreFormat.Contains("{0}"))
                {
                    m_BestScoreFormat = "BEST {0}";
                }
            }

            m_BestScoreText.text = string.Format(m_BestScoreFormat, BestScore.Value);
        }
    }
}
