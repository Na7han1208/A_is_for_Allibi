using UnityEngine;

public class KinematicToggle : MonoBehaviour
{
    private Rigidbody rb;
    private Collider cl;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        cl = GetComponent<MeshCollider>();
    }

    void OnTriggerEnter(Collider collision)
    {
        if (rb.isKinematic)
        {
            rb.isKinematic = false;
            cl.isTrigger = false;
            cl.gameObject.layer = 7;
        }       
    }
}
