using NUnit.Framework;
using System.Collections.Generic;

public class WinConditionTests
{
    [Test]
    public void PartiallyConnectedPuzzle_IsNotSolved()
    {
        // 3 pieces, 2 groups -> not solved
        var groups = new Dictionary<int, PuzzleGroup>();
        var g1 = new PuzzleGroup(0, 0);
        g1.AddPiece(1);
        var g2 = new PuzzleGroup(1, 2);
        groups[0] = g1;
        groups[1] = g2;

        bool result = WinCondition.CheckWin(groups, 3);
        Assert.IsFalse(result);
    }

    [Test]
    public void FullyConnectedPuzzle_IsSolved()
    {
        // 3 pieces, 1 group -> solved
        var groups = new Dictionary<int, PuzzleGroup>();
        var g1 = new PuzzleGroup(0, 0);
        g1.AddPiece(1);
        g1.AddPiece(2);
        groups[0] = g1;

        bool result = WinCondition.CheckWin(groups, 3);
        Assert.IsTrue(result);
    }

    [Test]
    public void OneGroupButMissingPieces_IsNotSolved()
    {
        // 1 group but only 2 of 3 pieces
        var groups = new Dictionary<int, PuzzleGroup>();
        var g1 = new PuzzleGroup(0, 0);
        g1.AddPiece(1);
        groups[0] = g1;

        bool result = WinCondition.CheckWin(groups, 3);
        Assert.IsFalse(result);
    }

    [Test]
    public void OnPuzzleSolved_EventFires_WhenWon()
    {
        bool eventFired = false;
        WinCondition.OnPuzzleSolved += () => eventFired = true;

        var groups = new Dictionary<int, PuzzleGroup>();
        var g1 = new PuzzleGroup(0, 0);
        g1.AddPiece(1);
        groups[0] = g1;

        WinCondition.CheckWin(groups, 2);
        Assert.IsTrue(eventFired);

        WinCondition.OnPuzzleSolved -= () => eventFired = true;
    }
}
