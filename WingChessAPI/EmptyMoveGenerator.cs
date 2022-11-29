namespace WingChessAPI;
using System.Collections.Generic;

public static class EmptyMoveGenerator
{
	public static IEnumerable<Move> Instance(Board _, int _2, int _3)
	{
		yield break;
	}

	public static IEnumerable<(Move, ResultDelegate)> WithResultInstance(Board _, int _2, int _3)
	{
		yield break;
	}
}
