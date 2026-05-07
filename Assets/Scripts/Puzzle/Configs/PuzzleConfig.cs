using UnityEngine;

[System.Serializable]
public class PuzzleConfig
{
    public Texture puzzleImage; 
    public int rows;
    public int columns;
    public int generationSeed;
    public PieceConfig pieceConfig;
}
