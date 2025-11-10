using UnityEngine;

public class MichealManager : MonoBehaviour
{
    [SerializeField] private GameObject ratInfestationTimeline;

    public void SpawnRats()
    {
        ratInfestationTimeline.SetActive(true);
    }
}
