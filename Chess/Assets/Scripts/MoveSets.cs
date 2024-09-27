using System;
using System.Collections;
using System.Collections.Generic;
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

    /// <summary>
    /// Calculates the moves a Pawn can make.<br/>
    /// <b>This does not account for moves that put your King in check, or do not save it from check</b>
    /// </summary>
    /// <param name="currentIndex">The index of the Pawn</param>
    /// <param name="colour">The colour of the Pawn</param>
    /// <param name="hasMoved">Whether or not the Pawn has moved before, so it can move 2 squares if it hasn't</param>
    /// <returns>A List of all the possible moves</returns>
    public static List<int> CalculatePawnMoves(int currentIndex, Colour colour, bool hasMoved)
    {
        Vector3 currPos = Utility.BoardIndexToWorldPos(currentIndex);
        Vector3 newPos;
        List<int> result = new();
        int indexTopLeft = (colour == Colour.White) ? (currentIndex + 7) : (currentIndex - 9);
        
        if (Utility.IsValidIndex(indexTopLeft))
        {
            newPos = Utility.BoardIndexToWorldPos(indexTopLeft);
            if (Mathf.Abs(newPos.x - currPos.x) <= 1)
            {
                byte pieceTopLeft = Board.square[indexTopLeft];
                if ((!Utility.IsNonePiece(pieceTopLeft) || Utility.IsPiece(pieceTopLeft, Piece.EnPassant)) && !Utility.IsColour(pieceTopLeft, colour))
                {
                    result.Add(indexTopLeft);
                }
            }
        }
        int indexTopRight = (colour == Colour.White) ? (currentIndex + 9) : (currentIndex - 7);
        if (Utility.IsValidIndex(indexTopRight))
        {
            newPos = Utility.BoardIndexToWorldPos(indexTopRight);
            if (Mathf.Abs(newPos.x - currPos.x) <= 1)
            {
                byte pieceTopRight = Board.square[indexTopRight];
                if ((!Utility.IsNonePiece(pieceTopRight) || Utility.IsPiece(pieceTopRight, Piece.EnPassant))
                    && !Utility.IsColour(pieceTopRight, colour))
                    result.Add(indexTopRight);
            }
        }
        int index1Forward = (colour == Colour.White) ? (currentIndex + 8) : (currentIndex - 8);
        if (Utility.IsValidIndex(index1Forward))
        {
            newPos = Utility.BoardIndexToWorldPos(index1Forward);
            if (newPos.x == currPos.x )
            {
                byte piece1Forward = Board.square[index1Forward];
                if (!Utility.IsNonePiece(piece1Forward))
                    return result;
                result.Add(index1Forward);
            }
        }
        if (hasMoved)
            return result;
        int index2Forward = (colour == Colour.White) ? (currentIndex + 16) : (currentIndex - 16);
        if (Utility.IsValidIndex(index2Forward))
        {
            newPos = Utility.BoardIndexToWorldPos(index2Forward);
            if (newPos.x == currPos.x)
            {
                byte piece2Forward = Board.square[index2Forward];
                if (Utility.IsNonePiece(piece2Forward))
                    result.Add(index2Forward);
            }
        }
        return result;
    }

    /// <summary>
    /// Calculates the moves a Knight can make.<br/>
    /// <b>This does not account for moves that put your King in check, or do not save it from check</b>
    /// </summary>
    /// <param name="currentIndex">The index of the Knight</param>
    /// <param name="colour">The colour of the Knight</param>
    /// <returns>A List of all the possible moves</returns>
    public static List<int> CalculateKnightMoves(int currentIndex, Colour colour)
    {
        List<int> result = new();
        foreach (int i in Knight)
        {
            if (!Utility.IsValidIndex(currentIndex + i)) continue;
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
            if (Utility.IsNoneOrEnemy(targetCode, colour))
                result.Add(currentIndex + i);
        }
        return result;
    }

    /// <summary>
    /// Calculates the moves a King can make.<br/>
    /// <b>This does not account for moves that put your King in check, or do not save it from check</b>
    /// </summary>
    /// <param name="currentIndex">The index of the King</param>
    /// <param name="colour">The colour of the King</param>
    /// <param name="hasMoved">Will be used for castling</param>
    /// <returns>A List of all the possible moves</returns>
    public static List<int> CalculateKingMoves(int currentIndex, Colour colour, bool hasMoved)
    {
        List<int> result = new();
        if (!hasMoved && !IsAttacked(currentIndex, colour))
        {
            if (currentIndex + 3 < Board.square.Length && 
                Board.square[currentIndex + 1] == Piece.None &&
                !IsAttacked(currentIndex + 1, colour) &&
                Board.square[currentIndex + 2] == Piece.None &&
                !IsAttacked(currentIndex + 2, colour) &&
                Utility.IsPiece(Board.square[currentIndex + 3], Piece.Rook) &&
                !Utility.HasMoved(Board.square[currentIndex + 3]))
            {
                // short castle possible
                result.Add(currentIndex + 2);
            }
            if (currentIndex - 3 > 0 &&
                Board.square[currentIndex - 1] == Piece.None &&
                !IsAttacked(currentIndex - 1, colour) &&
                Board.square[currentIndex - 2] == Piece.None &&
                !IsAttacked(currentIndex - 2, colour) &&
                Board.square[currentIndex - 3] == Piece.None &&
                !IsAttacked(currentIndex - 3, colour) &&
                Utility.IsPiece(Board.square[currentIndex - 4], Piece.Rook) &&
                !Utility.HasMoved(Board.square[currentIndex - 4]))
            {
                // long castle possible
                result.Add(currentIndex - 2);
            }
        }
        foreach (int i in King)
        {
            if (!Utility.IsValidIndex(currentIndex + i))
                continue;
            Vector2 currPos = Utility.BoardIndexToWorldPos(currentIndex);
            Vector2 newPos = Utility.BoardIndexToWorldPos(currentIndex + i);
            if (Mathf.Abs(newPos.x - currPos.x) > 1 || Mathf.Abs(newPos.y - currPos.y) > 1)
                continue; // move wraps
            byte targetCode = Board.square[currentIndex + i];
            if (IsAttacked(currentIndex + i, colour))
                continue; // unable to move into check
            if (Utility.IsNoneOrEnemy(targetCode, colour))
                result.Add(currentIndex + i);
        }
        return result;
    }

    /// <summary>
    /// Calculates the moves a Rook can make.<br/>
    /// <b>This does not account for moves that put your King in check, or do not save it from check</b>
    /// </summary>
    /// <param name="currentIndex">The index of the Rook</param>
    /// <param name="colour">The colour of the Rook</param>
    /// <returns>A List of all the possible moves</returns>
    public static List<int> CalculateRookMoves(int currentIndex, Colour colour)
    {
        List<int> result = new();
        foreach (int[] dir in Rook)
        {
            foreach (int i in dir)
            {
                if (!Utility.IsValidIndex(currentIndex + i)) break; // Dir went off the board
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

    /// <summary>
    /// Calculates the moves a Bishop can make.<br/>
    /// <b>This does not account for moves that put your King in check, or do not save it from check</b>
    /// </summary>
    /// <param name="currentIndex">The index of the Bishop</param>
    /// <param name="colour">The colour of the Bishop</param>
    /// <returns>A List of all the possible moves</returns>
    public static List<int> CalculateBishopMoves(int currentIndex, Colour colour)
    {
        List<int> result = new();
        foreach (int[] dir in Bishop)
        {
            foreach (int i in dir)
            {
                if (!Utility.IsValidIndex(currentIndex + i)) break; // Dir went off the board
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

    /// <summary>
    /// Calculates the moves a Queen can make.<br/>
    /// <b>This does not account for moves that put your King in check, or do not save it from check</b>
    /// </summary>
    /// <param name="currentIndex">The index of the Queen</param>
    /// <param name="colour">The colour of the Queen</param>
    /// <returns>A List of all the possible moves</returns>
    public static List<int> CalculateQueenMoves(int currentIndex, Colour colour)
    {
        List<int> result = CalculateBishopMoves(currentIndex, colour);
        result.AddRange(CalculateRookMoves(currentIndex, colour));
        return result;
    }

    /// <summary>
    /// Checks if a given square is under attack by any enemy piece<br/>
    /// This overload uses the default Board.square position
    /// </summary>
    /// <param name="index">The index of the square to be checked</param>
    /// <param name="colour">The colour of the friendly pieces</param>
    /// <returns>True if the given square is attacked by any enemy pieces, otherwise false</returns>
    public static bool IsAttacked(int index, Colour colour)
    {
        return IsAttacked(index, colour, Board.square);
    }

    /// <summary>
    /// Checks if a given square is under attack by any enemy piece
    /// </summary>
    /// <param name="index">The index of the square to be checked</param>
    /// <param name="colour">The colour of the friendly pieces</param>
    /// <param name="boardPosition">The position of the board, this can be used to check theoretical positions</param>
    /// <returns>True if the given square is attacked by any enemy pieces, otherwise false</returns>
    public static bool IsAttacked(int index, Colour colour, byte[] boardPosition)
    {
        Colour enemyColour = (colour == Colour.White) ? Colour.Black : Colour.White;
        Vector2 currPos = Utility.BoardIndexToWorldPos(index);
        Vector2 newPos;
        #region PAWN_CHECK
        int indexLeft = (colour == Colour.White) ? (index + 7) : (index - 9);
        if (Utility.IsValidIndex(indexLeft))
        {
            byte pieceLeft = boardPosition[indexLeft];
            newPos = Utility.BoardIndexToWorldPos(indexLeft);
            bool valid = true;
            if (Mathf.Abs(currPos.x - newPos.x) != 1 &&  Mathf.Abs(currPos.y - newPos.y) != 1)
                valid = false; 
            if (valid && Utility.IsPiece(pieceLeft, Piece.Pawn) && Utility.IsColour(pieceLeft, enemyColour))
                return true;
        }
        int indexRight = (colour == Colour.White) ? (index + 9) : (index - 7);
        if (Utility.IsValidIndex(indexRight))
        {
            byte pieceRight = boardPosition[indexRight];
            newPos = Utility.BoardIndexToWorldPos(indexRight);
            bool valid = true;
            if (Mathf.Abs(currPos.x - newPos.x) != 1 && Mathf.Abs(newPos.y - newPos.y) != 1)
                valid = false;
            if (valid && Utility.IsPiece(pieceRight, Piece.Pawn) && Utility.IsColour(pieceRight, enemyColour))
                return true;
        }
        #endregion
        #region ROOK_CHECK
        foreach (int[] dir in Rook)
        {
            foreach (int i in dir)
            {
                if (!Utility.IsValidIndex(index + i)) break; // Dir went off the board
                byte targetCode = boardPosition[index + i];
                newPos = Utility.BoardIndexToWorldPos(index + i);
                if (currPos.x != newPos.x && currPos.y != newPos.y)
                    break; // resolves wrapping
                if (Utility.IsNonePiece(targetCode))
                    continue;
                if (Utility.IsColour(targetCode, colour))
                    break; //friendly piece protecting
                if (Utility.IsPiece(targetCode, Piece.Rook) || Utility.IsPiece(targetCode, Piece.Queen))
                    return true;
                break; // finish with this direction
            }
        }
        #endregion
        #region BISHOP_CHECK
        foreach (int[] dir in Bishop)
        {
            foreach (int i in dir)
            {
                if (!Utility.IsValidIndex(index + i)) break; // Dir went off the board
                byte targetCode = boardPosition[index + i];
                newPos = Utility.BoardIndexToWorldPos(index + i);
                if (Mathf.Abs(newPos.x - currPos.x) != Mathf.Abs(newPos.y - currPos.y))
                    break; // resolves wrapping
                if (Utility.IsNonePiece(targetCode))
                    continue;
                if (Utility.IsColour(targetCode, colour))
                    break; //friendly piece protecting
                if (Utility.IsPiece(targetCode, Piece.Bishop) || Utility.IsPiece(targetCode, Piece.Queen))
                    return true;
                break; // finish with this direction
            }
        }
        #endregion
        #region KNIGHT_CHECK
        foreach (int i in Knight)
        {
            if (!Utility.IsValidIndex(index + i)) continue;
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
                return true;
        }
        #endregion
        #region KING_CHECK
        foreach (int i in King)
        {
            if (!Utility.IsValidIndex(index + i))
                continue;
            newPos = Utility.BoardIndexToWorldPos(index + i);
            if (Mathf.Abs(newPos.x - currPos.x) > 1 || Mathf.Abs(newPos.y - currPos.y) > 1)
                continue; // move wraps
            byte targetCode = boardPosition[index + i];
            if (Utility.IsPiece(targetCode, Piece.King) && Utility.IsColour(targetCode, enemyColour))
                return true;
        }
        #endregion
        return false;
    }

    /// <summary>
    /// Checks if a given move will protect your King from check
    /// </summary>
    /// <param name="from">The index of the square the piece is moving from</param>
    /// <param name="to">The index of the square the piece is moving to</param>
    /// <param name="colour">The colour of the moving piece</param>
    /// <returns>True if the move is safe to play, otherwise false (the move does not protect your king, or puts it in check)</returns>
    public static bool ProtectsCheck(int from, int to, Colour colour)
    {
        if (!Utility.IsValidIndex(from))
        {
            Debug.Log("Invalid from index in ProtectsCheck");
            return false;
        }
        if (!Utility.IsValidIndex(to))
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
        theoryPosition[to] = Board.square[from];
        theoryPosition[from] = Piece.None;
        int kingIndex = Board.FindPiece((byte)(Piece.King | colourCode), theoryPosition);
        if (kingIndex == -1)
        {
            Debug.LogError($"King not found in theory position, code: ({(byte)(Piece.King | colourCode)})");
            Debug.Log("Theory Board: ");
            Board.PrintBoard(theoryPosition);
        }
        protects = !IsAttacked(kingIndex, colour, theoryPosition);
        return protects;
    }
}
