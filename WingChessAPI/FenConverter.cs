namespace WingChessAPI;

public static class FenConverter
{
	public static Board ConvertFen(Game game, string fenString)
	{
		var board = new Board(game);
		var split = fenString.Split();
		var piecePlacement = split[0];
		var activeColor = split[1];
		var castling = split[2];
		var enPassant = split[3];
		var halfmoveClock = split[4];
		var fullmoveNumber = split[5];

		var ranks = piecePlacement.Split('/');
		for (var i = 0; i < ranks.Length; i++)
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
					board[fileId, rankId] = game.FenMap[c].CreateUnit(
						char.IsUpper(c)
							? Team.White
							: Team.Black,
						board.GetNotation(fileId, rankId)
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
