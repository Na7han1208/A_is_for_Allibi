using UnityEngine;
using UnityEngine.UI;

public class Star : MonoBehaviour
{
    [Header("Star Setup")]
    public string starId;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    private Image starImage;
    public bool IsUnlocked { get; private set; }

    private void Start()
    {
        starImage = GetComponent<Image>();
        starImage.sprite = lockedSprite;
    }

    public void Unlock()
    {
        IsUnlocked = true;
        starImage.sprite = unlockedSprite;
    }
}
