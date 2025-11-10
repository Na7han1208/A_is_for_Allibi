using System.Collections;
using UnityEngine;

public class NaproomEndingManager : MonoBehaviour
{
    public GameObject CutsceneCanvas;
    public GameObject Door;
    public GameObject Door2;
    private bool cutsceneShown = false;

    public void Start()
    {
        CutsceneCanvas.SetActive(false);
    }

    public void StartEnding()
    {
        if (cutsceneShown) return;
        cutsceneShown = true;
        //CutsceneCanvas.SetActive(true);
        FindFirstObjectByType<FPController>().SetPuzzleActive(true);
        SoundManager.Instance.StopAll();
        CutsceneManager.Instance.PlayCutscene("RedStain");
        //SoundManager.Instance.PlayComplex("Credits", transform);

        Camera playerCam = FindFirstObjectByType<FPController>().GetComponentInChildren<Camera>();
        playerCam.transform.LookAt(Door.transform.position);
    }

    public void OpenDoor()
    {
        SoundManager.Instance.PlayComplex("Unlock", transform);
        SoundManager.Instance.PlayComplex("Naproom", transform);
        Door.SetActive(false);
        Door2.SetActive(true);
        CutsceneCanvas.SetActive(false);
        FindFirstObjectByType<FPController>().SetPuzzleActive(false);
        FindFirstObjectByType<FPController>().isInspecting = false;
        FindFirstObjectByType<FPController>().heldRb.isKinematic = false;
        FindFirstObjectByType<FPController>().DropObject();
    }
}
