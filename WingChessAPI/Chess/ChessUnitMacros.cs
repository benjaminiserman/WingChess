namespace WingChessAPI.Chess;

internal static class ChessUnitMacros
{
	public static GenerateMovesDelegate PromotionWrapper(GenerateMovesDelegate moveFunction, MoveType _)
	{
		ResultDelegate GetResult(UnitType promotionUnit, ResultDelegate currentResult)
		{
			void Result(Board board, Move move)
			{
				currentResult(board, move);
				board[move.NewX, move.NewY] = move.Unit with
				{
					Name = promotionUnit.Name
				};
			}

			return Result;
		}

		IEnumerable<Move> GenerateMoves(Board board, int x, int y)
		{
			foreach (var move in moveFunction(board, x, y))
			{
				var currentUnit = board.GetUnitType(board[x, y]);
				var yieldCount = 0;
				if (move.NewY == (int)board.Variables[$"{board.ToMove.Name}_promotion_rank"])
				{
					foreach (var promotionUnit in board.Game.UnitSet.Values
						.Where(unit => currentUnit.Tags.Contains($"can_promote_{unit.Name}")))
					{
						yieldCount++;
						var currentNotation = move.GetNotation;
						yield return move with
						{
							Result = GetResult(promotionUnit, move.Result),
							GetNotation = move => $"{currentNotation(move)}={promotionUnit.Fen}"
						};
					}
				}

				if (yieldCount == 0)
				{
					yield return move;	
				}
			}
		}

		return GenerateMoves;
	}
}
