using Unity.VisualScripting;
using UnityEngine;

public class BlockLockPuzzle : MonoBehaviour
{
    public GameObject block1;
    public GameObject block2;
    public GameObject block3;

    public GameObject lockPos1;
    public GameObject lockPos2;
    public GameObject lockPos3;

    void Update()
    {
        if (Vector3.Distance(block1.transform.position, lockPos1.transform.position) < 0.2)
        {
            block1.transform.position = lockPos1.transform.position;
            block1.transform.rotation = Quaternion.identity;
            block1.GetComponent<Rigidbody>().isKinematic = true;
        }

        if (Vector3.Distance(block2.transform.position, lockPos2.transform.position) < 0.2)
        {
            block2.transform.position = lockPos2.transform.position;
            block2.transform.rotation = Quaternion.identity;
            block2.GetComponent<Rigidbody>().isKinematic = true;
        }

        if (Vector3.Distance(block3.transform.position, lockPos3.transform.position) < 0.2)
        {
            block3.transform.position = lockPos3.transform.position;
            block3.transform.rotation = Quaternion.identity;
            block3.GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}
