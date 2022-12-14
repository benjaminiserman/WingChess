namespace WingChessAPI.Delegates;

public static class DefaultResult
{
    public static ResultDelegate Instance => (board, move) =>
    {
        if (move.OldX == move.NewX && move.OldY == move.NewY)
        {
            return;
        }

        board[move.NewX, move.NewY] = board[move.OldX, move.OldY];
        board[move.OldX, move.OldY] = Unit.Empty;
    };
}
