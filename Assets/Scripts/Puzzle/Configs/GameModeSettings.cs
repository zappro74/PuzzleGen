using UnityEngine;

[System.Serializable]
public class GameModeSettings
{
    [Header("Mode")]
    public string modeName;

    [Header("Puzzle Size")]
    public int rows = 4;
    public int columns = 4;

    [Header("Gameplay")]
    public bool allowRotation = false;
    public bool allowShuffle = true;
    public bool allowPieceCollision = false;

    [Header("Snapping")]
    public float snapTolerance = 0.5f;

    [Header("Explosion Shuffle")]
    public float explosionForce = 1.5f;
    public float randomForce = 0.5f;
    public float torqueForce = 0.5f;
    public float shuffleDuration = 1.5f;

    [Header("Visual/Gameplay")]
    public bool showFullImagePreview = true;
    public bool allowGroupBreaking = false;

    [Header("Rotation")]
    public bool randomizeInitialRotation = false;
}