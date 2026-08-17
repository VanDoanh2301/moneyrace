using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649

/// <summary>
/// Màn Settings: chỉnh âm lượng + tắt/bật tiếng, đọc/ghi qua SoundManager.
/// Cùng cấu trúc show/close với ShopScreen (component nằm trên node luôn active).
/// </summary>
public class SettingsScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject m_SettingsPanel;

    [SerializeField]
    private GameObject m_SettingsButton;

    [SerializeField]
    private Slider m_VolumeSlider;

    [SerializeField]
    private Toggle m_MuteToggle;

    public bool IsOpened
    {
        get { return m_SettingsPanel != null && m_SettingsPanel.activeSelf; }
    }

    private void Start()
    {
        Close(); // im lặng, giống ShopScreen.Start()
    }

    private void OnEnable()
    {
        if (m_VolumeSlider != null) m_VolumeSlider.onValueChanged.AddListener(OnSliderChanged);
        if (m_MuteToggle != null) m_MuteToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnDisable()
    {
        if (m_VolumeSlider != null) m_VolumeSlider.onValueChanged.RemoveListener(OnSliderChanged);
        if (m_MuteToggle != null) m_MuteToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    public void ShowSettingsScreen()
    {
        if (m_SettingsPanel != null) m_SettingsPanel.SetActive(true);
        if (m_SettingsButton != null) m_SettingsButton.SetActive(false);

        RefreshFromSoundManager();
    }

    public void CloseSettingsScreen()
    {
        Close();
    }

    private void Close()
    {
        if (m_SettingsPanel != null) m_SettingsPanel.SetActive(false);
        if (m_SettingsButton != null) m_SettingsButton.SetActive(true);
    }

    public void ToggleSettingsScreen()
    {
        if (IsOpened)
            CloseSettingsScreen();
        else
            ShowSettingsScreen();
    }

    private void OnSliderChanged(float value)
    {
        SoundManager.SetVolume(value);
    }

    private void OnToggleChanged(bool isOn)
    {
        SoundManager.SetMuted(isOn);
    }

    /// <summary>Đồng bộ slider/toggle từ SoundManager mà không tự kích hoạt lại listener.</summary>
    private void RefreshFromSoundManager()
    {
        if (m_VolumeSlider != null) m_VolumeSlider.SetValueWithoutNotify(SoundManager.Volume);
        if (m_MuteToggle != null) m_MuteToggle.SetIsOnWithoutNotify(SoundManager.Muted);
    }
}
