using System.Collections;
using UnityEngine;

public class NaproomEndingManager : MonoBehaviour
{
    public GameObject CutsceneCanvas;
    public GameObject Door;

    public void Start()
    {
        CutsceneCanvas.SetActive(false);
    }

    public void StartEnding()
    {
        CutsceneCanvas.SetActive(true);
        FindFirstObjectByType<FPController>().SetPuzzleActive(true);
        if (SoundManager.Instance == null) Debug.Log("wtf why am i null");
        SoundManager.Instance.StopAll();
        SoundManager.Instance.PlayComplex("NaproomEndingDialogue", transform);
        StartCoroutine(WaitThenOpenDoor());
    }

    private IEnumerator WaitThenOpenDoor()
    {
        yield return new WaitForSeconds(38);
        Door.transform.rotation = Quaternion.Euler(90, Door.transform.rotation.eulerAngles.y, Door.transform.rotation.eulerAngles.z);
        CutsceneCanvas.SetActive(false);
        FindFirstObjectByType<FPController>().SetPuzzleActive(false);
    }
}
