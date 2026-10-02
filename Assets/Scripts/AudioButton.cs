

using UnityEngine;
using UnityEngine.UI;

public class AudioButton : MonoBehaviour
{
	public Sprite onSprite;

	public Sprite offSprite;

	public bool efx;

	public GameObject spriteObject;

	private void Start()
	{
		if (efx)
		{
			spriteObject.GetComponent<Image>().sprite = ((!AudioManager.Instance.IsEfxMute()) ? onSprite : offSprite);
		}
		else
		{
			spriteObject.GetComponent<Image>().sprite = ((!AudioManager.Instance.IsMusicMute()) ? onSprite : offSprite);
		}
	}

	public void MusicButtonClicked()
	{
		spriteObject.GetComponent<Image>().sprite = ((!AudioManager.Instance.IsMusicMute()) ? offSprite : onSprite);
		AudioManager.Instance.MuteMusic();
	}

	public void EfxButtonClicked()
	{
		spriteObject.GetComponent<Image>().sprite = ((!AudioManager.Instance.IsEfxMute()) ? offSprite : onSprite);
		AudioManager.Instance.MuteEfx();
	}
}
