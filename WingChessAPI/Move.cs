namespace WingChessAPI;

public record Move(Unit Unit, int OldX, int OldY, int NewX, int NewY, MoveType MoveType, Board Board, ResultDelegate? Result)
{
	public string Notation => Board.GetUnitType(Unit).GetNotation(Board, OldX, OldY, NewX, NewY);
	public bool Capture => Board[NewX, NewY] != Unit.Empty;
	public Move(Board board, int oldX, int oldY, int newX, int newY, MoveType moveType) : this(
		board[oldX, oldY],
		oldX, oldY,
		newX, newY,
		moveType,
		board,
		null
	)
	{ }
}
