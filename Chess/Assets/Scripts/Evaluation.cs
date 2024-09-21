using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Evaluation
{
    /// <summary>
    /// Evaluation maps as found on http://www.chessprogramming.org/Simplified_Evaluation_Function
    /// </summary>
    
    #region EvalMaps
    public static readonly int[] PawnMap =
    {   
        0,  0,  0,  0,  0,  0,  0,  0,
        5, 10, 10,-20,-20, 10, 10,  5,
        5, -5,-10,  0,  0,-10, -5,  5,
        0,  0,  0, 20, 20,  0,  0,  0,
        5,  5, 10, 25, 25, 10,  5,  5,
        10, 10, 20, 30, 30, 20, 10, 10,
        50, 50, 50, 50, 50, 50, 50, 50,
        650, 650, 650, 650, 650, 650, 650, 650,
    };
    public static readonly int[] KnightMap = 
    {
        -50,-40,-30,-30,-30,-30,-40,-50,
        -40,-20,  0,  5,  5,  0,-20,-40,
        -30,  5, 10, 15, 15, 10,  5,-30,
        -30,  0, 15, 20, 20, 15,  0,-30,
        -30,  5, 15, 20, 20, 15,  5,-30,
        -30,  0, 10, 15, 15, 10,  0,-30,
        -40,-20,  0,  0,  0,  0,-20,-40,
        -50,-40,-30,-30,-30,-30,-40,-50
    };
    public static readonly int[] BishopMap = 
    {
        -20,-10,-10,-10,-10,-10,-10,-20,
        -10,  5,  0,  0,  0,  0,  5,-10,
        -10, 10, 10, 10, 10, 10, 10,-10,
        -10,  0, 10, 10, 10, 10,  0,-10,
        -10,  5,  5, 10, 10,  5,  5,-10,
        -10,  0,  5, 10, 10,  5,  0,-10,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -20,-10,-10,-10,-10,-10,-10,-20
    };
    public static readonly int[] RookMap =
    {
         0,  0,  0,  5,  5,  0,  0,  0,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
        -5,  0,  0,  0,  0,  0,  0, -5,
         5, 10, 10, 10, 10, 10, 10,  5,
         0,  0,  0,  0,  0,  0,  0,  0
    };
    public static readonly int[] QueenMap = 
    {
        -20,-10,-10, -5, -5,-10,-10,-20,
        -10,  0,  5,  0,  0,  0,  0,-10,
        -10,  5,  5,  5,  5,  5,  0,-10,
          0,  0,  5,  5,  5,  5,  0, -5,
         -5,  0,  5,  5,  5,  5,  0, -5,
        -10,  0,  5,  5,  5,  5,  0,-10,
        -10,  0,  0,  0,  0,  0,  0,-10,
        -20,-10,-10, -5, -5,-10,-10,-20
    };
    public static readonly int[] KingMidGameMap = 
    {
        20, 30, 10,  0,  0, 10, 30, 20,
        20, 20,  0,  0,  0,  0, 20, 20,
       -10,-20,-20,-20,-20,-20,-20,-10,
       -20,-30,-30,-40,-40,-30,-30,-20,
       -30,-40,-40,-50,-50,-40,-40,-30,
       -30,-40,-40,-50,-50,-40,-40,-30,
       -30,-40,-40,-50,-50,-40,-40,-30,
       -30,-40,-40,-50,-50,-40,-40,-30
    };
    public static readonly int[] KingEndGameMap =
    {
        -50,-30,-30,-30,-30,-30,-30,-50,
        -30,-30,  0,  0,  0,  0,-30,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 30, 40, 40, 30,-10,-30,
        -30,-10, 20, 30, 30, 20,-10,-30,
        -30,-20,-10,  0,  0,-10,-20,-30,
        -50,-40,-30,-20,-20,-30,-40,-50
    };
    #endregion

    #region PieceValues
    public static int pawnValue = 100;
    public static int knightValue = 320;
    public static int bishopValue = 330;
    public static int rookValue = 500;
    public static int queenValue = 900;
    public static int kingValue = 20000;
    #endregion

    /// <summary>
    /// Get the value of a given piece code
    /// </summary>
    /// <param name="pieceCode">The piece code to be used</param>
    /// <returns>The value of the corresponding piece</returns>
    public static int GetValue(byte pieceCode)
    {
        int material = Utility.TypeCode(pieceCode) switch
        {
            Piece.Pawn => pawnValue,
            Piece.Knight => knightValue,
            Piece.Bishop => bishopValue,
            Piece.Rook => rookValue,
            Piece.Queen => queenValue,
            Piece.King => kingValue,
            _ => 0, // En Passent or error
        };
        return material;
    }

    /// <summary>
    /// Determines whether a given position is considered "endgame" based on the rules outlined
    /// <see href="http://www.chessprogramming.org/Simplified_Evaluation_Function">here</see>.
    /// However, this function does not consider the presence of minor pieces.
    /// </summary>
    /// <param name="boardPosition">The position to be checked</param>
    /// <returns>True if the position is in the endgame, otherwise false</returns>
    public static bool IsEndgame(byte[] boardPosition)
    {
        int numWhiteQueens = 0;
        int numBlackQueens = 0;
        int numOtherPieces = 0;
        foreach (byte pieceCode in boardPosition)
        {
            if (Utility.IsPiece(pieceCode, Piece.Queen))
            {
                if (Utility.IsColour(pieceCode, Colour.White))
                    numWhiteQueens++;
                else if (Utility.IsColour(pieceCode, Colour.Black))
                    numBlackQueens++;
            }
            else if (!Utility.IsNonePiece(pieceCode))
            {
                numOtherPieces++;
            }
        }
        if (numWhiteQueens == 0 && numBlackQueens == 0)
            return true;
        if (numWhiteQueens == numBlackQueens && numOtherPieces == 0) 
            return true;
        return false;
    }

    public static int EvalBoard(byte[] boardPosition)
    {
        int eval = 0;
        for (int i = 0; i < boardPosition.Length; i++)
        {
            byte piece = boardPosition[i];
            if (Utility.IsNonePiece(piece)) continue;
            int[] map = (Utility.RemoveMetadata(piece) | Utility.ColourCode(piece)) switch
            {
                (Piece.Pawn | Piece.White) => PawnMap,
                (Piece.Pawn | Piece.Black) => PawnMap.Reverse().ToArray(),
                (Piece.Knight | Piece.White) => KnightMap,
                (Piece.Knight | Piece.Black) =>KnightMap.Reverse().ToArray(),
                (Piece.Bishop | Piece.White) => BishopMap,
                (Piece.Bishop | Piece.Black) =>BishopMap.Reverse().ToArray(),
                (Piece.Queen | Piece.White) => QueenMap,
                (Piece.Queen | Piece.Black) =>QueenMap.Reverse().ToArray(),
                _ => new int[64]
            };
            if (IsEndgame(boardPosition))
            {
                map = (Utility.RemoveMetadata(piece) | Utility.ColourCode(piece)) switch
                {
                    (Piece.King | Piece.White) => KingEndGameMap,
                    (Piece.King | Piece.Black) =>KingEndGameMap.Reverse().ToArray(),
                    _ => map
                };
            }
            else
            {
                map = (Utility.RemoveMetadata(piece) | Utility.ColourCode(piece)) switch
                {
                    (Piece.King | Piece.White) => KingMidGameMap,
                    (Piece.King | Piece.Black) => KingMidGameMap.Reverse().ToArray(),
                    _ => map
                };
            }
            int colourSign = (Utility.ColourCode(piece) == Piece.White) ? 1 : -1;
            int contribution = (GetValue(piece) + map[i]) * colourSign;
            eval += contribution;
        }
        return eval;
    }
}
