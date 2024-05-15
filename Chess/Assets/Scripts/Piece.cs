using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour
{
    #region PIECE_CODES
    /* Bit pattern for byte piece format
     *   0          0           1       01      100 
     *   ^    |     ^       |   ^   |   ^^   |  ^^^
     * unused |  pickedUp   | moved |  White | Bishop
     */

    public const byte None = 0;
    public const byte King = 1;
    public const byte Pawn = 2;
    public const byte Knight = 3;
    public const byte Bishop = 4;
    public const byte Rook = 5;
    public const byte Queen = 6;
    public const byte EnPassant = 7;

    public const byte White = 8;
    public const byte Black = 16;

    public const byte HasMoved = 32;
    public const byte PickedUp = 64;
    #endregion

    public byte pieceCode;
    public SpriteRenderer sr;
    float prevX = 0;
    float prevY = 0;
    public Colour colour;
    public int boardIndex;
    public List<int> legalMoves = new();
    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = GetSpriteFromPieceCode(Utility.RemoveMetadata(pieceCode));
    }
    private void Update()
    {
        if (IsPickedUp())
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos.z = -1;
            transform.position = pos;
        }
    }
    public Sprite GetSpriteFromPieceCode(byte pieceCode)
    {
        return pieceCode switch
        {
            9 => BoardManager.Instance.kingSprites[0],
            17 => BoardManager.Instance.kingSprites[1],
            10 => BoardManager.Instance.pawnSprites[0],
            18 => BoardManager.Instance.pawnSprites[1],
            11 => BoardManager.Instance.knightSprites[0],
            19 => BoardManager.Instance.knightSprites[1],
            12 => BoardManager.Instance.bishopSprites[0],
            20 => BoardManager.Instance.bishopSprites[1],
            13 => BoardManager.Instance.rookSprites[0],
            21 => BoardManager.Instance.rookSprites[1],
            14 => BoardManager.Instance.queenSprites[0],
            22 => BoardManager.Instance.queenSprites[1],
            15 => null, // white en passent
            23 => null, // black en passent
            _ => BoardManager.Instance.errorSprite // in case of an error, show a red dot
        };
    }

    /// <summary>
    /// Invokes Utility.IsColour() on this piece
    /// </summary>
    /// <param name="colour">The colour <b>code</b> to be checked against</param>
    /// <returns>True if the piece is the same colour, otherwise false</returns>
    public bool IsColour(byte colour)
    {
        return Utility.IsColour(pieceCode, colour);
    }

    /// <summary>
    /// Invokes Utility.HasMoved() on this piece
    /// </summary>
    /// <returns>True if the piece has moved, otherwise false</returns>
    public bool HasPieceMoved()
    {
        return Utility.HasMoved(pieceCode);
    }

    /// <summary>
    /// Invokes Utility.IsPiece() on this piece
    /// </summary>
    /// <param name="pieceCode">The code of the piece to be checked for</param>
    /// <returns>True if the piece is a match, otherwise false</returns>
    public bool IsPiece(byte pieceCode)
    {
        return Utility.IsPiece(this.pieceCode, pieceCode);
    }

    /// <summary>
    /// Invokes Utility.IsPickedUp on this piece
    /// </summary>
    /// <returns>True if the piece is picked up, otherwise false</returns>
    public bool IsPickedUp()
    {
        return Utility.IsPickedUp(pieceCode);
    }

    /// <summary>
    /// Move this piece in both world space and in the Board.square array
    /// </summary>
    /// <param name="x">The x position in world space to be moved to</param>
    /// <param name="y">The y position in world space to be moved to</param>
    /// <param name="updateHasMoved">Whether or not to update the 'HasMoved' property of the piece</param>
    /// <param name="updatePickedUp">Whether or not to update the 'PickedUp' property of the piece</param>
    /// <param name="prevX">The previous x position in world space of the piece</param>
    /// <param name="prevY">The previous y position in world space of the piece</param>
    /// <param name="doEnPassant">Whether or not to account for En Passant in the move</param>
    /// <param name="doPrint">Whether or not to Debug.Log() the board position after the move</param>
    /// <returns>Whether or not sounds should be played, this will be false if a sound was played during Move()</returns>
    public bool Move(float x, float y, bool updateHasMoved, bool updatePickedUp, float prevX, float prevY, bool doEnPassant, bool doPrint)
    {
        bool playSound = true;
        transform.position = new Vector3(x, y, 0);
        if (updatePickedUp)
            pieceCode ^= (byte)(pieceCode & PickedUp);
        Board.square[boardIndex] = 0;
        boardIndex = Utility.WorldPosToBoardIndex(x, y);
        Board.square[boardIndex] = pieceCode;
        if (IsPiece(King))
        {
            if (x - prevX == 2)
            {
                // short castled
                Utility.PieceObjectAtWorldPos(x + 1, y).GetComponent<Piece>().Move(x - 1, y, true, false, x + 1, y, true, false);
                BoardManager.Instance.castleSound.Play();
                playSound = false;
            }
            else if (x - prevX == -2)
            {
                // long castled
                Utility.PieceObjectAtWorldPos(x - 2, y).GetComponent<Piece>().Move(x + 1, y, true, false, x - 2, y, true, false);
                BoardManager.Instance.castleSound.Play();
                playSound = false;
            }
            if (IsColour(White))
                Board.whiteKingIndex = boardIndex;
            else
                Board.blackKingIndex = boardIndex;
        }
        if ((x != prevX || y != prevY) && doEnPassant) // if it actually moved and En Passant is accounted for
        {
            if (BoardManager.Instance.enPassentPiece != null)
            {
                Board.pieceObjs.Remove(BoardManager.Instance.enPassentPiece);
                Destroy(BoardManager.Instance.enPassentPiece);
                BoardManager.Instance.enPassentPiece = null;
                if (BoardManager.Instance.enPassantIndex != -1)
                {
                    if (Utility.IsPiece(Board.square[BoardManager.Instance.enPassantIndex], EnPassant))
                    {
                        Board.square[BoardManager.Instance.enPassantIndex] = None;
                    }
                    BoardManager.Instance.enPassantIndex = -1;
                }
            }
        }
        if (updateHasMoved)
            pieceCode |= HasMoved;
        // if this piece isn't a pawn or doEnPassant is false, none of the following code matters
        if (!IsPiece(Pawn) || !doEnPassant)
        {
            if (doPrint)
                Board.PrintBoard(Board.square);
            return playSound;
        }
        // if the move was a double square move
        if (Mathf.Abs(y-prevY) >= 2)
        {
            float epY = IsColour(White) ? prevY + 1 : prevY - 1;
            BoardManager.Instance.enPassantIndex = Utility.WorldPosToBoardIndex(x, epY);
            byte epCode = (byte)(Utility.ColourCode(pieceCode) | EnPassant);
            BoardManager.Instance.enPassentPiece = Board.InstantiatePiece(
                BoardManager.Instance.enPassantIndex,
                epCode
            );
            Board.square[BoardManager.Instance.enPassantIndex] = epCode;
            BoardManager.Instance.enPassentPiece.name = "En Passent";
            Board.pieceObjs.Add(BoardManager.Instance.enPassentPiece);
        }
        else if (boardIndex > 55 ||  boardIndex < 8)
        {
            BoardManager.Instance.promotingPiece = this;
            BoardManager.Instance.DisplayButtons();
        }
        if (doPrint)
            Board.PrintBoard(Board.square);
        return playSound;
    }

    /// <summary>
    /// Invokes the corresponding move calculator from MoveSets.cs and filters out unsafe moves
    /// </summary>
    /// <returns>A List of all the possible moves</returns>
    public List<int> CalculateMoves()
    {
        List<int> movesPreFilter = Utility.TypeCode(pieceCode) switch
        {
            Pawn => MoveSets.CalculatePawnMoves(boardIndex, colour, HasPieceMoved()),
            Knight => MoveSets.CalculateKnightMoves(boardIndex, colour),
            King => MoveSets.CalculateKingMoves(boardIndex, colour, HasPieceMoved()),
            Bishop => MoveSets.CalculateBishopMoves(boardIndex, colour),
            Rook => MoveSets.CalculateRookMoves(boardIndex, colour),
            Queen => MoveSets.CalculateQueenMoves(boardIndex, colour),
            _ => new(),
        };
        List<int> movesPostFilter = new();
        foreach (int move in movesPreFilter)
        {
            if (MoveSets.ProtectsCheck(boardIndex, move, colour))
                movesPostFilter.Add(move);
        }
        return movesPostFilter;
    }

    /// <summary>
    /// Capture a given enemy piece
    /// </summary>
    /// <param name="enemyObj">The GameObject of the piece to be captured</param>
    /// <param name="enemyCode">The piece code of the piece to be captured</param>
    /// <param name="enemyIndex">The board index of the piece to be captured</param>
    /// <param name="x">The x position in Unity world space to move to after the opponent has been captured</param>
    /// <param name="y">The y position in Unity world space to move to after the opponent has been captured</param>
    private void Capture(GameObject enemyObj, byte enemyCode, int enemyIndex, float x, float y)
    {
        if (Utility.IsValidIndex(enemyIndex))
            Board.square[enemyIndex] = None;
        Move(x, y, true, true, prevX, prevY, true, false);
        Board.ChangeTurn();
        Board.AddMaterial(Utility.GetMaterial(enemyCode), colour);
        Board.pieceObjs.Remove(enemyObj);
        Destroy(enemyObj);
        int enemyKingIndex = (colour == Colour.White) ? Board.blackKingIndex : Board.whiteKingIndex;
        Colour enemyColour = (colour == Colour.White) ? Colour.Black : Colour.White;
        if (MoveSets.IsAttacked(enemyKingIndex, enemyColour))
            BoardManager.Instance.checkSound.Play();
        else
            BoardManager.Instance.captureSound.Play();
    }

    /// <summary>
    /// Pick up this piece, causing it to follow the mouse cursor and render dots on possible moves
    /// </summary>
    private void PickUp()
    {
        pieceCode |= PickedUp;
        prevX = transform.position.x;
        prevY = transform.position.y;
        legalMoves = CalculateMoves();
        Board.RenderMoveDots(legalMoves);
    }

    private void OnMouseDown()
    {
        if (Board.turn != colour || !Board.canClick) // it's not your turn, or someone is promoting
            return;
        if (!IsPickedUp())
        {
            PickUp();
            Board.HighlightSquare(boardIndex);
            return;
        }
        float x = Mathf.Round(transform.position.x + 0.5f) - 0.5f;
        float y = Mathf.Round(transform.position.y + 0.5f) - 0.5f;

        // if the target square is the square the piece is on (didn't move)
        if (x == prevX && y == prevY)
        {
            Move(x, y, false, true, prevX, prevY, true, false);
            Board.UnRenderMoveDots();
            Board.RemoveHighlight();
            return;
        }
        if (!legalMoves.Contains(Utility.WorldPosToBoardIndex(x, y)))
            return; // Illegal move
        Board.UnRenderMoveDots();
        Board.RemoveHighlight();
        byte targetSquareCode = Utility.PieceCodeAtWorldPos(x, y);
        
        // if there is a piece of the same colour at the position, don't put the piece down
        if (Utility.IsColour(targetSquareCode, Utility.ColourCode(pieceCode)))
            return;

        if (!Utility.IsNonePiece(targetSquareCode)) // if it's a capture
        { 
            Capture(Utility.PieceObjectAtWorldPos(x, y), targetSquareCode, -1, x, y);
        }
        else if (Utility.IsPiece(targetSquareCode, EnPassant) && IsPiece(Pawn))
        {
            // if it's an En Passant capture
            float enemyY = (colour == Colour.Black) ? y + 1 : y - 1;
            int targetIndex = Utility.WorldPosToBoardIndex(x, enemyY);
            Capture(Utility.PieceObjectAtWorldPos(x, enemyY), targetSquareCode, targetIndex, x, y);
        }
        else
        {
            bool playSounds = Move(x, y, true, true, prevX, prevY, true, false);
            Board.ChangeTurn();
            int enemyKingIndex = (colour == Colour.White) ? Board.blackKingIndex : Board.whiteKingIndex;
            Colour enemyColour = (colour == Colour.White) ? Colour.Black : Colour.White;
            if (playSounds)
            {
                if (MoveSets.IsAttacked(enemyKingIndex, enemyColour))
                    BoardManager.Instance.checkSound.Play();
                else
                    BoardManager.Instance.moveSound.Play();
            }
        }
        
    }
}
