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
            explosionAudio.pitch = Random.Range(0.9f, 1.1f);
            explosionAudio.Play();
        }

        Vector2 center = GetCenter(pieces);

        foreach (GameObject piece in pieces)
        {
            Rigidbody2D rb = piece.GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                rb = piece.AddComponent<Rigidbody2D>();
            }

            Collider2D colider = piece.GetComponent<Collider2D>();

            if (colider != null)
            {
                colider.enabled = false;
            }

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.linearDamping = 2f;
            rb.angularDamping = 3f;
            rb.constraints = RigidbodyConstraints2D.None;

            Vector2 direction = ((Vector2)piece.transform.position - center).normalized;

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
            }

            Collider2D colider = piece.GetComponent<Collider2D>();

            if (colider != null)
            {
                colider.enabled = true;
            }

            piece.tag = "Piece";
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