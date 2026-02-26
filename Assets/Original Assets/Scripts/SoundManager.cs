using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioSource music;
    public AudioSource sound;
    public AudioClip[] audioClip;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnOffMusic(float volume)
    {
        music.volume = volume;
    }
    public void OnOffSound(float volume)
    {
        sound.volume = volume;
    }
    public void PlaySound(int idSound)
    {
        sound.PlayOneShot(audioClip[idSound], 1f);
    }
    public void OpenKindButton()
    {
        SoundManager.instance.PlaySound(2);
    }

    public void CloseKindButton()
    {
        SoundManager.instance.PlaySound(1);
    }
}
