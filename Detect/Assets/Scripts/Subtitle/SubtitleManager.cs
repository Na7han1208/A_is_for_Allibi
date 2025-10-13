using UnityEngine;
using TMPro;
using System.Collections;

public class SubtitleManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextMeshProUGUI speakerText;

    [Header("Subtitle Data")]
    [SerializeField] private SubtitleSequence currentSequence;

    private Coroutine subtitleRoutine;

    private void Awake()
    {
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
            // Wait until it's time to show this line
            yield return new WaitUntil(() => Time.time - startTime >= line.startTime);

            // Update UI
            if (speakerText != null)
                speakerText.text = line.speaker;

            if (subtitleText != null)
                subtitleText.text = line.text;

            // Keep the subtitle on screen for its duration
            yield return new WaitForSeconds(line.duration);

            // Clear after each line
            if (subtitleText != null)
                subtitleText.text = "";
            if (speakerText != null)
                speakerText.text = "";
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
