namespace WingChessAPI.Chess;

internal static class ChessEndStates
{
	public static string NoPossibleMoves(Board board)
	{
		if (!board.GetAvailableMoves().Any())
		{
			var nextToMove = board.Game.NextToMove(board.ToMove);
			return ChessRules.CaptureWithTagPossible(board, nextToMove, "royal")
				? $"{Rule.Win}_{nextToMove.Name}_Checkmate"
				: $"{Rule.Draw}_Stalemate";
		}
		else
		{
			return Rule.Ongoing;
		}
	}

	public static int CountRepetitions(Board board) => board.BoardHistory.Count(oldBoard => oldBoard.StructuralEquals(board));

	public static string ThreefoldRepetition(Board board) =>
		CountRepetitions(board) < 3
			? Rule.Ongoing
			: $"{Rule.Draw}_ThreefoldRepetition";

	public static string FivefoldRepetition(Board board) =>
		CountRepetitions(board) < 5
			? Rule.Ongoing
			: $"{Rule.Draw}_FivefoldRepetition";

	public static string FiftyMovesWithoutCapture(Board board) => MovesWithoutCapture(board, 50, "Fifty");
	public static string SeventyFiveMovesWithoutCapture(Board board) => MovesWithoutCapture(board, 75, "SeventyFive");

	public static string MovesWithoutCapture(Board board, int moves, string movesWord)
	{
		if (board.History.Count < moves)
		{
			return Rule.Ongoing;
		}

		return board.History.TakeLast(moves).Any(move => move.Capture)
			? Rule.Ongoing
			: $"{Rule.Draw}_{movesWord}MovesWithoutCapture";
	}

	private static readonly List<string> _kingOnlyList = new() { "king" };
	private static readonly List<string> _knightAndKingList = new() { "king", "knight" };
	private readonly static List<string> _bishopAndKingList = new() { "king", "bishop" };
	public static string DeadPosition(Board board)
	{
		static bool CombinationIncludes(List<string> whiteUnits, List<string> blackUnits, List<string> oneSideHas, List<string> otherSideHas)
		{
			whiteUnits.Sort();
			blackUnits.Sort();
			oneSideHas.Sort();
			otherSideHas.Sort();

			return (whiteUnits.SequenceEqual(oneSideHas) && blackUnits.SequenceEqual(otherSideHas))
				|| (blackUnits.SequenceEqual(oneSideHas) && whiteUnits.SequenceEqual(otherSideHas));
		}

		if (board.Count() > 4)
		{
			return Rule.Ongoing;
		}

		var blackUnits = board
			.Where(x => x.unit.Team == Team.Black)
			.Select(x => x.unit.Name)
			.ToList();

		var whiteUnits = board
			.Where(x => x.unit.Team == Team.White)
			.Select(x => x.unit.Name)
			.ToList();

		if (CombinationIncludes(whiteUnits, blackUnits, _kingOnlyList, _kingOnlyList))
		{
			return $"{Rule.Draw}_DeadPosition";
		}

		if (CombinationIncludes(whiteUnits, blackUnits, _kingOnlyList, _knightAndKingList))
		{
			return $"{Rule.Draw}_DeadPosition";
		}

		if (CombinationIncludes(whiteUnits, blackUnits, _kingOnlyList, _bishopAndKingList))
		{
			return $"{Rule.Draw}_DeadPosition";
		}

		if (CombinationIncludes(whiteUnits, blackUnits, _bishopAndKingList, _bishopAndKingList))
		{
			var bishops = board
				.Where(kvp => kvp.unit.Name == "bishop")
				.Select(kvp => kvp.pos)
				.ToList();

			return (bishops[0].x + bishops[0].y) % 2 == (bishops[1].x + bishops[1].y) % 2
				? $"{Rule.Draw}_DeadPosition"
				: Rule.Ongoing;
		}

		return Rule.Ongoing;
	}
}
