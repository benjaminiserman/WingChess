namespace WingChessAPI;

using System.Text.RegularExpressions;
using System.Reflection;

public class UnitType
{
    public string Name { get; set; }
    public string? ShortForm { get; set; }
    public int? Value { get; set; }
    public List<string> Tags { get; set; } = new();
    public char? Fen { get; set; }
    public List<SpecialMoveData> SpecialMoveData { get; set; } = new();

    public List<MoveType> MoveTypes { get; set; } = new();
    public GenerateMovesDelegate GenerateMoves
    {
        get
        {
            IEnumerable<Move> GenerateMoves(Board board, int x, int y)
            {
                foreach (var moveType in MoveTypes)
                {
                    foreach (var move in moveType.GenerateMoves(board, x, y))
                    {
                        yield return move;
                    }
                }
            }

            return GenerateMoves;
        }
    }

    public Func<Board, int, int, int, int, string> GetNotation { get; set; } = (board, oldX, oldY, newX, newY) => $"{board.GetNotation(oldX, oldY)}{board.GetNotation(newX, newY)}";

    public UnitType(string name)
    {
        Name = name;
    }

    internal void CompileMovesFromShortform()
    {
        if (ShortForm is string shortformString && !string.IsNullOrWhiteSpace(shortformString))
        {
            MoveTypes = Regex.Split(shortformString, @",\s*")
                .Select(moveString => new MoveType(shortForm: moveString))
                .ToList();
        }
    }

    internal void CompileSpecialMoves()
    {
        foreach (var move in SpecialMoveData)
        {
            Console.WriteLine(move);
            GenerateMovesWrapperDelegate wrapper = EmptyMoveGenerator.Wrapper;
            if (move.Method is not null)
            {
                var split = move.Method.Split("::");
                var packageName = split[0];
                var methodName = split[1];

                var methods = Assembly
                    .GetExecutingAssembly()
                    .GetTypes()
                    .Where(t => t.Namespace == packageName)
                    .Where(t => t
                        .GetMethods()
                        .Any(m => m.Name == methodName))
                    .Select(t => t
                        .GetMethods()
                        .First(m => m.Name == methodName))
                    .ToList();

                if (methods.Count == 0)
                {
                    throw new($"Compilation failed on unit {Name}. No method could be found that matches {packageName}::{methodName}.");
                }
                else if (methods.Count > 1)
                {
                    throw new($"Compilation failed on unit {Name}. Pattern {packageName}::{methodName} is ambiguous between {{ {string.Join(", ", methods.Select(m => m?.DeclaringType?.Name + "::" + m?.Name))} }}.");
                }
                else
                {
                    var methodFound = methods.First();
                    wrapper = (GenerateMovesWrapperDelegate)Delegate.CreateDelegate(typeof(GenerateMovesWrapperDelegate), methodFound);
                }
            }

			var moveType = new MoveType(wrapper, move.Shortform);
			if (move.Tags is List<string> tags && tags.Count != 0)
			{
				moveType.Tags = tags;
			}

            MoveTypes.Add(moveType);
		}
    }

	public Unit CreateUnit(Team team, string id) => new(Name, team, id);
}

/*self.name: str = name
        self.generate_moves: typing.Callable = empty_generator
        self.move_types: list[MoveType] = []
        self.tags: list[str] = []
        self.shortform: str|None = None
        self.value: float|None = None
        self.fen: str|None = None
        */