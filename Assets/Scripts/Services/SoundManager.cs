using UnityEngine;
using UnityEngine.Events;

#pragma warning disable CS0649

namespace TapTap
{
    /// <summary>
    /// Phát nhạc nền + hiệu ứng âm thanh. Truy cập qua <c>Main.Get&lt;SoundManager&gt;()</c>
    /// hoặc các hàm static tiện dụng (<see cref="PlayTap"/>, <see cref="PlayCoin"/>…),
    /// các hàm static tự bỏ qua nếu chưa có SoundManager trong scene.
    /// </summary>
    public class SoundManager : Entity, IInstance, IAwakeEvent
    {
        private const string PPK_SOUND = "TapTap_SoundOn";
        private const string PPK_MUSIC = "TapTap_MusicOn";

        [Header("Clips")]

        [SerializeField]
        private AudioClip m_MusicLoop;

        [SerializeField]
        private AudioClip m_Tap;

        [SerializeField]
        private AudioClip m_Coin;

        [SerializeField]
        private AudioClip m_GameOver;

        [SerializeField]
        private AudioClip m_Button;

        [SerializeField]
        private AudioClip m_Purchase;

        [SerializeField]
        private AudioClip m_Best;

        [Header("Volume")]

        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float m_MusicVolume = 0.35f;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float m_SfxVolume = 0.7f;

        [Tooltip("Số AudioSource dùng luân phiên cho SFX (cho phép chồng tiếng).")]
        [SerializeField]
        private int m_SfxVoices = 6;

        private AudioSource m_MusicSource;

        private AudioSource[] m_SfxSources;

        private int m_SfxIndex;

        /// <summary>Coin nhặt liên tiếp thì lên cao dần cho đã tai; reset khi hết ván.</summary>
        private int m_CoinStreak;

        public static UnityAction<bool> m_OnSoundChanged;

        public static UnityAction<bool> m_OnMusicChanged;

        public static bool SoundOn
        {
            get { return PlayerPrefs.GetInt(PPK_SOUND, 1) == 1; }
            set
            {
                PlayerPrefs.SetInt(PPK_SOUND, value ? 1 : 0);
                PlayerPrefs.Save();

                if (m_OnSoundChanged != null) m_OnSoundChanged.Invoke(value);
            }
        }

        public static bool MusicOn
        {
            get { return PlayerPrefs.GetInt(PPK_MUSIC, 1) == 1; }
            set
            {
                PlayerPrefs.SetInt(PPK_MUSIC, value ? 1 : 0);
                PlayerPrefs.Save();

                if (m_OnMusicChanged != null) m_OnMusicChanged.Invoke(value);

                SoundManager instance = Instance;
                if (instance != null) instance.ApplyMusicState();
            }
        }

        private static SoundManager Instance
        {
            get { return Main.Singleton != null ? Main.Get<SoundManager>() : null; }
        }

        public void OnAwakeEvent()
        {
            m_MusicSource = gameObject.AddComponent<AudioSource>();
            m_MusicSource.clip = m_MusicLoop;
            m_MusicSource.loop = true;
            m_MusicSource.playOnAwake = false;
            m_MusicSource.volume = m_MusicVolume;

            if (m_SfxVoices < 1) m_SfxVoices = 1;

            m_SfxSources = new AudioSource[m_SfxVoices];
            for (int i = 0; i < m_SfxVoices; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.volume = m_SfxVolume;

                m_SfxSources[i] = source;
            }

            ApplyMusicState();
        }

        private void ApplyMusicState()
        {
            if (m_MusicSource == null || m_MusicSource.clip == null) return;

            if (MusicOn)
            {
                if (!m_MusicSource.isPlaying) m_MusicSource.Play();
            }
            else
            {
                m_MusicSource.Pause();
            }
        }

        /// <param name="pitch">Cao độ; 1 = nguyên bản.</param>
        public void Play(AudioClip clip, float pitch = 1.0f, float volumeScale = 1.0f)
        {
            if (clip == null || !SoundOn || m_SfxSources == null) return;

            AudioSource source = m_SfxSources[m_SfxIndex];
            m_SfxIndex = (m_SfxIndex + 1) % m_SfxSources.Length;

            source.pitch = pitch;
            source.volume = m_SfxVolume;
            // PlayOneShot nhân thêm volumeScale với source.volume, nên không set volume kèm ở trên.
            source.PlayOneShot(clip, volumeScale);
        }

        // ----- API static: gọi được từ mọi nơi, tự no-op nếu chưa có SoundManager -----

        public static void PlayTap()
        {
            SoundManager s = Instance;
            if (s != null) s.Play(s.m_Tap);
        }

        public static void PlayCoin()
        {
            SoundManager s = Instance;
            if (s == null) return;

            // Mỗi coin liên tiếp cao hơn nửa cung, tối đa 1 quãng tám.
            float pitch = Mathf.Pow(1.05946f, Mathf.Min(s.m_CoinStreak, 12));
            s.m_CoinStreak++;

            s.Play(s.m_Coin, pitch);
        }

        public static void ResetCoinStreak()
        {
            SoundManager s = Instance;
            if (s != null) s.m_CoinStreak = 0;
        }

        public static void PlayGameOver()
        {
            SoundManager s = Instance;
            if (s != null) s.Play(s.m_GameOver);
        }

        public static void PlayButton()
        {
            SoundManager s = Instance;
            if (s != null) s.Play(s.m_Button);
        }

        public static void PlayPurchase()
        {
            SoundManager s = Instance;
            if (s != null) s.Play(s.m_Purchase);
        }

        public static void PlayBest()
        {
            SoundManager s = Instance;
            if (s != null) s.Play(s.m_Best);
        }

        public static void ToggleSound()
        {
            SoundOn = !SoundOn;
        }

        public static void ToggleMusic()
        {
            MusicOn = !MusicOn;
        }
    }
}
