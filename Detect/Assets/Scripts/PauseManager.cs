using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    public GameObject ResumeButton;
    public GameObject ControlsButton;
    public GameObject QuitButton;
    public GameObject ControlsScreen;
    public GameObject BackButton;

    private bool inMenu = false;
    public RectTransform cursor;
    private MainMenuManager mainMenuManager;

    void Start()
    {
        mainMenuManager = FindAnyObjectByType<MainMenuManager>();
        HideMenu();
    }

    void Update()
    {
        if (inMenu)
        {
            cursor.position = Input.mousePosition;
        }
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

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
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

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        FindFirstObjectByType<FPController>().SetPuzzleActive(false);
        cursor.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
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
