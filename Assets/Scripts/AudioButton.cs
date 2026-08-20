using UnityEngine;
using UnityEngine.UI;

public class AudioButton : MonoBehaviour
{
    public bool efx;
    public Sprite musicOnSprite, musicOffSprite, efxOnSprite, efxOffSprite;
    public Image spriteButton;


    //set button sprite
    void Start()
    {
        SetButton();
    }

    public void MusicButtonClicked()
    {
        AudioManager_jump.Instance.MuteMusic();
        AudioManager_jump.Instance.PlayEffects(AudioManager_jump.Instance.buttonClick);
        SetButton();
    }

    public void EfxButtonClicked()
    {
        AudioManager_jump.Instance.MuteEfx();
        AudioManager_jump.Instance.PlayEffects(AudioManager_jump.Instance.buttonClick);
        SetButton();
    }

    void SetButton()
    {
        if ((!AudioManager_jump.Instance.IsMusicMute() && !efx) || (!AudioManager_jump.Instance.IsEfxMute() && efx))
            if (efx)
                spriteButton.sprite = efxOnSprite;
            else
                spriteButton.sprite = musicOnSprite;
        else
            if (efx)
                spriteButton.sprite = efxOffSprite;
            else
                spriteButton.sprite = musicOffSprite;
    }
}
