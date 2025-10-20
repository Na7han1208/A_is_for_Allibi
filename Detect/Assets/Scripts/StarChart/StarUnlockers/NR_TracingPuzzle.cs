using UnityEngine;

public class NR_TracingPuzzle : MonoBehaviour
{
    public TracingPuzzle tp;

    void Update()
    {
        if (tp.CheckCompletion())
        {
            StarChartManager.Instance.UnlockStar("GS2");
            SoundManager.Instance.PlayComplex("StarUnlock", transform);
            FindAnyObjectByType<TutorialHelper>().StarChartHint();
            Destroy(this);
        }
    }
}
