using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

// example bot de-verbosed using linq
// 1) plays capture moves if possible
// 2) plays mate-in-one, if it sees one
// 3) plays the first move it sees otherwise

public class MyBot : IChessBot
{
    // pawn, knight, bishop, rook, queen, king
    private readonly int[] PieceValues = { 1, 3, 3, 5, 9, 999 };

    private readonly double[,] PawnPositionalEvaluation = new double[,]
    {
        { 0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0 },
        { 5.0,  5.0,  5.0,  5.0,  5.0,  5.0,  5.0,  5.0 },
        { 1.0,  1.0,  2.0,  3.0,  3.0,  2.0,  1.0,  1.0 },
        { 0.5,  0.5,  1.0,  2.5,  2.5,  1.0,  0.5,  0.5 },
        { 0.0,  0.0,  0.0,  2.0,  2.0,  0.0,  0.0,  0.0 },
        { 0.5, -0.5, -1.0,  0.0,  0.0, -1.0, -0.5,  0.5 },
        { 0.5,  1.0,  1.0, -2.0, -2.0,  1.0,  1.0,  0.5 },
        { 0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0 }
    };

    private readonly double[,] KnightPositionalEvaluation = new double[,]
    {
        { -5.0, -4.0, -3.0, -3.0, -3.0, -3.0, -4.0, -5.0 },
        { -4.0, -2.0,  0.0,  0.0,  0.0,  0.0, -2.0, -4.0 },
        { -3.0,  0.0,  1.0,  1.5,  1.5,  1.0,  0.0, -3.0 },
        { -3.0,  0.5,  1.5,  2.0,  2.0,  1.5,  0.5, -3.0 },
        { -3.0,  0.0,  1.5,  2.0,  2.0,  1.5,  0.0, -3.0 },
        { -3.0,  0.5,  1.0,  1.5,  1.5,  1.0,  0.5, -3.0 },
        { -4.0, -2.0,  0.0,  0.5,  0.5,  0.0, -2.0, -4.0 },
        { -5.0, -4.0, -3.0, -3.0, -3.0, -3.0, -4.0, -5.0 }
    };

    private readonly double[,] BishopPositionalEvaluation = new double[,]
    {
        { -2.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -2.0 },
        { -1.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -1.0 },
        { -1.0,  0.0,  0.5,  1.0,  1.0,  0.5,  0.0, -1.0 },
        { -1.0,  0.5,  0.5,  1.0,  1.0,  0.5,  0.5, -1.0 },
        { -1.0,  0.0,  1.0,  1.0,  1.0,  1.0,  0.0, -1.0 },
        { -1.0,  1.0,  1.0,  1.0,  1.0,  1.0,  1.0, -1.0 },
        { -1.0,  0.5,  0.0,  0.0,  0.0,  0.0,  0.5, -1.0 },
        { -2.0, -1.0, -1.0, -1.0, -1.0, -1.0, -1.0, -2.0 }
    };

    private readonly double[,] RookPositionalEvaluation = new double[,]
    {
        {  0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0 },
        {  0.5,  1.0,  1.0,  1.0,  1.0,  1.0,  1.0,  0.5 },
        { -0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5 },
        { -0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5 },
        { -0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5 },
        { -0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5 },
        { -0.5,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -0.5 },
        {  0.0,  0.0,  0.0,  0.5,  0.5,  0.0,  0.0,  0.0 }
    };

    private readonly double[,] QueenPositionalEvaluation = new double[,]
    {
        { -2.0, -1.0, -1.0, -0.5, -0.5, -1.0, -1.0, -2.0 },
        { -1.0,  0.0,  0.0,  0.0,  0.0,  0.0,  0.0, -1.0 },
        { -1.0,  0.0,  0.5,  0.5,  0.5,  0.5,  0.0, -1.0 },
        { -0.5,  0.0,  0.5,  0.5,  0.5,  0.5,  0.0, -0.5 },
        {  0.0,  0.0,  0.5,  0.5,  0.5,  0.5,  0.0, -0.5 },
        { -1.0,  0.5,  0.5,  0.5,  0.5,  0.5,  0.0, -1.0 },
        { -1.0,  0.0,  0.5,  0.0,  0.0,  0.0,  0.0, -1.0 },
        { -2.0, -1.0, -1.0, -0.5, -0.5, -1.0, -1.0, -2.0 }
    };

    private readonly double[,] KingPositionalEvaluation = new double[,]
    {
        { -3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0 },
        { -3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0 },
        { -3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0 },
        { -3.0, -4.0, -4.0, -5.0, -5.0, -4.0, -4.0, -3.0 },
        { -2.0, -3.0, -3.0, -4.0, -4.0, -3.0, -3.0, -2.0 },
        { -1.0, -2.0, -2.0, -2.0, -2.0, -2.0, -2.0, -1.0 },
        {  2.0,  2.0,  0.0,  0.0,  0.0,  0.0,  2.0,  2.0 },
        {  2.0,  3.0,  1.0,  0.0,  0.0,  1.0,  3.0,  2.0 }
    };


    public Move Think(Board board, Timer timer)
    {
        Move[] legalMoves = board.GetLegalMoves();

        List<Move> capturelegalMoves = legalMoves.Where(move => move.IsCapture).ToList();

        if (capturelegalMoves.Count == 0)
            return legalMoves[0];

        Move bestCaptureMove = capturelegalMoves
            .OrderByDescending(
                move => PieceValues[(int)move.CapturePieceType]
                * (MoveIsCheckmate(board, move) ? 100 : 1))
            .FirstOrDefault();

        return bestCaptureMove;
    }

    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }
}
