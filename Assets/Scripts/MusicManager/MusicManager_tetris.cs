using UnityEngine;

public class MusicManager_tetris : MonoBehaviour
{
    private static MusicManager_tetris instance;
    [SerializeField]
    private AudioSourceHandler _musicHandler;
    [SerializeField]
    private AudioSourceHandler _sfxHandler;

    public IAudioSource MusicHandler { get => _musicHandler; }
    public IAudioSource SfxHandler { get => _sfxHandler; }

    public static MusicManager_tetris Instance { get { return instance; } }
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
           // DontDestroyOnLoad(gameObject);

            _musicHandler.playClipSelf("Random");
        }
        else
        {
            DestroyImmediate(gameObject);
        }

    }

}
