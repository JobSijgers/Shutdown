using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string name;

    public AudioClip clip;

    public AudioMixerGroup output;

    [Range(0f, 1f)]
    public float volume;

    public bool loop;

    public bool mute;

    public bool playOnAwake;
    [HideInInspector]
    public AudioSource source;
}
