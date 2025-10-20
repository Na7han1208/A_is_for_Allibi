using UnityEngine;
using UnityEngine.Audio;

public enum SoundType { SFX, Music }

[System.Serializable]
public class Sound {

    public string name;
    public AudioClip clip;

    [Range(0f, 1f)] public float volume;
    [Range(.1f,2f)] public float pitch;
    public bool loop;
    public SoundType type;

    [HideInInspector]
    public AudioSource source;
}
