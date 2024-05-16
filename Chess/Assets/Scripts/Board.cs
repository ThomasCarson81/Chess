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
    public static int blackMaterial = 0;
    public static int whiteMaterial = 0;
    public static Colour turn = Colour.White;
    public static List<GameObject> moveDots = new();
    public static int whiteKingIndex, blackKingIndex;
    public static bool canClick = true;

    public Board()
    {
        square = PositionFromFEN(startFEN);
        for (int i = 0; i < square.Length; i++)
        {
            if (Utility.IsNonePiece(square[i])) continue;
            pieceObjs.Add(InstantiatePiece(i, square[i]));
        }
    }

    /// <summary>
    /// Instantiate a piece GameObject with a script attached
    /// </summary>
    /// <param name="boardIndex">The index at which to place the new piece</param>
    /// <param name="pieceCode">The piece code of the new piece</param>
    /// <returns>The instantiated GameObject</returns>
    public static GameObject InstantiatePiece(int boardIndex, byte pieceCode)
    {
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

    /// <summary>
    /// Add materials points to a player
    /// </summary>
    /// <param name="material">The number of points to add</param>
    /// <param name="colour">The player to add it to</param>
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

    /// <summary>
    /// Get a byte-array of a board position from chess FEN notation
    /// </summary>
    /// <param name="fen">the string of the FEN notation</param>
    /// <returns>The corresponding board position</returns>
    byte[] PositionFromFEN(string fen)
    {
        bool whiteCastleQueenside = false;
        bool blackCastleQueenside = false;
        bool whiteCastleKingside = false;
        bool blackCastleKingside = false;
        byte[] result = new byte[64];
        string[] splitFEN = fen.Split('/'); // "rnbqkbnr", "pppppppp", "8", "8", "8", "8", "PPPPPPPP", "RNBQKBNR w KQkq e3 0 1"
        string turnStr = splitFEN[^1].Split(' ')[1];
        string epLocation = splitFEN[^1].Split(' ')[3];
        string castleAvailability = splitFEN[^1].Split(' ')[2];
        splitFEN[^1] = splitFEN[^1].Split(' ')[0];
        if (char.ToLower(turnStr[0]) == 'b')
            ChangeTurn(); // turn is white by default, so change it if it should be black
        foreach (char c in castleAvailability)
        {
            switch (c)
            {
                case 'K':
                    whiteCastleKingside = true;
                    break;
                case 'Q':
                    whiteCastleQueenside = true;
                    break;
                case 'k':
                    blackCastleQueenside = true;
                    break;
                case 'q':
                    blackCastleKingside = true;
                    break;
                default:
                    Debug.LogError("Invalid FEN in castle availability!");
                    break;
            }
        }
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
                    skips = c - '0'; // converts char to int
                    while (skips > 0)
                    {
                        result[index] = Piece.None;
                        skips--;
                        index--;
                    }
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
                index--;
            }
        }
        if (whiteKings != 1 || blackKings != 1)
            Debug.LogError("Please use exactly 1 of each colour king");
        int epIndex = -1;
        if (epLocation != "-")
            epIndex = Utility.NotationToBoardIndex(epLocation);
        if (Utility.IsValidIndex(epIndex))
        {
            byte epColour = (turn == Colour.White) ? Piece.Black : Piece.White;
            result[epIndex] = (byte)(epColour | Piece.EnPassant);
        }
        if (!whiteCastleQueenside && Utility.IsPiece(result[0], Piece.Rook) && Utility.IsColour(result[0], Piece.White))
            result[0] |= Piece.HasMoved;
        if (!whiteCastleKingside && Utility.IsPiece(result[7], Piece.Rook) && Utility.IsColour(result[7], Piece.White))
            result[7] |= Piece.HasMoved;
        if (!blackCastleQueenside && Utility.IsPiece(result[56], Piece.Rook) && Utility.IsColour(result[56], Piece.Black))
            result[56] |= Piece.HasMoved;
        if (!blackCastleKingside && Utility.IsPiece(result[63], Piece.Rook) && Utility.IsColour(result[63], Piece.Black))
            result[63] |= Piece.HasMoved;
        return result;
    }

    public string GetFEN(byte[] boardPosition)
    {
        string fen = "";

        return fen;
    }

    /// <summary>
    /// Change the turn of the game and update the UI
    /// </summary>
    public static void ChangeTurn()
    {
        turn = (turn == Colour.White) ? Colour.Black : Colour.White;
        string turnStr = (turn == Colour.White) ? "White" : "Black";
        BoardManager.Instance.turnText.text = $"{turnStr} to move";
    }

    /// <summary>
    /// Render dots on the squares provided
    /// </summary>
    /// <param name="moves">A list of board indexes to render dots on</param>
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

    /// <summary>
    /// Hide the dots rendered by RenderMoveDots()
    /// </summary>
    public static void UnRenderMoveDots()
    {
        foreach (GameObject gameObject in moveDots)
        {
            Object.Destroy(gameObject);
        }
        moveDots.Clear();
    }

    /// <summary>
    /// Debug.Log() a given board position in both string representation
    /// </summary>
    /// <param name="boardPosition"></param>
    public static void PrintBoard(byte[] boardPosition)
    {
        string resStr1 = "\n";
        for (int i = 56; i >= 0 ; i++)
        {
            resStr1 += Utility.PieceCodeToString(boardPosition[i]) + " ";
            if (i % 8 == 7)
            {
                i -= 16;
                resStr1 += "\n";
            }
        }
        Debug.Log(resStr1);
    }
    
    /// <summary>
    /// Find the index of the first occurrence of a given piece code in a given position
    /// </summary>
    /// <param name="pieceCode">The piece code to be searched for</param>
    /// <param name="boardPosition">The board position to be searched</param>
    /// <returns>The index of the first occurrence of the piece code</returns>
    public static int FindPiece(byte pieceCode, byte[] boardPosition)
    {
        for (int i = 0; i < boardPosition.Length; i++)
        {
            if (Utility.RemoveMetadata(boardPosition[i]) == Utility.RemoveMetadata(pieceCode))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Highlight (in yellow) the square at the given index
    /// </summary>
    /// <param name="index">The square to highlight</param>
    public static void HighlightSquare(int index)
    {
        if (!Utility.IsValidIndex(index))
        {
            Debug.LogError("Cannot highlight square at invalid index");
            return;
        }
        RemoveHighlight();
        Vector3 pos = Utility.BoardIndexToWorldPos(index);
        pos.z = 0.5f;
        GameObject highlightSquare = Object.Instantiate(BoardManager.Instance.highlightPrefab, pos, Quaternion.identity);
        BoardManager.Instance.highlight = highlightSquare;
    }

    /// <summary>
    /// Remove the yellow highlight made by HighlightSquare()
    /// </summary>
    public static void RemoveHighlight()
    {
        Object.Destroy(BoardManager.Instance.highlight);
    }
    public static bool CheckForMate(Colour colour)
    {
        if (pieceObjs.Count < 3)
        {
            // insufficient material - TODO: add check for knights when checking for insufficient material
            BoardManager.Instance.Stalemate();
            return true;
        }
        for (int i = 0; i < square.Length; i++)
        {
            if (Utility.IsColour(square[i], colour))
            {
                Vector3 pos = Utility.BoardIndexToWorldPos(i);
                if (!Utility.PieceObjectAtWorldPos(pos.x, pos.y).TryGetComponent<Piece>(out var pc))
                    Debug.LogError("Piece with no script detected");
                if (pc.CalculateMoves().Count > 0)
                    return false;
            }
        }
        int kingIndex = (colour == Colour.White) ? whiteKingIndex : blackKingIndex;
        if (MoveSets.IsAttacked(kingIndex, colour))
        {
            Colour loser = (colour == Colour.White) ? Colour.Black : Colour.White;
            BoardManager.Instance.Checkmate(loser);
        }
        else
        {
            BoardManager.Instance.Stalemate();
        }
        return true;
    }

}
public enum Colour
{
    Black = 0,
    White = 1
}
