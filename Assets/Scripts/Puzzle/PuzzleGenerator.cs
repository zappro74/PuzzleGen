using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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
        var rand = new System.Random(generationSeed);

        for(int row = 1; row <= rows; row++)
        {
            for(int column = 1; column <= columns; column++)
            {
                PieceData piece = new PieceData(idNumber, row, column);

                //ASSIGNING FLAT EDGES
                if(row == 1){ piece.TopEdge = EdgeType.Flat; }
                if(row == rows){ piece.BottomEdge = EdgeType.Flat; }
                if(column == 1){ piece.LeftEdge = EdgeType.Flat; }
                if(column == columns){ piece.RightEdge = EdgeType.Flat; }


                //ASSIGNING RIGHT EDGE
                if(piece.RightEdge != EdgeType.Flat)
                {
                    piece.RightEdge = GetSeededEdgeType(row, column, PieceDirection.Right);
                }
                
                //ASSIGNING BOTTOM EDGE
                if(piece.BottomEdge != EdgeType.Flat)
                {
                    piece.BottomEdge = GetSeededEdgeType(row, column, PieceDirection.Bottom);
                }

                //ASSIGNING LEFT EDGE
                if(piece.LeftEdge != EdgeType.Flat)
                {
                    if(generatedPieces[idNumber - 1].RightEdge == EdgeType.Intruded)
                    {
                        piece.LeftEdge = EdgeType.Extruded;
                    }
                    else
                    {
                        piece.LeftEdge = EdgeType.Intruded;
                    }
                }

                //ASSIGNING TOP EDGE
                if(piece.TopEdge != EdgeType.Flat)
                {
                    if(generatedPieces[idNumber - columns].BottomEdge == EdgeType.Intruded)
                    {
                        piece.TopEdge = EdgeType.Extruded;
                    }
                    else
                    {
                        piece.TopEdge = EdgeType.Intruded;
                    }
                }

                generatedPieces.Add(piece);
                idNumber++;
            }
        }
        return generatedPieces;
    }
}
