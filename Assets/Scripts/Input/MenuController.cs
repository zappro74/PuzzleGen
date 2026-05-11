using UnityEngine;
using UnityEngine.InputSystem;
using SimpleFileBrowser;
using System.IO;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("Script Connections")]
    public GameStateManager stateManager;
    public Image imageLoad; 

    [Header("UI Connections")]
    public GameObject pauseMenu;
    private bool menuOpen = true;
    [Header("Leaderboard Text")]
    [SerializeField] private TextMeshProUGUI easyBestTimeText;
    [SerializeField] private TextMeshProUGUI mediumBestTimeText;
    [SerializeField] private TextMeshProUGUI hardBestTimeText;

    void Update()
    {
        if (FileBrowser.IsOpen) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OpenMenu();
        }
    }
    private void OnEnable()
    {
        UpdateLeaderboardDisplay();
    }
    private void UpdateLeaderboardDisplay()
    {
        if (easyBestTimeText != null)
        {
            easyBestTimeText.text = $"Easy:   {Leaderboard.GetBestTimeFormatted(GameMode.Easy)}";
        }  

        if (mediumBestTimeText != null)
        {
            mediumBestTimeText.text = $"Medium: {Leaderboard.GetBestTimeFormatted(GameMode.Medium)}";
        }

        if (hardBestTimeText != null)
        {
            hardBestTimeText.text = $"Hard:   {Leaderboard.GetBestTimeFormatted(GameMode.Hard)}";
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

        if (stateManager.hasWon)
        {
            Debug.Log("Puzzle already completed, skipping autosave.");

            if (!string.IsNullOrEmpty(JSONFunctions.JSONFileFunctions.CurrentSaveFilePath))
            {
                if (File.Exists(JSONFunctions.JSONFileFunctions.CurrentSaveFilePath))
                {
                    File.Delete(JSONFunctions.JSONFileFunctions.CurrentSaveFilePath);
                    Debug.Log("Deleted completed puzzle save file on exit.");
                }
                else
                {
                    Debug.Log("No save file found to delete.");
                }
            }
            else
            {
                Debug.Log("CurrentSaveFilePath was empty on exit.");
            }
        }
        else if (!string.IsNullOrEmpty(JSONFunctions.JSONFileFunctions.CurrentImagePath))
        {
            JSONFunctions.JSONFileFunctions.CreateOrEditJSON(JSONFunctions.JSONFileFunctions.CurrentImagePath);
            Debug.Log("Game autosaved on exit.");
        }
        else
        {
            Debug.LogWarning("No CurrentImagePath set. Game was not saved.");
        }

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
