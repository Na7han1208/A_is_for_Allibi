using UnityEngine;
using UnityEngine.UI;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] private RectTransform cursorImage;
    [SerializeField] private Canvas canvas;

    private bool isVisible = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Cursor.visible = false;
        if (cursorImage != null)
            cursorImage.gameObject.SetActive(isVisible);
    }

    private void Update()
    {
        if (cursorImage != null && isVisible)
            cursorImage.position = Input.mousePosition;
    }

    public void ShowCursor(bool show)
    {
        isVisible = show;
        ApplyVisibility();
    }

    public void ToggleCursor()
    {
        isVisible = !isVisible;
        ApplyVisibility();
    }

    public void SetCursor(Sprite newSprite)
    {
        if (cursorImage != null && cursorImage.TryGetComponent(out Image img))
            img.sprite = newSprite;
    }

    private void ApplyVisibility()
    {
        if (cursorImage != null)
            cursorImage.gameObject.SetActive(isVisible);
    }
}
