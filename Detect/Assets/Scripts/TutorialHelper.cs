using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TutorialHelper : MonoBehaviour
{
    [Header("Have they done it?")]
    private bool moved = false;
    private bool looked = false;
    public bool pickedUp = false;
    private bool crouched = false;
    private bool inspected = false;

    public Canvas TutorialCanvas;
    public GameObject InteractTip;
    public GameObject MovementTip;
    public GameObject LookTip;

    private FPController fPController;

    void Start()
    {
        fPController = FindFirstObjectByType<FPController>();

        fPController.SetPuzzleActive(true);

        // ensure all tips start disabled
        InteractTip.SetActive(false);
        MovementTip.SetActive(false);
        LookTip.SetActive(false);
    }

    public void ToggleInteraction(bool active)
    {
        pickedUp = true;
        StartCoroutine(FadeImage(
            InteractTip.GetComponent<Image>(),
            active ? 1f : 0f,
            2f
        ));
    }

    public void DisplayMovement()
    {
        StartCoroutine(MovementCoroutine());
    }

    private IEnumerator MovementCoroutine()
    {
        // wait before showing tips
        yield return new WaitForSeconds(16f);

        fPController.SetPuzzleActive(false);
        Debug.Log("Okie you can move now");

        yield return StartCoroutine(FadeImage(MovementTip.GetComponent<Image>(), 1f, 2f));
        yield return StartCoroutine(FadeImage(LookTip.GetComponent<Image>(), 1f, 2f));

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(FadeImage(MovementTip.GetComponent<Image>(), 0f, 2f));
        yield return StartCoroutine(FadeImage(LookTip.GetComponent<Image>(), 0f, 2f));
    }

    private IEnumerator FadeImage(Image img, float targetAlpha, float duration)
    {
        // make sure it's active if we are fading in
        if (!img.gameObject.activeSelf && targetAlpha > 0f)
        {
            img.gameObject.SetActive(true);
            // force starting alpha to 0 if newly activated
            Color init = img.color;
            img.color = new Color(init.r, init.g, init.b, 0f);
        }

        Color start = img.color;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float a = Mathf.Lerp(start.a, targetAlpha, time / duration);
            img.color = new Color(start.r, start.g, start.b, a);
            yield return null;
        }

        img.color = new Color(start.r, start.g, start.b, targetAlpha);

        // disable when faded out
        if (Mathf.Approximately(targetAlpha, 0f))
        {
            img.gameObject.SetActive(false);
        }
    }
}
