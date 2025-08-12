using Unity.VisualScripting;
using UnityEngine;

public class BlockLockPuzzle : MonoBehaviour
{
    public GameObject[] Blocks;
    public GameObject[] LockPos;

    public Transform confettiPosition;

    private bool[] isLocked;
    private bool puzzleSolved = false;
    public GameObject dartPrefab;

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
            if (Vector3.Distance(Blocks[i].transform.position, LockPos[i].transform.position) < 0.2f && !isLocked[i])
            {
                isLocked[i] = true;
                Blocks[i].transform.SetPositionAndRotation(LockPos[i].transform.position, Quaternion.identity);
                Blocks[i].GetComponent<Rigidbody>().isKinematic = true;

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
            Debug.Log("Puzzle solved");
            for (int i = 0; i < 300; i++)
            {
                Instantiate(dartPrefab, confettiPosition.position, confettiPosition.rotation);
                Debug.Log("DARTS");
            }
        }
    }
}
