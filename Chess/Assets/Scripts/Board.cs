using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class Board
{
    public static string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public static byte[] square;
    public static List<GameObject> pieceObjs = new();
    public Board()
    {
        string boardStr = string.Empty;
        square = new byte[64];
        for (int i = 0; i < square.Length; i++)
        {
            square[i] = 0;
        }
        square = PositionFromFEN(startFEN);
        foreach (byte p in square)
        {
            boardStr += p + " ";
        }
        Debug.Log(boardStr);
        GameObject obj;
        for (int i = 0; i < square.Length; i++)
        {
            obj = InstantiatePiece(square[i]);
            if (obj != null) pieceObjs.Add(obj);
        }
    }
    public static GameObject InstantiatePiece(int boardIndex)
    {
        if (Utility.IsNonePiece(square[boardIndex])) return null;
        Vector3 pos = (Vector3)Utility.WorldPosFromBoardIndex(boardIndex);
        if (BoardManager.Instance == null)
        {
            Debug.LogError("BoardManager.Instance = null");
        }
        if (BoardManager.Instance.piecePrefab == null)
        {
            Debug.LogError("BoardManager.Instance.piecePrefab = null");
        }
        GameObject piece = Object.Instantiate(BoardManager.Instance.piecePrefab, pos, Quaternion.identity);
        Piece pieceScript = piece.AddComponent<Piece>();
        pieceScript.pieceCode = square[boardIndex];
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
        int skips = -1;
        foreach (string rank in splitFEN.Reverse())
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
                    resStr += skips.ToString() + "S ";
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
                // place a piece with the specified colour at this position
                result[index] = (byte)(pieceType + colour); 
                resStr += result[index] + " ";
                index--;
            }
        }
        Debug.Log($"FEN:\n{resStr}");
        Debug.Log($"len(FEN)={result.Length}");
        return result;
    }
}
