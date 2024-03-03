using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private float maxMusicVolume = -10f;
    public Sound[] sounds;


    private void Awake()
    {
        Instance = this;
        GetOrSetPlayerPrefs();
        foreach (Sound sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.outputAudioMixerGroup = sound.output;

            sound.source.volume = sound.volume;
            sound.source.loop = sound.loop;
            sound.source.mute = sound.mute;
            
            if (sound.playOnAwake)
            {
                Play(sound.name);
            }
        }
    }
    public void ChangeMasterVolume(float vol)
    {
        mixer.SetFloat("Master", vol);
        PlayerPrefs.SetFloat("MasterVolume", vol);
    }
    public void ChangeMusicVolume(float vol)
    {
        mixer.SetFloat("Music", vol);
        PlayerPrefs.SetFloat("MusicVolume", vol);
    }
    public void ChangeSFXVolume(float vol)
    {
        mixer.SetFloat("SFX", vol);
        PlayerPrefs.SetFloat("SFXVolume", vol);
    }
    public void Play(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);
        if (sound == null)
        {
            Debug.LogError("Sound: " + name + " not found");
            return;
        }
        sound.source.Play();
    }

    public void Stop(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);

        if (sound == null)
        {
            Debug.LogError("Sound: " + name + " not found");
        }
        sound.source.Stop();
    }
    private void GetOrSetPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("MasterVolume"))
        {
            PlayerPrefs.SetFloat("MasterVolume", maxMusicVolume);
            ChangeMasterVolume(maxMusicVolume);
        }
        else
        {
            ChangeMasterVolume(PlayerPrefs.GetFloat("MasterVolume"));
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
        }

        if (!PlayerPrefs.HasKey("MusicVolume"))
        {
            PlayerPrefs.SetFloat("MusicVolume", maxMusicVolume);
            ChangeMusicVolume(maxMusicVolume);
        }
        else
        {
            ChangeMasterVolume(PlayerPrefs.GetFloat("MusicVolume"));
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        }

        if (!PlayerPrefs.HasKey("SFXVolume"))
        {
            PlayerPrefs.SetFloat("SFXVolume", maxMusicVolume);
            ChangeSFXVolume(maxMusicVolume);
        }
        else
        {
            ChangeMasterVolume(PlayerPrefs.GetFloat("SFXVolume"));
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        }
    }
}
        
