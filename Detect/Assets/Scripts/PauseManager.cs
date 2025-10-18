using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public GameObject ResumeButton;
    public GameObject ControlsButton;
    public GameObject QuitButton;
    public GameObject ControlsScreen;
    public GameObject BackButton;
    public GameObject crosshair;

    private bool inMenu = false;
    private MainMenuManager mainMenuManager;

    void Start()
    {
        mainMenuManager = FindAnyObjectByType<MainMenuManager>();
        HideMenu();
    }

    public void OnPausePressed(InputAction.CallbackContext context)
    {
        if (mainMenuManager.InMainMenu) return;
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
        BackButton.SetActive(false);

        inMenu = true;
        crosshair.SetActive(false);

        Time.timeScale = 0f;
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

        inMenu = false;
        crosshair.SetActive(true);

        Time.timeScale = 1f;
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

    public void Back()
    {
        ShowMenu();
    }

    public void Quit()
    {
        Application.Quit(0);
    }
}
