namespace WingChessAPI;

public record struct Team(string Name)
{
	public static Team White => new("White");
	public static Team Black => new("Black");
}
