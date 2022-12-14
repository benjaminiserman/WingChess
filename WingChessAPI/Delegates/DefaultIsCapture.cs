namespace WingChessAPI.Delegates;

public static class DefaultIsCapture
{
    public static IsCaptureDelegate Instance => (board, move) => board[move.NewX, move.NewY] != Unit.Empty;

    public static IsCaptureDelegate Never => (_, _) => false;
    public static IsCaptureDelegate Always => (_, _) => true;
}
