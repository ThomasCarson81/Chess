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
        sr.sprite = GetSpriteFromPieceCode(Utility.RemoveMetadata(pieceCode));
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
}
