using System.Collections;
using System.Collections.Generic;
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
        Vector3 pos;
        pos = Utility.BoardIndexToWorldPos(currentIndex + 7);
        byte piecetld = Utility.PieceCodeAtWorldPos(pos.x, pos.y);
        if (Utility.IsNonePiece(piecetld) || !Utility.IsColour(piecetld, colour))
        {
            result.Add(currentIndex + 8);
        }
        pos = Utility.BoardIndexToWorldPos(currentIndex + 9);
        byte piecetrd = Utility.PieceCodeAtWorldPos(pos.x, pos.y);
        if (Utility.IsNonePiece(piecetrd) || !Utility.IsColour(piecetrd, colour))
        {
            result.Add(currentIndex + 9);
        }
        pos = Utility.BoardIndexToWorldPos(currentIndex + 8);
        byte piece1forward = Utility.PieceCodeAtWorldPos(pos.x, pos.y);
        pos = Utility.BoardIndexToWorldPos(currentIndex + 16);
        byte piece2forward = Utility.PieceCodeAtWorldPos(pos.x, pos.y);
        if (Utility.IsNonePiece(piece1forward))
        {
            result.Add(currentIndex + 8);
        }
        else
        {
            return result;
        }
        if (!hasMoved && Utility.IsNonePiece(piece2forward))
        {
            result.Add(currentIndex + 16);
        }
        return result;
    }

}
