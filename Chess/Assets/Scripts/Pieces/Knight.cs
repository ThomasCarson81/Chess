using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
{
    void Start()
    {
        type = PieceType.KNIGHT;
        material = 3;
    }
}
