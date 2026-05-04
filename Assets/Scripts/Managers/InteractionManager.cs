using System.Linq;
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
            // we love linq
            var topPiece = Physics2D.RaycastAll(mousePosition, Vector2.zero).Where(hit => hit.collider.CompareTag("Piece")).OrderByDescending(hit => hit.transform.GetComponent<Renderer>().sortingOrder).FirstOrDefault();

            if (topPiece.collider != null)
            {
                selection = topPiece.transform;
                offset = selection.position - mousePosition;
                renderer = topPiece.transform.GetComponent<Renderer>();
                order = Mathf.Max(order, renderer.sortingOrder) + 1;
                renderer.sortingOrder = order;
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