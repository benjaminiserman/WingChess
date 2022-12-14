namespace WingChessAPI.Chess;

using System;
using System.Collections.Generic;
using System.Linq;
using WingChessAPI.Delegates;
using WingChessAPI.Helpers;

internal static class ChessSpecialMoves
{
    public static GenerateMovesDelegate EnPassant(GenerateMovesDelegate generateMoves, MoveType _)
    {
        void Result(Board board, Move move)
        {
            DefaultResult.Instance(board, move);
            board[move.NewX, move.NewY - board.TransformDeltaY(1, move.Unit.Team)] = Unit.Empty;
        }

        IEnumerable<Move> GenerateMoves(Board board, int x, int y)
        {
            foreach (var move in generateMoves(board, x, y))
            {
                var enPassantee = board[move.NewX, move.NewY - board.TransformDeltaY(1, move.Unit.Team)];

                if (enPassantee != Unit.Empty
                    && board.History.Count > 0
                    && board.History[^1].Unit == enPassantee
                    && board.History[^1].MoveType.Tags.Contains("en_passantable"))
                {
                    yield return move with
                    {
                        Result = Result,
                        IsCapture = DefaultIsCapture.Always
                    };
                }
            }
        }

        return GenerateMoves;
    }

    public static ResultDelegate GetCastlingResult(Unit castle, int castleX, int castleY)
    {
        void Result(Board board, Move move)
        {
            board[castle] = Unit.Empty;
            board[move.OldX, move.OldY] = Unit.Empty;

            board[castleX, castleY] = castle;
            board[move.NewX, move.NewY] = move.Unit;
        }

        return Result;
    }

    public static GenerateMovesDelegate AbsoluteCastle(GenerateMovesDelegate _, MoveType moveType)
    {
        IEnumerable<Move> GenerateMoves(Board board, int x, int y)
        {
            var castler = board[x, y];
            if (!board.History.Any(m => m.Unit == castler))
            {
                if (ChessRules.MoveAllowedRoyalCapture(board.ApplyMove(new(board, x, y, x, y, moveType), isRecursive: true)) == Rule.Illegal) // no castling while checked $$$ replace with helper method call
                {
                    yield break;
                }

                var castlingUnits = board.BoardHistory[0]
                    .Where(kvp => board.GetUnitType(kvp.unit).Tags.Contains("castle")
                        && kvp.unit.Team == castler.Team
                        && !board.History.Any(move => move.Unit == kvp.unit))
                    .Select(kvp => kvp);

                var homeRank = (int)board.Variables[$"{castler.Team.Name}_home_rank"];
                // queen castle
                var queenCastle = castlingUnits.FirstOrDefault(kvp => kvp.pos.x < x);
                if (queenCastle != default
                    && board[1, homeRank] == Unit.Empty
                    && board[2, homeRank] == Unit.Empty
                    && ChessRules.MoveAllowedRoyalCapture(board.ApplyMove(new(board, x, y, 2, homeRank, moveType), isRecursive: true)) != Rule.Illegal
                    && board[3, homeRank] == Unit.Empty
                    && ChessRules.MoveAllowedRoyalCapture(board.ApplyMove(new(board, x, y, 3, homeRank, moveType), isRecursive: true)) != Rule.Illegal)
                {
                    yield return new Move
                    (
                        board[x, y],
                        x, y,
                        2, homeRank,
                        moveType,
                        board,
                        GetCastlingResult(queenCastle.unit, 3, homeRank),
                        DefaultIsCapture.Never,
                        _ => "O-O-O"
                    );
                }

                // king castle
                var kingCastle = castlingUnits.FirstOrDefault(kvp => kvp.pos.x > x);
                if (kingCastle != default
                    && board[6, homeRank] == Unit.Empty
                    && board[5, homeRank] == Unit.Empty
                    && ChessRules.MoveAllowedRoyalCapture(board.ApplyMove(new(board, x, y, 5, homeRank, moveType), isRecursive: true)) != Rule.Illegal)
                {
                    yield return new(
                        board[x, y],
                        x, y,
                        6, homeRank,
                        moveType,
                        board,
                        GetCastlingResult(kingCastle.unit, 5, homeRank),
                        DefaultIsCapture.Never,
                        _ => "O-O"
                    );
                }
            }
        }

        return GenerateMoves;
    }

    public static GenerateMovesDelegate RelativeCastle(GenerateMovesDelegate _, MoveType moveType)
    {
        IEnumerable<Move> GenerateMoves(Board board, int x, int y)
        {
            var castler = board[x, y];

            if (!board.History.Any(m => m.Unit == castler))
            {
                if (ChessRules.MoveAllowedRoyalCapture(board.ApplyMove(new(board, x, y, x, y, moveType), isRecursive: true)) == Rule.Illegal) // no castling while checked $$$ replace with helper method call
                {
                    yield break;
                }

                var castlingUnits = board.BoardHistory[0]
                    .Where(kvp => board.GetUnitType(kvp.unit).Tags.Contains("castle")
                        && kvp.unit.Team == castler.Team
                        && !board.History.Any(move => move.Unit == kvp.unit))
                    .Select(kvp => kvp);
                foreach (var (castlePos, castleUnit) in castlingUnits)
                {
                    var breakFlag = false;
                    if (castlePos.x == x)
                    {
                        foreach (var i in castlePos.y..y)
                        {
                            if (i != castlePos.y
                                && (board[x, i] != Unit.Empty
                                    || ChessRules.MoveAllowedRoyalCapture(board.ApplyMove(new(board, x, y, x, i, moveType), isRecursive: true)) == Rule.Illegal))
                            {
                                breakFlag = true;
                                break;
                            }
                        }

                        if (!breakFlag)
                        {
                            yield return new(
                                board[x, y],
                                x, y,
                                x, y + 2 * Math.Sign(castlePos.y - y),
                                moveType,
                                board,
                                GetCastlingResult(castleUnit, x, y + Math.Sign(castlePos.y - y)),
                                DefaultIsCapture.Never,
                                move =>
                                    move.NewX < move.OldX
                                    || move.NewY < move.OldY
                                    ? "O-O-O"
                                    : "O-O"
                            );
                        }
                    }
                    else if (castlePos.y == y)
                    {
                        foreach (var i in castlePos.x..x)
                        {
                            if (i != castlePos.x && (board[i, y] != Unit.Empty
                                    || ChessRules.MoveAllowedRoyalCapture(board.ApplyMove(new(board, x, y, i, y, moveType), isRecursive: true)) == Rule.Illegal))
                            {
                                breakFlag = true;
                                break;
                            }
                        }

                        if (!breakFlag)
                        {
                            yield return new(
                                board[x, y],
                                x, y,
                                x + 2 * Math.Sign(castlePos.x - x), y,
                                moveType,
                                board,
                                GetCastlingResult(castleUnit, x + Math.Sign(castlePos.x - x), y),
                                DefaultIsCapture.Never,
                                move =>
                                    move.NewX < move.OldX
                                    || move.NewY < move.OldY
                                    ? "O-O-O"
                                    : "O-O"
                            );
                        }
                    }
                }
            }
        }

        return GenerateMoves;
    }
}
