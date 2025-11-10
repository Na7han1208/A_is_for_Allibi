using System.Collections;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlaygroundEndManager : MonoBehaviour
{
    [SerializeField] private GameObject eddy;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EddyEye"))
        {
            StartCoroutine(Ending());
        }
    }

    private IEnumerator Ending()
    {
        FindFirstObjectByType<PlayerInput>().SwitchCurrentActionMap("Puzzle");
        StartCoroutine(LookAt());
        yield return new WaitForSeconds(3f);
        FindAnyObjectByType<PlayerInput>().SwitchCurrentActionMap("Player");
        SoundManager.Instance.StopAll();
        CutsceneManager.Instance.PlayCutscene("Final");
    }

    private IEnumerator LookAt()
    {
        var fpc = FindFirstObjectByType<FPController>();
        Transform camT = (fpc != null && fpc.cameraTransform != null) ? fpc.cameraTransform : Camera.main?.transform;
        if (camT == null || eddy == null) yield break;

        if (fpc != null) fpc.SetPuzzleActive(true);

        Vector3 dir = eddy.transform.position - camT.position;
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

    public void FinalCutsceneOver()
    {
        CutsceneManager.Instance.PlayCutscene("Credits");
    }

    public void CreditsOver()
    {
        SceneManager.LoadScene("Naproom");
    }
}
