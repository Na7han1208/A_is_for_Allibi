using UnityEngine;

public class ProximityKinematicTrigger : MonoBehaviour
{
    [Header("Objects to Check")]
    [SerializeField] private Rigidbody[] targetObjects;

    [Header("Detection Settings")]
    [SerializeField] private float activationDistance = 2f;

    private bool hasUnlocked = false;

    void Update()
    {
        if (hasUnlocked) return;
        foreach (Rigidbody rb in targetObjects)
        {
            if (rb == null) continue;

            float distance = Vector3.Distance(transform.position, rb.transform.position);

            if (distance <= activationDistance)
            {
                this.gameObject.layer = 7;
                Debug.Log("Vent Unlocked");
                hasUnlocked = true;   
            }
        }
    }
}
