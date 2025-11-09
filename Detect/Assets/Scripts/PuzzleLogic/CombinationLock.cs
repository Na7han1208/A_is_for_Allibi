using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CombinationLock : MonoBehaviour
{
    [SerializeField] private int[] correctCombo;
    private int[] currentCombo = { 0, 0, 0, 0 };

    [SerializeField] private Button[] upButtons;
    [SerializeField] private Button[] downButtons;
    [SerializeField] private TMP_Text[] numDisplays;
    [SerializeField] private Button returnButton;

    private bool timmySpoken = false;

    private PlayerInput playerInput;
    private int currentIndex = 0;

    private void Awake()
    {
        playerInput = FindFirstObjectByType<PlayerInput>();
        HidePuzzle();
    }

    public void ShowPuzzle()
    {
        if (!timmySpoken) SoundManager.Instance.PlayComplex("Lock", transform);

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Puzzle");

        FPController controller = FindFirstObjectByType<FPController>();
        if (controller != null)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        foreach (var b in upButtons) b.gameObject.SetActive(true);
        foreach (var b in downButtons) b.gameObject.SetActive(true);
        foreach (var t in numDisplays) t.gameObject.SetActive(true);
        returnButton.gameObject.SetActive(true);

        for (int i = 0; i < 4; i++)
            numDisplays[i].text = currentCombo[i].ToString();

        currentIndex = 0;
        HighlightDigit();
    }

    public void HidePuzzle()
    {
        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Player");

        FPController controller = FindFirstObjectByType<FPController>();
        if (controller != null)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        foreach (var b in upButtons) b.gameObject.SetActive(false);
        foreach (var b in downButtons) b.gameObject.SetActive(false);
        foreach (var t in numDisplays) t.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(false);
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        Vector2 dir = context.ReadValue<Vector2>();

        if (dir.x > 0.5f) currentIndex = (currentIndex + 1) % 4;
        if (dir.x < -0.5f) currentIndex = (currentIndex + 3) % 4;

        if (dir.y > 0.5f) ButtonIncrease(currentIndex);
        if (dir.y < -0.5f) ButtonDecrease(currentIndex);

        HighlightDigit();
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
        if (context.performed)
            CheckIfSolved();
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        if (context.performed)
            HidePuzzle();
    }

    private void HighlightDigit()
    {
        if (IsUsingGamepad())
        {
            for (int i = 0; i < numDisplays.Length; i++) numDisplays[i].color = i == currentIndex ? Color.yellow : Color.black;
        }
        else
        {
            for (int i = 0; i < numDisplays.Length; i++) numDisplays[i].color = Color.black;
        }
    }

    public void ButtonIncrease(int index)
    {
        currentCombo[index] = (currentCombo[index] + 1) % 10;
        numDisplays[index].text = currentCombo[index].ToString();
        CheckIfSolved();
    }

    public void ButtonDecrease(int index)
    {
        currentCombo[index] = (currentCombo[index] + 9) % 10;
        numDisplays[index].text = currentCombo[index].ToString();
        CheckIfSolved();
    }

    private void CheckIfSolved()
    {
        if (currentCombo.SequenceEqual(correctCombo))
        {
            Debug.Log("PUZZLE SOLVED");
            HidePuzzle();
            SoundManager.Instance.PlayComplex("Unlock", this.transform);
            FindFirstObjectByType<FPController>().PlaySuccessParticles();
            this.enabled = false;
        }
    }

    private bool IsUsingGamepad()
    {
        return Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;
    }
}
