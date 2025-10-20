using UnityEngine;

public class NR_TracingPuzzle : MonoBehaviour
{
    public TracingPuzzle tp;
    public SubtitleSequence subs;
    private bool isSolved = false;

    void Update()
    {
        if (tp.CheckCompletion())
        {
            if (isSolved) return;
            StarChartManager.Instance.UnlockStar("GS2");
            SoundManager.Instance.PlayComplex("StarUnlock", transform);
            StartCoroutine(FindAnyObjectByType<TutorialHelper>().StarChartHint());
            SubtitleManager.Instance.PlaySequence(subs);
            isSolved = true;
        }
    }
}
