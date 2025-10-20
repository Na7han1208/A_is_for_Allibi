using UnityEngine;

public class NR_BlockLock : MonoBehaviour
{
    public BlockLockPuzzle blp;
    public SubtitleSequence BlockLockSolve;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (blp.puzzleSolved)
        {
            StarChartManager.Instance.UnlockStar("GS1");
            FindAnyObjectByType<TutorialHelper>().StarChartHint();
            Destroy(this);

            SoundManager.Instance.PlayComplex("BlockLockSolve", this.transform);
            SubtitleManager.Instance.PlaySequence(BlockLockSolve);
        }
    }
}
