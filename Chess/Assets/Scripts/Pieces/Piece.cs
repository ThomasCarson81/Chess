using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Piece : MonoBehaviour
{
    [NonSerialized] public PieceType type;
    [NonSerialized] public int material;
    [SerializeField] bool canMove;
    public PlayerColour player;
    SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }
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
