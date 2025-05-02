using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicControl : MonoBehaviour
{
    public string musicName;
    public bool noBGM;
    public bool continueToNextScene;
    public bool stopContinueMusic;
    private void Awake()
    {
        //Debug.Log("StartMusic Awake");

    }

    void Start()
    {
        //delay 0.1s to make sure the audio manager is ready
        Invoke("ChangeMusic", 0.1f);
        

        if (continueToNextScene)
        {
            DontDestroyOnLoad( AudioManager.instance.gameObject);
        }
        if (stopContinueMusic)
        {
            //let audio manager move out of dont destroy on load
            //GameManager.instance.objectsToDestroy.Add(AudioManagerNew.instance.gameObject);
            
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ChangeMusic()
    {
        
        if (AudioManager.instance.currentMusic != musicName  && AudioManager.instance.currentMusic != null && musicName != null)
        {   
            AudioManager.instance.StopMusic();
            if (noBGM)
            {
                Debug.Log("NoBGM");
                AudioManager.instance.currentMusic = null;
                
            }
            else
            {
                AudioManager.instance.PlayMusic(musicName);
                Debug.Log("PlayMusic: " + musicName);
                AudioManager.instance.currentMusic = musicName;
            }
            
            
            
        }
        else
        {
            if (musicName != "")
            {
                Debug.Log("Already playing: " + musicName);
                AudioManager.instance.PlayMusic(musicName);
            }

        }
    }
}
