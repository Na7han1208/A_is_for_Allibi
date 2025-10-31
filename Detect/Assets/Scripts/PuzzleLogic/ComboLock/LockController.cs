using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.Shapes;

public class LockController : MonoBehaviour
{
    [Header("Lock Settings")]
    public string correctCode = "001008";

    [Header("Camera Settings")]
    public Transform cameraTarget;
    public float cameraMoveSpeed = 5f;

    public LockDigit[] digits;

    private Camera mainCamera;
    private Vector3 originalCamPos;
    private Quaternion originalCamRot;

    private PlayerInput playerInput;
    private bool puzzleActive = false;
    private bool isCompleted = false;
    private bool canExit = false;
    private Coroutine cameraCoroutine = null;

    [Header("Door Settings")]
    public GameObject door;
    public GameObject doorNewPos;
    public GameObject Lock;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (digits == null || digits.Length == 0)
            digits = GetComponentsInChildren<LockDigit>();
        playerInput = FindFirstObjectByType<PlayerInput>();
    }

    public void ShowPuzzle()
    {
        if (puzzleActive || isCompleted) return;

        var fp = FindFirstObjectByType<FPController>();
        if (fp != null) fp.SetInvisible(true);
        var mainMenu = FindFirstObjectByType<MainMenuManager>();
        if (mainMenu != null) mainMenu.ToggleInMainMenu();

        puzzleActive = true;
        originalCamPos = mainCamera.transform.position;
        originalCamRot = mainCamera.transform.rotation;

        var player = FindFirstObjectByType<FPController>();
        if (player != null) player.SetPuzzleActive(true);

        if (playerInput != null)
            playerInput.SwitchCurrentActionMap("Puzzle");

        canExit = false;

        if (cameraCoroutine != null) StopCoroutine(cameraCoroutine);
        cameraCoroutine = StartCoroutine(MoveCameraToTarget(cameraTarget.position, cameraTarget.rotation, () =>
        {
            StartCoroutine(EnableExitAfterDelay(0.12f));
        }));

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HidePuzzle()
    {
        if (!puzzleActive) return;

        canExit = false;

        var fp = FindFirstObjectByType<FPController>();
        if (fp != null) fp.SetInvisible(false);
        var mainMenu = FindFirstObjectByType<MainMenuManager>();
        if (mainMenu != null) mainMenu.ToggleInMainMenu();

        puzzleActive = false;

        if (cameraCoroutine != null) StopCoroutine(cameraCoroutine);
        cameraCoroutine = StartCoroutine(MoveCameraToTarget(originalCamPos, originalCamRot, () =>
        {
            var player = FindFirstObjectByType<FPController>();
            if (player != null) player.SetPuzzleActive(false);

            if (playerInput != null)
                playerInput.SwitchCurrentActionMap("Player");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            StartCoroutine(EnableExitAfterDelay(0.05f));
        }));
    }

    IEnumerator MoveCameraToTarget(Vector3 targetPos, Quaternion targetRot, System.Action onComplete = null)
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

        onComplete?.Invoke();
    }

    IEnumerator EnableExitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        canExit = true;
    }

    public string GetCurrentCode()
    {
        string code = "";
        foreach (var d in digits)
            code += d.GetValue().ToString();
        return code;
    }

    public void CheckCode()
    {
        if (isCompleted) return;
        if (GetCurrentCode() == correctCode)
            CompletePuzzle();
    }

    void CompletePuzzle()
    {
        isCompleted = true;
        Debug.Log("Lock SOLVED");
        HidePuzzle();
        var rb = GetComponentInParent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
        StartCoroutine(MoveDoor());
    }


    IEnumerator MoveDoor()
    {
        if (door == null || doorNewPos == null) yield break;

        Vector3 startPos = door.transform.position;
        Quaternion startRot = door.transform.rotation;
        Vector3 endPos = doorNewPos.transform.position;
        Quaternion endRot = doorNewPos.transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 1f;
            door.transform.position = Vector3.Lerp(startPos, endPos, t);
            door.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }
        Lock.SetActive(false);
    }

    public void OnExit(InputAction.CallbackContext ctx)
    {
        Debug.Log("EXIT");
        if (!canExit) return;
        Debug.Log("Can exit");
        HidePuzzle();
    }
}