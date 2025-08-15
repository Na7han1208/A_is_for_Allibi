using UnityEngine;
using System;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public Sound[] Sounds;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        // For each unique sound create a gameObject of type audio source and instatiate attributes of the sounds class
        foreach (Sound s in Sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
        }
    }


    public void Play(string name)
    {
        foreach (Sound s in Sounds)
        {
            if (s.name == name)
            {
                try
                {
                    s.source.volume = s.volume * PlayerPrefs.GetFloat("MasterVolume", 1f);
                }
                catch (Exception e)
                {
                    s.source.volume = s.volume;
                    Debug.LogWarning(e);
                }
                s.source.pitch = s.pitch;
                s.source.loop = s.loop;
                s.source.Play();
            }
        }
    }

    public bool isPlaying(string name)
    {
        foreach (Sound s in Sounds)
        {
            if (s.name == name)
            {
                Debug.Log(name + "is playing.");
                return s.source.isPlaying;
            }
        }
        return false;
    }

    public void Stop(string name)
    {
        foreach (Sound s in Sounds)
        {
            if (s.name == name)
            {
                s.source.Stop();
                return;
            }
        }
    }

    public void StopAll()
    {
        foreach (Sound s in Sounds)
        {
            s.source.Stop();
        }
    }
}
