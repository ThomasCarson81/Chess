using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public static class MoveSets
{
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
        new int[] {-9, -18, -27, -36, -45, -54, -63 },
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
    public static bool IsValidIndex(int index)
    {
        return index >= 0 && index < 64;
    }
    static bool IsNoneOrEnemy(byte piece, Colour friendlyColour)
    {
        return Utility.IsNonePiece(piece) || (!Utility.IsColour(piece, friendlyColour) && !Utility.IsPiece(piece, Piece.EnPassant));
    }
    public static List<int> CalculatePawnMoves(int currentIndex, Colour colour, bool hasMoved)
    {
        List<int> result = new();
        int indexTopLeft = (colour == Colour.White) ? (currentIndex + 7) : (currentIndex - 9);
        if (IsValidIndex(indexTopLeft))
        {
            byte pieceTopLeft = Board.square[indexTopLeft];
            if ((!Utility.IsNonePiece(pieceTopLeft) || Utility.IsPiece(pieceTopLeft, Piece.EnPassant)) && !Utility.IsColour(pieceTopLeft, colour))
            {
                result.Add(indexTopLeft);
            }
        }
        int indexTopRight = (colour == Colour.White) ? (currentIndex + 9) : (currentIndex - 7);
        if (IsValidIndex(indexTopRight))
        {
            byte pieceTopRight = Board.square[indexTopRight];
            if ((!Utility.IsNonePiece(pieceTopRight) || Utility.IsPiece(pieceTopRight, Piece.EnPassant))
                && !Utility.IsColour(pieceTopRight, colour))
                result.Add(indexTopRight);
        }
        int index1Forward = (colour == Colour.White) ? (currentIndex + 8) : (currentIndex - 8);
        if (IsValidIndex(index1Forward))
        {
            byte piece1Forward = Board.square[index1Forward];
            if (!Utility.IsNonePiece(piece1Forward))
                return result;
            result.Add(index1Forward);
        }
        if (hasMoved)
            return result;
        int index2Forward = (colour == Colour.White) ? (currentIndex + 16) : (currentIndex - 16);
        if (IsValidIndex(index2Forward))
        {
            byte piece2Forward = Board.square[index2Forward];
            if (Utility.IsNonePiece(piece2Forward))
                result.Add(index2Forward);
        }
        return result;
    }
    public static List<int> CalculateKnightMoves(int currentIndex, Colour colour)
    {
        List<int> result = new();
        foreach (int i in Knight)
        {
            if (!IsValidIndex(currentIndex + i)) continue;
            byte targetCode = Board.square[currentIndex + i];
            Vector2 currPos = Utility.BoardIndexToWorldPos(currentIndex);
            Vector2 newPos = Utility.BoardIndexToWorldPos(currentIndex + i);
            float xDif = Mathf.Abs(newPos.x - currPos.x);
            float yDif = Mathf.Abs(newPos.y - currPos.y);
            bool valid = false;
            if ( (xDif == 2) && (yDif == 1) )
                valid = true;
            else if ( (xDif == 1) &&  (yDif == 2) ) 
                valid = true;
            if (!valid)
                continue;
            if (IsNoneOrEnemy(targetCode, colour))
                result.Add(currentIndex + i);
        }
        return result;
    }
    public static List<int> CalculateKingMoves(int currentIndex, Colour colour, bool hasMoved, bool checkForCheck)
    {
        List<int> result = new();
        // hasMoved will be used for castling
        foreach (int i in King)
        {
            if (!IsValidIndex(currentIndex + i))
                continue;
            Vector2 currPos = Utility.BoardIndexToWorldPos(currentIndex);
            Vector2 newPos = Utility.BoardIndexToWorldPos(currentIndex + i);
            if (Mathf.Abs(newPos.x - currPos.x) > 1 || Mathf.Abs(newPos.y - currPos.y) > 1)
                continue; // move wraps
            byte targetCode = Board.square[currentIndex + i];
            if (checkForCheck && IsAttacked(currentIndex + i, colour))
                continue; // unable to move into check
            if (IsNoneOrEnemy(targetCode, colour))
                result.Add(currentIndex + i);
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
                if (!IsValidIndex(currentIndex + i)) break; // Dir went off the board
                byte targetCode = Board.square[currentIndex + i];
                Vector2 currPos = Utility.BoardIndexToWorldPos(currentIndex);
                Vector2 newPos = Utility.BoardIndexToWorldPos(currentIndex + i);
                if (currPos.x != newPos.x && currPos.y != newPos.y)
                    break; // resolves wrapping
                if (Utility.IsNonePiece(targetCode))
                {
                    result.Add(currentIndex + i);
                    continue;
                }
                if (!Utility.IsColour(targetCode, colour))
                    result.Add(currentIndex + i);
                break; // finish with this direction
            }
        }
        return result;
    }
    public static List<int> CalculateBishopMoves(int currentIndex, Colour colour)
    {
        List<int> result = new();
        foreach (int[] dir in Bishop)
        {
            foreach (int i in dir)
            {
                if (!IsValidIndex(currentIndex + i)) break; // Dir went off the board
                byte targetCode = Board.square[currentIndex + i];
                Vector2 currPos = Utility.BoardIndexToWorldPos(currentIndex);
                Vector2 newPos = Utility.BoardIndexToWorldPos(currentIndex + i);
                if (Mathf.Abs(newPos.x - currPos.x) != Mathf.Abs(newPos.y - currPos.y))
                    break; // resolves wrapping
                if (Utility.IsNonePiece(targetCode))
                {
                    result.Add(currentIndex + i);
                    continue;
                }
                if (!Utility.IsColour(targetCode, colour))
                {
                    result.Add(currentIndex + i);
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
    public static bool IsAttacked(int index, Colour colour)
    {
        return IsAttacked(index, colour, Board.square);
    }
    public static bool IsAttacked(int index, Colour colour, byte[] boardPosition)
    {
        Colour enemyColour = (colour == Colour.White) ? Colour.Black : Colour.White;
        Vector2 currPos = Utility.BoardIndexToWorldPos(index);
        Vector2 newPos;
        #region PAWN_CHECK
        int indexLeft = (colour == Colour.White) ? (index + 7) : (index - 9);
        if (IsValidIndex(indexLeft))
        {
            byte pieceLeft = boardPosition[indexLeft];
            newPos = Utility.BoardIndexToWorldPos(indexLeft);
            bool valid = true;
            if (Mathf.Abs(currPos.x - newPos.x) != 1 &&  Mathf.Abs(currPos.y - newPos.y) != 1)
            {
                valid = false; 
            }
            if (valid && Utility.IsPiece(pieceLeft, Piece.Pawn) && Utility.IsColour(pieceLeft, enemyColour))
            {
                //Debug.Log($"{indexLeft}-LP-(X{Mathf.Abs(currPos.x - newPos.x)},Y{Mathf.Abs(newPos.y - newPos.y)})");
                return true;
            }
        }
        int indexRight = (colour == Colour.White) ? (index + 9) : (index - 7);
        if (IsValidIndex(indexRight))
        {
            byte pieceRight = boardPosition[indexRight];
            newPos = Utility.BoardIndexToWorldPos(indexRight);
            bool valid = true;
            if (Mathf.Abs(currPos.x - newPos.x) != 1 && Mathf.Abs(newPos.y - newPos.y) != 1)
            {
                valid = false;
            }
            if (valid && Utility.IsPiece(pieceRight, Piece.Pawn) && Utility.IsColour(pieceRight, enemyColour))
            {
                //Debug.Log($"{indexLeft}-RP-(X{Mathf.Abs(currPos.x - newPos.x)},Y{Mathf.Abs(newPos.y - newPos.y)})");
                return true;
            }
        }
        #endregion
        #region ROOK_CHECK
        foreach (int[] dir in Rook)
        {
            foreach (int i in dir)
            {
                if (!IsValidIndex(index + i)) break; // Dir went off the board
                byte targetCode = boardPosition[index + i];
                newPos = Utility.BoardIndexToWorldPos(index + i);
                if (currPos.x != newPos.x && currPos.y != newPos.y)
                    break; // resolves wrapping
                if (Utility.IsNonePiece(targetCode))
                    continue;
                if (Utility.IsColour(targetCode, colour))
                    break; //friendly piece protecting
                if (Utility.IsPiece(targetCode, Piece.Rook) || Utility.IsPiece(targetCode, Piece.Queen))
                {
                    //Debug.Log($"{index}-R/Q");
                    return true;
                }
                break; // finish with this direction
            }
        }
        #endregion
        #region BISHOP_CHECK
        foreach (int[] dir in Bishop)
        {
            foreach (int i in dir)
            {
                if (!IsValidIndex(index + i)) break; // Dir went off the board
                byte targetCode = boardPosition[index + i];
                newPos = Utility.BoardIndexToWorldPos(index + i);
                if (Mathf.Abs(newPos.x - currPos.x) != Mathf.Abs(newPos.y - currPos.y))
                    break; // resolves wrapping
                if (Utility.IsNonePiece(targetCode))
                    continue;
                if (Utility.IsColour(targetCode, colour))
                    break; //friendly piece protecting
                if (Utility.IsPiece(targetCode, Piece.Bishop) || Utility.IsPiece(targetCode, Piece.Queen))
                {
                    //Debug.Log($"{index}-B/Q");
                    return true;
                }
                break; // finish with this direction
            }
        }
        #endregion
        #region KNIGHT_CHECK
        foreach (int i in Knight)
        {
            if (!IsValidIndex(index + i)) continue;
            byte targetCode = boardPosition[index + i];
            newPos = Utility.BoardIndexToWorldPos(index + i);
            float xDif = Mathf.Abs(newPos.x - currPos.x);
            float yDif = Mathf.Abs(newPos.y - currPos.y);
            bool valid = false;
            if ((xDif == 2) && (yDif == 1))
                valid = true;
            else if ((xDif == 1) && (yDif == 2))
                valid = true;
            if (!valid)
                continue;
            if (Utility.IsPiece(targetCode, Piece.Knight) && Utility.IsColour(targetCode, enemyColour))
            {
                //Debug.Log($"{index}-N");
                return true;
            }
        }
        #endregion
        #region KING_CHECK
        foreach (int i in King)
        {
            if (!IsValidIndex(index + i))
                continue;
            newPos = Utility.BoardIndexToWorldPos(index + i);
            if (Mathf.Abs(newPos.x - currPos.x) > 1 || Mathf.Abs(newPos.y - currPos.y) > 1)
                continue; // move wraps
            byte targetCode = boardPosition[index + i];
            if (Utility.IsPiece(targetCode, Piece.King) && Utility.IsColour(targetCode, enemyColour))
            {
                //Debug.Log($"{index}-K");
                return true;
            }
        }
        #endregion
        return false;
    }
    public static bool ProtectsCheck(int from, int to, Colour colour, GameObject movingPieceObj)
    {
        if (!IsValidIndex(from))
        {
            Debug.Log("Invalid from index in ProtectsCheck");
            return false;
        }
        if (!IsValidIndex(to))
        {
            Debug.Log("Invalid to index in ProtectsCheck");
            return false;
        }
        bool protects;
        byte[] theoryPosition = new byte[64];
        byte colourCode = (colour == Colour.White) ? Piece.White : Piece.Black;
        for (int i = 0; i < Board.square.Length; i++)
        {
            theoryPosition[i] = Board.square[i];
        }
        Vector3 posFrom = Utility.BoardIndexToWorldPos(from);
        Vector3 posTo = Utility.BoardIndexToWorldPos(to);
        if (!movingPieceObj.TryGetComponent(out Piece movingPiece))
        {
            Debug.Log("Invalid from index in ProtectsCheck");
            return false;
        }
        byte movingPieceCode = movingPiece.pieceCode;
        //int kingIndexPreMove = (colour == Colour.White) ? Board.whiteKingIndex : Board.blackKingIndex;
        //movingPiece.Move(posTo.x, posTo.y, false, false, posFrom.x, posFrom.y, false, false);
        theoryPosition[to] = movingPieceCode;
        theoryPosition[from] = Piece.None;
        int kingIndex = Board.FindPiece((byte)(Piece.King | colourCode), theoryPosition);
        if (kingIndex == -1)
        {
            Debug.LogError($"King not found in theory position, code: ({(byte)(Piece.King | colourCode)})");
            Debug.Log("Theory Board: ");
            Board.PrintBoard(theoryPosition);
        }
        protects = !IsAttacked(kingIndex, colour, theoryPosition);
        //movingPiece.Move(posFrom.x, posFrom.y, false, false, posTo.x, posTo.y, false, false);
        return protects;
    }
}
