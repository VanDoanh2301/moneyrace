using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

namespace TapTap
{
    /// <summary>
    /// Nút bật/tắt âm thanh hoặc nhạc. Đặt được ở bất kỳ đâu (menu, pause…),
    /// mọi nút cùng loại tự đồng bộ icon với nhau qua sự kiện của <see cref="SoundManager"/>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class AudioToggleButton : MonoBehaviour
    {
        public enum Channel
        {
            Sound,
            Music
        }

        [SerializeField]
        private Channel m_Channel = Channel.Sound;

        [SerializeField]
        private Image m_Icon;

        [SerializeField]
        private Sprite m_OnSprite;

        [SerializeField]
        private Sprite m_OffSprite;

        private Button m_Button;

        private void Awake()
        {
            m_Button = GetComponent<Button>();
            m_Button.onClick.AddListener(OnClick);

            if (m_Icon == null)
            {
                m_Icon = GetComponent<Image>();
            }
        }

        private void OnEnable()
        {
            SoundManager.m_OnSoundChanged += OnChanged;
            SoundManager.m_OnMusicChanged += OnChanged;

            Refresh();
        }

        private void OnDisable()
        {
            SoundManager.m_OnSoundChanged -= OnChanged;
            SoundManager.m_OnMusicChanged -= OnChanged;
        }

        private void OnClick()
        {
            if (m_Channel == Channel.Sound)
            {
                SoundManager.ToggleSound();
            }
            else
            {
                SoundManager.ToggleMusic();
            }

            // Phát sau khi đổi: bật thì nghe thấy phản hồi, tắt thì im lặng — đúng như mong đợi.
            SoundManager.PlayButton();
        }

        private void OnChanged(bool value)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (m_Icon == null) return;

            bool on = m_Channel == Channel.Sound ? SoundManager.SoundOn : SoundManager.MusicOn;

            Sprite sprite = on ? m_OnSprite : m_OffSprite;
            if (sprite != null) m_Icon.sprite = sprite;
        }
    }
}
