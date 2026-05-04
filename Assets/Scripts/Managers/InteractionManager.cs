using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    private Camera camera;
    private Transform selection;
    private Vector3 offset;
    private Renderer renderer;
    private int order = 1;

    void Start()
    {
        camera = Camera.main;
    }

    void Update()
    {
        if (Mouse.current == null) 
        {
            return;
        }

        var leftButton = Mouse.current.leftButton;
        var mousePosition = camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if (leftButton.wasPressedThisFrame)
        {
            var hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("Piece"))
            {
                selection = hit.transform;
                offset = selection.position - mousePosition;

                renderer = selection.GetComponent<Renderer>();
                if (renderer != null)
                {
                    order++;
                    renderer.sortingOrder = order;
                }
            }
        }

        if (leftButton.isPressed && selection != null)
        {
            selection.position = mousePosition + offset;
        }

        if (leftButton.wasReleasedThisFrame)
        {
            selection = null;
        }
    }
}