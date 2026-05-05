using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    private Camera gameCamera;
    private Transform selection;
    private Vector3 offset;
    private Renderer render;
    private int order = 1;
  
    void Start()
    {
        gameCamera = Camera.main;
    }
    void Update()
    {
        if (Mouse.current == null) 
        {
            return;
        }

        var leftButton = Mouse.current.leftButton;
        var mousePosition = gameCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if (leftButton.wasPressedThisFrame)
        {
            // we love linq
            var topPiece = Physics2D.RaycastAll(mousePosition, Vector2.zero).Where(cast => cast.collider.CompareTag("Piece")).OrderByDescending(cast => cast.transform.GetComponent<Renderer>().sortingOrder).FirstOrDefault();

            if (topPiece.collider != null)
            {   
                selection = topPiece.transform;
                offset = selection.position - mousePosition;
                render = topPiece.transform.GetComponent<Renderer>();
                order = Mathf.Max(order, render.sortingOrder) + 1;
                render.sortingOrder = order;
            }
        }

        if (leftButton.isPressed && selection != null)
        {
            var movement = mousePosition + offset;
            var screenHeight = gameCamera.orthographicSize;
            var screenWidth = screenHeight * gameCamera.aspect;
            var cameraPosition = gameCamera.transform.position;
            var left = cameraPosition.x - screenWidth;
            var right = cameraPosition.x + screenWidth;
            var bottom = cameraPosition.y - screenHeight;
            var top = cameraPosition.y + screenHeight;

            movement.x = Mathf.Clamp(movement.x, left, right - render.bounds.size.x);
            movement.y = Mathf.Clamp(movement.y, bottom, top - render.bounds.size.y);            
            selection.position = movement;
        }

        if (leftButton.wasReleasedThisFrame)
        {
            selection = null;
        }
    }
}