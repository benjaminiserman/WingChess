namespace WingChessAPI.Chess;

using System.Linq;

internal static class ChessRules
{
	public static bool CaptureWithTagPossible(Board board, Team team, string tag)
	{
		int CountRoyals(Board board)
		{
			return board.Count((kvp) => board.GetUnitType(kvp.unit).Tags.Contains(tag));
		}

		var royalCount = CountRoyals(board);

		var currentTeam = board.ToMove; // cache
		board.ToMove = team;

		var royalCapturePossible = board.GetAvailableMoves(isRecursive: true)
			.Any(move => CountRoyals(board.ApplyMove(move, isRecursive: true)) < royalCount);

		board.ToMove = currentTeam; // restore
		return royalCapturePossible;
	}

	public static string MoveAllowedRoyalCapture(Board board) =>
		CaptureWithTagPossible(board, board.ToMove, "royal")
			? Rule.Illegal
			: Rule.Ongoing;

	public static bool TeamIsChecked(Board board, Team team) => CaptureWithTagPossible(board, board.Game.NextToMove(team), "royal");
}
