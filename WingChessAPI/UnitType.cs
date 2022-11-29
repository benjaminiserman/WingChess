namespace WingChessAPI;
using System.Text.RegularExpressions;

public class UnitType
{
    public string Name { get; set; }
    public string? ShortForm { get; set; }
    public int? Value { get; set; }
    public List<string> Tags { get; set; } = new();
    public char? Fen { get; set; }

    public List<MoveType> MoveTypes { get; set; } = new();
    public GenerateMovesDelegate GenerateMoves { get; set; } = EmptyMoveGenerator.Instance;
    public Func<Board, int, int, int, int, string> GetNotation { get; set; } = (board, oldX, oldY, newX, newY) => $"{board.GetNotation(oldX, oldY)}{board.GetNotation(newX, newY)}";

    public UnitType(string name)
    {
        Name = name;
    }

    internal void CompileMovesFromShortform()
    {
        GenerateMovesDelegate CollateMoves(List<MoveType> moveTypes)
        {
            IEnumerable<Move> GenerateMoves(Board board, int x, int y)
            {
                foreach (var moveType in moveTypes)
                {
                    foreach (var move in moveType.GenerateMoves(board, x, y))
                    {
                        yield return move;
                    }
                }
            }

            return GenerateMoves;
        }

        if (ShortForm is string shortformString && !string.IsNullOrWhiteSpace(shortformString))
        {
			MoveTypes = Regex.Split(shortformString, @",\s*")
                .Select(moveString => new MoveType(shortForm: moveString))
                .ToList();
            GenerateMoves = CollateMoves(MoveTypes);
        }
    }

    public Unit CreateUnit(Team team) => new(Name, team);
}

/*self.name: str = name
        self.generate_moves: typing.Callable = empty_generator
        self.move_types: list[MoveType] = []
        self.tags: list[str] = []
        self.shortform: str|None = None
        self.value: float|None = None
        self.fen: str|None = None
        */