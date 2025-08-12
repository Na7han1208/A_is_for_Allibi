using Unity.VisualScripting;
using UnityEngine;

public class BlockLockPuzzle : MonoBehaviour
{
    public GameObject[] Blocks;
    public GameObject[] LockPos;
    private bool[] isLocked;

    void Update()
    {
        for (int i = 0; i < Blocks.Length; i++)
        {
            if (Vector3.Distance(Blocks[i].transform.position, LockPos[i].transform.position) < 0.2f && !isLocked[i])
            {
                isLocked[i] = true;
                Blocks[i].transform.position = LockPos[i].transform.position;
                Blocks[i].transform.rotation = Quaternion.identity;
                Destroy(Blocks[i].GetComponent<Rigidbody>());
            }
        }
    }
}
