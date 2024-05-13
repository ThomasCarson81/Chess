using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public static class Utility
{
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
    public static bool IsNonePiece(byte pieceCode)
    {
        return pieceCode == 0 || IsPiece(pieceCode, Piece.EnPassant);
    }
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
    public static bool IsColour(byte pieceCode, Colour colour)
    {
        // returns true if the input pieceCode is a NonePiece
        byte byteColour = (colour == Colour.White) ? Piece.White : Piece.Black;
        return IsColour(pieceCode, byteColour);
    }
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
    public static bool IsPiece(byte currentCode, byte questionCode)
    {
        /* Explanation
         * 00001011 (a white knight which hasn't moved) Bitwise AND'd with
         * 00000011 (Knight code) =
         * 00000011
         * they are equal, so it is true
         */
        return (currentCode & questionCode) == questionCode;
    }
    public static Vector3 BoardIndexToWorldPos(int boardIndex)
    {
        // Note: this is only for **squares**, not the pieces on those squares.
        string notation = BoardIndexToNotation(boardIndex);
        return NotationToWorldPos(notation);
    }
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
    public static int WorldPosToBoardIndex(float x, float y)
    {
        return NotationToBoardIndex(WorldPosToNotation(x, y));
    }
    public static int NotationToBoardIndex(string sqr)
    {
        // sqr must be length 2, with the 1st being a char and the 2nd an int 
        if (sqr.Length != 2 || !char.IsLetter(sqr[0]) || !char.IsDigit(sqr[1])) return -1;
        int file = sqr[0] - 'a'; // a = 0, b = 1, c = 2, etc
        int rank = sqr[1] - '0' - 1; // converts from char to int, but subtracts 1 because arrays a 0-based indexed
        return file + (8 * rank);
    }
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
}
