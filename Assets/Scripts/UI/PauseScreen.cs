using UnityEngine;

#pragma warning disable CS0649

namespace TapTap
{
    /// <summary>
    /// Bảng tạm dừng: Resume / Restart / Home. Bật tắt nhạc và âm thanh do
    /// <see cref="AudioToggleButton"/> lo, nên panel này không giữ icon nào.
    /// Component nằm trên node LUÔN ACTIVE, panel mới là thứ được bật/tắt.
    /// </summary>
    public class PauseScreen : Entity, IInstance
    {
        [SerializeField]
        private GameObject m_PausePanel;

        [SerializeField]
        private GameObject m_PauseButton;

        private GameLogic GameLogic
        {
            get { return Main.Get<GameLogic>(); }
        }

        public bool IsOpened
        {
            get { return m_PausePanel != null && m_PausePanel.activeSelf; }
        }

        private void Start()
        {
            // Chỉ ẩn panel, KHÔNG bật nút pause: lúc này MainMenuScreen đang mở menu và
            // chính nó quyết định nút pause hiện hay không (thứ tự Start giữa hai script không xác định).
            if (m_PausePanel != null) m_PausePanel.SetActive(false);

            if (GameLogic != null) GameLogic.SetPaused(false);
        }

        public void Pause()
        {
            SoundManager.PlayButton();

            if (GameLogic != null) GameLogic.SetPaused(true);

            if (m_PausePanel != null) m_PausePanel.SetActive(true);
            if (m_PauseButton != null) m_PauseButton.SetActive(false);
        }

        public void Resume()
        {
            SoundManager.PlayButton();

            Close();
        }

        public void Restart()
        {
            SoundManager.PlayButton();

            Close();

            if (GameLogic != null) GameLogic.DoReset();
        }

        public void GoHome()
        {
            if (m_PausePanel != null) m_PausePanel.SetActive(false);

            MainMenuScreen menu = Main.Get<MainMenuScreen>();
            if (menu != null)
            {
                // GoHome() tự phát tiếng nút, bỏ pause, reset ván và mở lại menu.
                menu.GoHome();
                return;
            }

            SoundManager.PlayButton();

            Close();

            if (GameLogic != null) GameLogic.DoReset();
        }

        private void Close()
        {
            if (GameLogic != null) GameLogic.SetPaused(false);

            if (m_PausePanel != null) m_PausePanel.SetActive(false);
            if (m_PauseButton != null) m_PauseButton.SetActive(true);
        }
    }
}
