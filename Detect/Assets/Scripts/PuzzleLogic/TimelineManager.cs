using Unity.VisualScripting;
using UnityEngine;

public class TimeLineManager : MonoBehaviour
{
    [Header("Blocks")]
    [SerializeField] private GameObject[] Blocks;

    [Header("Lock Positions")]
    [SerializeField] private GameObject[] LockPos;
    [SerializeField] private Vector3 offset;
    public GameObject SlidingDoor;

    private bool[] isLocked;
    public bool puzzleSolved = false;

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
                    case 3: SoundManager.Instance.PlayComplex("G4", transform); break;
                    case 4: SoundManager.Instance.PlayComplex("G5", transform); break;
                }
                isLocked[i] = true;
                Vector3 offset = new Vector3(0, 3, 0);
                Blocks[i].transform.SetPositionAndRotation(LockPos[i].transform.position, LockPos[i].transform.rotation);
                Blocks[i].GetComponent<Rigidbody>().isKinematic = true;
                Blocks[i].gameObject.layer = 0;
                
                FPController player = FindFirstObjectByType<FPController>();
                if (player != null && player.heldObject == Blocks[i])
                {
                    player.DropObject();
                }
                Blocks[i].layer = 0;
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
            SoundManager.Instance.PlayComplex("StarUnlock", this.transform);
            FindFirstObjectByType<FPController>().PlaySuccessParticles();
            SlidingDoor.transform.position += new Vector3(-2, 0, 0);    
        }
    }
}
