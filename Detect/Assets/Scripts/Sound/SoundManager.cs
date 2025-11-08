using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public Sound[] Sounds;

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

        foreach (Sound s in Sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.playOnAwake = false;
        }

        UpdateVolumes();
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
                source.volume = s.volume * PlayerPrefs.GetFloat("SFXVolume", 1f);
                source.pitch = s.pitch;
                source.loop = s.loop;

                source.spatialBlend = 0f;
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
        bool stopped = false;

        // Stop any persistent (non-temp) AudioSources
        foreach (Sound s in Sounds)
        {
            if (s.name == name && s.source != null && s.source.isPlaying)
            {
                s.source.Stop();
                stopped = true;
            }
        }

        // Stop and clean up any temporary sources (spawned by PlayComplex)
        for (int i = activeTempSources.Count - 1; i >= 0; i--)
        {
            AudioSource src = activeTempSources[i];
            if (src == null)
            {
                activeTempSources.RemoveAt(i);
                continue;
            }

            if (src.clip != null && (src.clip.name == name || src.gameObject.name.Contains(name)))
            {
                src.Stop();
                Destroy(src.gameObject);
                activeTempSources.RemoveAt(i);
                stopped = true;
            }
        }

        if (!stopped)
        {
            Debug.LogWarning($"[SoundManager] Tried to stop '{name}' but it was not playing or not found.");
        }
    }

    public void StopAll()
    {
        foreach (Sound s in Sounds)
        {
            s.source.Stop();
        }

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

    public void UpdateVolumes()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        foreach (Sound s in Sounds)
        {
            if (s.source == null) continue;

            // We never use the current AudioSource.volume here
            // We always start from the base Sound.volume
            if (s.loop)
                s.source.volume = s.volume * musicVolume;
            else
                s.source.volume = s.volume * sfxVolume;
        }

        for (int i = activeTempSources.Count - 1; i >= 0; i--)
        {
            AudioSource tempSource = activeTempSources[i];
            if (tempSource == null)
            {
                activeTempSources.RemoveAt(i);
                continue;
            }

            if (tempSource.loop)
                tempSource.volume = GetBaseVolume(tempSource) * musicVolume;
            else
                tempSource.volume = GetBaseVolume(tempSource) * sfxVolume;
        }
    }

    private float GetBaseVolume(AudioSource src)
    {
        foreach (Sound s in Sounds)
        {
            if (s.clip == src.clip)
            {
                return s.volume;
            }
        }
        return src.volume;
    }
}