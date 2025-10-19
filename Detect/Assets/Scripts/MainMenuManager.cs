using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject PlayButton;

    public bool InMainMenu;

    public float delayBeforeFade = 4f;
    public float fadeDuration = 1.5f;

    private CanvasGroup playButtonGroup;

    void Start()
    {
        CutsceneManager.Instance.PlayCutscene("MainMenu");
        playButtonGroup = GetOrAddCanvasGroup(PlayButton);

        playButtonGroup.alpha = 0f;
        PlayButton.SetActive(false);
        InMainMenu = true;

        StartCoroutine(FadeInUI());

        CursorManager.Instance.ShowCursor(true);
    }

    private IEnumerator FadeInUI()
    {
        CursorManager.Instance.ShowCursor(true);
        yield return new WaitForSeconds(delayBeforeFade);
        PlayButton.SetActive(true);
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            playButtonGroup.alpha = t;
            yield return null;
        }
        playButtonGroup.alpha = 1f;
        CursorManager.Instance.ShowCursor(true);
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
        this.gameObject.SetActive(false);
        InMainMenu = false;

        CutsceneManager.Instance.PlayCutscene("Intro");
        SoundManager.Instance.PlayComplex("G1", transform);
        SoundManager.Instance.PlayComplex("G2", transform);
        SoundManager.Instance.PlayComplex("G3", transform);
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
        button.transform.localScale *= 1.05f;
    }

    public void OnButtonHoverExit(GameObject button)
    {
        button.transform.localScale /= 1.05f;
    }
}
