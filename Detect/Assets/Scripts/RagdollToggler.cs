using UnityEngine;

public class RagdollToggler : MonoBehaviour
{
    public static RagdollToggler Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetRagdoll(GameObject root, bool enableRagdoll)
    {
        if (root == null)
        {
            Debug.LogWarning("RagdollToggler: Tried to toggle a null GameObject.");
            return;
        }

        Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);

        foreach (Rigidbody rb in bodies)
        {
            rb.isKinematic = !enableRagdoll;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        Animator animator = root.GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.enabled = !enableRagdoll;
        }

        Debug.Log($"Ragdoll {(enableRagdoll ? "enabled" : "disabled")} for {root.name}");
    }
}
