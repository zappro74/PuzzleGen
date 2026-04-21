using UnityEngine;
using System.IO;
using SimpleFileBrowser;

public class Image : MonoBehaviour
{
    [Header("Connections")]
    public GameStateManager stateManager; 
    public ImageReferencing imageReference;

    void Start()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".png", ".jpg", ".jpeg"));
        FileBrowser.SetDefaultFilter(".png");
    }
    public void OpenImageBrowser()
    {
        FileBrowser.ShowLoadDialog((paths) => { OnFileSelected(paths[0]); },
            () => { Debug.Log("File selection cancelled..."); },
            FileBrowser.PickMode.Files, false, null, null, "Select Puzzle Image", "Load"
        );
    }
    private void OnFileSelected(string filePath)
    {
        Debug.Log($"Path chosen: {filePath}");
        LoadImageFromDisk(filePath);
    }
    public void LoadImageFromDisk(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"Error: File could not be found at path: {filePath}");
            return;
        }

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);

        if (texture.LoadImage(fileData))
        {
            Debug.Log($"File successfully loaded at: {filePath} - with size: {texture.width} x {texture.height}");

            imageReference.imageReference.gameObject.SetActive(true);

            stateManager.image = texture;
            
            if (imageReference != null) 
            {
                imageReference.UpdateReferenceImages();
            }
        }
        else
        {
            Debug.LogError("Error: File is not a supported image type.");
        }
    }
}
