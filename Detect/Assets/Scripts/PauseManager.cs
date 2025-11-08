using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public GameObject ResumeButton;
    public GameObject ControlsButton;
    public GameObject QuitButton;
    public GameObject SettingsButton;

    public GameObject ControlsScreen;
    public GameObject BackButton;
    public GameObject crosshair;
    public GameObject background;

    private bool inMenu = false;
    private MainMenuManager mainMenuManager;
    private SettingsManager settingsManager;

    void Start()
    {
        mainMenuManager = FindAnyObjectByType<MainMenuManager>();
        settingsManager = FindAnyObjectByType<SettingsManager>();
        HideMenu();
    }

    public void OnPausePressed(InputAction.CallbackContext context)
    {
        if (mainMenuManager.InMainMenu || CutsceneManager.Instance.IsInCutscene) return;
        if (inMenu)
        {
            HideMenu();
        }
        else
        {
            ShowMenu();
        }
    }

    public void ShowMenu()
    {
        ResumeButton.SetActive(true);
        ControlsButton.SetActive(true);
        QuitButton.SetActive(true);
        ControlsScreen.SetActive(false);
        SettingsButton.SetActive(true);
        BackButton.SetActive(false);
        settingsManager.ShowUI(false);
        background.SetActive(true);

        inMenu = true;
        crosshair.SetActive(false);

        //Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        CursorManager.Instance.ShowCursor(true);
        FindFirstObjectByType<FPController>().SetPuzzleActive(true);
    }

    public void HideMenu()
    {
        ResumeButton.SetActive(false);
        ControlsButton.SetActive(false);
        QuitButton.SetActive(false);
        ControlsScreen.SetActive(false);
        BackButton.SetActive(false);
        SettingsButton.SetActive(false);
        background.SetActive(false);

        inMenu = false;
        crosshair.SetActive(true);

        //Time.timeScale = 1f;
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        CursorManager.Instance.ShowCursor(false);
        FindFirstObjectByType<FPController>().SetPuzzleActive(false);
    }

    public void Resume()
    {
        HideMenu();
    }

    public void Controls()
    {
        ResumeButton.SetActive(false);
        ControlsButton.SetActive(false);
        QuitButton.SetActive(false);
        ControlsScreen.SetActive(true);
        BackButton.SetActive(true);
    }
    
    public void ShowSettings()
    {
        settingsManager.ShowUI(true);
        BackButton.SetActive(true);
        background.SetActive(false);
    }

    public void Back()
    {
        ShowMenu();
    }

    public void Quit()
    {
        Application.Quit(0);
    }
}