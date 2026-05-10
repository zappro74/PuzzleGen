using UnityEngine;
using UnityEngine.InputSystem;
using SimpleFileBrowser;
using JSONFunctions;
using UnityEditor;

public class MenuController : MonoBehaviour
{
    [Header("Script Connections")]
    public GameStateManager stateManager;
    public Image imageLoad;
    public JSONFileFunctions JSON;


    [Header("UI Connections")]
    public GameObject pauseMenu;
    private bool menuOpen = true;

    string filepath;
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

    public void LoadGame()
    {
        OpenMenu();
        stateManager.RestartGame();
        JSONFileFunctions.OpenJSONBrowser();
    }

    public void SaveGame()
    {
        OpenMenu();
        string path;
        if (Image.filepath is not null)
        {
            path = Image.filepath;
        }
        else if (JSONFileFunctions.FilePath is not null)
        {
            path = JSONFileFunctions.FilePath;
        }
        else
        {
            return;
        }
        JSONFileFunctions.CreateOrEditJSON(path);
    }


    public void RestartGame()
    {
        OpenMenu();
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
