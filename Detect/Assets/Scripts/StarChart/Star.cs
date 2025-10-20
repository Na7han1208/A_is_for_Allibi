using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class Star : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string starId;
    public bool IsUnlocked;
    public Image starImage;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    public Sprite hoverSprite;
    public Sprite hoverSpriteAlt;

    private Coroutine hoverRoutine;

    private void Start()
    {
        UpdateVisual();
    }

    public void Unlock()
    {
        IsUnlocked = true;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (starImage == null) return;
        starImage.sprite = IsUnlocked ? unlockedSprite : lockedSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsUnlocked && starImage != null)
        {
            if (hoverRoutine != null) StopCoroutine(hoverRoutine);
            hoverRoutine = StartCoroutine(HoverBlink());
            if (!SoundManager.Instance.IsPlaying("StarHover"))
                SoundManager.Instance.PlayComplex("StarHover", transform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverRoutine != null)
        {
            StopCoroutine(hoverRoutine);
            hoverRoutine = null;
        }
        if (IsUnlocked && starImage != null)
        {
            starImage.sprite = unlockedSprite;
            //SoundManager.Instance.Stop("StarHover");            
        }
    }

    private IEnumerator HoverBlink()
    {
        bool toggle = false;
        while (true)
        {
            starImage.sprite = toggle ? hoverSprite : hoverSpriteAlt;
            toggle = !toggle;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
