using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class LockDigit : MonoBehaviour
{
    private Transform incButton;
    private Transform decButton;
    private TMP_Text numDisplay;
    private int currentValue = 0;
    private Camera mainCam;

    private MeshRenderer incRenderer;
    private MeshRenderer decRenderer;
    private Color incBaseColor;
    private Color decBaseColor;
    private float hoverDarken = 0.8f;
    private float clickDarken = 0.5f;
    private Transform hoveredButton;
    private Transform clickedButton;
    private float clickTimer = 0f;
    private float clickFlashTime = 0.15f;

    void Start()
    {
        mainCam = Camera.main;

        foreach (Transform child in transform)
        {
            string lower = child.name.ToLower();

            if (lower.Contains("inc"))
            {
                incButton = child;
                incRenderer = child.GetComponent<MeshRenderer>();
                if (incRenderer != null) incBaseColor = incRenderer.material.color;
            }
            else if (lower.Contains("dec"))
            {
                decButton = child;
                decRenderer = child.GetComponent<MeshRenderer>();
                if (decRenderer != null) decBaseColor = decRenderer.material.color;
            }
            else if (lower.Contains("num"))
            {
                numDisplay = child.GetComponent<TMP_Text>();
                if (numDisplay == null)
                    numDisplay = child.GetComponentInChildren<TMP_Text>();
            }
        }

        if (incButton == null || decButton == null || numDisplay == null)
        {
            Debug.LogError($"[LockDigit] Missing child setup on {gameObject.name}");
            enabled = false;
            return;
        }

        UpdateDisplay();
    }

    void Update()
    {
        HandleHover();
        HandleInteractionInput();

        if (clickTimer > 0f)
        {
            clickTimer -= Time.deltaTime;
            if (clickTimer <= 0f)
                clickedButton = null;
        }
    }

    void HandleHover()
    {
        if (Mouse.current == null) return;

        Vector3 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
            hoveredButton = hit.transform == incButton || hit.transform == decButton ? hit.transform : null;
        else
            hoveredButton = null;

        UpdateButtonVisuals();
    }

    void HandleInteractionInput()
    {
        bool interactPressed = false;

        if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame))
            interactPressed = true;

        if (Gamepad.current != null && (Gamepad.current.buttonWest.wasPressedThisFrame || Gamepad.current.rightTrigger.wasPressedThisFrame))
            interactPressed = true;

        if (interactPressed && hoveredButton != null)
        {
            if (hoveredButton == incButton)
            {
                Increment();
                clickedButton = incButton;
            }
            else if (hoveredButton == decButton)
            {
                Decrement();
                clickedButton = decButton;
            }

            clickTimer = clickFlashTime;
        }
    }

    void UpdateButtonVisuals()
    {
        UpdateButtonColor(incButton, incRenderer, incBaseColor);
        UpdateButtonColor(decButton, decRenderer, decBaseColor);
    }

    void UpdateButtonColor(Transform button, MeshRenderer renderer, Color baseColor)
    {
        if (renderer == null) return;
        Color target = baseColor;

        if (button == clickedButton)
            target *= clickDarken;
        else if (button == hoveredButton)
            target *= hoverDarken;

        renderer.material.color = Color.Lerp(renderer.material.color, target, Time.deltaTime * 15f);
    }

    void Increment()
    {
        currentValue = (currentValue + 1) % 10;
        UpdateDisplay();
    }

    void Decrement()
    {
        currentValue = (currentValue - 1 + 10) % 10;
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        numDisplay.text = currentValue.ToString();
        GetComponentInParent<LockController>()?.CheckCode();
    }

    public int GetValue()
    {
        return currentValue;
    }
}
