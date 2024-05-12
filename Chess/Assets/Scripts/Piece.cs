using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    SpriteRenderer sr;
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
    Sprite GetSpriteFromPieceCode(byte pieceCode)
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
            15 => null, // BoardManager.Instance.errorSprite, // white en passent
            23 => null, // BoardManager.Instance.errorSprite, // black en passent
            _ => BoardManager.Instance.errorSprite // in case of an error, show a red dot
        };
    }
    public bool IsColour(byte colour)
    {
        return Utility.IsColour(pieceCode, colour);
    }
    public bool HasPieceMoved()
    {
        return Utility.HasMoved(pieceCode);
    }
    public bool IsPiece(byte pieceCode)
    {
        return Utility.IsPiece(this.pieceCode, pieceCode);
    }
    public bool IsPickedUp()
    {
        return Utility.IsPickedUp(pieceCode);
    }
    //public List<int> LegalMoves()
    //{
    //    legalMoves = CalculateMoves();
    //    List<int> moves = new();
    //    foreach (int i in legalMoves)
    //    {
    //        moves.Add(i+boardIndex);
    //    }
    //    return moves;
    //}
    void Move(float x, float y, bool updateHasMoved, float prevX, float prevY)
    {
        transform.position = new Vector3(x, y, 0);
        pieceCode ^= (byte)(pieceCode & PickedUp);
        boardIndex = Utility.WorldPosToBoardIndex(x, y);
        if (x != prevX || y != prevY)
        {
            if (BoardManager.Instance.enPassentPiece != null)
            {
                Board.pieceObjs.Remove(BoardManager.Instance.enPassentPiece);
                Destroy(BoardManager.Instance.enPassentPiece);
            }
                BoardManager.Instance.enPassentPiece = null;
        }
        if (updateHasMoved) pieceCode |= HasMoved;
        if (!IsPiece(Pawn)) return;
        if (Mathf.Abs(y-prevY) >= 2)
        {
            float epX = x;
            float epY = IsColour(White) ? (prevY + 1) : (prevY - 1);
            BoardManager.Instance.enPassentPiece = Board.InstantiatePiece(
                Utility.WorldPosToBoardIndex(epX, epY),
                (byte)(Utility.ColourCode(pieceCode) | EnPassant)
            );
            BoardManager.Instance.enPassentPiece.name = "En Passent";
        }
    }
    List<int> CalculateMoves()
    {
        return Utility.TypeCode(pieceCode) switch
        {
            Pawn => MoveSets.CalculatePawnMoves(boardIndex, colour, HasPieceMoved()),
            Knight => MoveSets.CalculateKnightMoves(boardIndex, colour),
            King => MoveSets.CalculateKingMoves(boardIndex, colour, HasPieceMoved()),
            Bishop => MoveSets.CalculateBishopMoves(boardIndex, colour),
            Rook => MoveSets.CalculateRookMoves(boardIndex, colour),
            Queen => MoveSets.CalculateQueenMoves(boardIndex, colour),
            _ => new(),
        };
    } 
    private void OnMouseDown()
    {
        if (Board.turn != colour) // it's not your turn
            return;
        if (!IsPickedUp())
        {
            pieceCode |= PickedUp; // pick up piece
            prevX = transform.position.x;
            prevY = transform.position.y;
            legalMoves = CalculateMoves();
            Board.RenderMoveDots(legalMoves);
            return;
        }
        float x = (float)Math.Round(transform.position.x + 0.5f) - 0.5f;
        float y = (float)Math.Round(transform.position.y + 0.5f) - 0.5f;

        // if the target square is the square the piece is on
        if (x == prevX && y == prevY)
        {
            Move(x, y, false, prevX, prevY);
            Board.UnRenderMoveDots();
            return;
        }
        if (!legalMoves.Contains(Utility.WorldPosToBoardIndex(x, y)))
            return; // Illegal move
        Board.UnRenderMoveDots();
        byte targetSquareCode = Utility.PieceCodeAtWorldPos(x, y);
            
        // if there is a piece of the same colour at the position, don't put the piece down
        if (Utility.IsColour(targetSquareCode, Utility.ColourCode(pieceCode)))
            return;
        // if it's a capture
        if (!Utility.IsNonePiece(targetSquareCode))
        {
            GameObject enemy = Utility.PieceObjectAtWorldPos(x, y);
            Board.AddMaterial(Utility.GetMaterial(targetSquareCode), colour);
            if (!Board.pieceObjs.Remove(enemy))
                Debug.Log("enemy removal failed");
            Destroy(enemy);
        }
        // if it's an En Passant capture
        else if (Utility.IsPiece(targetSquareCode, EnPassant) && IsPiece(Pawn))
        {
            GameObject enemy;
            if (colour == Colour.Black)
                enemy = Utility.PieceObjectAtWorldPos(x, y + 1);
            else
                enemy = Utility.PieceObjectAtWorldPos(x, y - 1);
            Board.AddMaterial(Utility.GetMaterial(targetSquareCode), colour);
            if (!Board.pieceObjs.Remove(enemy))
                Debug.Log("enemy removal failed");
            Destroy(enemy);
        }

        Move(x, y, true, prevX, prevY);
        Board.ChangeTurn();
    }
}
