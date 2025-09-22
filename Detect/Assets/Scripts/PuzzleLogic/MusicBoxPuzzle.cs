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

    [Header("Controller Settings")]
    public RectTransform pointer;
    private int selectedIndex = 0;
    private PlayerInput playerInput;

    private string inputSequence = "";
    private string correctSequence = "54425";

    private bool puzzleActive = false;

    void Start()
    {
        mainCamera = Camera.main;
        puzzleUI.SetActive(false);

        // Add button listeners
        for (int i = 0; i < numberButtons.Count; i++)
        {
            int index = i; // capture variable
            numberButtons[i].onClick.AddListener(() => ButtonPressed(index));
        }

        playerInput = FindFirstObjectByType<PlayerInput>();

        if (pointer != null)
            MovePointerToButton(selectedIndex);
    }

    public void Update()
    {
        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f)
        {
            pointer.gameObject.SetActive(false);
        }

        if (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.01f)
        {
            pointer.gameObject.SetActive(true);
        }
    }

    public void ShowPuzzle()
    {
        if (!puzzleActive)
        {
            puzzleActive = true;
            puzzleUI.SetActive(true);

            originalCamPos = mainCamera.transform.position;
            originalCamRot = mainCamera.transform.rotation;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var player = FindFirstObjectByType<FPController>();
            if (player != null) player.SetPuzzleActive(true);

            if (playerInput != null)
                playerInput.SwitchCurrentActionMap("Puzzle");

            Vector3 fixedPuzzlePos = cameraTarget.position;
            Quaternion fixedPuzzleRot = Quaternion.Euler(90f, -35f, 0f); // look straight down

            StartCoroutine(MoveCameraToTarget(fixedPuzzlePos, fixedPuzzleRot));

            selectedIndex = 0;
            MovePointerToButton(selectedIndex);
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

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
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
        audioSource.PlayOneShot(buttonSounds[index]);

        inputSequence += numberButtons[index].ToString()[3];
        Debug.Log(inputSequence);

        if (buttonClickParticles != null)
            buttonClickParticles.Play();

        // check if puzzle completed
        if (inputSequence.Length >= correctSequence.Length)
        {
            if (inputSequence.Contains(correctSequence))
            {
                PuzzleCompleted();
            }
            else if (inputSequence.Length > 10)
            {
                // reset or do nothing
            }
        }
    }

    void PuzzleCompleted()
    {
        Debug.Log("Puzzle Completed!");
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

    // ---------------- CONTROLLER INPUT ----------------

    public void OnNavigate(InputAction.CallbackContext ctx)
    {
        if (!puzzleActive || !ctx.performed) return;

        Vector2 nav = ctx.ReadValue<Vector2>();

        if (nav.y > 0.5f || nav.x < -0.5f)
        {
            // move left/up
            selectedIndex = (selectedIndex - 1 + numberButtons.Count) % numberButtons.Count;
            MovePointerToButton(selectedIndex);
        }
        else if (nav.y < -0.5f || nav.x > 0.5f)
        {
            // move right/down
            selectedIndex = (selectedIndex + 1) % numberButtons.Count;
            MovePointerToButton(selectedIndex);
        }
    }

    public void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (!puzzleActive || !ctx.performed) return;

        numberButtons[selectedIndex].onClick.Invoke();
    }

    private void MovePointerToButton(int index)
    {
        if (pointer != null && numberButtons.Count > 0)
        {
            pointer.position = numberButtons[index].transform.position;
        }
    }
}
