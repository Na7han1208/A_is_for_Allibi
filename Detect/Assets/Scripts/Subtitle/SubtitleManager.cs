using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextMeshProUGUI speakerText;

    [Header("Subtitle Data")]
    [SerializeField] private SubtitleSequence currentSequence;

    private Coroutine subtitleRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (subtitleText != null) subtitleText.text = "";
        if (speakerText != null) speakerText.text = "";
    }

    public void PlaySequence(SubtitleSequence sequence)
    {
        if (subtitleRoutine != null)
            StopCoroutine(subtitleRoutine);

        currentSequence = sequence;
        subtitleRoutine = StartCoroutine(PlaySubtitles());
    }

    private IEnumerator PlaySubtitles()
    {
        if (currentSequence == null || currentSequence.lines.Length == 0)
            yield break;

        float startTime = Time.time;

        foreach (var line in currentSequence.lines)
        {
            yield return new WaitForSeconds(line.startTime);

            if (speakerText != null)
                speakerText.text = line.speaker;

            if (subtitleText != null)
                subtitleText.text = line.text;

            yield return new WaitForSeconds(line.duration);
        }

        subtitleRoutine = null;
    }

    public void StopSubtitles()
    {
        if (subtitleRoutine != null)
        {
            StopCoroutine(subtitleRoutine);
            subtitleRoutine = null;
        }

        if (subtitleText != null)
            subtitleText.text = "";
        if (speakerText != null)
            speakerText.text = "";
    }
}