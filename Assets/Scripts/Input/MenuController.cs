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
    public static string filepath;

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
    public void LoadGame()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        menuOpen = false;

        stateManager.RestartGame();
        JSONFunctions.JSONFileFunctions.OpenJSONBrowser();
    }

    public void SaveGame()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        menuOpen = false;
        if (filepath is not null)
        {
            JSONFunctions.JSONFileFunctions.CreateOrEditJSON(filepath);
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
        Debug.Log("Current image path: " + JSONFunctions.JSONFileFunctions.CurrentImagePath);

        if (!string.IsNullOrEmpty(JSONFunctions.JSONFileFunctions.CurrentImagePath))
        {
            JSONFunctions.JSONFileFunctions.CreateOrEditJSON(JSONFunctions.JSONFileFunctions.CurrentImagePath);
            Debug.Log("Game autosaved on exit.");
        }
        else
        {
            Debug.LogWarning("No CurrentImagePath set. Game was not saved.");
        }

    //for testing in editor
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
    public bool MenuCheck()
    {
        if (menuOpen) return true; else return false;
    }

}
