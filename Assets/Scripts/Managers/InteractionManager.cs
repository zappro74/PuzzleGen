using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    private Camera camera;
    private Transform selection;
    private Vector3 offset;

    void Start()
    {
        camera = Camera.main;
    }

    void Update()
    {
        var leftButton = Mouse.current.leftButton;
        var mousePosition = camera.ScreenToWorldPoint(Mouse.current.position.ReadValue());


        if (Mouse.current == null) 
        {
            return;
        }

        if (leftButton.wasPressedThisFrame)
        {
            var hit = Physics2D.Raycast(mousePosition, Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("Piece"))
            {
                selection = hit.transform;
                offset = selection.position - mousePosition;
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