using UnityEngine;

public class ClassroomBlockLock : MonoBehaviour
{
    [Header("Possible Words")]
    [SerializeField] private GameObject[] Blocks_DRUGS;
    [SerializeField] private GameObject[] Blocks_SUGAR;

    [Header("Lock Positions (shared)")]
    [SerializeField] private GameObject[] LockPos;

    private bool[] isLocked_DRUGS;
    private bool[] isLocked_SUGAR;

    public bool puzzleSolved = false;

    [Header("Settings")]
    [SerializeField] private float lockRange = 0.5f;
    [SerializeField] private float unlockRange = 0.7f;
    public GameObject OutOfSugarTimeline;

    void Start()
    {
        isLocked_DRUGS = new bool[LockPos.Length];
        isLocked_SUGAR = new bool[LockPos.Length];
    }

    void FixedUpdate()
    {
        if (puzzleSolved) return;

        CheckWord(Blocks_DRUGS, LockPos, isLocked_DRUGS);
        CheckWord(Blocks_SUGAR, LockPos, isLocked_SUGAR);

        bool allLocked_DRUGS = AllTrue(isLocked_DRUGS);
        bool allLocked_SUGAR = AllTrue(isLocked_SUGAR);

        if ((allLocked_DRUGS || allLocked_SUGAR) && !puzzleSolved)
        {
            puzzleSolved = true;
            SoundManager.Instance.PlayComplex("StarUnlock", transform);

            FPController player = FindFirstObjectByType<FPController>();
            if (player != null)
                player.PlaySuccessParticles();
            StarChartManager.Instance.UnlockStar("CR1");
        }
    }

    private void CheckWord(GameObject[] blocks, GameObject[] lockPos, bool[] lockState)
    {
        for (int i = 0; i < lockPos.Length; i++)
        {
            if (blocks[i] == null) continue;

            float distance = Vector3.Distance(blocks[i].transform.position, lockPos[i].transform.position);

            if (!lockState[i] && distance < lockRange)
            {
                LockBlock(blocks[i], lockPos[i], i);
                lockState[i] = true;
            }

            else if (lockState[i] && distance > unlockRange)
            {
                UnlockBlock(blocks[i]);
                lockState[i] = false;
            }
        }
    }

    private void LockBlock(GameObject block, GameObject lockPos, int index)
    {
        SoundManager.Instance.PlayComplex("G" + (index + 1), transform);

        block.transform.SetPositionAndRotation(lockPos.transform.position, Quaternion.identity);

        Rigidbody rb = block.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        FPController player = FindFirstObjectByType<FPController>();
        if (player != null && player.heldObject == block)
            player.DropObject();
    }

    private void UnlockBlock(GameObject block)
    {
        Rigidbody rb = block.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;
    }

    private bool AllTrue(bool[] array)
    {
        foreach (bool b in array)
            if (!b) return false;

        CutsceneManager.Instance.PlayCutscene("Addiction");
        OutOfSugarTimeline.SetActive(true);
        return true;
    }
}