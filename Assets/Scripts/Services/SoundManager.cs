using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Quản lý âm lượng/tắt tiếng toàn game qua AudioListener.volume (áp dụng cho mọi
/// AudioSource.Play() có sẵn trong project mà không cần sửa từng chỗ gọi).
/// Bền vững qua PlayerPrefs, singleton DontDestroyOnLoad — cùng kiểu với GameManager/Adcontrol.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    private const string PPK_VOLUME = "RingGame_SoundVolume";
    private const string PPK_MUTED = "RingGame_SoundMuted";

    private static bool m_IsLoaded;
    private static float m_Volume;
    private static bool m_Muted;

    /// <summary>Volume thực tế đã áp dụng (0..1, không tính mute).</summary>
    public static UnityAction<float> m_OnVolumeChanged;
    /// <summary>Trạng thái mute thay đổi.</summary>
    public static UnityAction<bool> m_OnMutedChanged;

    public static float Volume
    {
        get
        {
            Load();
            return m_Volume;
        }
    }

    public static bool Muted
    {
        get
        {
            Load();
            return m_Muted;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        Apply();
    }

    private static void Load()
    {
        if (m_IsLoaded) return;

        m_Volume = PlayerPrefs.GetFloat(PPK_VOLUME, 1f);
        m_Muted = PlayerPrefs.GetInt(PPK_MUTED, 0) == 1;
        m_IsLoaded = true;
    }

    public static void SetVolume(float value)
    {
        Load();

        m_Volume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(PPK_VOLUME, m_Volume);
        PlayerPrefs.Save();

        Apply();

        if (m_OnVolumeChanged != null) m_OnVolumeChanged.Invoke(m_Volume);
    }

    public static void SetMuted(bool value)
    {
        Load();

        m_Muted = value;
        PlayerPrefs.SetInt(PPK_MUTED, m_Muted ? 1 : 0);
        PlayerPrefs.Save();

        Apply();

        if (m_OnMutedChanged != null) m_OnMutedChanged.Invoke(m_Muted);
    }

    public static void ToggleMuted()
    {
        SetMuted(!Muted);
    }

    private static void Apply()
    {
        Load();

        AudioListener.volume = m_Muted ? 0f : m_Volume;
    }
}
