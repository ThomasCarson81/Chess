using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public sealed class Board
{
    public static byte[] square = new byte[64];
    public static List<GameObject> pieceObjs = new();
    public static Colour turn = Colour.White;
    public static List<GameObject> moveDots = new();
    public static int whiteKingIndex, blackKingIndex;
    public static bool canClick = true;
    public static bool canMove = true;
    public static int halfmoveClock = 0;
    public static int fullmoveNumber = 0;

    public Board()
    {
        if (BoardManager.botMode)
        {
            BoardManager.Instance.botText.enabled = true;
            BoardManager.Instance.playerText.enabled = true;
        }
        else
        {
            BoardManager.Instance.botText.enabled = false;
            BoardManager.Instance.playerText.enabled = false;
        }
        square = new byte[64];
        foreach (GameObject obj in pieceObjs)
        {
            Object.Destroy(obj);
        }
        pieceObjs = new();
        turn = Colour.White;
        moveDots = new();
        canClick = true;
        canMove = true;
        halfmoveClock = 0;
        fullmoveNumber = 0;
        square = PositionFromFEN(BoardManager.startFEN);
        string turnStr = (turn == Colour.White) ? "White" : "Black";
        BoardManager.Instance.turnText.text = $"{turnStr} to move";
        BoardManager.Instance.moveText.text = $"Move:\n{fullmoveNumber}";
        for (int i = 0; i < square.Length; i++)
        {
            if (Utility.IsNonePiece(square[i])) continue;
            pieceObjs.Add(InstantiatePiece(i, square[i]));
        }
        CheckForMate(Colour.White);
        CheckForMate(Colour.Black);
        UpdateMaterial();
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
    public static void UpdateMaterial()
    {
        int dif = GetMaterialDifference(square);
        if (dif == 0)
        {
            BoardManager.Instance.scoreText.text = $"Material:\n- - -";
            return;
        }
        if (dif > 0)
        {
            BoardManager.Instance.scoreText.text = $"Material:\nWhite +{dif}";
            return;
        }
        if (dif < 0)
        {
            BoardManager.Instance.scoreText.text = $"Material:\nBlack +{-dif}";
            return;
        }
    }

    /// <summary>
    /// Get a byte-array of a board position from chess FEN notation
    /// </summary>
    /// <param name="fen">the string of the FEN notation</param>
    /// <returns>The corresponding board position</returns>
    public static byte[] PositionFromFEN(string fen)
    {
        bool whiteCastleQueenside = false;
        bool blackCastleQueenside = false;
        bool whiteCastleKingside = false;
        bool blackCastleKingside = false;
        byte[] result = new byte[64];
        string[] splitFEN = fen.Split('/'); // "rnbqkbnr", "pppppppp", "8", "8", "8", "8", "PPPPPPPP", "RNBQKBNR w KQkq - 0 1"
        string positionInfoStr = splitFEN[^1]; 
        positionInfoStr = positionInfoStr.Replace(positionInfoStr.Split(' ')[0], ""); // " w KQkq - 0 1"
        positionInfoStr =  positionInfoStr.Remove(0, 1); // "w KQkq - 0 1"
        string[] positionInfoArr = positionInfoStr.Split(' '); // "w", "KQkq", "-", "0", "1"
        string turnStr = positionInfoArr[0]; // "w"
        string epLocation = positionInfoArr[2]; // "-" ("-" means none)
        string castleAvailability = positionInfoArr[1]; // "KQkq"
        halfmoveClock = int.TryParse(positionInfoArr[3], out halfmoveClock) ? halfmoveClock : 0;
        fullmoveNumber = int.TryParse(positionInfoArr[4], out fullmoveNumber) ? fullmoveNumber : 1;
        splitFEN[^1] = splitFEN[^1].Split(' ')[0]; // "rnbqkbnr", "pppppppp", "8", "8", "8", "8", "PPPPPPPP", "RNBQKBNR"
        if (char.ToLower(turnStr[0]) == 'b' && turn == Colour.White)
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
                if (pieceType == Piece.None)
                    throw new System.Exception("Illegal character in FEN");
                if (colour == Piece.White && pieceType == Piece.King)
                    whiteKings++;
                else if (colour == Piece.Black && pieceType == Piece.King)
                    blackKings++;
                // place a piece with the specified colour at this position
                result[index] = (byte)(pieceType | colour); 
                index--;
            }
        }
        if (whiteKings != 1 || blackKings != 1)
            throw new System.Exception("FEN must have exactly 1 of each colour king.");
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

    /// <summary>
    /// Get the Forsyth-Edwards Notation (<b>FEN</b>) of a given position
    /// </summary>
    /// <param name="boardPosition">A byte array of the board position</param>
    /// <returns>A string representation of the position using FEN</returns>
    public static string GetFEN(byte[] boardPosition)
    {
        string fen = "";
        int skips = 0;
        for (int i = 56; i >= 0; i++)
        {
            byte piece = boardPosition[i];
            if (Utility.IsNonePiece(piece))
            {
                skips++;
                if (i % 8 == 7)
                {
                    fen += skips.ToString();
                    skips = 0;
                    fen += '/';
                    i -= 16;
                }
                continue;
            }
            if (skips > 0)
            {
                fen += skips.ToString();
                skips = 0;
            }
            char pieceChar = Utility.TypeCode(piece) switch
            {
                Piece.Pawn => 'p',
                Piece.Rook => 'r',
                Piece.Knight => 'n',
                Piece.Bishop => 'b',
                Piece.Queen => 'q',
                Piece.King => 'k',
                _ => '?'
            };
            if (Utility.IsColour(piece, Piece.White))
            {
                pieceChar = char.ToUpper(pieceChar);
            }
            fen += pieceChar;
            if (i % 8 == 7)
            {
                fen += '/';
                i -= 16;
            }
        }
        fen = fen.Remove(fen.Length - 1); // remove trailing '/'
        char moveChar = (turn == Colour.White) ? 'w' : 'b';
        string castleAvailability = "";
        int whiteKingIndex = FindPiece(Piece.King | Piece.White, boardPosition);
        int blackKingIndex = FindPiece(Piece.King | Piece.Black, boardPosition);
        bool wK = true, wQ = true, bK = true, bQ = true; // white & black kingside & queenside castle availability

        // castling only works when the king has not moved
        if (whiteKingIndex != 4 && !Utility.HasMoved(boardPosition[whiteKingIndex]))
        {
            wK = false;
            wQ = false;
        }
        else
        {
            // white queenside castling is only available when
            // there is a white rook on a1 which has not moved
            if (!Utility.IsColour(boardPosition[0], Piece.White) ||
                !Utility.IsPiece(boardPosition[0], Piece.Rook) ||
                Utility.HasMoved(boardPosition[0])
                )
            {
                wQ = false;
            }
            // white kingside castling is only available when
            // there is a white rook on h1 which has not moved
            if (!Utility.IsColour(boardPosition[7], Piece.White) ||
                !Utility.IsPiece(boardPosition[7], Piece.Rook) ||
                Utility.HasMoved(boardPosition[7])
                )
            {
                wK = false;
            }
        }
        // castling only works when the king has not moved
        if (blackKingIndex != 60 && !Utility.HasMoved(boardPosition[whiteKingIndex]))
        {
            bK = false;
            bQ = false;
        }
        else
        {
            // black queenside castling is only available when
            // there is a black rook on a8 which has not moved
            if (!Utility.IsColour(boardPosition[56], Piece.Black) ||
                !Utility.IsPiece(boardPosition[56], Piece.Rook) ||
                Utility.HasMoved(boardPosition[56])
                )
            {
                bQ = false;
            }
            // black kingside castling is only available when
            // there is a black rook on h8 which has not moved
            if (!Utility.IsColour(boardPosition[63], Piece.Black) ||
                !Utility.IsPiece(boardPosition[63], Piece.Rook) ||
                Utility.HasMoved(boardPosition[63])
                )
            {
                bK = false;
            }
        }
        if (wK)
            castleAvailability += 'K';
        if (wQ)
            castleAvailability += 'Q';
        if (bK)
            castleAvailability += 'k';
        if (bQ)
            castleAvailability += 'q';

        fen += ' ';
        fen += moveChar;
        if (castleAvailability == "")
            castleAvailability = "-";
        fen += ' ' + castleAvailability;
        if (BoardManager.Instance.enPassantIndex == -1)
            fen += " -";
        else
            fen += $" {Utility.BoardIndexToNotation(BoardManager.Instance.enPassantIndex)}";
        fen += $" {halfmoveClock} {fullmoveNumber}";
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
        BoardManager.Instance.moveText.text = $"Move:\n{fullmoveNumber}";
        if (BoardManager.botMode && turn == Colour.Black)
        {
            BoardManager.Instance.botMoveStart = Time.time;
        }
        UpdateMaterial();
        Debug.Log($"eval: {Evaluation.EvalBoard(square)}");
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
            Vector3 pos = Utility.BoardIndexToWorldPos(i);
            pos.z = -2;
            dot.transform.position = pos;
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

    /// <summary>
    /// Check for a mate of any kind, including checkmate, stalemate, or a draw of any other kind
    /// </summary>
    /// <param name="colour">The colour of the potentially losing king</param>
    /// <returns>True if the game should end, otherwise false</returns>
    public static bool CheckForMate(Colour colour)
    {
        // Insufficient Material
        if (CheckForInsufficientMaterial(square))
        {
            BoardManager.Instance.Draw(DrawCause.InsufficientMaterial);
            return true;
        }

        // 50 Move Rule
        if (halfmoveClock >= 100) // 100 halfmoves = 50 moves
            BoardManager.Instance.Draw(DrawCause.FiftyMoveRule);

        // Stalemate
        for (int i = 0; i < square.Length; i++)
        {
            if (Utility.IsColour(square[i], colour))
            {
                Vector3 pos = Utility.BoardIndexToWorldPos(i);
                GameObject obj = Utility.PieceObjectAtWorldPos(pos.x, pos.y);
                if (obj == null)
                {
                    return false;
                }
                if (pos == null)
                {
                    Debug.Log("pos = null");
                }
                if (!obj.TryGetComponent<Piece>(out var pc))
                    Debug.LogError("Piece with no script detected");
                if (pc == null)
                {
                    Debug.Log("pc = null");
                }
                if (pc.CalculateMoves().Count > 0)
                    return false;
            }
        }

        // Checkmate
        int kingIndex = (colour == Colour.White) ? whiteKingIndex : blackKingIndex;
        if (MoveSets.IsAttacked(kingIndex, colour))
        {
            Colour loser = (colour == Colour.White) ? Colour.Black : Colour.White;
            BoardManager.Instance.Checkmate(loser);
        }
        else
        {
            BoardManager.Instance.Draw(DrawCause.Stalemate);
        }
        return true;
    }

    public static void BotMove()
    {
        List<Move> moves = Bot.AllMoves(Colour.Black, square);
        for (int i = 0; i < moves.Count; i++)
        {
            byte[] theoryPos = (byte[])square.Clone();
            Bot.MakeTheoryMove(theoryPos, moves[i]);
            int rating = Bot.RateBoardNormalised(theoryPos, Colour.Black, 3, 0, 0);
            moves[i] = new Move(moves[i].GetOldPos(), moves[i].GetNewPos(), rating);
        }
        //foreach (Move mv in moves)
        //{
        //    Debug.Log($"{mv.GetOldPos()} to {mv.GetNewPos()} = {mv.GetRating()}");
        //}
        Move bestMove = Bot.BestMove(moves, Colour.Black); // chooses move with lowest eval (good for black)
        byte[] nextPos = (byte[])square.Clone();
        Bot.MakeTheoryMove(nextPos, bestMove);
        //Evaluation.EvalBoard(nextPos, true);
        int oldPos = bestMove.GetOldPos();
        int newPos = bestMove.GetNewPos();
        Debug.Log($"best move: {bestMove.GetOldPos()} to {bestMove.GetNewPos()}, rating={bestMove.GetRating()}");
        Vector3 oldPiecePos = Utility.BoardIndexToWorldPos(oldPos);
        Vector3 newPiecePos = Utility.BoardIndexToWorldPos(newPos);
        GameObject botObj = Utility.PieceObjectAtWorldPos(oldPiecePos.x, oldPiecePos.y);
        if (botObj == null)
        {
            Debug.Log($"no piece at ({oldPiecePos.x},{oldPiecePos.y})");
        }
        Piece botPiece = botObj.GetComponent<Piece>();
        bool playSound = false;
        bool isEPTake = square[newPos] == Piece.EnPassant
            && Utility.IsPiece(botPiece.pieceCode, Piece.Pawn);
        if (isEPTake)
        {
            GameObject enemyObj = Utility.PieceObjectAtWorldPos(newPiecePos.x, newPiecePos.y);
            int enemyIndex = newPos;
            botPiece.Capture(enemyObj, enemyIndex, newPiecePos.x, newPiecePos.y, square);
        }
        else
        {
            if (square[newPos] == Piece.None)
            {
                playSound = botPiece.Move(newPiecePos.x, newPiecePos.y, true, false, oldPiecePos.x, oldPiecePos.y, true, false, square);
            }
            else
            {
                GameObject enemyObj = Utility.PieceObjectAtWorldPos(newPiecePos.x, newPiecePos.y);
                int enemyIndex = newPos;
                botPiece.Capture(enemyObj, enemyIndex, newPiecePos.x, newPiecePos.y, square);
            }
        }
        fullmoveNumber++;
        BoardManager.Instance.moveText.text = $"Move:\n{fullmoveNumber}";
        if (CheckForMate(Colour.Black))
        {
            Debug.Log("Checkmate! White wins");
            return;
        }
        if (CheckForMate(Colour.White))
        {
            Debug.Log("Checkmate! Black wins");
            return;
        }
        if (CheckForInsufficientMaterial(square))
        {
            Debug.Log("Draw due to insufficient material.");
            return;
        }
        if (playSound)
        {
            BoardManager.Instance.moveSound.Play();
        }
        ChangeTurn();
    }

    /// <summary>
    /// Check if the game should end due to insufficient material
    /// </summary>
    /// <returns>True if the game should end, otherwise false</returns>
    static bool CheckForInsufficientMaterial(byte[] boardPosition)
    {
        int whiteKnights = 0;
        int blackKnights = 0;
        int whiteBishops = 0;
        int blackBishops = 0;
        foreach (byte piece in boardPosition)
        {
            if (Utility.IsNonePiece(piece)) continue;
            switch (Utility.TypeCode(piece))
            {
                case Piece.Pawn:
                    return false;
                case Piece.Rook:
                    return false;
                case Piece.Queen:
                    return false;
            }
            if (Utility.IsColour(piece, Piece.White))
            {
                switch (Utility.TypeCode(piece))
                {
                    case Piece.Knight:
                        whiteKnights++;
                        break;
                    case Piece.Bishop:
                        whiteBishops++;
                        break;
                }
            }
            else
            {
                switch (Utility.TypeCode(piece))
                {
                    case Piece.Knight:
                        blackKnights++;
                        break;
                    case Piece.Bishop:
                        blackBishops++;
                        break;
                }
            }
        }
        if (whiteBishops == 0 && blackBishops == 0)
        {
            // Only knights or lone kings
            if (whiteKnights + blackKnights <= 1)
                return true; // lone kings or knight vs lone king
            if (whiteKnights > 1 && blackKnights > 0)
                return true; // 2 (or more) knights vs lone king
            if (blackKnights > 1 && whiteKnights > 0)
                return true; // 2 (or more) knights vs lone king
            if (whiteKnights == 1 && blackKnights == 1)
                return true; // knight vs knight
        }
        if (blackKnights == 0 && whiteKnights == 0)
        {
            if (whiteBishops + blackBishops <= 1)
                return true;
            if (whiteBishops == 1 && blackBishops == 1)
            {
                int whiteBishopIndex = FindPiece(Piece.Bishop | Piece.White, boardPosition);
                int blackBishopIndex = FindPiece(Piece.Bishop | Piece.Black, boardPosition);
                // if the index of the square and
                // the index of the square integer divided by 8 are both even or both odd,
                // the square is dark, otherwise it is light
                // (thanks to Kevin Cheung for major help making this formula)
                bool whiteBishopIsLight = whiteBishopIndex / 8 % 2 != whiteBishopIndex % 2;
                bool blackBishopIsLight = blackBishopIndex / 8 % 2 != blackBishopIndex % 2;
                
                if (whiteBishopIsLight == blackBishopIsLight)
                    return true;
            }
        }
        return false;
    }

    static int GetMaterial(Colour colour, byte[] boardPosition)
    {
        int material = 0;
        foreach (byte pc in boardPosition)
        {
            if (Utility.IsColour(pc, colour))
            {
                if (!Utility.IsPiece(pc, Piece.EnPassant))
                {
                    material += Utility.GetMaterial(pc);
                }
            }
        }
        return material;
    }

    static int GetMaterialDifference(byte[] boardPosition)
    {
        return GetMaterial(Colour.White, boardPosition) - GetMaterial(Colour.Black, boardPosition);
    }
}
public enum Colour
{
    Black = 0,
    White = 1
}
public struct Move
{
    public int OldPos;
    public int NewPos;
    public int Rating;
    public Move(int oldPos, int newPos)
    {
        OldPos = oldPos;
        NewPos = newPos;
        Rating = 0;
    }
    public Move(int oldPos, int newPos, int rating)
    {
        OldPos = oldPos;
        NewPos = newPos;
        Rating = rating;
    }
    public void SetRating(int rating) { Rating = rating; }
    public void SetOldPos(int oldPos) { OldPos = oldPos; }
    public void SetNewPos(int newPos) { NewPos = newPos; }
    public readonly int GetOldPos() { return OldPos; }
    public readonly int GetNewPos() { return NewPos; }
    public readonly int GetRating() { return Rating; }
}
