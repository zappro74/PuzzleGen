using UnityEngine;
using UnityEngine.UI;

public class ImageReferencing : MonoBehaviour
{
    [Header("Script Connections")]
    public GameStateManager stateManager;
    
    [Header("UI Connections")]
    public RawImage imageReference;
    public RawImage centerImage;
    public GameObject centerPanel;

    [Header("Maximum Image Sizes")]
    public Vector2 maxCenterSize = new Vector2(1000, 800);
    public Vector2 maxImageReference = new Vector2(250, 250);

    public void UpdateImages()
    {
        if (stateManager.image != null)
        {
            Debug.Log("Updating Image...");
            
            var texture = stateManager.image;
            
            imageReference.texture = texture;
            centerImage.texture = texture;

            Fit(imageReference, maxImageReference, texture);
            Fit(centerImage, maxCenterSize, texture);
        }
    }

    private void Fit(RawImage image, Vector2 maxBounds, Texture2D texture)
    {
        var imageRect = image.GetComponent<RectTransform>();
        var widthRatio = maxBounds.x / texture.width;
        var heightRatio = maxBounds.y / texture.height;
        var scaleFactor = Mathf.Min(widthRatio, heightRatio);

        imageRect.localRotation = Quaternion.identity;
        imageRect.localScale = Vector3.one;

        if (scaleFactor > 1f)
        {
            scaleFactor = 1f;
        }

        imageRect.sizeDelta = new Vector2(texture.width * scaleFactor, texture.height * scaleFactor);
    }
    public void OpenLargePreview()
    {
        centerPanel.gameObject.SetActive(true);
        imageReference.gameObject.SetActive(false);
        Debug.Log("Center image opened, reference closed.");
    }
    public void CloseLargePreview()
    {
        centerPanel.gameObject.SetActive(false);
        imageReference.gameObject.SetActive(true);
        Debug.Log($"Center image closed, reference opened.");
    }
}