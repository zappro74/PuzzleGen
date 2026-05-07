using UnityEngine;
using System.IO;
using SimpleFileBrowser;

public class Image : MonoBehaviour
{
    [Header("Script Connections")]
    public GameStateManager stateManager; 
    public ImageReferencing imageReference;
    public GifLoader gifLoader;

    void Start()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".png", ".jpg", ".jpeg", ".gif"));
        FileBrowser.SetDefaultFilter(".png");
    }
    public void OpenImageBrowser()
    {
        FileBrowser.ShowLoadDialog((paths) => { OnFileSelected(paths[0]); },
            () => { Debug.Log("File selection cancelled."); },
            FileBrowser.PickMode.Files, false, null, null, "Select Puzzle Image", "Load"
        );
    }
    private void OnFileSelected(string path)
    {
        Debug.Log($"Path chosen: {path}");
        LoadImageFromDisk(path);
    }
    public void LoadImageFromDisk(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError($"File could not be found: {path}");
            return;
        }

        if (Path.GetExtension(path).ToLower() == ".gif")
        {
            if (gifLoader != null)
            {
                gifLoader.LoadGif(path);
            }
            else
            {
                Debug.LogWarning("GifLoader is missing.");
            }
            
            return; 
        }

        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);

        if (texture.LoadImage(fileData))
        {
            Debug.Log($"File successfully loaded at: {path} - with size: {texture.width} x {texture.height}");

            imageReference.imageReference.gameObject.SetActive(true);

            if (gifLoader != null && gifLoader.gifProjector != null)
            {
                gifLoader.gifProjector.StopProjection();
            }

            stateManager.image = texture;
            
            if (imageReference != null) 
            {
                imageReference.UpdateImages();
            }

            stateManager.PrepareNewGame(texture);
        }
        else
        {
            Debug.LogError("Error: File is not a supported image type.");
        }
    }
}
