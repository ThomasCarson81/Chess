using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public class Piece : MonoBehaviour
{
    #region PIECE_CODES
    /* Bit pattern for ushort piece format
     * 0000000     0        01     000100 
     * ^^^^^^^ |   ^    |   ^^   | ^^^^^^
     *  unused | moved  |  White | Bishop
     */

    public const ushort None = 0;
    public const ushort King = 1;
    public const ushort Pawn = 2;
    public const ushort Knight = 3;
    public const ushort Bishop = 4;
    public const ushort Rook = 5;
    public const ushort Queen = 6;

    public const ushort White = 8;
    public const ushort Black = 16;

    public const ushort HasMoved = 32;
    #endregion


    
    public ushort pieceCode;


    public bool IsColour(ushort colour)
    {
        /* Explanation
         * 0000000101000100 (a white bishop which has moved) Bitwise AND'd with
         * 0000000001000000 (White code) =
         * 0000000001000000
         * this is > 0, so it is true
         */
        return (pieceCode & colour) > 0;
    }

    public bool IsPiece(ushort pieceCode)
    {
        /* Explanation
         * 0000000 0 10 000011 (a white knight which hasn't moved) Bitwise AND'd with
         * 0000000 0 00 000011 (Knight code) =
         * 0000000 0 00 000011
         * this is > 0, so it is true
         */
        return (this.pieceCode & pieceCode) > 0;
    }

}
