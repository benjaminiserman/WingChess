namespace WingChessAPI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using WingChessAPI.Delegates;
using WingChessAPI.Helpers;

public class UnitType
{
    public string Name { get; set; }
    public string? ShortForm { get; set; }
    public int? Value { get; set; }
    public List<string> Tags { get; set; } = new();
    public char? Fen { get; set; }
    public List<SpecialMoveData> SpecialMoveData { get; set; } = new();
    public List<string> MacroNames { get; set; } = new();

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
            GenerateMovesWrapperDelegate wrapper = EmptyMoveGenerator.Wrapper;
            if (move.Method is not null)
            {
                var methodFound = AssemblyHelper.AssertSingletonAndGetValue(AssemblyHelper.GetMethods(move.Method), Name, move.Method, "move");
                wrapper = (GenerateMovesWrapperDelegate)Delegate.CreateDelegate(typeof(GenerateMovesWrapperDelegate), methodFound);
            }

            var moveType = new MoveType(wrapper, move.Shortform);
            if (move.Tags is List<string> tags && tags.Count != 0)
            {
                moveType.Tags = tags;
            }

            MoveTypes.Add(moveType);
        }
    }

    internal void CompileMacros()
    {
        foreach (var macroName in MacroNames)
        {
            GenerateMovesWrapperDelegate wrapper = EmptyMoveGenerator.Wrapper;
            var methodFound = AssemblyHelper.AssertSingletonAndGetValue(AssemblyHelper.GetMethods(macroName), Name, macroName, "macro");
            wrapper = (GenerateMovesWrapperDelegate)Delegate.CreateDelegate(typeof(GenerateMovesWrapperDelegate), methodFound);

            MoveTypes = MoveTypes
                .Select(moveType => new MoveType(moveType, wrapper))
                .ToList();
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