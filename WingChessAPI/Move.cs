namespace WingChessAPI;

public record Move(Unit Unit, string Notation, List<string> Tags, int OldX, int OldY, int NewX, int NewY, ResultDelegate Result)
{
	public Move(Board board, int oldX, int oldY, int newX, int newY, List<string>? tags = null, ResultDelegate? result = null) : this(
		board[oldX, oldY],
		board.GetUnitType(board[oldX, oldY]).GetNotation(board, oldX, oldY, newX, newY),
		tags ?? new(),
		oldX, oldY,
		newX, newY,
		result ?? DefaultResult.Instance
	)
	{ }
}
