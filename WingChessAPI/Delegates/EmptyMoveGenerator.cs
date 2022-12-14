namespace WingChessAPI.Delegates;

using System.Collections.Generic;

public static class EmptyMoveGenerator
{
    public static IEnumerable<Move> Instance(Board _, int _2, int _3)
    {
        yield break;
    }

    public static GenerateMovesDelegate Wrapper(GenerateMovesDelegate generateMoves, MoveType _)
    {
        IEnumerable<Move> GenerateMoves(Board board, int x, int y)
        {
            foreach (var move in generateMoves(board, x, y))
            {
                yield return move;
            }
        }

        return GenerateMoves;
    }
}
