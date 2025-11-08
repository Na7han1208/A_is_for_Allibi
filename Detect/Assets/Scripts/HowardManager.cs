using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HowardManager : MonoBehaviour
{
    [SerializeField] private GameObject mirror;
    [SerializeField] private GameObject lampLight;
    [SerializeField] private GameObject cutsceneZone;
    [SerializeField] private GameObject suspectDrawing;

    private static bool beenPickedUp = false;
    private static bool cutscenePlayed = false;

    void Start()
    {
        lampLight.GetComponent<Light>().enabled = false;
    }

    public void PickUpLogic()
    {
        if (beenPickedUp) return;
        beenPickedUp = true;
        StartCoroutine(HowardPickipCoroutine());
    }

    private IEnumerator HowardPickipCoroutine()
    {
        SoundManager.Instance.PlayComplex("HowardPickup", transform);
        FindFirstObjectByType<PlayerInput>().SwitchCurrentActionMap("Puzzle");
        yield return new WaitForSeconds(10.5f);
        StartCoroutine(LookAtMirrorCoroutine());
        yield return new WaitForSeconds(3.5f);
        FindFirstObjectByType<PlayerInput>().SwitchCurrentActionMap("Player");
    }

    private IEnumerator LookAtMirrorCoroutine()
    {
        var fpc = FindFirstObjectByType<FPController>();
        Transform camT = (fpc != null && fpc.cameraTransform != null) ? fpc.cameraTransform : Camera.main?.transform;
        if (camT == null || mirror == null) yield break;

        if (fpc != null) fpc.SetPuzzleActive(true);

        Vector3 dir = mirror.transform.position - camT.position;
        if (dir.sqrMagnitude <= 0.0001f)
        {
            if (fpc != null) fpc.SetPuzzleActive(false);
            yield break;
        }

        Quaternion startRot = camT.rotation;
        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        float speed = 90f;
        float angle = Quaternion.Angle(startRot, targetRot);
        float duration = Mathf.Max(0.01f, angle / speed);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            camT.rotation = Quaternion.Slerp(startRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        camT.rotation = targetRot;
        yield return new WaitForSeconds(1f);

        if (fpc != null)
        {
            Vector3 flatForward = new Vector3(camT.forward.x, 0f, camT.forward.z).normalized;
            if (flatForward.sqrMagnitude > 0.001f)
                fpc.transform.rotation = Quaternion.LookRotation(flatForward);

            fpc.verticalRotation = camT.localEulerAngles.x;
            if (fpc.verticalRotation > 180f) fpc.verticalRotation -= 360f;
            fpc.SetPuzzleActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Triggered by: {other.name}, Tag: {other.tag}");
        SoundManager.Instance.StopAll();
        if (other.CompareTag("HowardInterrogation"))
        {
            if (cutscenePlayed) return;
            cutscenePlayed = true;
            StartCoroutine(CutsceneCoroutine());
        }
    }

    private IEnumerator CutsceneCoroutine()
    {
        Debug.Log("PlayingCutscene");
        lampLight.GetComponent<Light>().enabled = true;
        transform.position = cutsceneZone.transform.position;
        transform.rotation = cutsceneZone.transform.rotation;
        this.GetComponent<Rigidbody>().isKinematic = true;
        FindFirstObjectByType<FPController>().DropObject();

        yield return new WaitForSeconds(3f);
        
        FindFirstObjectByType<CutsceneManager>().PlayCutscene("HowardInterrogation");
        transform.position = cutsceneZone.transform.position + new Vector3(0,0,-2f);
        transform.rotation = new Quaternion(0, 20f, 0, 0);
        GetComponent<Rigidbody>().isKinematic = false;
    }
    
    public void ShowDrawing()
    {
        suspectDrawing.SetActive(true);
    }
}
