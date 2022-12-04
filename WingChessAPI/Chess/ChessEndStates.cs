namespace WingChessAPI.Chess;

internal static class ChessEndStates
{
	public static string NoPossibleMoves(Board board)
	{
		if (!board.GetAvailableMoves().Any())
		{
			var nextToMove = board.Game.NextToMove(board.ToMove);
			return ChessRules.CaptureWithTagPossible(board, nextToMove, "royal")
				? $"{nextToMove.Name}_Checkmate"
				: "Stalemate";
		}
		else
		{
			return Rule.Ongoing;
		}
	}
}
