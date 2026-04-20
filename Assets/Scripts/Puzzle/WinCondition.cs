using System;
using System.Collections.Generic;

public static class WinCondition
{
    public static event Action OnPuzzleSolved;

    public static bool CheckWin(Dictionary<int, PuzzleGroup> groups, int totalPieceCount)
    {
        if (groups == null || groups.Count != 1)
            return false;

        foreach (var group in groups.Values)
        {
            if (group.Count == totalPieceCount)
            {
                OnPuzzleSolved?.Invoke();
                return true;
            }
        }
        return false;
    }
}
