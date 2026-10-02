
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	[Header("Audio Sources")]
	public AudioSource efxSource;

	public AudioSource musicSource;

	[Header("Background Music")]
	public AudioClip bgMusic;

	public AudioClip gameMusic;

	[Header("Sound Effects")]
	public AudioClip buttonClick;

	public AudioClip highscore;

	public AudioClip sameColor;

	public AudioClip wrongColor;

	public AudioClip gameOver;

	private bool muteMusic;

	private bool muteEfx;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
		muteMusic = ((PlayerPrefs.GetInt("MuteMusic") == 1) ? true : false);
		muteEfx = ((PlayerPrefs.GetInt("MuteEfx") == 1) ? true : false);
		PlayMusic(bgMusic);
	}

	public void PlayMusic(AudioClip clip)
	{
		if (!muteMusic)
		{
			musicSource.clip = clip;
			if (!musicSource.isPlaying)
			{
				musicSource.Play();
			}
		}
	}

	private void StopMusic()
	{
		musicSource.Stop();
	}

	public void PlayEffects(AudioClip clip)
	{
		if (!muteEfx)
		{
			efxSource.PlayOneShot(clip);
		}
	}

	public void MuteMusic()
	{
		if (muteMusic)
		{
			muteMusic = false;
			PlayMusic(bgMusic);
			PlayerPrefs.SetInt("MuteMusic", 0);
		}
		else
		{
			muteMusic = true;
			StopMusic();
			PlayerPrefs.SetInt("MuteMusic", 1);
		}
	}

	public void MuteEfx()
	{
		if (muteEfx)
		{
			PlayerPrefs.SetInt("MuteEfx", 0);
		}
		else
		{
			PlayerPrefs.SetInt("MuteEfx", 1);
		}
		muteEfx = !muteEfx;
	}

	public bool IsMusicMute()
	{
		return muteMusic;
	}

	public bool IsEfxMute()
	{
		return muteEfx;
	}
}
