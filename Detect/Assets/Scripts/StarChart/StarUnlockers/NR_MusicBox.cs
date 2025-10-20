using UnityEngine;

public class NR_MusicBox : MonoBehaviour
{
    public MusicBoxPuzzle mbp;
    private bool isSolved = false;

    void Update()
    {
        if (mbp.isCompleted)
        {
            if (isSolved) return;
            StarChartManager.Instance.UnlockStar("GS3");
            SoundManager.Instance.PlayComplex("StarUnlock", transform);
            StartCoroutine(FindAnyObjectByType<TutorialHelper>().StarChartHint());
            //Destroy(this);
            isSolved = true;
        }
    }
}
