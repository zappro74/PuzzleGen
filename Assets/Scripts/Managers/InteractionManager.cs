using UnityEngine;
using UnityEngine.InputSystem;
using SimpleFileBrowser;
public class InteractionManager : MonoBehaviour
{
    [Header("Script Connections")]
    public MenuController menuController;
    public CameraController cameraController;
    public PieceController pieceController;

    [Header("UI Connections")]
    public GameObject centerPanel;

    private Camera gameCamera;
    void Start()
    {
        gameCamera = Camera.main;
    }
    void Update()
    {
        if (Mouse.current == null || gameCamera == null || FileBrowser.IsOpen || menuController.MenuCheck() || centerPanel.gameObject.activeInHierarchy) return;
        
        var leftButton = Mouse.current.leftButton;
        var y = Mouse.current.scroll.ReadValue().y;
        Vector3 mousePosition = gameCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0f;

        if (Mathf.Abs(y) > 0.01f)
        {
            cameraController.ZoomCamera(y, mousePosition);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            pieceController.TryRotate(mousePosition);
        }

        if (leftButton.wasPressedThisFrame)
        {
            bool selected = pieceController.TryPickup(mousePosition);

            if (!selected)
            {
                cameraController.StartPanning(mousePosition);
            }
        }
        else if (leftButton.isPressed)
        {
            if (pieceController.IsHoldingPiece())
            {
                pieceController.DragPiece(mousePosition);
            }
            else if (cameraController.isPanning)
            {
                cameraController.PanCamera(mousePosition);
            }
        }
        else if (leftButton.wasReleasedThisFrame)
        {
            pieceController.ReleasePiece();
            cameraController.StopPanning();
        }
    }
}