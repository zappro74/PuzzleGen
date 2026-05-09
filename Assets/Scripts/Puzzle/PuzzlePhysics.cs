using UnityEngine;

public class PuzzlePhysics: MonoBehaviour
{
    [Header("Friction & Bounciness")]
    public float bounciness = 0.70f;
    public float friction = 1f; 

    [Header("Grab Physics")]
    public float dragSpeed = 15f;
    
    private PhysicsMaterial2D bouncyMaterial;

    void Awake()
    {
        bouncyMaterial = new PhysicsMaterial2D("PieceBouncy");
        bouncyMaterial.bounciness = bounciness;
        bouncyMaterial.friction = 0f;
    }
    public void EnablePhysics(Rigidbody2D rigidbody, Collider2D collider)
    {
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        rigidbody.gravityScale = 0f;
        rigidbody.linearDamping = friction; 
        rigidbody.angularDamping = 1.5f;
        rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate; 

        if (collider != null)
        {
            collider.sharedMaterial = bouncyMaterial;
        }
    }

    public void DragPiece(Rigidbody2D rigidbody, Vector3 target)
    {
        var direction = target - (Vector3)rigidbody.position;
        rigidbody.linearVelocity = direction * dragSpeed;
    }

    public void HandleSnappingPhysics(Transform root, Transform target)
    {
        var rigidbodies = root.GetComponentsInChildren<Rigidbody2D>();
        
        foreach (var rigidbody in rigidbodies)
        {
            Destroy(rigidbody);
        }
    }
}