using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public Sound[] Sounds;

    // keep track of dynamically spawned AudioSources
    private List<AudioSource> activeTempSources = new List<AudioSource>();

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

        // prepare default sources for each sound
        foreach (Sound s in Sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.playOnAwake = false;
        }
    }

    public void PlayComplex(string name, Transform caller)
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

                // spatial audio config
                source.spatialBlend = 0f; // i know its not 3d rn but that a later problem
                source.minDistance = 1f;
                source.maxDistance = 20f;
                source.rolloffMode = AudioRolloffMode.Linear;

                source.Play();
                activeTempSources.Add(source);

                if (!s.loop)
                {
                    Destroy(tempGameObject, s.clip.length + 0.5f);
                }

                return;
            }
        }

        Debug.LogWarning("Sound not found: " + name);
    }

    public bool IsPlaying(string name)
    {
        foreach (Sound s in Sounds)
        {
            if (s.name == name)
            {
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

        // also stop matching temp sources
        for (int i = activeTempSources.Count - 1; i >= 0; i--)
        {
            if (activeTempSources[i] == null)
            {
                activeTempSources.RemoveAt(i);
                continue;
            }

            if (activeTempSources[i].clip != null && activeTempSources[i].clip.name == name)
            {
                activeTempSources[i].Stop();
                Destroy(activeTempSources[i].gameObject);
                activeTempSources.RemoveAt(i);
            }
        }
    }

    public void StopAll()
    {
        // stop static sounds
        foreach (Sound s in Sounds)
        {
            s.source.Stop();
        }

        // dtop and destroy temp sounds
        for (int i = activeTempSources.Count - 1; i >= 0; i--)
        {
            if (activeTempSources[i] == null)
            {
                activeTempSources.RemoveAt(i);
                continue;
            }

            activeTempSources[i].Stop();
            Destroy(activeTempSources[i].gameObject);
            activeTempSources.RemoveAt(i);
        }
    }
}