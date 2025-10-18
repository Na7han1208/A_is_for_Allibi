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

        if (cursorImage != null)
            cursorImage.gameObject.SetActive(true);

        Cursor.visible = false;
    }

    private void Update()
    {
        cursorImage.position = Input.mousePosition;
    }

    public void ShowCursor(bool show)
    {
        isVisible = show;
        if (cursorImage != null)
            cursorImage.gameObject.SetActive(show);
    }

    public void ToggleCursor()
    {
        isVisible = !isVisible;
        if (cursorImage != null)
            cursorImage.gameObject.SetActive(isVisible);
    }

    public void SetCursor(Sprite newSprite)
    {
        if (cursorImage != null && cursorImage.TryGetComponent(out Image img))
            img.sprite = newSprite;
    }
}
