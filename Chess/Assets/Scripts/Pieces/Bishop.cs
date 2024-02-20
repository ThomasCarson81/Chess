using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bishop : Piece
{
    void Start()
    {
        type = PieceType.BISHOP;
        material = 3;
    }
}
