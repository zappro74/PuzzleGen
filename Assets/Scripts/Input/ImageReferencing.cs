using UnityEngine;
using UnityEngine.UI;

public class ImageReferencing : MonoBehaviour
{
    [Header("Connections")]
    public GameStateManager stateManager;
    
    [Header("UI Elements")]
    public RawImage imageReference;
    public RawImage centerImage;

    [Header("Maximum Allowed Sizes")]
    public Vector2 maxCenterSize = new Vector2(1000, 800);
    public Vector2 maxImageReference = new Vector2(250, 250);

    public void UpdateReferenceImages()
    {
        if (stateManager.image != null)
        {
            Texture2D tex = stateManager.image;
            
            imageReference.texture = tex;
            centerImage.texture = tex;

            Fit(imageReference, maxImageReference, tex);
            Fit(centerImage, maxCenterSize, tex);
        }
    }

    private void Fit(RawImage img, Vector2 maxBounds, Texture2D tex)
    {
        RectTransform imgRect = img.GetComponent<RectTransform>();

        imgRect.localRotation = Quaternion.identity;
        imgRect.localScale = Vector3.one;

        float widthRatio = maxBounds.x / tex.width;
        float heightRatio = maxBounds.y / tex.height;

        float scaleFactor = Mathf.Min(widthRatio, heightRatio);

        if (scaleFactor > 1f)
        {
            scaleFactor = 1f;
        }

        imgRect.sizeDelta = new Vector2(tex.width * scaleFactor, tex.height * scaleFactor);
    }
    public void OpenLargePreview()
    {
        centerImage.gameObject.SetActive(true);
        imageReference.gameObject.SetActive(false);
        Debug.Log("Center image opened, reference closed.");
    }
    public void CloseLargePreview()
    {
        centerImage.gameObject.SetActive(false);
        imageReference.gameObject.SetActive(true);
        Debug.Log($"Center image closed, reference opened.");
    }
}