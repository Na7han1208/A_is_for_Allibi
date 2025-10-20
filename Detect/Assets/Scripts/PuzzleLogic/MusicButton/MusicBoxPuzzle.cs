using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MusicBoxPuzzle : MonoBehaviour
{
    public Transform cameraTarget;
    public float cameraMoveSpeed = 5f;
    public List<PhysicalButton> physicalButtons;
    public List<AudioClip> buttonSounds;
    public AudioSource audioSource;
    public ParticleSystem buttonClickParticles;
    public bool isCompleted = false;
    public GameObject canvas;

    private Camera mainCamera;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;
    private PlayerInput playerInput;
    private bool puzzleActive;
    private string inputSequence = "";
    private string correctSequence = "54425";

    private enum InputMode { Mouse, Controller }
    private InputMode currentMode = InputMode.Mouse;
    private float navCooldown = 0.2f;
    private float lastNavTime = 0f;
    private int selectedIndex = 0;

    void Start()
    {
        mainCamera = Camera.main;
        playerInput = FindFirstObjectByType<PlayerInput>();
    }

    void Update()
    {
        if (!puzzleActive) return;

        if (Mouse.current != null && Mouse.current.delta.ReadValue().sqrMagnitude > 0.01f) currentMode = InputMode.Mouse;
        else if (Gamepad.current != null && (Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.2f || Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.1f)) currentMode = InputMode.Controller;

        if (currentMode == InputMode.Mouse)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            PhysicalButton hb = GetButtonUnderMouse();
            UpdateHover(hb);
            if (Mouse.current.leftButton.wasPressedThisFrame && hb != null)
            {
                hb.Press();
                int idx = hb.buttonIndex;
                DoPressEffects(idx);
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (Gamepad.current != null)
            {
                if (Time.time - lastNavTime > navCooldown)
                {
                    Vector2 nav = Gamepad.current.leftStick.ReadValue();
                    if (nav.y > 0.5f || nav.x < -0.5f) { selectedIndex = (selectedIndex - 1 + physicalButtons.Count) % physicalButtons.Count; lastNavTime = Time.time; }
                    else if (nav.y < -0.5f || nav.x > 0.5f) { selectedIndex = (selectedIndex + 1) % physicalButtons.Count; lastNavTime = Time.time; }
                }
                PhysicalButton target = physicalButtons[selectedIndex];
                UpdateHover(target);
                if (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.rightTrigger.wasPressedThisFrame)
                {
                    target.Press();
                    DoPressEffects(target.buttonIndex);
                }
            }
        }
    }

    PhysicalButton GetButtonUnderMouse()
    {
        if (mainCamera == null) return null;
        Vector3 mp = (Mouse.current != null) ? (Vector3)Mouse.current.position.ReadValue() : Input.mousePosition;
        Ray r = mainCamera.ScreenPointToRay(mp);
        if (Physics.Raycast(r, out RaycastHit hit, 100f))
        {
            return hit.collider.GetComponentInParent<PhysicalButton>();
        }
        return null;
    }

    void UpdateHover(PhysicalButton hb)
    {
        for (int i = 0; i < physicalButtons.Count; i++)
            physicalButtons[i].SetHover(physicalButtons[i] == hb);
    }

    void DoPressEffects(int idx)
    {
        if (idx >= 0 && idx < buttonSounds.Count && audioSource != null) audioSource.PlayOneShot(buttonSounds[idx]);
        if (buttonClickParticles != null) buttonClickParticles.Play();
        inputSequence += idx.ToString();
        if (inputSequence.Length >= correctSequence.Length)
        {
            if (inputSequence.Contains(correctSequence)) PuzzleCompleted();
            else if (inputSequence.Length > 10) inputSequence = inputSequence.Substring(inputSequence.Length - 5);
        }
    }

    public void ShowPuzzle()
    {
        if (puzzleActive) return;
        puzzleActive = true;
        originalCamPos = mainCamera.transform.position;
        originalCamRot = mainCamera.transform.rotation;
        var player = FindFirstObjectByType<FPController>();
        if (player != null) player.SetPuzzleActive(true);
        if (playerInput != null) playerInput.SwitchCurrentActionMap("Puzzle");
        StartCoroutine(MoveCameraToTarget(cameraTarget.position, Quaternion.Euler(90f, -35f, 0f)));
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        canvas.SetActive(true);
        GetComponent<Collider>().enabled = false;
    }

    public void HidePuzzle()
    {
        if (!puzzleActive) return;
        puzzleActive = false;
        inputSequence = "";
        mainCamera.transform.position = originalCamPos;
        mainCamera.transform.rotation = originalCamRot;
        var player = FindFirstObjectByType<FPController>();
        if (player != null) player.SetPuzzleActive(false);
        if (playerInput != null) playerInput.SwitchCurrentActionMap("Player");
        for (int i = 0; i < physicalButtons.Count; i++) physicalButtons[i].SetHover(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        canvas.SetActive(false);
        GetComponent<Collider>().enabled = true;

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

    void PuzzleCompleted()
    {
        isCompleted = true;
        var FPC = FindFirstObjectByType<FPController>();
        if (FPC != null) FPC.PlaySuccessParticles();
        HidePuzzle();
        SoundManager.Instance.PlayComplex("PaperTraceCompleted", transform);
        var rb = GetComponentInParent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
        StartCoroutine(waitThenCommitSuicide(0.1f));
    }

    IEnumerator waitThenCommitSuicide(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this);
    }

    public void OnExit(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) HidePuzzle();
    }
}
