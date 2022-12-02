namespace WingChessAPI;
using Newtonsoft.Json.Linq;

public class Game
{
    public Dictionary<string, UnitType> UnitSet { get; set; } = new();
    public List<Rule> Rules { get; set; } = new();
    public string? DefaultBoardFen { get; set; } = null;
    public Dictionary<char, UnitType> FenMap { get; set; } = new();
    public int MaxMove { get; set; } = 16;

    private Func<Team, Team>? _nextToMoveField = null;
    public Func<Team, Team> NextToMove
    {
        get => _nextToMoveField ?? DefaultNextToMove;
        set => _nextToMoveField = value;
    }
    private static Team DefaultNextToMove(Team team) =>
        team == Team.White
        ? Team.Black
        : Team.White;

    private static Game? _cachedChess = null;
    public static Game Chess
    {
        get
        {
            if (_cachedChess is null)
            {
                ReloadChess();
			}
			
            return _cachedChess!;
        }
    }

    public static void ReloadChess()
    {
		_cachedChess = new();
		_cachedChess.LoadGame("chess.json");
	}

    private void AssignFenToUnits()
    {
        var fenSet = new HashSet<char>();
        foreach (var unit in UnitSet.Values)
        {
            bool TryAssignFen(char character)
            {
                var lowercase = char.ToLower(character);
                if (!fenSet.Contains(lowercase))
                {
                    fenSet.Add(lowercase);
                    unit.Fen = lowercase;
                    return true;
                }

                return false;
            }

            var fenAssigned = false;
            var assignedChar = '\0';
            foreach (var character in unit.Name)
            {
                fenAssigned = TryAssignFen(character);
                if (fenAssigned)
                {
                    assignedChar = character;
                    break;
                }
            }

            if (!fenAssigned)
            {
                foreach (var character in "abcdefghijklmnopqrstuvwxyz")
                {
                    fenAssigned = TryAssignFen(character);
                    if (fenAssigned)
                    {
                        assignedChar = character;
                        break;
                    }
                }
            }

            if (!fenAssigned)
            {
                throw new($"Unable to assign FEN to unit {unit.Name}.");
            }

            FenMap.Add(char.ToLower(assignedChar), unit);
            FenMap.Add(char.ToUpper(assignedChar), unit);
        }
    }

    void LoadGame(string loadFilePath)
    {
        var json = JObject.Parse(File.ReadAllText(loadFilePath));
        DefaultBoardFen = (string?)json["default_board"];

        if (json["unit_set"] is JObject unit_set_data)
        {
            foreach (var kvp in unit_set_data)
            {
                var name = kvp.Key!;
                var token = (JObject)kvp.Value!;

                var newUnit = new UnitType(name)
                {
                    ShortForm = (string?)token["shortform"],
                };

                var tags = (JArray?)token["tags"];
                if (tags is not null)
                {
                    foreach (var tag in tags)
                    {
                        newUnit.Tags.Add((string)tag!);
                    }
                }

                newUnit.Value = (int?)token["value"];

				newUnit.CompileMovesFromShortform();

				var specialMoves = (JArray?)token["special_moves"];
                if (specialMoves is not null)
                {
                    var specialMovesList = specialMoves
                        .Select(x => new SpecialMoveData(
                            (string?)x["method"],
                            (string?)x["shortform"],
                            ((JArray?)x["tags"])?.Select(s => (string)s!).ToList() ?? new()))
                        .ToList();

                    newUnit.SpecialMoveData = specialMovesList;
                    newUnit.CompileSpecialMoves();
                }
                
                UnitSet.Add(newUnit.Name, newUnit);
            }

            AssignFenToUnits();
        }

        if (json["rules"] is JObject ruleData)
        {
            foreach (var kvp in ruleData)
            {
                Rules.Add(new(kvp.Key,
                    (string)kvp.Value!["condition"]!,
                    (string)kvp.Value!["stage"]!,
                    (string)kvp.Value!["result"]!
                ));

                //Rules[^1].Compile();
            }
        }
    }
}