using UnityEngine;

[CreateAssetMenu(fileName = "SubtitleSequence", menuName = "Subtitles/Subtitle Sequence")]
public class SubtitleSequence : ScriptableObject
{
    [Tooltip("List of subtitle lines that make up this sequence.")]
    public SubtitleLine[] lines;
}
