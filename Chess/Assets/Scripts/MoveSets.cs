using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class MoveSets
{
    //public static readonly int[] Pawn = { 7, 8, 9, 16 };
    public static readonly int[] King = { 7, 8, 9, -1, 1, -9, -8, -7 };
    public static readonly int[] Knight = { -17, -15, -10, -6, 6, 10, 15, 17 };
    public static readonly int[][] Rook = 
    { 
        new int[] {1, 2, 3, 4, 5, 6, 7 },
        new int[] {-1, -2, -3, -4, -5, -6, -7 },
        new int[] {8, 16, 24, 32, 40, 48, 56 },
        new int[] {-8, -16, -24, -32, -40, -48, -56 } 
    };
    public static readonly int[][] Bishop =
    {
        new int[] {9, 18, 27, 36, 45, 54, 63 },
        new int[] {-9, -18, -27, -36, -54, -63 },
        new int[] {7, 14, 21, 28, 35, 42, 49 },
        new int[] {-7, -14, -21, -28, -35, -42, -49 }
    };
    public static readonly int[][] Queen = 
    {
        new int[] {1, 2, 3, 4, 5, 6, 7 },
        new int[] {-1, -2, -3, -4, -5, -6, -7 },
        new int[] {8, 16, 24, 32, 40, 48, 56 },
        new int[] {-8, -16, -24, -32, -40, -48, -56 },
        new int[] {9, 18, 27, 36, 45, 54, 63 },
        new int[] {-9, -18, -27, -36, -54, -63 },
        new int[] {7, 14, 21, 28, 35, 42, 49 },
        new int[] {-7, -14, -21, -28, -35, -42, -49 }
    };

    public static List<int> CalculatePawnMoves(int currentIndex, Colour colour, bool hasMoved)
    {
        List<int> result = new();
        int indexTopLeft = (colour == Colour.White) ? (currentIndex + 7) : currentIndex - 9;
        if (indexTopLeft >=0 && indexTopLeft < 64)
        {
            byte pieceTopLeft = Board.PieceCodeAtIndex(indexTopLeft);
            if (Utility.IsNonePiece(pieceTopLeft) || !Utility.IsColour(pieceTopLeft, colour))
            {
                result.Add(indexTopLeft);
            }
        }
        int indexTopRight = (colour == Colour.White) ? (currentIndex + 9) : (currentIndex - 7);
        if (indexTopRight >=0 && indexTopRight < 64)
        {
            byte pieceTopRight = Board.PieceCodeAtIndex(indexTopRight);
            if (Utility.IsNonePiece(pieceTopRight) || !Utility.IsColour(pieceTopRight, colour))
            {
                result.Add(indexTopRight);
            }
        }
        int index1Forward = (colour == Colour.White) ? (currentIndex + 8) : (currentIndex - 8);
        if (index1Forward >=0 && index1Forward < 64)
        {
            byte piece1Forward = Board.PieceCodeAtIndex(index1Forward);
            if (!Utility.IsNonePiece(piece1Forward)) return result;
            result.Add(index1Forward); 
        }
        if (hasMoved) return result;
        int index2Forward = (colour == Colour.White) ? (currentIndex + 16) : (currentIndex - 16);
        if (index2Forward >=0 && index2Forward < 64)
        {
            byte piece2Forward = Board.PieceCodeAtIndex(index2Forward);
            if (Utility.IsNonePiece(piece2Forward))
            {
                result.Add(index2Forward);
            }
        }
        return result;
    }
    public static List<int> CalculateKnightMoves(int currentIndex, Colour colour)
    {
        List<int> result = new();
        foreach (int i in Knight)
        {
            byte targetCode = Board.PieceCodeAtIndex(currentIndex + i);
            if (Utility.IsNonePiece(targetCode) || !Utility.IsColour(targetCode, colour))
            {
                result.Add(i);
            }
        }
        return result;
    }
    public static List<int> CalculateKingMoves(int currentIndex, Colour colour, bool hasMoved)
    {
        List<int> result = new();
        // hasMoved will be used for castling
        foreach (int i in King)
        {
            byte targetCode = Board.PieceCodeAtIndex(currentIndex + i);
             // TODO: check if the move puts the king in check
            if (Utility.IsNonePiece(targetCode) || !Utility.IsColour(targetCode, colour))
            {
                result.Add(i);
            }
        }
        return result;
    }
    public static List<int> CalculateRookMoves(int currentIndex, Colour colour)
    {
        List<int> result = new();
        foreach (int[] dir in Rook)
        {
            foreach (int i in dir)
            {
                byte targetCode = Board.PieceCodeAtIndex(currentIndex + i);
                if (Utility.IsNonePiece(targetCode))
                {
                    result.Add(i);
                    continue;
                }
                if (!Utility.IsColour(targetCode, colour))
                {
                    result.Add(i);
                }
                break; // finish with this direction
            }
        }
        return result;
    }
    public static List<int> CalculateBishopMoves(int currentIndex, Colour colour)
    {
        List<int> result = new();
        foreach (int[] dir in Rook)
        {
            foreach (int i in dir)
            {
                byte targetCode = Board.PieceCodeAtIndex(currentIndex + i);
                if (Utility.IsNonePiece(targetCode))
                {
                    result.Add(i);
                    continue;
                }
                if (!Utility.IsColour(targetCode, colour))
                {
                    result.Add(i);
                }
                break; // finish with this direction
            }
        }
        return result;
    }
    public static List<int> CalculateQueenMoves(int currentIndex, Colour colour)
    {
        List<int> result = CalculateBishopMoves(currentIndex, colour);
        result.AddRange(CalculateRookMoves(currentIndex, colour));
        return result;
    }
}
