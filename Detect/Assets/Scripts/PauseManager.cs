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

    void Start()
    {
        HideMenu();
    }

    void Update()
    {
        
    }

    public void OnPausePressed(InputAction.CallbackContext context)
    {
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
        Cursor.visible = true;
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
        Cursor.visible = false;
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
