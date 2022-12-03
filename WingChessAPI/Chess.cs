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

	public static GenerateMovesDelegate Castle(GenerateMovesDelegate _, MoveType moveType)
	{
		ResultDelegate GetResult(Unit castle, int castleX, int castleY, Board board)
		{
			void Result(Board board, Move move)
			{
				board[castle] = Unit.Empty;
				board[move.OldX, move.OldY] = Unit.Empty;

				board[castleX, castleY] = castle;
				board[move.NewX, move.NewY] = move.Unit;
			}

			return Result;
		}

		IEnumerable<Move> GenerateMoves(Board board, int x, int y)
		{
			var castler = board[x, y];
			if (!board.Tags.Contains($"{castler.Team}_castled")
				&& !board.History.Any(m => m.Unit == castler))
			{ // $$$ add checks for check or squares attacked
				var castlingUnits = board.BoardHistory[0]
					.Where(kvp => board.GetUnitType(kvp.unit).Tags.Contains("castle")
						&& kvp.unit.Team == castler.Team
						&& !board.History.Any(move => move.Unit == kvp.unit))
					.Select(kvp => kvp);

				if (board.Tags.Contains("absolute_castle"))
				{
					var homeRank = (int)board.Variables[$"{castler.Team.Name}_home_rank"];
					// queen castle
					var queenCastle = castlingUnits.FirstOrDefault(kvp => kvp.pos.x < x);
					if (queenCastle != default
						&& board[1, homeRank] == Unit.Empty
						&& board[2, homeRank] == Unit.Empty
						&& board[3, homeRank] == Unit.Empty)
						// $$$ add checks for attacked squares
					{
						yield return new Move
						(
							board[x, y],
							x, y,
							2, homeRank,
							moveType,
							board,
							GetResult(queenCastle.unit, 3, homeRank, board)
						);
					}

					// king castle
					var kingCastle = castlingUnits.FirstOrDefault(kvp => kvp.pos.x > x);
					if (kingCastle != default
						&& board[6, homeRank] == Unit.Empty
						&& board[5, homeRank] == Unit.Empty)
					// $$$ add checks for attacked squares
					{
						yield return new(
							board[x, y],
							x, y,
							6, homeRank,
							moveType,
							board,
							GetResult(kingCastle.unit, 5, homeRank, board)
						);
					}
				}
				else
				{
					foreach (var (castlePos, castleUnit) in castlingUnits)
					{
						// $$$ check for attacked squares
						var breakFlag = false;
						if (castlePos.x == x)
						{
							foreach (var i in castlePos.y..y)
							{
								if (i != castlePos.y && board[x, i] != Unit.Empty)
								{
									breakFlag = true;
									break;
								}
							}

							if (!breakFlag)
							{
								yield return new(
									board[x, y],
									x, y,
									x, y + 2 * Math.Sign(castlePos.y - y),
									moveType,
									board,
									GetResult(castleUnit, x, y + Math.Sign(castlePos.y - y), board)
								);
							}
						}
						else if (castlePos.y == y)
						{
							foreach (var i in castlePos.x..x)
							{
								if (i != castlePos.x && board[i, y] != Unit.Empty)
								{
									breakFlag = true;
									break;
								}
							}

							if (!breakFlag)
							{
								yield return new(
									board[x, y],
									x, y,
									x + 2 * Math.Sign(castlePos.x - x), y,
									moveType,
									board,
									GetResult(castleUnit, x + Math.Sign(castlePos.x - x), y, board)
								);
							}
						}
					}
				}
			}
		}

		return GenerateMoves;
	}
}
