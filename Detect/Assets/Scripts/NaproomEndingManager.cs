using System.Collections;
using UnityEngine;

public class NaproomEndingManager : MonoBehaviour
{
    public GameObject CutsceneCanvas;
    public GameObject Door;
    private bool cutsceneShown = false;

    public void Start()
    {
        CutsceneCanvas.SetActive(false);
    }

    public void StartEnding()
    {
        if (cutsceneShown) return;
        cutsceneShown = true;
        CutsceneCanvas.SetActive(true);
        FindFirstObjectByType<FPController>().SetPuzzleActive(true);
        if (SoundManager.Instance == null) Debug.Log("wtf why am i null");
        SoundManager.Instance.StopAll();
        SoundManager.Instance.PlayComplex("NaproomEndingDialogue", transform);
        SoundManager.Instance.PlayComplex("Credits", transform);

        Camera playerCam = FindFirstObjectByType<FPController>().GetComponentInChildren<Camera>();
        playerCam.transform.LookAt(Door.transform.position);

        StartCoroutine(WaitThenOpenDoor());
    }

    private IEnumerator WaitThenOpenDoor()
    {
        yield return new WaitForSeconds(38);
        Door.SetActive(false);
        CutsceneCanvas.SetActive(false);
        FindFirstObjectByType<FPController>().SetPuzzleActive(false);
    }
}
