using UnityEngine;

public class NR_MusicBox : MonoBehaviour
{
    public MusicBoxPuzzle mbp;

    void Update()
    {
        if (mbp.isCompleted)
        {
            StarChartManager.Instance.UnlockStar("GS3");
            SoundManager.Instance.PlayComplex("StarUnlock", transform);
            StartCoroutine(FindAnyObjectByType<TutorialHelper>().StarChartHint());
            Destroy(this);
        }
    }
}
