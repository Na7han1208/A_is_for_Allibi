using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MainMenuManager menuManager;

    public void OnPointerEnter(PointerEventData eventData)
    {
        menuManager.OnButtonHoverEnter(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        menuManager.OnButtonHoverExit(gameObject);
    }
}
