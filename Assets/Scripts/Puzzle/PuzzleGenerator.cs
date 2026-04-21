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
        var rowTracker = 1;
        var columnTracker = 1;

        for(var i = 0; i < rows; i++)
        {
            for(var j = 0; j < columns; j++)
            {
                PieceData piece = new PieceData(idNumber, rowTracker, columnTracker);
                idNumber++;
                columnTracker++;
                generatedPieces.Add(piece);

            }
            rowTracker++;
        }
        return generatedPieces;
    }
}