using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Star : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Settings")]
    public string starId;
    public GameObject stickyNote;
    public bool IsUnlocked { get; private set; }

    private void Start()
    {
        gameObject.SetActive(false);
        if (stickyNote != null)
            stickyNote.SetActive(false);
    }

    public void Unlock()
    {
        IsUnlocked = true;
        gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsUnlocked && stickyNote != null)
            stickyNote.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (stickyNote != null)
            stickyNote.SetActive(false);
    }
}
