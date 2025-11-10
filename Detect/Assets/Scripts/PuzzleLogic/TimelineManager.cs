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

    [Header("Fog and Rain")]
    public float FogDensity = 0.3f;
    public ParticleSystem RainParticles;

    void Start()
    {
        isLocked = new bool[Blocks.Length];
        for (int i = 0; i < Blocks.Length; i++)
            isLocked[i] = false;

        RainParticles.Stop();
    }

    void Update()
    {
        for (int i = 0; i < Blocks.Length; i++)
        {
            if (Vector3.Distance(Blocks[i].transform.position, LockPos[i].transform.position) < 0.9f && !isLocked[i])
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
                Blocks[i].transform.SetPositionAndRotation(LockPos[i].transform.position, LockPos[i].transform.rotation);
                Blocks[i].GetComponent<Rigidbody>().isKinematic = true;
                Blocks[i].gameObject.layer = 0;

                FPController player = FindFirstObjectByType<FPController>();
                if (player != null && player.heldObject == Blocks[i])
                    player.DropObject();
            }
        }

        bool allLocked = true;
        foreach (bool locked in isLocked)
        {
            if (!locked)
            {
                allLocked = false;
                break;
            }
        }

        if (allLocked && !puzzleSolved)
        {
            puzzleSolved = true;
            SoundManager.Instance.StopAll();
            SoundManager.Instance.PlayComplex("StarUnlock", transform);
            SoundManager.Instance.PlayComplex("ClassroomSolve", transform);
            FindFirstObjectByType<FPController>().PlaySuccessParticles();
            SlidingDoor.transform.position += new Vector3(-2, 0, 0);
        }
    }
}
