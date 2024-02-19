using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pawn : Piece
{
    [SerializeField] bool hasMoved;
    [SerializeField] bool canMove;
    [SerializeField] Sprite[] sprites;

    SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        type = PieceType.PAWN;
        if (player == PlayerColour.WHITE)
        {
            sr.sprite = sprites[0];
        }
        else
        {
            sr.sprite = sprites[1];
        }
    }

}
