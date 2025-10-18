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
    private bool checkedOutSomething = false;

    public Canvas TutorialCanvas;
    public GameObject InteractTip;
    public GameObject MovementTip;
    public GameObject LookTip;
    public GameObject DrawTip;
    public GameObject ThrowTip;
    public GameObject InspectTip;
    public GameObject CrouchTip;
    public GameObject JumpTip;
    public GameObject Crosshair;

    public Vector2 cursorDefaultPosition;

    private FPController fPController;

    void Start()
    {
        cursorDefaultPosition = Crosshair.GetComponent<RectTransform>().anchoredPosition;

        Crosshair.SetActive(false);
        fPController = FindFirstObjectByType<FPController>();

        fPController.SetPuzzleActive(true);

        // ensure all tips start disabled
        InteractTip.SetActive(false);
        MovementTip.SetActive(false);
        LookTip.SetActive(false);
        DrawTip.SetActive(false);
    }

    public void ToggleInteraction(bool active)
    {
        StartCoroutine(FadeImage(
            InteractTip.GetComponent<Image>(),
            active ? 1f : 0f,
            2f
        ));
        Crosshair.SetActive(true);
    }

    public void ToggleInspectThrowTip()
    {
        if (checkedOutSomething) return;
        checkedOutSomething = true;
        StartCoroutine(InspectCoroutine());
    }

    private IEnumerator InspectCoroutine()
    {
        yield return StartCoroutine(FadeImage(InspectTip.GetComponent<Image>(), 1f, 2f));
        yield return StartCoroutine(FadeImage(ThrowTip.GetComponent<Image>(), 1f, 2f));

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(FadeImage(InspectTip.GetComponent<Image>(), 0f, 2f));
        yield return StartCoroutine(FadeImage(ThrowTip.GetComponent<Image>(), 0f, 2f));
    }

    public void ToggleDrawTip(bool active)
    {
        DrawTip.SetActive(active);
    }

    public void DisplayMovement()
    {
        if (moved) return;
        StartCoroutine(MovementCoroutine());
    }

    private IEnumerator MovementCoroutine()
    {
        moved = true;
        // wait before showing tips
        yield return new WaitForSeconds(16f);

        fPController.SetPuzzleActive(false);
        Debug.Log("Okie you can move now");

        yield return StartCoroutine(FadeImage(MovementTip.GetComponent<Image>(), 1f, 2f));
        yield return StartCoroutine(FadeImage(LookTip.GetComponent<Image>(), 1f, 2f));

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(FadeImage(MovementTip.GetComponent<Image>(), 0f, 2f));
        yield return StartCoroutine(FadeImage(LookTip.GetComponent<Image>(), 0f, 2f));

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(FadeImage(CrouchTip.GetComponent<Image>(), 1f, 2f));
        yield return StartCoroutine(FadeImage(JumpTip.GetComponent<Image>(), 1f, 2f));

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(FadeImage(CrouchTip.GetComponent<Image>(), 0f, 2f));
        yield return StartCoroutine(FadeImage(JumpTip.GetComponent<Image>(), 0f, 2f));
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
