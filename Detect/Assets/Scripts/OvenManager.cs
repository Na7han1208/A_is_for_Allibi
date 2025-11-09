using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class OvenManager : MonoBehaviour
{
    [SerializeField] private GameObject Door1;
    [SerializeField] private GameObject Door2;
    [SerializeField] private float openSpeed = 3f;
    [SerializeField] private GameObject OutOfSugarTimeline;
    [SerializeField] private GameObject Vent;
    private bool hasEntered = false;

    private bool isOpening = false;

    void OnTriggerEnter(Collider other)
    {
        if (hasEntered) return;
        hasEntered = true;
        if (other.CompareTag("Oven") && FindFirstObjectByType<ClassroomBlockLock>().puzzleSolved)
        {
            StartCoroutine(OvenOpening());
        }
    }

    private IEnumerator OvenOpening()
    {
        isOpening = true;
        SoundManager.Instance.PlayComplex("Sweets2", transform);
        FindFirstObjectByType<PlayerInput>().SwitchCurrentActionMap("Puzzle");
        StartCoroutine(LookAt(Door1));
        yield return new WaitForSeconds(4f);

        Vector3 startPos = Door1.transform.position;
        Vector3 targetPos = Door2.transform.position;

        Quaternion startRot = Door1.transform.rotation;
        Quaternion targetRot = Door2.transform.rotation;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;
            Door1.transform.position = Vector3.Lerp(startPos, targetPos, t);
            Door1.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        Door1.transform.rotation = targetRot;
        yield return new WaitForSeconds(10f);
        LookAt(Vent);
        yield return new WaitForSeconds(7f);
        FindFirstObjectByType<PlayerInput>().SwitchCurrentActionMap("Player");
        OutOfSugarTimeline.SetActive(true);
    }

    private IEnumerator LookAt(GameObject target)
    {
        var fpc = FindFirstObjectByType<FPController>();
        Transform camT = (fpc != null && fpc.cameraTransform != null) ? fpc.cameraTransform : Camera.main?.transform;
        if (camT == null || target == null) yield break;

        if (fpc != null) fpc.SetPuzzleActive(true);

        Vector3 dir = target.transform.position - camT.position;
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
}
