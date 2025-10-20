using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject PlayButton;
    public GameObject ExitButton;
    public GameObject canvas;

    [Header("Settings")]
    public float delayBeforeFade = 6f;
    public float fadeDuration = 1.5f;

    public bool InMainMenu { get; private set; }

    private CanvasGroup playButtonGroup;
    private CanvasGroup exitButtonGroup;

    void Start()
    {
        CutsceneManager.Instance.PlayCutscene("MainMenu");
        InMainMenu = true;

        playButtonGroup = GetOrAddCanvasGroup(PlayButton);
        exitButtonGroup = GetOrAddCanvasGroup(ExitButton);

        playButtonGroup.alpha = 0f;
        exitButtonGroup.alpha = 0f;
        PlayButton.SetActive(false);
        ExitButton.SetActive(false);

        CursorManager.Instance.ShowCursor(true);
        SoundManager.Instance.PlayComplex("MainMenuAmbience", transform);

        StartCoroutine(FadeInUI());
    }

    private IEnumerator FadeInUI()
    {
        yield return new WaitForSeconds(0.1f);
        CursorManager.Instance.ShowCursor(true);
        // wait before fade starts
        yield return new WaitForSeconds(delayBeforeFade);

        PlayButton.SetActive(true);
        ExitButton.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            playButtonGroup.alpha = t;
            exitButtonGroup.alpha = t;
            yield return null;
        }

        playButtonGroup.alpha = 1f;
        exitButtonGroup.alpha = 1f;
    }

    private CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        if (go == null) return null;
        CanvasGroup group = go.GetComponent<CanvasGroup>();
        if (group == null) group = go.AddComponent<CanvasGroup>();
        return group;
    }

    public void StartGame()
    {
        canvas.SetActive(false);
        //InMainMenu = false;

        SoundManager.Instance.StopAll();
        CutsceneManager.Instance.PlayCutscene("Intro");
        SoundManager.Instance.PlayComplex("G1", transform);
        SoundManager.Instance.PlayComplex("G2", transform);
        SoundManager.Instance.PlayComplex("G3", transform);
    }

    public void ToggleInMainMenu()
    {
        InMainMenu = !InMainMenu;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void OnButtonHoverEnter(GameObject button)
    {
        switch (Random.Range(1, 4))
        {
            case 1: SoundManager.Instance.PlayComplex("G1", button.transform); break;
            case 2: SoundManager.Instance.PlayComplex("G2", button.transform); break;
            case 3: SoundManager.Instance.PlayComplex("G3", button.transform); break;
        }
        //button.transform.localScale *= 1.2f;
    }

    public void OnButtonHoverExit(GameObject button)
    {
        //button.transform.localScale /= 1.2f;
    }
}
