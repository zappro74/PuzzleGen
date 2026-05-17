using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionShuffle : MonoBehaviour
{
    public float delayBeforeExplosion = 0.75f;
    public float explosionForce = 1.5f;
    public float randomForce = .5f;
    public float torqueForce = .5f;
    public float shuffleDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource explosionAudio;

    public void ExplodePieces(List<GameObject> pieces)
    {
        StartCoroutine(ExplodeRoutine(pieces));
    }

    private IEnumerator ExplodeRoutine(List<GameObject> pieces)
    {
        yield return new WaitForSeconds(delayBeforeExplosion);


        if (explosionAudio != null)
        {
            explosionAudio.volume = 0.5f;
            explosionAudio.pitch = Random.Range(0.9f, 1.1f);
            explosionAudio.Play();
        }

        Vector2 center = GetCenter(pieces);
        var colliders = new List<Collider2D>();

        foreach (GameObject piece in pieces)
        {
            Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                rb = piece.AddComponent<Rigidbody2D>();
            }

            var colider = piece.GetComponent<Collider2D>();
            if (colider != null)
            {
                colliders.Add(colider);
            }

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.linearDamping = 2f;
            rb.angularDamping = 3f;
            rb.constraints = RigidbodyConstraints2D.None;

            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; 
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        for (int i = 0; i < colliders.Count; i++)
        {
            for (int j = i + 1; j < colliders.Count; j++)
            {
                Physics2D.IgnoreCollision(colliders[i], colliders[j], true);
            }
        }

        foreach (GameObject piece in pieces)
        {
            var rb = piece.GetComponent<Rigidbody2D>();
            var direction = ((Vector2)piece.transform.position - center).normalized;

            if (direction == Vector2.zero)
            {
                direction = Random.insideUnitCircle.normalized;
            }

            rb.AddForce(direction * explosionForce + Random.insideUnitCircle * randomForce, ForceMode2D.Impulse);
            rb.AddTorque(Random.Range(-torqueForce, torqueForce), ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(shuffleDuration);

        foreach (GameObject piece in pieces)
        {
            Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

                rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
            }
            piece.tag = "Piece";
        }

        for (int i = 0; i < colliders.Count; i++)
        {
            for (int j = i + 1; j < colliders.Count; j++)
            {
                Physics2D.IgnoreCollision(colliders[i], colliders[j], false);
            }
        }
    }

    private Vector2 GetCenter(List<GameObject> pieces)
    {
        Vector2 total = Vector2.zero;

        foreach (GameObject piece in pieces)
        {
            total += (Vector2)piece.transform.position;
        }

        return total / pieces.Count;
    }
}