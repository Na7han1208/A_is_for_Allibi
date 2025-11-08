using UnityEngine;

[CreateAssetMenu(fileName = "SubtitleLine", menuName = "Subtitles/Subtitle Line")]
public class SubtitleLine : ScriptableObject
{
    [Tooltip("Optional: Name of the character speaking.")]
    public string speaker;

    [TextArea(2, 5)]
    [Tooltip("The actual subtitle text to display.")]
    public string text;

    [Tooltip("Time (in seconds) after sequence start to display this line.")]
    public float startTime;

    [Tooltip("How long this subtitle should remain visible.")]
    public float duration = 2f;
}
