namespace WingChessAPI;

public class Chess
{
	public static GenerateMovesDelegate EnPassant(GenerateMovesDelegate generateMoves, MoveType _)
	{
		void Result(Board board, Move move)
		{
			DefaultResult.Instance(board, move);
			board[move.NewX, move.NewY - board.TransformDeltaY(1, move.Unit.Team)] = Unit.Empty;
		}

		IEnumerable<Move> GenerateMoves(Board board, int x, int y)
		{
			foreach (var move in generateMoves(board, x, y))
			{
				var enPassantee = board[move.NewX, move.NewY - board.TransformDeltaY(1, move.Unit.Team)];

				if (enPassantee != Unit.Empty
					&& board.History.Count > 0
					&& board.History[^1].Unit == enPassantee 
					&& board.History[^1].MoveType.Tags.Contains("en_passant"))
				{
					yield return move with
					{
						Result = Result
					};
				}
			}
		}

		return GenerateMoves;
	}

	public static GenerateMovesDelegate Castle(GenerateMovesDelegate _, MoveType _2)
	{
		/*IEnumerable<Move> GenerateMoves(Board board, int x, int y)
		{
			var unit = board[x, y];
			if (!board.Tags.Contains($"{unit.Team}_castled")
				&& !board.History.Any(m => m.Unit == unit))
			{ // $$$ add checks for check or squares attacked
				var castlingUnits = board.BoardHistory[0]
					.Where(kvp => board.GetUnitType(kvp.Item2).Tags.Contains("castle")
						&& kvp.Item2.Team == unit.Team)
					.Select(kvp => kvp.Item2);
				// king castle


				// queen castle
			}
		}*/

		return EmptyMoveGenerator.Instance;
	}
}
