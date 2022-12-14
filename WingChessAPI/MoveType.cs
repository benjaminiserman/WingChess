namespace WingChessAPI;

using System.Collections.Generic;
using WingChessAPI.Delegates;

public class MoveType
{
	public GenerateMovesDelegate GenerateMoves { get; private set; } = EmptyMoveGenerator.Instance;
	public string? ShortForm = null;
	public ResultDelegate DefaultResult { get; private set; } = Delegates.DefaultResult.Instance;
	public List<string> Tags { get; set; } = new();

	public MoveType(GenerateMovesWrapperDelegate? generateMovesWrapper = null, string? shortForm = null)
	{
		ShortForm = shortForm;

		GenerateMovesDelegate shortformMoves = EmptyMoveGenerator.Instance;
		if (ShortForm is not null)
		{
			shortformMoves = Shortform.CompileShortform(ShortForm, this);
		}

		GenerateMoves =
			generateMovesWrapper is null
			? shortformMoves
			: generateMovesWrapper(shortformMoves, this);
	}

	public MoveType(MoveType moveType, GenerateMovesWrapperDelegate generateMovesWrapper)
	{
		ShortForm = moveType.ShortForm;
		Tags = moveType.Tags;
		DefaultResult = moveType.DefaultResult;

		GenerateMoves = generateMovesWrapper(moveType.GenerateMoves, this);
	}
}
