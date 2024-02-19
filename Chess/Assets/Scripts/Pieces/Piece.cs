using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Piece : MonoBehaviour
{
    [NonSerialized] public PieceType type;
    public PlayerColour player;
}

public enum PieceType
{
    PAWN,
    ROOK,
    KNIGHT,
    BISHOP,
    QUEEN,
    KING
}
public enum PlayerColour
{
    BLACK,
    WHITE
}
