namespace WingChessAPI.Delegates;

using System.Collections.Generic;

public delegate IEnumerable<Move> GenerateMovesDelegate(Board board, int x, int y);
