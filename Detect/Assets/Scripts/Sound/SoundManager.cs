using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;

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


    public void Play(string name, Transform caller)
    {
        foreach (Sound s in Sounds)
        {
            if (s.name == name)
            {
                if (s.clip == null)
                {
                    Debug.LogWarning("Sound clip is null: " + name);
                    return;
                }

                GameObject tempGameObject = new GameObject("TempAudio_" + s.name);
                tempGameObject.transform.position = caller.position;
                AudioSource source = tempGameObject.AddComponent<AudioSource>();

                source.clip = s.clip;
                source.volume = s.volume;
                source.pitch = s.pitch;
                source.loop = s.loop;

                source.spatialBlend = 0f;
                source.minDistance = 1f;
                source.maxDistance = 20f;
                source.rolloffMode = AudioRolloffMode.Linear;

                source.PlayDelayed(0.01f); //just incase theres issues with instantiating the audio source on object
                Debug.Log("PLAYING SOUND" + name);
                Debug.Log($"AudioSource: {source}, Clip: {source.clip}");


                if (!s.loop)
                {
                    Destroy(source, s.clip.length);
                }
                return;
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
