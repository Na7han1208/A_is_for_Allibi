using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MusicBoxPuzzle : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform cameraTarget;
    public float cameraMoveSpeed = 5f;
    private Camera mainCamera;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    [Header("UI Settings")]
    public GameObject puzzleUI;
    public List<Button> numberButtons;
    public List<AudioClip> buttonSounds;
    public AudioSource audioSource;
    public ParticleSystem buttonClickParticles;
    public RectTransform cursor;
    private Vector2 defaultCursorPosition;
    public RectTransform pointer;
    public GraphicRaycaster raycaster;

    [Header("Controller Settings")]
    private int selectedIndex = 0;
    private PlayerInput playerInput;

    private string inputSequence = "";
    private string correctSequence = "54425";

    private bool puzzleActive = false;

    private enum InputMode { Mouse, Controller }
    private InputMode currentMode = InputMode.Mouse;

    private float navCooldown = 0.2f;
    private float lastNavTime = 0f;

    void Start()
    {
        defaultCursorPosition = cursor.anchoredPosition;

        mainCamera = Camera.main;
        puzzleUI.SetActive(false);

        for (int i = 0; i < numberButtons.Count; i++)
        {
            int index = i;
            numberButtons[i].onClick.AddListener(() => ButtonPressed(index));
        }

        playerInput = FindFirstObjectByType<PlayerInput>();

        if (pointer != null)
            MovePointerToButton(selectedIndex);
    }

    private void Update()
    {
        if (!puzzleActive) return;

        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
        {
            SetInputMode(InputMode.Mouse);
        }
        else if (Gamepad.current != null)
        {
            if (Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.2f ||
                Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.1f)
            {
                SetInputMode(InputMode.Controller);
            }
        }

        if (currentMode == InputMode.Mouse && cursor.gameObject.activeSelf)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                cursor.parent as RectTransform,
                Mouse.current.position.ReadValue(),
                null,
                out localPoint
            );
            cursor.anchoredPosition = localPoint;
        }
    }

    private void SetInputMode(InputMode mode)
    {
        if (currentMode == mode) return;
        currentMode = mode;

        if (mode == InputMode.Mouse)
        {
            cursor.gameObject.SetActive(true);
            pointer.gameObject.SetActive(false);
            if (raycaster != null) raycaster.enabled = true;

            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            cursor.anchoredPosition = new Vector2(-999999, -999999);
            cursor.gameObject.SetActive(false);
            pointer.gameObject.SetActive(true);
            if (raycaster != null) raycaster.enabled = false;

            Cursor.lockState = CursorLockMode.Locked;

            MovePointerToButton(selectedIndex);
        }
    }

    public void ShowPuzzle()
    {
        if (!puzzleActive)
        {
            puzzleActive = true;
            puzzleUI.SetActive(true);

            pointer.gameObject.SetActive(false);

            originalCamPos = mainCamera.transform.position;
            originalCamRot = mainCamera.transform.rotation;

            var player = FindFirstObjectByType<FPController>();
            if (player != null) player.SetPuzzleActive(true);

            if (playerInput != null)
                playerInput.SwitchCurrentActionMap("Puzzle");

            Vector3 fixedPuzzlePos = cameraTarget.position;
            Quaternion fixedPuzzleRot = Quaternion.Euler(90f, -35f, 0f);

            StartCoroutine(MoveCameraToTarget(fixedPuzzlePos, fixedPuzzleRot));

            selectedIndex = 0;
            SetInputMode(InputMode.Mouse);
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void HidePuzzle()
    {
        if (puzzleActive)
        {
            puzzleUI.SetActive(false);
            puzzleActive = false;
            inputSequence = "";
            mainCamera.transform.position = originalCamPos;

            var player = FindFirstObjectByType<FPController>();
            if (player != null) player.SetPuzzleActive(false);

            if (playerInput != null)
                playerInput.SwitchCurrentActionMap("Player");

            cursor.gameObject.SetActive(true);
            cursor.anchoredPosition = defaultCursorPosition;

            Cursor.lockState = CursorLockMode.None;
        }
    }

    IEnumerator MoveCameraToTarget(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * cameraMoveSpeed;
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
    }

    void ButtonPressed(int index)
    {
        if (currentMode != InputMode.Mouse) return;
        ProcessButtonPress(index);
    }

    private void ProcessButtonPress(int index)
    {
        audioSource.PlayOneShot(buttonSounds[index]);
        inputSequence += numberButtons[index].name[numberButtons[index].name.Length - 1];
        if (buttonClickParticles != null)
            buttonClickParticles.Play();

        if (inputSequence.Length >= correctSequence.Length)
        {
            if (inputSequence.Contains(correctSequence))
            {
                PuzzleCompleted();
            }
            else if (inputSequence.Length > 10)
            {
                string temp = inputSequence.Substring(5);
                inputSequence = temp;
            }
        }
    }

    void PuzzleCompleted()
    {
        var FPC = FindFirstObjectByType<FPController>();
        if (FPC != null) FPC.PlaySuccessParticles();
        HidePuzzle();
        SoundManager.Instance.PlayComplex("PaperTraceCompleted", this.transform);
        StartCoroutine(waitThenCommitSuicide(0.1f));
    }

    private IEnumerator waitThenCommitSuicide(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this);
    }

    public void OnNavigate(InputAction.CallbackContext ctx)
    {
        if (!puzzleActive || !ctx.performed) return;
        if (currentMode != InputMode.Controller) return;
        if (Time.time - lastNavTime < navCooldown) return;

        Vector2 nav = ctx.ReadValue<Vector2>();

        if (nav.y > 0.5f || nav.x < -0.5f)
        {
            selectedIndex = (selectedIndex - 1 + numberButtons.Count) % numberButtons.Count;
            MovePointerToButton(selectedIndex);
            lastNavTime = Time.time;
        }
        else if (nav.y < -0.5f || nav.x > 0.5f)
        {
            selectedIndex = (selectedIndex + 1) % numberButtons.Count;
            MovePointerToButton(selectedIndex);
            lastNavTime = Time.time;
        }
    }

    public void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (!puzzleActive || !ctx.performed) return;
        if (currentMode != InputMode.Controller) return;
        ProcessButtonPress(selectedIndex);
    }

    private void MovePointerToButton(int index)
    {
        if (pointer != null && numberButtons.Count > 0)
        {
            RectTransform btnRect = numberButtons[index].GetComponent<RectTransform>();
            pointer.position = btnRect.position;
        }
    }
}