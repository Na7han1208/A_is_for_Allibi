using Unity.VisualScripting;
using UnityEngine;

public class BlockLockPuzzle : MonoBehaviour
{
    [Header("Blocks")]
    [SerializeField] private GameObject[] Blocks;

    [Header("Lock Positions")]
    [SerializeField] private  GameObject[] LockPos;

    private bool[] isLocked;
    private bool puzzleSolved = false;

    void Start()
    {
        isLocked = new bool[Blocks.Length];
        for (int i = 0; i < Blocks.Length; i++)
        {
            isLocked[i] = false;
        }
    }

    void Update()
    {
        for (int i = 0; i < Blocks.Length; i++)
        {
            if (Vector3.Distance(Blocks[i].transform.position, LockPos[i].transform.position) < 0.5f && !isLocked[i])
            {
                switch (i)
                {
                    case 0: SoundManager.Instance.PlayComplex("G1", transform); break;
                    case 1: SoundManager.Instance.PlayComplex("G2", transform); break;
                    case 2: SoundManager.Instance.PlayComplex("G3", transform); break;
                }
                isLocked[i] = true;
                Blocks[i].transform.SetPositionAndRotation(LockPos[i].transform.position, Quaternion.identity);
                Blocks[i].GetComponent<Rigidbody>().isKinematic = true;
                Blocks[i].layer = 0;

                FPController player = FindFirstObjectByType<FPController>();
                if (player != null && player.heldObject == Blocks[i])
                {
                    player.DropObject();
                }
            }
        }

        bool allLocked = true;
        for (int i = 0; i < isLocked.Length; i++)
        {
            if (!isLocked[i])
            {
                allLocked = false;
                break;
            }
        }

        if (allLocked && !puzzleSolved)
        {
            puzzleSolved = true;
            SoundManager.Instance.PlayComplex("Unlock", this.transform);
            SoundManager.Instance.PlayComplex("BlockLockSolve", this.transform);

            FindFirstObjectByType<FPController>().PlaySuccessParticles();       
        }
    }
}
