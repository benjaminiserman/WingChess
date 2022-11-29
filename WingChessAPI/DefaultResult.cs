namespace WingChessAPI;

public static class DefaultResult
{
	public static ResultDelegate Instance => (board, move) =>
	{
		board[move.NewX, move.NewY] = board[move.OldX, move.OldY];
		board[move.OldX, move.OldY] = Unit.Empty;
	};
}
