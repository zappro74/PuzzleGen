using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GifLoader : MonoBehaviour
{
    [Header("Connections")]
    public GifProjector gifProjector;
    public GameStateManager gameStateManager;

    public void LoadGif(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogError($"File not found: {path}");
            return;
        }

        var bytes = File.ReadAllBytes(path);
        StartCoroutine(Decode(bytes));
    }

    private IEnumerator Decode(byte[] bytes)
    {
        var frames = new List<Texture2D>();
        var delays = new List<float>();
        
        bool isDecoding = true;

        yield return StartCoroutine(UniGif.GetTextureListCoroutine(bytes, (textureList, count, width, height) =>
        {
            if (textureList != null)
            {
                foreach (var texture in textureList)
                {
                    frames.Add(texture.m_texture2d);
                    delays.Add(texture.m_delaySec);
                }
            }
            else
            {
                Debug.LogError("Failed to decode GIF.");
            }
            
            isDecoding = false;
        }));

        while (isDecoding)
        {
            yield return null;
        }

        if (frames.Count > 0)
        {
            gifProjector.StartProjection(frames, delays, frames[0].width, frames[0].height);
            gameStateManager.GenerateNewPuzzle(gifProjector.canvas);
        }
    }
}