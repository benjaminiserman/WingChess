namespace WingChessAPI;

using WingChessAPI.Delegates;

public record Move(Unit Unit, int OldX, int OldY, int NewX, int NewY, MoveType MoveType, Board Board, ResultDelegate Result, IsCaptureDelegate IsCapture, GetMoveNotationDelegate GetNotation)
{
	public string Notation => GetNotation(this);
	public Move(Board board, int oldX, int oldY, int newX, int newY, MoveType moveType) : this(
		board[oldX, oldY],
		oldX, oldY,
		newX, newY,
		moveType,
		board,
		DefaultResult.Instance,
		DefaultIsCapture.Instance,
		DefaultGetMoveNotation.Instance
	)
	{ }
}
