using UnityEngine;

public class NR_BlockLock : MonoBehaviour
{
    public BlockLockPuzzle blp;
    
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
        }
    }
}
