using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    [Header("Script Connections")]
    public GameStateManager stateManager;
    public Image imageLoad; // We need this to trigger the file browser

    [Header("UI Connections")]
    public GameObject pauseMenu;

    private bool menuOpen = false;

    void Update()
    {
        // Continuously looks for the esc key
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OpenMenu();
        }
    }
    public void OpenMenu()
    {
        menuOpen = !menuOpen;
        pauseMenu.SetActive(menuOpen);

        if (menuOpen)
        {
            stateManager.PauseGame();
        }
        else
        {
            stateManager.ResumeGame();
        }
    }
    public void StartNewGame()
    {
        OpenMenu();
        stateManager.RestartGame();
        imageLoad.OpenImageBrowser(); 
    }
    public void RestartGame()
    {
        OpenMenu(); 
        stateManager.RestartTimer();
    }
    public void ExitGame()
    {
        Debug.Log("Exiting Application");
        Application.Quit(); 
    }

}
