namespace WingChessAPI;

using System.Collections;

public class Board : IEnumerable<((int x, int y) pos, Unit unit)>
{
	private Dictionary<(int, int), Unit> Units { get; set; } = new();
	public Team ToMove { get; set; } = Team.White;
	public Dictionary<string, dynamic> Variables { get; set; } = new();
	public List<string> Tags { get; set; } = new();
	public List<Move> History { get; set; } = new();
	public List<Board> BoardHistory { get; set; } = new();
	public int? XSize { get; private set; }
	public int? YSize { get; private set; }
	public bool AllowOutOfTurnPlay { get; set; } = false;
	public string EndResult { get; private set; } = Rule.Ongoing;

	private Func<int, int, string>? _getNotationField = null;
	public Func<int, int, string> GetNotation
	{
		get => _getNotationField ?? DefaultGetNotation;
		set => _getNotationField = value;
	}

	private Func<int, Team, int>? _transformDeltaXField = null;
	public Func<int, Team, int> TransformDeltaX
	{
		get => _transformDeltaXField ?? DefaultTransformDeltaX;
		set => _transformDeltaXField = value;
	}

	private Func<int, Team, int>? _transformDeltaYField = null;
	public Func<int, Team, int> TransformDeltaY
	{
		get => _transformDeltaYField ?? DefaultTransformDeltaY;
		set => _transformDeltaYField = value;
	}

	private Func<int, int, bool>? _withinBoardField = null;
	public Func<int, int, bool> WithinBoard
	{
		get => _withinBoardField ?? DefaultWithinBoard;
		set => _withinBoardField = value;
	}

	public Game Game { get; private init; }

	public Board(Game game)
	{
		Game = game;
		Tags = new(game.DefaultTags);
		Variables = new(game.DefaultVariables);
		BoardHistory.Add(this);
	}

	public Board(Board board, List<Move> history, List<Board> boardHistory) // $$$ del parameters?
	{
		Game = board.Game;
		Units = new(board.Units);
		ToMove = board.ToMove;
		Variables = new(board.Variables);
		Tags = new(board.Tags);
		History = new(history);
		BoardHistory = new(boardHistory);
		XSize = board.XSize;
		YSize = board.YSize;
		_getNotationField = board._getNotationField;
		_transformDeltaXField = board._transformDeltaXField;
		_transformDeltaYField = board._transformDeltaYField;
		_withinBoardField = board._withinBoardField;
		AllowOutOfTurnPlay = board.AllowOutOfTurnPlay;
		EndResult = board.EndResult;
	}

	public Board(Board board) : this(board, new(board.History), new(board.BoardHistory)) { }

	private static string DefaultGetNotation(int x, int y) => $"{(char)('a' + x)}{y + 1}";
	private static int DefaultTransformDeltaX(int x, Team team) => x;
	private static int DefaultTransformDeltaY(int y, Team team) =>
		team == Team.White
		? y
		: -y;

	private bool DefaultWithinBoard(int x, int y)
	{
		if (XSize is int && YSize is int)
		{
			return 0 <= x
				&& x < XSize
				&& 0 <= y
				&& y < YSize;
		}
		else
		{
			return true;
		}
	}

	public void SetBounds(int x, int y)
	{
		XSize = x;
		YSize = y;
	}

	public IEnumerable<Move> GetAvailableMoves(Team? team = null, bool isRecursive = false, bool trace = false)
	{
		team ??= ToMove;

		foreach (var kvp in Units)
		{
			var (x, y) = kvp.Key;
			var unit = kvp.Value;
			if (unit.Team == team)
			{
				foreach (var move in GetUnitType(unit).GenerateMoves(this, x, y))
				{
					var testBoard = ApplyMove(move, true);
					if (!Game.Rules.Any(rule => (!isRecursive || rule.AllowsRecursion) && rule.Method(testBoard) == Rule.Illegal))
					{
						yield return move;
					}
				}
			}
		}
	}

	public Board ApplyMove(Move move, bool isRecursive = false)
	{
		if (EndResult != Rule.Ongoing)
		{
			throw new($"Cannot apply move after game ended (with state {EndResult})");
		}

		if (move.Unit.Team != ToMove && !AllowOutOfTurnPlay)
		{
			throw new($"{move.Unit.Team} attempted to play on {ToMove}'s turn. If this was intended, set AllowOutOfTurnPlay to true.");
		}

		Board newBoard = new(this, History, BoardHistory)
		{
			ToMove = Game.NextToMove(ToMove)
		};

		var result = move.Result;
		result ??= move.MoveType.DefaultResult;
		result(newBoard, move);

		newBoard.History.Add(move);
		newBoard.BoardHistory.Add(newBoard);

		if (!isRecursive)
		{
			foreach (var rule in Game.EndStateRules)
			{
				newBoard.EndResult = rule.Method(newBoard);
				//if (newBoard.EndResult.StartsWith(Rule.Draw))
				//{
				//	Debug.PrintBoard(newBoard);
				//	throw new();
				//}

				if (newBoard.EndResult != Rule.Ongoing)
				{
					break;
				}
			}
		}

		return newBoard;
	}

	public UnitType GetUnitType(Unit unit) => Game.UnitSet[unit.Name];

	public bool StructuralEquals(Board b)
	{
		if (Units.Count != b.Units.Count)
		{
			return false;
		}

		foreach (var (x, y) in b.Units.Keys)
		{
			if (!Units.ContainsKey((x, y)) 
				|| !this[x, y].StructuralEquals(b[x, y]))
			{
				return false;
			}
		}

		return true;
	}

	public IEnumerator<((int x, int y) pos, Unit unit)> GetEnumerator()
	{
		foreach (var kvp in Units)
		{
			yield return (kvp.Key, kvp.Value);
		}
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public Unit this[int x, int y]
	{
		get
		{
			if (Units.ContainsKey((x, y)))
			{
				return Units[(x, y)];
			}
			else
			{
				return Unit.Empty;
			}
		}
		set
		{
			if (value == Unit.Empty)
			{
				Units.Remove((x, y));
			}
			else
			{
				if (Units.ContainsKey((x, y)))
				{
					Units[(x, y)] = value;
				}
				else
				{
					Units.Add((x, y), value);
				}
			}
		}
	}

	public Unit this[string id]
	{
		get => Units.FirstOrDefault(x => x.Value.Id == id).Value;
		set
		{
			var (x, y) = Units.FirstOrDefault(x => x.Value.Id == id).Key;
			this[x, y] = value;	
		}
	}

	public Unit this[Unit unit]
	{
		get => this[unit.Id];
		set => this[unit.Id] = value;
	}
}
