using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GifProjector : MonoBehaviour
{
    public RenderTexture canvas { get; private set; } 

    private List<Texture2D> frames;
    private List<float> delays;
    private int current = 0;
    private Coroutine coroutine;

    public void StartProjection(List<Texture2D> gifFrames, List<float> gifDelays, int width, int height)
    {
        StopProjection();

        frames = gifFrames;
        delays = gifDelays;
        current = 0;

        canvas = new RenderTexture(width, height, 0);

        coroutine = StartCoroutine(AnimateGif());
    }

    public void StopProjection()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
        
        if (canvas != null)
        {
            canvas.Release();
            Destroy(canvas);
            canvas = null;
        }
    }

    private IEnumerator AnimateGif()
    {
        while (frames != null && frames.Count > 0)
        {
            Graphics.Blit(frames[current], canvas);

            yield return new WaitForSeconds(delays[current]);

            current = (current + 1) % frames.Count;
        }
    }
}