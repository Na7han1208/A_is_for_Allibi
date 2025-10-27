using UnityEngine;

public class NR_BlockLock : MonoBehaviour
{
    public BlockLockPuzzle blp;
    public SubtitleSequence BlockLockSolve;
    private bool isSolved = false;
    
    void Update()
    {
        if (blp.puzzleSolved)
        {
            if (isSolved) return;
            StarChartManager.Instance.UnlockStar("GS1");
            StartCoroutine(FindAnyObjectByType<TutorialHelper>().StarChartHint());

            SoundManager.Instance.PlayComplex("BlockLockSolve", this.transform);
            SubtitleManager.Instance.PlaySequence(BlockLockSolve);
            isSolved = true;
        }
    }
}
