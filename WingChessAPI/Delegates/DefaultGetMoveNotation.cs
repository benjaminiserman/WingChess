namespace WingChessAPI.Delegates;

public static class DefaultGetMoveNotation
{
    public static GetMoveNotationDelegate Instance => move => $"{move.Board.GetNotation(move.OldX, move.OldY)}{move.Board.GetNotation(move.NewX, move.NewY)}";
}
