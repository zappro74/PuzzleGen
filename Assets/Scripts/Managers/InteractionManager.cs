using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using SimpleFileBrowser;
public class InteractionManager : MonoBehaviour
{
    [Header("Script Connections")]
    public SnappingManager snapManager;
    public MenuController menuController;
    public GameStateManager gameStateManager;
    public GameModeController modeController;

    [Header("UI Connections")]
    public GameObject centerPanel;

  
    void Start()
    {
        gameCamera = Camera.main;
        BoundaryLines();
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
            ZoomCamera(y, mousePosition);
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            TryRotate(mousePosition);
        }

        if (leftButton.wasPressedThisFrame)
        {
            TryPickup(mousePosition);

            if (selection == null)
            {
                isPanning = true;
                origin = mousePosition;
            }
        }
        else if (leftButton.isPressed)
        {
            if (selection != null && render != null) 
            {
                Vector3 groupCenter = GetGroupCenter(selection);

                Vector3 centerToRoot = selection.position - groupCenter;
                Vector3 targetPosition = mousePosition + centerToRoot;

                targetPosition.z = selection.position.z;
                targetPosition = WorldBoundaries(targetPosition);

                int groupSize = selection.GetComponentsInChildren<PuzzlePiece>().Length;

                float adjustedSmoothTime = dragSmoothTime * (1f + ((groupSize - 1) * 0.15f));

                selection.position = Vector3.SmoothDamp(selection.position, targetPosition, ref dragVelocity, adjustedSmoothTime);
                
                float speed = (selection.position - lastDragPosition).magnitude / Time.deltaTime;

                bool isSnapping = false;

                UpdateDragAudio(speed, groupSize, isSnapping);

                lastDragPosition = selection.position;

                if (snapManager != null && modeController.currentGameMode == GameMode.Easy)
                {
                    bool didSnap = snapManager.TryAutoSnap(selection);

                    if (didSnap)
                    {
                        selection = null;
                        render = null;
                        dragVelocity = Vector3.zero;
                        isPanning = false;

                        StartCoroutine(FadeOutDragAudio());
                        return;
                    }
                }
            }
            else if (isPanning)
            {
                PanCamera(mousePosition);
            }
        }
        
        if (leftButton.wasReleasedThisFrame)
        {
            if (selection != null && snapManager != null && gameStateManager != null && modeController.currentGameMode != GameMode.Easy)
            {
                snapManager.TrySnap(selection);
            }

            StartCoroutine(FadeOutDragAudio());

            selection = null;
            render = null;
            dragVelocity = Vector3.zero;
            isPanning = false;
        }
    }
}