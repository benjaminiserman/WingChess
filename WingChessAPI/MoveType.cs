namespace WingChessAPI;

public class MoveType
{
	private GenerateMovesDelegate _providedGenerateMoves;
	public GenerateMovesDelegate GenerateMoves { get; private set; } = EmptyMoveGenerator.Instance;
	public string? ShortForm = null;
	public ResultDelegate Result { get; private set; } = DefaultResult.Instance;

	public MoveType(GenerateMovesDelegate? generateMoves = null, string? shortForm = null)
	{
		_providedGenerateMoves = generateMoves ?? EmptyMoveGenerator.Instance;
		ShortForm = shortForm;
		CompileShortform();
	}

	public void CompileShortform()
	{
		if (ShortForm is null)
		{
			throw new ArgumentNullException();
		}

		var shortformMoves = Shortform.CompileShortform(ShortForm);

		if (_providedGenerateMoves != EmptyMoveGenerator.Instance)
		{
			GenerateMovesDelegate AppendGenerateMoves(GenerateMovesDelegate shortformMoves)
			{
				IEnumerable<Move> TrueGenerateMoves(Board board, int x, int y)
				{
					foreach (var move in shortformMoves(board, x, y))
					{
						yield return move;
					}

					foreach (var move in _providedGenerateMoves(board, x, y))
					{
						yield return move;
					}
				}

				return TrueGenerateMoves;
			}

			GenerateMoves = AppendGenerateMoves(shortformMoves);
		}
		else
		{
			GenerateMoves = shortformMoves;
		}
	}
}
