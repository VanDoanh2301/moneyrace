using UnityEngine;

public enum AudioType
{
    JUMP,
    LANDING,
    GAMEOVER
}

public class SoundController : Singleton<SoundController>
{
    bool _musicEnable = true;
    bool _fxEnable = true;

    [Space(10)]
    [Range(0, 1)] [SerializeField] float _musicVolume = 1f;
    [Range(0, 1)] [SerializeField] float _fxVolume = 1f;

    [Space(10)]
    [SerializeField] AudioSource _bgMusicSource;
    [SerializeField] AudioClip _bgMusicClip;

    [Header("Sound Effect Clip :")]
    [SerializeField] AudioClip _jump;
    [SerializeField] AudioClip _landing;
    [SerializeField] AudioClip _gameover;

    GameObject oneShotGameObject;
    AudioSource oneShotAudioSource;

    private void Start()
    {
        _fxEnable = PlayerPrefs.GetInt("sfxState") == 0;
        _musicEnable = PlayerPrefs.GetInt("musicState") == 0;

        if (_musicEnable) PlayBackgroundMusic(_bgMusicClip);
    }

    public void PlayAudio(AudioType type)
    {
        // return if audio fx is disable
        if (!_fxEnable) return;

        if (oneShotGameObject == null)
        {
            oneShotGameObject = new GameObject("Sound");
            oneShotAudioSource = oneShotGameObject.AddComponent<AudioSource>();
        }

        AudioClip clip = GetClip(type);

        oneShotAudioSource.volume = _fxVolume;
        oneShotAudioSource.PlayOneShot(clip);
    }

    public void ToggleMusic(ref bool state)
    {
        _musicEnable = !_musicEnable;
        UpdateMusic();
        state = _musicEnable;
        PlayerPrefs.SetInt("musicState", _musicEnable ? 0 : 1);
    }

    public void ToggleFX(ref bool state)
    {
        _fxEnable = !_fxEnable;
        state = _fxEnable;
        PlayerPrefs.SetInt("sfxState", _fxEnable ? 0 : 1);
    }

    void UpdateMusic()
    {
        if (!_musicEnable)
            _bgMusicSource.Stop();
        else
            PlayBackgroundMusic(_bgMusicClip);
    }

    private AudioClip GetClip(AudioType audioType)
    {
        switch (audioType)
        {
            case AudioType.JUMP:
                return _jump;
            case AudioType.LANDING:
                return _landing;
            case AudioType.GAMEOVER:
                return _gameover;
            default:
                return null;
        }
    }

    private void PlayBackgroundMusic(AudioClip clip)
    {
        _bgMusicSource.Stop();
        _bgMusicSource.clip = clip;
        _bgMusicSource.volume = _musicVolume;
        _bgMusicSource.Play();
    }
}
