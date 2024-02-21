using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public sealed class Board
{
    public static string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public static ushort[] square;
    public Board()
    {
        square = new ushort[64];
        square = PositionFromFEN(startFEN);
    }
    public static int NotationToBoardIndex(string sqr)
    {
        // sqr must be length 2, with the 1st being a char and the 2nd an int 
        if (sqr.Length != 2 || !char.IsLetter(sqr[0]) || !char.IsDigit(sqr[1]))
        {
            return -1;
        }
        int rank = sqr[0] - 'a'; // a = 0, b = 1, c = 2, etc
        int file = sqr[1] - '0' - 1; // converts from char to int, but subtracts 1 as arrays are 0-based indexed
        return file + 8 * rank;
    }
    public static string BoardIndexToNotation(int boardIndex)
    {
        string notation = "";
        if (boardIndex < 0 ||  boardIndex > 63)
        {
            return notation;
        }
        int rankInt = boardIndex % 8;
        char rank = (char)(rankInt + 'a');
        char file = (char)((boardIndex - rankInt) / 8 + 1 + '0');
        notation = $"{rank}{file}";
        return notation;
    }
    ushort[] PositionFromFEN(string fen)
    {
        ushort[] result = new ushort[64];
        string[] splitFEN = fen.Split("/");
        splitFEN[^1] = splitFEN[^1].Split(" ")[0];
        int index = 63; // start at the top of the board
        ushort colour;
        ushort pieceType;
        int skips = 0;
        foreach (string rank in splitFEN)
        {
            foreach (char c in rank.Reverse()) // indices decrease right to left, so flip the rank
            {
                if (skips >= 0)
                {
                    result[index] = Piece.None; // place no piece at this position
                    index--;
                    skips--;
                    continue;
                }
                if (char.IsDigit(c))
                {
                    skips = c - '0'; // converts char to int
                }
                colour = (char.IsUpper(c)) ? Piece.White : Piece.Black; // UPPER = White, Lower = black
                pieceType = char.ToLower(c) switch
                {
                    'p' => Piece.Pawn,
                    'k' => Piece.King,
                    'n' => Piece.Knight,
                    'b' => Piece.Bishop,
                    'r' => Piece.Rook,
                    'q' => Piece.Queen,
                    _ => Piece.None // illegal character in FEN, just add no piece
                };
                // place a piece with the specified colour at this position
                result[index] = (ushort)(pieceType + colour); 
                index--;
            }
        }
        return result;
    }
}
