namespace WingChessAPI;

public delegate IEnumerable<Move> GenerateMovesDelegate(Board board, int x, int y);
