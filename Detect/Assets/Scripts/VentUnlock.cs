using UnityEngine;

public class ProximityKinematicTrigger : MonoBehaviour
{
    [Header("Objects to Check")]
    [SerializeField] private Rigidbody[] targetObjects;

    [Header("Detection Settings")]
    [SerializeField] private float activationDistance = 1f;

    void Update()
    {
        foreach (Rigidbody rb in targetObjects)
        {
            if (rb == null) continue;

            float distance = Vector3.Distance(transform.position, rb.transform.position);

            if (distance <= activationDistance)
            {
                if (rb.isKinematic)
                    rb.isKinematic = false;
            }
        }
    }
}
