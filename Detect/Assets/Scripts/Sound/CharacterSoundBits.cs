using Unity.VisualScripting;
using UnityEngine;

/*
    Place this on each character that will contain sound bits, when interacted with it will play the next sound bit in the array
*/

public class CharacterSoundBits : MonoBehaviour
{
    private int currentSoundBit = 1;

    [Header("SoundBits")]
    [SerializeField] private AudioClip[] clips;
    private AudioSource audioSource;

    public void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void OnInteract()
    {
        audioSource.PlayOneShot(clips[currentSoundBit]);
        currentSoundBit++;
        if (currentSoundBit == 4)
        {
            currentSoundBit = 1;
        }
    }
}
