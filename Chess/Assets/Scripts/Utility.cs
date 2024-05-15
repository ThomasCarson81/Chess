using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Utility
{
    /// <summary>
    /// Convert a piece code to a string representation<br/>
    /// Meanings:<br/>
    /// <b>?</b> - Unknown (likely due to an invalid piece code)<br/>
    /// <b>W</b> - White<br/>
    /// <b>B</b> - Black<br/>
    /// <b>_</b> - None<br/>
    /// <b>K</b> - King<br/>
    /// <b>P</b> - Pawn<br/>
    /// <b>N</b> - Knight<br/>
    /// <b>B</b> - Bishop<br/>
    /// <b>R</b> - Rook<br/>
    /// <b>Q</b> - Queen<br/>
    /// <b>E</b> - En Passant square<br/>
    /// </summary>
    /// <param name="pieceCode">The piece code to be converted</param>
    /// <returns>The string representation of the piece code, e.g. "<b>WP</b>"</returns>
    public static string PieceCodeToString(byte pieceCode)
    {
        string resStr = "";
        resStr += ColourCode(pieceCode) switch
        {
            Piece.White => 'W',
            Piece.Black => 'B',
            Piece.None => '_',
            _ => '?'
        };
        resStr += TypeCode(pieceCode) switch
        {
            Piece.Pawn => 'P',
            Piece.King => 'K',
            Piece.Queen => 'Q',
            Piece.Rook => 'R',
            Piece.Knight => 'N',
            Piece.Bishop => 'B',
            Piece.EnPassant => 'E',
            Piece.None => '_',
            _ => '?'
        };
        return resStr;
    }

    /// <summary>
    /// Removes irrelevant data from a piece code, leaving only the type and colour
    /// </summary>
    /// <param name="pieceCode">The piece code to be used</param>
    /// <returns>A new piece code, containing information only on the piece type and colour</returns>
    public static byte RemoveMetadata(byte pieceCode)
    {
        /* Explanation
         * 00101100 (a white bishop which has moved and is not picked up) Bitwise AND'd with
         * 00011111 (colour section = 31) =
         * 00001111
         * end the metadata is gone
         */
        return (byte)(pieceCode & 31);
    }

    /// <summary>
    /// Get the colour code from a piece code
    /// </summary>
    /// <param name="pieceCode">The piece code to be used</param>
    /// <returns>A new piece code, containing information only on the colour</returns>
    public static byte ColourCode(byte pieceCode)
    {
        /* Explanation
         * 00101100 (a white bishop which has moved and is not picked up) Bitwise AND'd with
         * 00011000 (colour section = 24) =
         * 00001000
         * so it's white
         */
        return (byte)(pieceCode & 24);
    }

    /// <summary>
    /// Get the type code from a piece code
    /// </summary>
    /// <param name="pieceCode">The piece code to be used</param>
    /// <returns>A new piece code, containing information only on the piece type</returns>
    public static byte TypeCode(byte pieceCode)
    {
        /* Explanation
         * 00101100 (a white bishop which has moved and is not picked up) Bitwise AND'd with
         * 00000111 (type section) =
         * 00000100
         * so it's a bishop
         */
        return (byte)(pieceCode & 7);
    }

    /// <summary>
    /// Check if a piece code is None, <b>OR</b> an En Passant square
    /// </summary>
    /// <param name="pieceCode">The piece code to be checked</param>
    /// <returns>True if the piece code is None or En Passant, otherwise false</returns>
    public static bool IsNonePiece(byte pieceCode)
    {
        return pieceCode == 0 || IsPiece(pieceCode, Piece.EnPassant);
    }

    /// <summary>
    /// Check if a piece is picked up by the player
    /// </summary>
    /// <param name="pieceCode">The piece code to be used</param>
    /// <returns>True if the piece is picked up, otherwise false</returns>
    public static bool IsPickedUp(byte pieceCode)
    {
        /* Explanation
        * 01101100 (a white bishop which has moved and is picked up) Bitwise AND'd with
        * 01000000 (PickedUp code) =
        * 01000000
        * this is > 0, so it is true
        */
        return (pieceCode & Piece.PickedUp) > 0;
    }

    /// <summary>
    /// Check if a piece is of a given colour
    /// </summary>
    /// <param name="pieceCode">The piece code to be checked</param>
    /// <param name="colour">The colour <b>code</b> to be checked against</param>
    /// <returns>True if the piece is the same colour, otherwise false</returns>
    public static bool IsColour(byte pieceCode, byte colour)
    {
        /* Explanation
         * 00101100 (a white bishop which has moved and is not picked up) Bitwise AND'd with
         * 00100000 (White code) =
         * 00100000
         * this is > 0, so it is true
         * 
         * this returns false if the input pieceCode is a NonePiece
         */
        return (pieceCode & colour) > 0;
    }

    /// <summary>
    /// Check if a piece is of a given colour
    /// </summary>
    /// <param name="pieceCode">The piece code to be checked</param>
    /// <param name="colour">The colour <b>enum</b> to be checked against</param>
    /// <returns>True if the piece is the same colour, otherwise false</returns>
    public static bool IsColour(byte pieceCode, Colour colour)
    {
        // returns true if the input pieceCode is a NonePiece
        byte byteColour = (colour == Colour.White) ? Piece.White : Piece.Black;
        return IsColour(pieceCode, byteColour);
    }

    /// <summary>
    /// Check if a piece has moved
    /// </summary>
    /// <param name="pieceCode">The piece code to be checked</param>
    /// <returns>True if the piece has moved, otherwise false</returns>
    public static bool HasMoved(byte pieceCode)
    {
        /* Explanation
         * 00101100 (a white bishop which has moved) Bitwise AND'd with
         * 00100000 (HasMoved code) =
         * 00100000
         * this is > 0, so it is true
         */
        return (pieceCode & Piece.HasMoved) > 0;
    }

    /// <summary>
    /// Check if a piece code is a certain piece
    /// </summary>
    /// <param name="pieceCode">The piece code to be checked</param>
    /// <param name="questionCode">The code of the piece to be checked for</param>
    /// <returns>True if the piece is a match, otherwise false</returns>
    public static bool IsPiece(byte pieceCode, byte questionCode)
    {
        /* Explanation
         * 00001011 (a white knight which hasn't moved) Bitwise AND'd with
         * 00000011 (Knight code) =
         * 00000011
         * they are equal, so it is true
         */
        return (pieceCode & 7) == questionCode;
    }

    /// <summary>
    /// Convert a board index to a position in Unity world space
    /// </summary>
    /// <param name="boardIndex">The index of the square</param>
    /// <returns>A Vector3 of the world position</returns>
    public static Vector3 BoardIndexToWorldPos(int boardIndex)
    {
        string notation = BoardIndexToNotation(boardIndex);
        return NotationToWorldPos(notation);
    }

    /// <summary>
    /// Convert a chess notation to a position in Unity world space
    /// </summary>
    /// <param name="notation">The notatiob to be converted, e.g. "e4"</param>
    /// <returns>A Vector3 of the world position</returns>
    public static Vector3 NotationToWorldPos(string notation)
    {
        float x = notation[0] switch
        {
            'a' => -3.5f,
            'b' => -2.5f,
            'c' => -1.5f,
            'd' => -0.5f,
            'e' => 0.5f,
            'f' => 1.5f,
            'g' => 2.5f,
            'h' => 3.5f,
            _ => 0.0f
        };
        float y = notation[1] switch
        {
            '1' => -3.5f,
            '2' => -2.5f,
            '3' => -1.5f,
            '4' => -0.5f,
            '5' => 0.5f,
            '6' => 1.5f,
            '7' => 2.5f,
            '8' => 3.5f,
            _ => 0.0f
        };
        return new(x, y, 0);
    }

    /// <summary>
    /// Convert a position in Unity world space to chess notation<br/>
    /// Note that the position <b>must</b> be in the centre of a square, otherwise the result will be incorrect
    /// </summary>
    /// <param name="x">The x position in world space</param>
    /// <param name="y">The y position in world space</param>
    /// <returns>A string of the chess notation, e.g. "e4", returns "a1" if invalid</returns>
    public static string WorldPosToNotation(float x, float y)
    {
        char file = x switch
        {
            -3.5f => 'a',
            -2.5f => 'b',
            -1.5f => 'c',
            -0.5f => 'd',
            0.5f => 'e',
            1.5f => 'f',
            2.5f => 'g',
            3.5f => 'h',
            _ => 'a'
        };
        char rank = y switch
        {
            -3.5f => '1',
            -2.5f => '2',
            -1.5f => '3',
            -0.5f => '4',
            0.5f => '5',
            1.5f => '6',
            2.5f => '7',
            3.5f => '8',
            _ => '1'
        };
        return $"{file}{rank}";
    }

    /// <summary>
    /// Convert a position in Unity world space to a board index<br/>
    /// Note that the position <b>must</b> be in the centre of a square, otherwise the result will be incorrect
    /// </summary>
    /// <param name="x">The x position in world space</param>
    /// <param name="y">The y position in world space</param>
    /// <returns>The corresponding board index, or -1 if invalid</returns>
    public static int WorldPosToBoardIndex(float x, float y)
    {
        int file = x switch
        {
            -3.5f => 0,
            -2.5f => 1,
            -1.5f => 2,
            -0.5f => 3,
            0.5f => 4,
            1.5f => 5,
            2.5f => 6,
            3.5f => 7,
            _ => 0
        };
        int rank = y switch
        {
            -3.5f => 0,
            -2.5f => 1,
            -1.5f => 2,
            -0.5f => 3,
            0.5f => 4,
            1.5f => 5,
            2.5f => 6,
            3.5f => 7,
            _ => 0
        };
        if ((file == 0 && x != -3.5f) || (rank == 0 && y != -3.5f))
        {
            Debug.LogError("Inapplicable coordinates provided to WorldPosToBoardIndex()");
            return -1;
        }
        return file + 8 * rank;
        //return NotationToBoardIndex(WorldPosToNotation(x, y));
    }

    /// <summary>
    /// Convert a chess notation to a board index<br/>
    /// </summary>
    /// <param name="sqr">A string of the notation, e.g. "e4"</param>
    /// <returns>The corresponding board index, or -1 if invalid</returns>
    public static int NotationToBoardIndex(string sqr)
    {
        // sqr must be length 2, with the 1st being a char and the 2nd an int 
        if (sqr.Length != 2 || !char.IsLetter(sqr[0]) || !char.IsDigit(sqr[1])) return -1;
        int file = sqr[0] - 'a'; // a = 0, b = 1, c = 2, etc
        int rank = sqr[1] - '0' - 1; // converts from char to int, but subtracts 1 because arrays a 0-based indexed
        return file + (8 * rank);
    }

    /// <summary>
    /// Convert a board index to chess notation<br/>
    /// </summary>
    /// <param name="boardIndex">The index of the square</param>
    /// <returns>A string of the chess notation, e.g. "e4", or "" if invalid</returns>
    public static string BoardIndexToNotation(int boardIndex)
    {
        if (boardIndex < 0 || boardIndex > 63)
        {
            Debug.Log("Invalid index provided to BoardIndexToNotation");
            return "";
        }
        int rankInt = boardIndex % 8;
        char rank = (char)(rankInt + 'a');
        char file = (char)((boardIndex - rankInt) / 8 + 1 + '0');
        return $"{rank}{file}";
    }

    /// <summary>
    /// Get the piece code at a given position in Unity world space
    /// </summary>
    /// <param name="x">The x position in world space</param>
    /// <param name="y">The y position in world space</param>
    /// <returns>The piece code at that point, or 0 if not found</returns>
    public static byte PieceCodeAtWorldPos(float x, float y)
    {
        List<Collider2D> colls = new();
        ContactFilter2D cf = new();
        Physics2D.OverlapPoint(new Vector2(x, y), cf, colls);
        foreach (Collider2D coll in colls)
        {
            if (coll.gameObject.TryGetComponent(out Piece pc))
            {
                if (pc.IsPickedUp())
                {
                    continue;
                }
                return pc.pieceCode;
            }
        }
        return 0;
    }

    /// <summary>
    /// Get the piece code at a given board index
    /// </summary>
    /// <param name="index">The index of the square</param>
    /// <returns>The piece code on that square</returns>
    public static byte PieceCodeAtIndex(int index)
    {
        // requires Board.square array to be representative of the board position
        return Board.square[index];
    }

    /// <summary>
    /// Get the GameObject of a piece at a given position in Unity world space<br/>
    /// This ignores pieces which are picked up by the player
    /// </summary>
    /// <param name="x">The x position in world space</param>
    /// <param name="y">The y position in world space</param>
    /// <returns>The GameObject of the piece at that point, or null if not found</returns>
    public static GameObject PieceObjectAtWorldPos(float x, float y)
    {
        List<Collider2D> colls = new();
        ContactFilter2D cf = new();
        Physics2D.OverlapPoint(new Vector2(x, y), cf, colls);
        foreach (Collider2D coll in colls)
        {
            if (coll.gameObject.TryGetComponent(out Piece pc))
            {
                if (pc.IsPickedUp())
                    continue;
                return pc.gameObject;
            }
        }
        return null;
    }

    /// <summary>
    /// Get the material of a given piece code
    /// </summary>
    /// <param name="pieceCode">The piece code to be used</param>
    /// <returns>The material of the corresponding piece</returns>
    public static int GetMaterial(byte pieceCode)
    {
        int material = TypeCode(pieceCode) switch
        {
            2 => 1, // Pawn
            3 => 3, // Knight
            4 => 3, // Bishop
            5 => 5, // Rook
            6 => 9, // Queen
            7 => 1, // En Passent (pawn)
            _ => 0, // King or error
        };
        return material;
    }

    /// <summary>
    /// Checks if the given index is valid to be used on the board array
    /// </summary>
    /// <param name="index">The index to be checked</param>
    /// <returns>True if the index is safe to use, otherwise false</returns>
    public static bool IsValidIndex(int index)
    {
        return index >= 0 && index < 64;
    }

    /// <summary>
    /// Checks if a given piece code is either a None piece, or an enemy to the given colour
    /// </summary>
    /// <param name="piece">The piece code to be checked</param>
    /// <param name="friendlyColour">The colour of the friendly pieces</param>
    /// <returns>True if the piece is None or an enemy to the given colour, otherwise false</returns>
    public static bool IsNoneOrEnemy(byte piece, Colour friendlyColour)
    {
        return Utility.IsNonePiece(piece) || (!Utility.IsColour(piece, friendlyColour) && !Utility.IsPiece(piece, Piece.EnPassant));
    }
}
