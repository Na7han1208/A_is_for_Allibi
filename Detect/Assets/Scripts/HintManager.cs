using System;
using UnityEngine;

public class HintManager : MonoBehaviour
{
    public static HintManager Instance;

    [Header("Dialogue Data")]
    public GameObject[] dialogueObjects;
    public AudioClip[] dialogueClips;
    public SubtitleSequence[] subtitleSequences;
    public bool[] hasInteracted;
    [Range(0f, 2f)] public float[] dialogueVolumes;

    [Header("Dialogue Conditions")]
    public bool[] onInspect;

    [Header("Audio")]
    public AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        int len = (dialogueObjects != null) ? dialogueObjects.Length : 0;

        if (hasInteracted == null || hasInteracted.Length != len)
        {
            bool[] newHas = new bool[len];
            if (hasInteracted != null)
            {
                int copy = Math.Min(hasInteracted.Length, len);
                for (int i = 0; i < copy; i++) newHas[i] = hasInteracted[i];
            }
            hasInteracted = newHas;
        }

        if (dialogueVolumes == null || dialogueVolumes.Length != len)
        {
            float[] newVol = new float[len];
            if (dialogueVolumes != null)
            {
                int copy = Math.Min(dialogueVolumes.Length, len);
                for (int i = 0; i < copy; i++) newVol[i] = dialogueVolumes[i];
            }
            for (int i = 0; i < len; i++) if (newVol[i] == 0f) newVol[i] = 1f;
            dialogueVolumes = newVol;
        }

        if (onInspect == null || onInspect.Length != len)
        {
            bool[] newOn = new bool[len];
            if (onInspect != null)
            {
                int copy = Math.Min(onInspect.Length, len);
                for (int i = 0; i < copy; i++) newOn[i] = onInspect[i];
            }
            for (int i = 0; i < len; i++) { }
            onInspect = newOn;
        }

        if (subtitleSequences == null || subtitleSequences.Length != len)
        {
            SubtitleSequence[] newSubs = new SubtitleSequence[len];
            if (subtitleSequences != null)
            {
                int copy = Math.Min(subtitleSequences.Length, len);
                for (int i = 0; i < copy; i++) newSubs[i] = subtitleSequences[i];
            }
            subtitleSequences = newSubs;
        }
    }

    public void TriggerPickupDialogue(GameObject targetObject)
    {
        TriggerDialogue(targetObject, false);
    }

    public void TriggerDialogue(GameObject targetObject, bool isInspect)
    {
        if (targetObject == null) return;
        Debug.Log($"[HintManager] TriggerDialogue called for '{targetObject.name}' (isInspect={isInspect})");

        for (int i = 0; i < dialogueObjects.Length; i++)
        {
            GameObject dialogObj = dialogueObjects[i];
            if (dialogObj == null) continue;

            bool match = dialogObj == targetObject
                         || targetObject.transform.IsChildOf(dialogObj.transform)
                         || dialogObj.transform.IsChildOf(targetObject.transform);

            if (!match) continue;

            bool wantsInspect = (onInspect != null && i < onInspect.Length) ? onInspect[i] : false;
            Debug.Log($"[HintManager] matched index {i}. wantsInspect={wantsInspect}, hasInteracted={(hasInteracted != null && i < hasInteracted.Length ? hasInteracted[i] : false)}");

            if (wantsInspect && !isInspect)
            {
                Debug.Log($"[HintManager] skipping: object set to play only on Inspect (index {i}).");
                return;
            }

            if (!wantsInspect && isInspect)
            {
                Debug.Log($"[HintManager] skipping: object set to play only on Pickup (index {i}).");
                return;
            }

            if (hasInteracted != null && i < hasInteracted.Length && hasInteracted[i])
            {
                Debug.Log($"[HintManager] skipping: already interacted (index {i}).");
                return;
            }

            if (i >= 9 && i <= 14)
            {
                for (int j = 9; j <= 14 && j < hasInteracted.Length; j++) hasInteracted[j] = true;
            }
            else
            {
                if (hasInteracted != null && i < hasInteracted.Length) hasInteracted[i] = true;
            }

            if (dialogueClips != null && i < dialogueClips.Length && dialogueClips[i] != null)
            {
                float volume = (dialogueVolumes != null && i < dialogueVolumes.Length) ? dialogueVolumes[i] : 1f;
                if (audioSource != null)
                {
                    audioSource.Stop();
                    audioSource.PlayOneShot(dialogueClips[i], volume);
                }

                Debug.Log($"[HintManager] Played {(isInspect ? "Inspect" : "Pickup")} dialogue for '{dialogObj.name}' at volume {volume}");

                if (SubtitleManager.Instance != null && subtitleSequences != null && i < subtitleSequences.Length && subtitleSequences[i] != null)
                {
                    Debug.Log($"[HintManager] Triggering subtitles for index {i}");
                    SubtitleManager.Instance.PlaySequence(subtitleSequences[i]);
                }
            }
            else
            {
                Debug.Log($"[HintManager] No audio clip assigned for index {i}.");
            }
            return;
        }

        Debug.Log($"[HintManager] No matching dialogueObject found for '{targetObject.name}'");
    }
}
