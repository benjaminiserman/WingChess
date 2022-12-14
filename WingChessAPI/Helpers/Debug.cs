namespace WingChessAPI.Helpers;
using System;

internal static class Debug
{
    public static void PrintBoard(Board board)
    {
        int? minX = null,
            minY = null,
            maxX = null,
            maxY = null;

        if (board.XSize is int xSize
            && board.YSize is int ySize)
        {
            minX = 0;
            minY = 0;
            maxX = xSize;
            maxY = ySize;
        }
        else
        {
            foreach (var ((x, y), _) in board)
            {
                if (minX is null || x < minX)
                {
                    minX = x;
                }

                if (minY is null || y < minY)
                {
                    minY = y;
                }

                if (maxX is null || x > maxX)
                {
                    maxX = x;
                }

                if (maxY is null || y > maxY)
                {
                    maxY = y;
                }
            }
        }

        if (minX is null || minY is null || maxX is null || maxY is null)
        {
            // board is empty
            return;
        }

        var boardX = (int)(maxX - minX);
        var boardY = (int)(maxY - minY);

        var displayBoard = new char[boardX, boardY];
        for (var i = 0; i < boardX; i++)
        {
            for (var j = 0; j < boardY; j++)
            {
                displayBoard[i, j] = ' ';
            }
        }

        foreach (var ((x, y), unit) in board)
        {
            var fen = board.GetUnitType(unit).Fen;
            var ny = boardY - y - 1;
            if (fen is not char fenChar)
            {
                displayBoard[ny, x] = '?';
            }
            else
            {
                displayBoard[ny, x] = unit.Team.Name switch
                {
                    "White" => char.ToUpper(fenChar),
                    "Black" => char.ToLower(fenChar),
                    _ => '?'
                };
            }
        }

        for (var i = 0; i < boardY; i++)
        {
            Console.Write($"{displayBoard.GetLength(0) - i} ");
            for (var j = 0; j < boardX; j++)
            {
                Console.Write($"{displayBoard[i, j]}");
            }

            Console.WriteLine();
        }

        Console.Write("  ");
        for (var i = 0; i < boardX; i++)
        {
            Console.Write($"{(char)('a' + i)}");
        }

        Console.WriteLine();
    }
}
