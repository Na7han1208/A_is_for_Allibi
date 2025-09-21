using UnityEngine;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance;

    [Header("Dialogue Data")]
    public GameObject[] dialogueObjects;
    public AudioClip[] dialogueClips;
    public bool[] hasInteracted;

    [Header("Audio")]
    public AudioSource audioSource;
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // ensure bool array matches length
        if (hasInteracted == null || hasInteracted.Length != dialogueObjects.Length)
        {
            hasInteracted = new bool[dialogueObjects.Length];
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
                    audioSource.Stop();
                    audioSource.PlayOneShot(dialogueClips[i]);
                    Debug.Log($"[DialogueManager] Played first-time dialogue for {pickedObject.name}");
                }
                return;
            }
        }
    }
}