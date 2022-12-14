namespace WingChessAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WingChessAPI.Delegates;

public static class Shortform
{
	private static readonly Dictionary<string, char> _shortformMap = new()
	{
		{ "glider", 'g' },
		{ "leaper", 'l' },

		{ "initial_only", 'i' },
		{ "capture_only", 'c' },
		{ "no_capture", 'm' },

		{ "upto", 'u' },
		{ "hopper", 'h' },
		{ "multimove", '+' },
		{ "can_capture_friendly", 'f' },
		{ "no_capture_enemy", 'e' },
	};

	private static HashSet<(int x, int y)> ConvertDelta(string delta)
	{
		IEnumerable<int> EnumerateSignOptions(int x, string? xSign)
		{
			switch (xSign)
			{
				case "+":
					yield return +x;
					break;
				case "-":
					yield return -x;
					break;
				case null or "":
					yield return +x;
					yield return -x;
					break;
				default:
					throw new($"invalid shortform format: {xSign}{x}");
			}
		}

		void AddOptions(int x, string xSign, int y, string ySign, HashSet<(int, int)> set)
		{
			foreach (var a in EnumerateSignOptions(x, xSign))
			{
				foreach (var b in EnumerateSignOptions(y, ySign))
				{
					set.Add((a, b));
				}
			}
		}

		var reverse = delta[0] == '*';
		var match = Regex.Match(delta, @"\*?(?<x_sign>[+-]?)(?<x>\d+)/(?<y_sign>[+-]?)(?<y>\d+)");

		if (match is null || match == Match.Empty)
		{
			throw new($"invalid shortform format: {delta}");
		}

		var xSign = match.Groups["x_sign"].Value;
		var ySign = match.Groups["y_sign"].Value;
		var x = int.Parse(match.Groups["x"].Value);
		var y = int.Parse(match.Groups["y"].Value);

		var deltaSet = new HashSet<(int, int)>();
		AddOptions(x, xSign, y, ySign, deltaSet);
		if (reverse)
		{
			AddOptions(y, ySign, x, xSign, deltaSet);
		}

		return deltaSet;
	}

	private static GenerateMovesDelegate GliderGenerator(HashSet<(int, int)> deltaSet, MoveType moveType)
	{
		IEnumerable<Move> GenerateMoves(Board board, int x, int y)
		{
			foreach (var (deltaX, deltaY) in deltaSet)
			{
				for (var i = 1; i <= board.Game.MaxMove; i++)
				{
					var dx = x + board.TransformDeltaX(deltaX, board[x, y].Team) * i;
					var dy = y + board.TransformDeltaY(deltaY, board[x, y].Team) * i;

					if (board.WithinBoard(dx, dy))
					{
						if (board[dx, dy] != Unit.Empty)
						{
							yield return new(board, x, y, dx, dy, moveType);
							break;
						}
						else
						{
							yield return new(board, x, y, dx, dy, moveType);
						}
					}
				}
			}
		}

		return GenerateMoves;
	}

	private static GenerateMovesDelegate LeaperGenerator(HashSet<(int, int)> deltaSet, MoveType moveType)
	{
		IEnumerable<Move> GenerateMoves(Board board, int x, int y)
		{
			foreach (var (deltaX, deltaY) in deltaSet)
			{
				var dx = x + board.TransformDeltaX(deltaX, board[x, y].Team);
				var dy = y + board.TransformDeltaY(deltaY, board[x, y].Team);

				if (board.WithinBoard(dx, dy))
				{
					yield return new(board, x, y, dx, dy, moveType);
				}
			}
		}

		return GenerateMoves;
	}

	private static GenerateMovesDelegate DefaultGenerator(HashSet<(int, int)> deltaSet, MoveType moveType)
	{
		IEnumerable<Move> GenerateMoves(Board board, int x, int y)
		{
			foreach (var (deltaX, deltaY) in deltaSet)
			{
				if (Math.Abs(deltaX) == Math.Abs(deltaY) || deltaX == 0 || deltaY == 0)
				{
					var sx = Math.Sign(board.TransformDeltaX(deltaX, board[x, y].Team));
					var sy = Math.Sign(board.TransformDeltaY(deltaY, board[x, y].Team));

					var endedEarly = false;
					for (var i = 1; i < Math.Abs(deltaX) - 1; i++)
					{
						var dx = x + sx * i;
						var dy = y + sy * i;
						if (!board.WithinBoard(dx, dy) || board[dx, dy] != Unit.Empty)
						{
							endedEarly = true;
							break;
						}
					}

					if (!endedEarly)
					{
						var dx = x + board.TransformDeltaX(deltaX, board[x, y].Team);
						var dy = y + board.TransformDeltaY(deltaY, board[x, y].Team);
						if (board.WithinBoard(dx, dy))
						{
							yield return new(board, x, y, dx, dy, moveType);
						}
					}
				}
				else
				{
					var dx = x + board.TransformDeltaX(deltaX, board[x, y].Team);
					var dy = y + board.TransformDeltaY(deltaY, board[x, y].Team);
					if ((board.WithinBoard(dx, y) && board[dx, y] == Unit.Empty)
						|| (board.WithinBoard(x, dy) && board[x, dy] == Unit.Empty))
					{
						if (board.WithinBoard(dx, dy))
						{
							yield return new(board, x, y, dx, dy, moveType);
						}
					}
				}
			}
		}

		return GenerateMoves;
	}

	private static GenerateMovesDelegate InitialOnlyWrapper(GenerateMovesDelegate moveFunction)
	{
		IEnumerable<Move> GenerateMoves(Board board, int x, int y)
		{
			if (board.History.Any(move => move.Unit == board[x, y]))
			{
				yield break;
			}
			else
			{
				foreach (var move in moveFunction(board, x, y))
				{

					yield return move;
				}
			}
		}

		return GenerateMoves;
	}

	private static GenerateMovesDelegate CaptureOnlyWrapper(GenerateMovesDelegate moveFunction)
	{
		IEnumerable<Move> GenerateMoves(Board board, int x, int y)
		{
			foreach (var move in moveFunction(board, x, y))
			{
				if (board[move.NewX, move.NewY] != Unit.Empty)
				{
					yield return move;
				}
			}
		}

		return GenerateMoves;
	}

	private static GenerateMovesDelegate NoCaptureWrapper(GenerateMovesDelegate moveFunction)
	{
		IEnumerable<Move> GenerateMoves(Board board, int x, int y)
		{
			foreach (var move in moveFunction(board, x, y))
			{
				if (board[move.NewX, move.NewY] == Unit.Empty)
				{
					yield return move;
				}
			}
		}

		return GenerateMoves;
	}

	private static GenerateMovesDelegate DefaultCaptureWrapper(GenerateMovesDelegate moveFunction)
	{
		IEnumerable<Move> GenerateMoves(Board board, int x, int y)
		{
			foreach (var move in moveFunction(board, x, y))
			{
				if (board[move.NewX, move.NewY] == Unit.Empty)
				{
					yield return move;
				}
				else if (board[move.NewX, move.NewY].Team != move.Unit.Team)
				{
					yield return move;
				}
			}
		}

		return GenerateMoves;
	}

	public static GenerateMovesDelegate CompileShortform(string shortform, MoveType moveType)
	{
		var match = Regex.Match(shortform, @"(?<flags>[a-z]*)(?<delta>\*?[+-]?\d+/[+-]?\d+)");
		if (match is null || match == Match.Empty)
		{
			throw new($"invalid shortform format: {shortform}");
		}

		var flags = match.Groups["flags"].Value;
		var delta = match.Groups["delta"].Value;
		var deltaSet = ConvertDelta(delta);

		GenerateMovesDelegate moveFunction = EmptyMoveGenerator.Instance;

		if (flags.Contains(_shortformMap["glider"]))
		{
			moveFunction = GliderGenerator(deltaSet, moveType);
		}
		else if (flags.Contains(_shortformMap["leaper"]))
		{
			moveFunction = LeaperGenerator(deltaSet, moveType);
		}
		else
		{
			moveFunction = DefaultGenerator(deltaSet, moveType);
		}

		if (flags.Contains(_shortformMap["initial_only"]))
		{
			moveFunction = InitialOnlyWrapper(moveFunction);
		}

		if (flags.Contains(_shortformMap["capture_only"]))
		{
			moveFunction = CaptureOnlyWrapper(moveFunction);
		}

		if (flags.Contains(_shortformMap["no_capture"]))
		{
			moveFunction = NoCaptureWrapper(moveFunction);
		}

		if (!flags.Contains(_shortformMap["can_capture_friendly"])
			&& !flags.Contains(_shortformMap["no_capture"]))
		{
			moveFunction = DefaultCaptureWrapper(moveFunction);
		}

		return moveFunction;
	}
}
