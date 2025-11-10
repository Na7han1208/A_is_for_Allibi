using UnityEngine;
using System.Collections;

public class RainSoundManager : MonoBehaviour
{
    public bool systemActive = false;

    [SerializeField] private AudioSource outdoorRain;
    [SerializeField] private AudioSource indoorRain;
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float minTriggerDelay = 3f;
    [SerializeField] private float outdoorVolume = 1f;
    [SerializeField] private float indoorVolume = 1f;
    
    private bool inClassroom = false;
    private bool isFading = false;
    private float lastTriggerTime;
    private float sfxMult;

    void Start()
    {
        if (!outdoorRain.isPlaying) outdoorRain.Play();
        if (!indoorRain.isPlaying) indoorRain.Play();
        float sfxMult = PlayerPrefs.GetFloat("SFXVolume", 1f);
        indoorRain.volume = 0f;
        outdoorRain.volume = 0f;

        indoorRain.loop = true;
        outdoorRain.loop = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!FindFirstObjectByType<TimeLineManager>().puzzleSolved) return;
        if (!systemActive) return;
        if (Time.time - lastTriggerTime < minTriggerDelay) return;
        lastTriggerTime = Time.time;

        if (other.CompareTag("Playground") && inClassroom)
        {
            inClassroom = false;
            StartCoroutine(Crossfade(false));
        }
        else if (other.CompareTag("Classroom") && !inClassroom)
        {
            inClassroom = true;
            StartCoroutine(Crossfade(true));
        }
    }

    void Update()
    {
        
        if (!systemActive)
        {
            sfxMult = 0;
        }
        else
        {
            sfxMult = PlayerPrefs.GetFloat("SFXVolume", 1);
        }
    }

    private IEnumerator Crossfade(bool toClassroom)
    {
        isFading = true;
        float elapsed = 0f;
        float startOutdoor = outdoorRain.volume;
        float startIndoor = indoorRain.volume;
        float targetOutdoor = toClassroom ? 0f : outdoorVolume * sfxMult;
        float targetIndoor = toClassroom ? indoorVolume * sfxMult : 0f;

        while (elapsed < fadeDuration)
        {
            if (!systemActive)
            {
                isFading = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            outdoorRain.volume = Mathf.Lerp(startOutdoor, targetOutdoor, t);
            indoorRain.volume = Mathf.Lerp(startIndoor, targetIndoor, t);
            yield return null;
        }

        outdoorRain.volume = targetOutdoor;
        indoorRain.volume = targetIndoor;
        isFading = false;
    }

    public void SetSystemActive(bool state)
    {
        systemActive = state;
    }
}
