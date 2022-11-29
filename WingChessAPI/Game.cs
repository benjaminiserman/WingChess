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

	public static Game Chess
    {
        get
        {
            var chess = new Game();
            chess.LoadGame("chess.json");
            return chess;
        }
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

				var tags = (JArray)json["tags"]!;
				if (tags is not null)
                {
                    newUnit.Tags.AddRange(tags.Cast<string>());
                }

				newUnit.Value = (int?)json["value"];

                newUnit.CompileMovesFromShortform();
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
                    (string)kvp.Value!["result"]!)
                );

                //Rules[^1].Compile();
            }
        }
	}
}