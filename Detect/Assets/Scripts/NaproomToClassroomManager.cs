using System.Collections;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class NaproomToClassroomManager : MonoBehaviour
{
    private bool hasPlayedFoxy = false;
    private bool hasPlayedClassIntro = false;
    public GameObject BathroomTimeline;
    public GameObject WhiteBoard;
    public GameObject Lock;

    [Header("Subs")]
    public SubtitleSequence timmyWait;
    public SubtitleSequence classroomIntro;

    void Start()
    {
        Lock.layer = 0;
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Naproom"))
        {
            if (FindFirstObjectByType<FPController>().heldObject.IsUnityNull()  || !FindFirstObjectByType<FPController>().heldObject.CompareTag("Foxy"))
            {
                if (hasPlayedFoxy) return;
                hasPlayedFoxy = true;
                SoundManager.Instance.PlayComplex("FoxyLeave", transform);
                SubtitleManager.Instance.PlaySequence(timmyWait);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Classroom"))
        {
            if (hasPlayedClassIntro) return;
            hasPlayedClassIntro = true;
            StartCoroutine(ClassroomIntro());
        }

    }

    private IEnumerator ClassroomIntro()
    {
        SoundManager.Instance.PlayComplex("ClassroomIntro", transform);
        SubtitleManager.Instance.PlaySequence(classroomIntro);
        yield return new WaitForSeconds(25f);
        FindFirstObjectByType<PlayerInput>().SwitchCurrentActionMap("Puzzle");
        yield return StartCoroutine(LookAtBoard());
        yield return new WaitForSeconds(10f);
        BathroomTimeline.SetActive(true);
        FindFirstObjectByType<PlayerInput>().SwitchCurrentActionMap("Player");
        Lock.layer = 7;
    }

    private IEnumerator LookAtBoard()
    {
        var fpc = FindFirstObjectByType<FPController>();
        Transform camT = (fpc != null && fpc.cameraTransform != null) ? fpc.cameraTransform : Camera.main?.transform;
        if (camT == null || WhiteBoard == null) yield break;

        if (fpc != null) fpc.SetPuzzleActive(true);

        Vector3 dir = WhiteBoard.transform.position - camT.position;
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
