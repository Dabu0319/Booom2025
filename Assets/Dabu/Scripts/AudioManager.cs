using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;


[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    public bool loop;
    public float volume;

    
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    
    
    public Sound[] musicSounds, sfxSounds;
    
    
    public AudioSource musicSource, sfxSource;

    public String currentMusic;
    private void Awake()
    {
        
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        PlayMusic(currentMusic);
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(musicSounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        musicSource.clip = s.clip;
        musicSource.loop = s.loop;
        musicSource.volume = s.volume;
        musicSource.Play();
    }
    
    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxSounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }
        sfxSource.clip = s.clip;
        sfxSource.loop = s.loop;
        sfxSource.volume = s.volume;
        sfxSource.Play();
    }
    
    public void StopMusic()
    {
        musicSource.Stop();
    }
    
    public void StopMusicByTime(float time)
    {
        //turn down the volume of the music over time and stop it
        musicSource.volume = Mathf.Lerp(musicSource.volume, 0, time);
        if (musicSource.volume <= 0.1f)
        {
            musicSource.Stop();
        }
        
    }
    
    //stop sfx
    public void StopSFX()
    {
        sfxSource.Stop();
    }
    
    public void StartMusicByTime(float time)
    {
        //turn up the volume of the music over time
        musicSource.Play();
        
        musicSource.volume = Mathf.Lerp(musicSource.volume, 1, time);
    }
    
    
    public void ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
    }
    public void ToggleSFX()
    {
        sfxSource.mute = !sfxSource.mute;
    }
    public void MusicVolume(float volume)
    {
        musicSource.volume = volume;
    }
    public void SFXVolume(float volume)
    {
        sfxSource.volume = volume;
    }
    
    public void FadeOutAudio(float fadeOutTime)
    {
        // 通过 DOTween 逐渐减小音量直到 0，然后停止播放
        DOTween.To(() => musicSource.volume, x => musicSource.volume = x, 0f, fadeOutTime)
            .OnComplete(() => musicSource.Stop());
        
        //fade out sfx
        DOTween.To(() => sfxSource.volume, x => sfxSource.volume = x, 0f, fadeOutTime) ;
    }
    
    public void PlayRandomSFX()
    {
        if (sfxSounds.Length == 0)
        {
            Debug.LogWarning("No SFX sounds available!");
            return;
        }

        // 随机选择一个 SFX
        int randomIndex = UnityEngine.Random.Range(0, sfxSounds.Length);
        Sound randomSound = sfxSounds[randomIndex];

        // 播放随机选择的 SFX
        sfxSource.clip = randomSound.clip;
        sfxSource.volume = randomSound.volume;
        sfxSource.loop = randomSound.loop;
        sfxSource.Play();
    }
}
