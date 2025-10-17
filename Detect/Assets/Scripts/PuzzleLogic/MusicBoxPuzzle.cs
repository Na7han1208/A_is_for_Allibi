using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MusicBoxPuzzle : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform cameraTarget;
    public float cameraMoveSpeed = 5f;
    private Camera mainCamera;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    [Header("Puzzle Settings")]
    public List<Transform> physicalButtons;
    public List<AudioClip> buttonSounds;
    public AudioSource audioSource;
    public ParticleSystem buttonClickParticles;
    private bool puzzleActive = false;

    private string inputSequence = "";
    private string correctSequence = "54425";
    private PlayerInput playerInput;

    private Color[] baseColors;
    private MeshRenderer[] renderers;
    private Transform hoveredButton;
    private Transform clickedButton;
    private float clickTimer;
    private float clickFlashTime = 0.15f;
    private float hoverDarken = 0.8f;
    private float clickDarken = 0.5f;

    void Start()
    {
        mainCamera = Camera.main;
        playerInput = FindFirstObjectByType<PlayerInput>();

        renderers = new MeshRenderer[physicalButtons.Count];
        baseColors = new Color[physicalButtons.Count];

        for (int i = 0; i < physicalButtons.Count; i++)
        {
            var rend = physicalButtons[i].GetComponent<MeshRenderer>();
            renderers[i] = rend;
            if (rend != null)
                baseColors[i] = rend.material.color;
        }
    }

    void Update()
    {
        if (!puzzleActive) return;

        HandleHover();

        if (Input.GetMouseButtonDown(0) && hoveredButton != null)
        {
            int index = physicalButtons.IndexOf(hoveredButton);
            if (index >= 0)
            {
                ProcessButtonPress(index);
                clickedButton = hoveredButton;
                clickTimer = clickFlashTime;
            }
        }

        if (clickTimer > 0f)
        {
            clickTimer -= Time.deltaTime;
            if (clickTimer <= 0f)
                clickedButton = null;
        }

        UpdateButtonColors();
    }

    void HandleHover()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
            hoveredButton = physicalButtons.Contains(hit.transform) ? hit.transform : null;
        else
            hoveredButton = null;
    }

    void UpdateButtonColors()
    {
        for (int i = 0; i < physicalButtons.Count; i++)
        {
            var rend = renderers[i];
            if (rend == null) continue;

            Color target = baseColors[i];

            if (physicalButtons[i] == clickedButton)
                target *= clickDarken;
            else if (physicalButtons[i] == hoveredButton)
                target *= hoverDarken;

            rend.material.color = Color.Lerp(rend.material.color, target, Time.deltaTime * 15f);
        }
    }

    void ProcessButtonPress(int index)
    {
        if (audioSource != null && buttonSounds.Count > index)
            audioSource.PlayOneShot(buttonSounds[index]);

        if (buttonClickParticles != null)
            buttonClickParticles.Play();

        string num = new string(physicalButtons[index].name
            .ToCharArray()[physicalButtons[index].name.Length - 1], 1);
        inputSequence += num;

        if (inputSequence.Length >= correctSequence.Length)
        {
            if (inputSequence.Contains(correctSequence))
                PuzzleCompleted();
            else if (inputSequence.Length > 10)
                inputSequence = inputSequence.Substring(5);
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

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Puzzle");

        Vector3 fixedPuzzlePos = cameraTarget.position;
        Quaternion fixedPuzzleRot = Quaternion.Euler(90f, -35f, 0f);

        StartCoroutine(MoveCameraToTarget(fixedPuzzlePos, fixedPuzzleRot));
        Cursor.lockState = CursorLockMode.None;
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

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Player");
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
        var FPC = FindFirstObjectByType<FPController>();
        if (FPC != null) FPC.PlaySuccessParticles();

        HidePuzzle();
        SoundManager.Instance.PlayComplex("PaperTraceCompleted", transform);
        Rigidbody rb = GetComponentInParent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        StartCoroutine(waitThenCommitSuicide(0.1f));
    }

    private IEnumerator waitThenCommitSuicide(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this);
    }

    public void OnExit(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) HidePuzzle();
    }
}