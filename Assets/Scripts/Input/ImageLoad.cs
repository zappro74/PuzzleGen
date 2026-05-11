using UnityEngine;
using System.IO;
using SimpleFileBrowser;

public class Image : MonoBehaviour
{
    [Header("Script Connections")]
    public GameStateManager stateManager; 
    public ImageReferencing referencing;
    public GifLoader gifLoader;

    public void OpenImageBrowser()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".png", ".jpg", ".jpeg", ".gif"));
        FileBrowser.SetDefaultFilter(".png");

        FileBrowser.ShowLoadDialog((paths) => { LoadImageFromDisk(paths[0]); },
            () => { Debug.Log("File selection cancelled."); },
            FileBrowser.PickMode.Files, false, null, null, "Select Puzzle Image", "Load"
        );
    }
    public void LoadImageFromDisk(string path)
    {
        Debug.Log($"Path chosen: {path}");
        if (!File.Exists(path))
        {
            Debug.LogError($"File could not be found: {path}");
            return;
        }

        JSONFunctions.JSONFileFunctions.CurrentImagePath = path;

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

        var texture = new Texture2D(2, 2);
        if (texture.LoadImage(File.ReadAllBytes(path)))
        {
            Debug.Log($"File successfully loaded at: {path} - with size: {texture.width} x {texture.height}");

            referencing.imageReference.gameObject.SetActive(true);

            if (gifLoader != null && gifLoader.gifProjector != null)
            {
                gifLoader.gifProjector.StopProjection();
            }

            stateManager.image = texture;
            
            if (referencing != null) 
            {
                referencing.UpdateImages();
            }

            stateManager.PrepareNewGame(texture);
        }
        else
        {
            Debug.LogError("Error: File is not a supported image type.");
        }
    }
}
