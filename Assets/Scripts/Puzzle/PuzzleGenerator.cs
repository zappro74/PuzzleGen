using System.Collections.Generic;
using UnityEngine;

public class PuzzleGenerator
{

    //AT THE MOMENT THIS METHOD IS VERY BASIC
    //ONCE I FINISH THE EDGE ASSIGNMENT, THIS METHOD WILL ALSO SET EACH PIECE'S EDGE DATA
    public List<PieceData> Generate(int rows, int columns, int generations)
    {
        var generatedPieces = new List<PieceData>();
        var idNumber = 0;

        for(int row = 1; row <= rows; i++)
        {
            for(int column = 1; column <= columns; j++)
            {
                PieceData piece = new PieceData(idNumber, rowTracker, columnTracker);
                generatedPieces.Add(piece);
                idNumber++;
            }
        }
        return generatedPieces;
    }
}
