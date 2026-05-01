using System;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleGenerator
{
    private int rows {get; set;}
    private int columns{get; set;}
    private int generationSeed {get;}

    //generates the puzzle seed
    public PuzzleGenerator(int rows, int columns, int generationSeed)
    {
        this.rows = rows;
        this.columns = columns;
        this.generationSeed = generationSeed;
    }

    public int GenerationSeed()
    {
        return generationSeed;
    }

    private EdgeType GetSeededEdgeType(int row, int column, PieceDirection direction)
    {
        int value = generationSeed;
        value ^= row * 7013;
        value ^= column * 3347;
        value ^= (int)direction * 8839;
        value = Mathf.Abs(value);
        return value % 2 == 0 ? EdgeType.Intruded : EdgeType.Extruded;
    }
        

    public List<PieceData> Generate()
    {
        var generatedPieces = new List<PieceData>();
        var idNumber = 0;

        for(int row = 0; row < rows; row++)
        {
            for(int column = 0; column < columns; column++)
            {
                PieceData piece = new PieceData(idNumber, row, column, idNumber);

                //ASSIGNING RIGHT EDGE
                if(column == columns - 1)
                {
                    piece.RightEdge = EdgeType.Flat;
                }
                else
                {
                    piece.RightEdge = GetSeededEdgeType(row, column, PieceDirection.Right);
                }
                
                //ASSIGNING BOTTOM EDGE
                if(row == rows - 1)
                {
                    piece.BottomEdge = EdgeType.Flat;
                }
                else
                {
                    piece.BottomEdge = GetSeededEdgeType(row, column, PieceDirection.Bottom);
                }

                //ASSIGNING LEFT EDGE
                if(column == 0)
                {
                    piece.LeftEdge = EdgeType.Flat;
                }
                else
                {
                    PieceData pieceLeft = generatedPieces[idNumber - 1];
                    piece.LeftEdge = InvertEdge(pieceLeft.RightEdge);
                }

                //ASSIGNING TOP EDGE
                if(row == 0)
                {
                    piece.TopEdge = EdgeType.Flat;
                }
                else
                {
                    PieceData pieceUp = generatedPieces[idNumber - columns];
                    piece.TopEdge = InvertEdge(pieceUp.BottomEdge);
                }

                generatedPieces.Add(piece);
                idNumber++;
            }
        }
        return generatedPieces;
    }

    private EdgeType InvertEdge(EdgeType edge)
    {
        if (edge == EdgeType.Extruded)
        {
            return EdgeType.Intruded;
        }

        if (edge == EdgeType.Intruded)
        {
            return EdgeType.Extruded;
        }

        return EdgeType.Flat;
    }
}
