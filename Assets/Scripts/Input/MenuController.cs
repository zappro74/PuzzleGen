using UnityEngine;
using UnityEngine.InputSystem;
using SimpleFileBrowser;

public class MenuController : MonoBehaviour
{
    [Header("Script Connections")]
    public GameStateManager stateManager;
    public Image imageLoad; 

    [Header("UI Connections")]
    public GameObject pauseMenu;
    private bool menuOpen = true;

    void Update()
    {
        if (FileBrowser.IsOpen) 
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OpenMenu();
        }
    }
    public void OpenMenu()
    {
        menuOpen = !menuOpen;
        pauseMenu.SetActive(menuOpen);

        if (MenuCheck())
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
        stateManager.RestartGame();
        stateManager.ResetPuzzle();
    }
    public void ExitGame()
    {
        Debug.Log("Exiting Application");
        Application.Quit(); 
    }
    public bool MenuCheck()
    {
        if (menuOpen) return true; else return false;
    }

}
