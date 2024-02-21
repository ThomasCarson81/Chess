using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class Piece : MonoBehaviour
{
    #region PIECE_CODES
    /* Bit pattern for byte piece format
     *   00       1       01      100 
     *   ^^   |   ^   |   ^^   |  ^^^
     * unused | moved |  White | Bishop
     */

    public const byte None = 0;
    public const byte King = 1;
    public const byte Pawn = 2;
    public const byte Knight = 3;
    public const byte Bishop = 4;
    public const byte Rook = 5;
    public const byte Queen = 6;

    public const byte White = 8;
    public const byte Black = 16;

    public const byte HasMoved = 32;
    #endregion

    public byte pieceCode;
    SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = (Utility.TypeCode(pieceCode) + Utility.ColourCode(pieceCode)) switch
        {
            (King   | White) => BoardManager.Instance.kingSprites   [0],
            (King   | Black) => BoardManager.Instance.kingSprites   [1],
            (Pawn   | White) => BoardManager.Instance.pawnSprites   [0],
            (Pawn   | Black) => BoardManager.Instance.pawnSprites   [1],
            (Knight | White) => BoardManager.Instance.knightSprites [0],
            (Knight | Black) => BoardManager.Instance.knightSprites [1],
            (Bishop | White) => BoardManager.Instance.bishopSprites [0],
            (Bishop | Black) => BoardManager.Instance.bishopSprites [1],
            (Rook   | White) => BoardManager.Instance.rookSprites   [0],
            (Rook   | Black) => BoardManager.Instance.rookSprites   [1],
            (Queen  | White) => BoardManager.Instance.queenSprites  [0],
            (Queen  | Black) => BoardManager.Instance.queenSprites  [1],
            _ => BoardManager.Instance.pawnSprites[0] // in case of an error, show a white pawn
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

}
