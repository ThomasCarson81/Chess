using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.Purchasing;

public sealed class Board
{
    public static string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public static byte[] square = new byte[64];
    public static List<GameObject> pieceObjs = new();
    public static int blackMaterial = 0;
    public static int whiteMaterial = 0;
    public static Colour turn = Colour.White;
    public static List<GameObject> moveDots = new();
    public static int whiteKingIndex, blackKingIndex;

    public Board()
    {
        square = PositionFromFEN(startFEN);
        for (int i = 0; i < square.Length; i++)
        {
            if (Utility.IsNonePiece(square[i])) continue;
            pieceObjs.Add(InstantiatePiece(i, square[i]));
        }
    }
    public static GameObject InstantiatePiece(int boardIndex, byte pieceCode)
    {
        //if (Utility.IsNonePiece(square[boardIndex])) return null;
        if (BoardManager.Instance == null) Debug.LogError("BoardManager.Instance = null");
        if (BoardManager.Instance.piecePrefab == null) Debug.LogError("BoardManager.Instance.piecePrefab = null");
        Vector3 pos = Utility.BoardIndexToWorldPos(boardIndex);
        GameObject piece = Object.Instantiate(BoardManager.Instance.piecePrefab, pos, Quaternion.identity);
        Piece script = piece.GetComponent<Piece>();
        script.pieceCode = pieceCode;
        script.colour = (Utility.ColourCode(pieceCode) == Piece.White) ? Colour.White : Colour.Black;
        script.boardIndex = boardIndex;
        if (script.IsPiece(Piece.King))
        {
            if (script.IsColour(Piece.White))
                whiteKingIndex = boardIndex;
            else
                blackKingIndex = boardIndex;
        }
        return piece;
    }
    public static void AddMaterial(int material, Colour colour)
    {
        switch (colour)
        {
            case Colour.Black:
                blackMaterial += material;
                break;
            case Colour.White: 
                whiteMaterial += material;
                break;
        }
        BoardManager.Instance.scoreText.text = $"White: {whiteMaterial}\nBlack: {blackMaterial}";
    }
    byte[] PositionFromFEN(string fen)
    {
        //string resStr = string.Empty;
        byte[] result = new byte[64];
        string[] splitFEN = fen.Split("/");
        splitFEN[^1] = splitFEN[^1].Split(" ")[0];
        int index = 63; // start at the top of the board
        byte colour;
        byte pieceType;
        int skips;
        int whiteKings = 0, blackKings = 0;
        foreach (string rank in splitFEN)
        {
            foreach (char c in rank.Reverse()) // indices decrease right to left, so flip the rank
            {
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
                    //resStr += "skip ";
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
                if (colour == Piece.White && pieceType == Piece.King)
                    whiteKings++;
                else if (colour == Piece.Black && pieceType == Piece.King)
                    blackKings++;
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
                //resStr += s + " ";
                index--;
            }
        }
        if (whiteKings != 1 || blackKings != 1)
            Debug.LogError("Please use exactly 1 of each colour king");
        //Debug.Log($"FEN:\n{resStr}");
        //Debug.Log($"len(FEN)={result.Length}");
        return result;
    }
    public static void ChangeTurn()
    {
        turn = (turn == Colour.White) ? Colour.Black : Colour.White;
        string turnStr = (turn == Colour.White) ? "White" : "Black";
        BoardManager.Instance.turnText.text = $"{turnStr} to move";
    }
    public static byte PieceCodeAtIndex(int index)
    {
        foreach (GameObject obj in pieceObjs)
        {
            if (obj == null)
            {
                Debug.Log("pieceObjs contains null");
                continue;
            }
            if (!obj.TryGetComponent<Piece>(out var pc))
            {
                Debug.Log("how tf does a piece not have a piece script??");
                break;
            }
            if (pc.boardIndex == index)
            {
                return pc.pieceCode;
            }
        }
        return 0;
    }
    public static void RenderMoveDots(List<int> moves)
    {
        UnRenderMoveDots();
        foreach (int i in moves)
        {
            if (i < 0 || i >= 64) continue;
            GameObject dot = Object.Instantiate(BoardManager.Instance.moveDotPrefab);
            dot.transform.position = Utility.BoardIndexToWorldPos(i);
            moveDots.Add(dot);
        }
    }
    public static void UnRenderMoveDots()
    {
        foreach (GameObject gameObject in moveDots)
        {
            Object.Destroy(gameObject);
        }
        moveDots.Clear();
    }
}
public enum Colour
{
    Black = 0,
    White = 1
}
