using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject PlayButton;
    public GameObject cursor;
    public GameObject IntroCutscene;

    public bool InMainMenu;

    public float delayBeforeFade = 4f;
    public float fadeDuration = 1.5f;

    private CanvasGroup playButtonGroup;
    private CanvasGroup cursorGroup;

    void Start()
    {
        playButtonGroup = GetOrAddCanvasGroup(PlayButton);
        cursorGroup = GetOrAddCanvasGroup(cursor);

        playButtonGroup.alpha = 0f;
        cursorGroup.alpha = 0f;
        PlayButton.SetActive(false);
        cursor.SetActive(false);
        InMainMenu = true;

        StartCoroutine(FadeInUI());

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
    }

    void Update()
    {
        cursor.GetComponent<RectTransform>().position = Input.mousePosition;
    }

    private IEnumerator FadeInUI()
    {
        yield return new WaitForSeconds(delayBeforeFade);
        Cursor.lockState = CursorLockMode.None;
        PlayButton.SetActive(true);
        cursor.SetActive(true);
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            playButtonGroup.alpha = t;
            cursorGroup.alpha = t;
            yield return null;
        }
        playButtonGroup.alpha = 1f;
        cursorGroup.alpha = 1f;
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
        IntroCutscene.SetActive(true);
        cursor.SetActive(false);
        this.gameObject.SetActive(false);
        InMainMenu = false;
    }
}
