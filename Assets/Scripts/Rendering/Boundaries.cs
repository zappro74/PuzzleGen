using UnityEngine;

public class Boundaries : MonoBehaviour
{
    [Header("Boundary Settings")]
    public Vector2 boundaries = new Vector2(30f, 20f);
    [SerializeField] private Color boundaryColor = Color.red;
    [SerializeField] private float boundaryThickness = 0.15f;

    private void BoundaryLines()
    {
        var x = boundaries.x / 2f;
        var y = boundaries.y / 2f;
        var lineObject = new GameObject("BoundaryBox");
        var renderer = lineObject.AddComponent<LineRenderer>();

        lineObject.transform.SetParent(transform); 
        
        renderer.material = new Material(Shader.Find("Sprites/Default")); 
        renderer.startWidth = boundaryThickness;
        renderer.endWidth = boundaryThickness;
        renderer.sortingOrder = 500; 
        renderer.startColor = boundaryColor;
        renderer.endColor = boundaryColor;

        renderer.positionCount = 4;
        renderer.SetPositions(new Vector3[] 
        { 
            new(-x, -y, 0),
            new(-x, y, 0),  
            new(x, y, 0),  
            new(x, -y, 0)   
        });
        renderer.loop = true;    

        var collider = lineObject.AddComponent<EdgeCollider2D>();
        var points = new Vector2[] 
        {
            new(-x, -y),
            new(-x, y),
            new(x, y),
            new(x, -y),
            new(-x, -y)
        };
        collider.points = points;

        var bounce = new PhysicsMaterial2D("Wall");
        bounce.bounciness = 1f; 
        bounce.friction = 0.1f;
        
        collider.sharedMaterial = bounce;
    }
    
}
