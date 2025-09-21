using UnityEngine;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance;

    [Header("Dialogue Data")]
    public GameObject[] dialogueObjects;
    public AudioClip[] dialogueClips;
    public bool[] hasInteracted;
    [Range(0f, 2f)] public float[] dialogueVolumes;

    [Header("Audio")]
    public AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (hasInteracted == null || hasInteracted.Length != dialogueObjects.Length)
            hasInteracted = new bool[dialogueObjects.Length];

        if (dialogueVolumes == null || dialogueVolumes.Length != dialogueObjects.Length)
        {
            dialogueVolumes = new float[dialogueObjects.Length];
            for (int i = 0; i < dialogueVolumes.Length; i++) dialogueVolumes[i] = 1f;
        }
    }

    public void TriggerPickupDialogue(GameObject pickedObject)
    {
        for (int i = 0; i < dialogueObjects.Length; i++)
        {
            if (dialogueObjects[i] == pickedObject && !hasInteracted[i])
            {
                hasInteracted[i] = true;

                if (dialogueClips.Length > i && dialogueClips[i] != null)
                {
                    float volume = (dialogueVolumes.Length > i) ? dialogueVolumes[i] : 1f;
                    audioSource.Stop();
                    audioSource.PlayOneShot(dialogueClips[i], volume);
                    Debug.Log($"[DialogueManager] Played first-time dialogue for {pickedObject.name} at volume {volume}");
                }
                return;
            }
        }
    }
}
