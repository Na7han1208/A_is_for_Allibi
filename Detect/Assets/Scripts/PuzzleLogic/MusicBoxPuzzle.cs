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

    private string inputSequence = "";
    private string correctSequence = "54425";

    private bool puzzleActive = false;

    void Start()
    {
        mainCamera = Camera.main;
        puzzleUI.SetActive(false);

        // add button listeners
        for (int i = 0; i < numberButtons.Count; i++)
        {
            int index = i; // capture variable
            numberButtons[i].onClick.AddListener(() => ButtonPressed(index));
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

            var playerInput = FindFirstObjectByType<PlayerInput>();
            if (playerInput != null)
                playerInput.SwitchCurrentActionMap("Puzzle");

            Vector3 fixedPuzzlePos = cameraTarget.position;
            Quaternion fixedPuzzleRot = Quaternion.Euler(90f, -35f, 0f); // look straight down

            StartCoroutine(MoveCameraToTarget(fixedPuzzlePos, fixedPuzzleRot));
        }
    }

    public void HidePuzzle()
    {
        if (puzzleActive)
        {
            puzzleUI.SetActive(false);
            puzzleActive = false;
            inputSequence = "";

            StartCoroutine(MoveCameraToTarget(originalCamPos, originalCamRot));

            var player = FindFirstObjectByType<FPController>();
            if (player != null) player.SetPuzzleActive(false);

            var playerInput = FindFirstObjectByType<PlayerInput>();
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

        // Check if puzzle completed
        if (inputSequence.Length >= correctSequence.Length)
        {
            if (inputSequence.Contains(correctSequence))
            {
                PuzzleCompleted();
            }
        }
    }

    void PuzzleCompleted()
    {
        Debug.Log("Puzzle Completed!");
        var FPC = FindFirstObjectByType<FPController>();
        if (FPC != null) FPC.PlaySuccessParticles();
        HidePuzzle();
        // You can trigger other effects here
    }
}
