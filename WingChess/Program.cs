using System.Text.RegularExpressions;
using WingChessAPI;

var game = Game.Chess;
//var game = new Game();
//game.LoadGame("test.json");

void PrintBoard(Board board)
{
	int? minX = null,
		minY = null,
		maxX = null,
		maxY = null;

	if (board.XSize is int xSize
		&& board.YSize is int ySize)
	{
		minX = 0;
		minY = 0;
		maxX = xSize;
		maxY = ySize;
	}
	else
	{
		foreach (var ((x, y), _) in board)
		{
			if (minX is null || x < minX)
			{
				minX = x;
			}

			if (minY is null || y < minY)
			{
				minY = y;
			}

			if (maxX is null || x > maxX)
			{
				maxX = x;
			}

			if (maxY is null || y > maxY)
			{
				maxY = y;
			}
		}
	}

	if (minX is null || minY is null || maxX is null || maxY is null)
	{
		// board is empty
		return;
	}

	var boardX = (int)(maxX - minX);
	var boardY = (int)(maxY - minY);

	var displayBoard = new char[boardX, boardY];
	for (var i = 0; i < boardX; i++)
	{
		for (var j = 0; j < boardY; j++)
		{
			displayBoard[i, j] = ' ';
		}
	}

	foreach (var ((x, y), unit) in board)
	{
		var fen = board.GetUnitType(unit).Fen;
		var ny = boardY - y - 1;
		if (fen is not char fenChar)
		{
			displayBoard[ny, x] = '?';
		}
		else
		{
			displayBoard[ny, x] = unit.Team.Name switch
			{
				"White" => char.ToUpper(fenChar),
				"Black" => char.ToLower(fenChar),
				_ => '?'
			};
		}
	}

	for (var i = 0; i < boardY; i++)
	{
		Console.Write($"{displayBoard.GetLength(0) - i} ");
		for (var j = 0; j < boardX; j++)
		{
			Console.Write($"{displayBoard[i, j]}");
		}

		Console.WriteLine();
	}

	Console.Write("  ");
	for (var i = 0; i < boardX; i++)
	{
		Console.Write($"{(char)('a' + i)}");
	}

	Console.WriteLine();
}

Board DoInput(Board board, List<Move> availableMoves)
{
	while (true)
	{
		var inputMove = Console.ReadLine()!.Trim();

		if (availableMoves.Any(x => x.Notation == inputMove))
		{
			return board.ApplyMove(availableMoves.First(x => x.Notation == inputMove));
		}

		var match = Regex.Match(inputMove, @"([a-z]\d?)??([A-Z])?x?([a-z]\d)");
		if (match is null || match == Match.Empty)
		{
			if (int.TryParse(inputMove, out var x))
			{
				return board.ApplyMove(availableMoves[x]);
			}
			else
			{
				Console.WriteLine("invalid pattern");
			}
		}
		else
		{
			var origin = match.Groups[1].Value;
			var piece = match.Groups[2].Value;
			var end = match.Groups[3].Value;

			var filter = availableMoves
				.Where(move => move.Notation[^2..] == end)
				.ToList();

			if (piece is not null && !string.IsNullOrWhiteSpace(piece))
			{
				filter = filter
					.Where(move => board.Game.UnitSet[move.Unit.Name].Fen == char.ToLower(piece[0]))
					.ToList();
			}

			if (origin is not null && !string.IsNullOrWhiteSpace(origin))
			{
				if (origin.Length == 1)
				{
					filter = filter
						.Where(move => move.Notation[0] == origin[0])
						.ToList();
				}
				else
				{
					filter = filter
						.Where(move => move.Notation[0..2] == origin)
						.ToList();
				}
			}

			if (filter.Count == 0)
			{
				Console.WriteLine("no move matched");
			}
			else if (filter.Count == 1)
			{
				return board.ApplyMove(filter[0]);
			}
			else
			{
				Console.WriteLine("Move ambiguous between:");
				foreach (var move in filter)
				{
					Console.WriteLine($" - {move.Notation}");
				}
			}
		}
	}
}

Board DoRandom(Board board, List<Move> availableMoves)
{
	return board.ApplyMove(availableMoves[Random.Shared.Next(availableMoves.Count)]);
}

void PrintMoves(List<Move> availableMoves)
{
	Console.WriteLine("Available moves:");
	foreach (var (move, i) in availableMoves.Select((x, i) => (x, i)))
	{
		Console.WriteLine($"{i}. {move.Notation}");
	}
}

var board = FenConverter.ConvertFen(game, game.DefaultBoardFen!, game.FenMap);

var team = Team.White;
while (true)
{
	var availableMoves = board.GetAvailableMoves(team).ToList();
	PrintMoves(availableMoves);
	PrintBoard(board);
	board = DoInput(board, availableMoves);
	team = board.ToMove;

	if (board.EndResult != Rule.Ongoing)
	{
		Console.WriteLine($"Game ended with result: {board.EndResult}");
		return;
	}

	availableMoves = board.GetAvailableMoves(team).ToList();
	PrintMoves(availableMoves);
	PrintBoard(board);
	board = DoInput(board, availableMoves);
	//board = DoRandom(board, availableMoves);
	team = board.ToMove;

	if (board.EndResult != Rule.Ongoing)
	{
		Console.WriteLine($"Game ended with result: {board.EndResult}");
		return;
	}
}
