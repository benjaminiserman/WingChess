namespace WingChessAPI;

using System.Collections;

public class Board : IEnumerable<((int, int), Unit)>
{
	private Dictionary<(int, int), Unit> Units { get; set; } = new();
	public Team ToMove { get; set; } = Team.White;
	public Dictionary<string, dynamic> Variables { get; set; } = new();
	public List<string> Tags { get; set; } = new();
	public List<Move> History { get; set; } = new();

	public Game Game { get; private init; }

	public Board(Game game)
	{
		Game = game;
	}

	public int? XSize { get; private set; }
	public int? YSize { get; private set; }

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

	public IEnumerable<Move> GetAvailableMoves(Team team)
	{
		foreach (var kvp in Units)
		{
			var (x, y) = kvp.Key;
			var unit = kvp.Value;
			if (unit.Team == team)
			{
				foreach (var move in GetUnitType(unit).GenerateMoves(this, x, y))
				{
					yield return move;
				}
			}
		}
	}

	public void ApplyMove(Move move)
	{
		move.Result(this, move);
		History.Add(move);
	}

	public UnitType GetUnitType(Unit unit) => Game.UnitSet[unit.Name];
	public IEnumerator<((int, int), Unit)> GetEnumerator()
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
}
