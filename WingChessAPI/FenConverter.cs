namespace WingChessAPI;

public static class FenConverter
{
	public static Board ConvertFen(string fenString, Dictionary<char, UnitType> fenMap)
	{
		var board = new Board(Game.Chess);
		var split = fenString.Split();
		var piecePlacement = split[0];
		var activeColor = split[1];
		var castling = split[2];
		var enPassant = split[3];
		var halfmoveClock = split[4];
		var fullmoveNumber = split[5];

		var ranks = piecePlacement.Split('/');
		for (int i = 0; i < ranks.Length; i++)
		{
			var rankId = ranks.Length - i - 1;
			var fileId = 0;
			foreach (var c in ranks[i])
			{
				if (char.IsDigit(c))
				{
					fileId += c - '0';
				}
				else
				{
					board[fileId, rankId] = fenMap[c].CreateUnit(
						char.IsUpper(c)
						? Team.White
						: Team.Black
					);
					fileId++;
				}
			}
		}

		board.ToMove =
			activeColor == "w"
			? Team.White
			: Team.Black;

		var ySize = ranks.Length;
		var xSize = 0;

		foreach (var c in ranks[0])
		{
			if (char.IsDigit(c))
			{
				xSize += c - '0';
			}
			else
			{
				xSize++;
			}
		}

		board.SetBounds(xSize, ySize);

		return board;
	}
}
