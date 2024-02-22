using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class Board
{
    public static string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public static byte[] square = new byte[64];
    public static List<GameObject> pieceObjs = new();
    public Board()
    {
        square = PositionFromFEN(startFEN);
        GameObject obj;
        for (int i = 0; i < square.Length; i++)
        {
            obj = InstantiatePiece(i);
            if (obj != null) pieceObjs.Add(obj);
        }
    }
    public static GameObject InstantiatePiece(int boardIndex)
    {
        if (Utility.IsNonePiece(square[boardIndex])) return null;
        Vector3 pos = Utility.BoardIndexToWorldPos(boardIndex);
        if (BoardManager.Instance == null)
        {
            Debug.LogError("BoardManager.Instance = null");
        }
        if (BoardManager.Instance.piecePrefab == null)
        {
            Debug.LogError("BoardManager.Instance.piecePrefab = null");
        }
        GameObject piece = Object.Instantiate(BoardManager.Instance.piecePrefab, pos, Quaternion.identity);
        Piece script = piece.GetComponent<Piece>();
        script.pieceCode = square[boardIndex];
        return piece;
    }
    byte[] PositionFromFEN(string fen)
    {
        string resStr = string.Empty;
        byte[] result = new byte[64];
        string[] splitFEN = fen.Split("/");
        splitFEN[^1] = splitFEN[^1].Split(" ")[0];
        int index = 63; // start at the top of the board
        byte colour;
        byte pieceType;
        int skips;
        foreach (string rank in splitFEN)
        {
            foreach (char c in rank.Reverse()) // indices decrease right to left, so flip the rank
            {
                //while (skips >= 0)
                //{
                //    result[index] = Piece.None; // place no piece at this position
                //    index--;
                //    skips--;
                //}
                if (char.IsDigit(c))
                {
                    //Debug.Log($"Skipping {c - '0'} times");
                    skips = c - '0'; // converts char to int
                    while (skips > 0)
                    {
                        result[index] = Piece.None;
                        skips--;
                        index--;
                    }
                    resStr += "skip ";
                    continue;
                }
                colour = char.IsUpper(c) ? Piece.White : Piece.Black; // UPPER = White, Lower = black
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
                string s = char.ToLower(c) switch
                {
                    'p' => "Pawn",
                    'k' => "King",
                    'n' => "Knight",
                    'b' => "Bishop",
                    'r' => "Rook",
                    'q' => "Queen",
                    _ => "Err: " + c.ToString() // illegal character in FEN, just add no piece
                };
                // place a piece with the specified colour at this position
                result[index] = (byte)(pieceType | colour); 
                resStr += s + " ";
                index--;
            }
        }
        //Debug.Log($"FEN:\n{resStr}");
        //Debug.Log($"len(FEN)={result.Length}");
        return result;
    }
}
