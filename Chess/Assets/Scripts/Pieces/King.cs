using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    void Start()
    {
        type = PieceType.BISHOP;
        material = int.MaxValue;
    }
}
