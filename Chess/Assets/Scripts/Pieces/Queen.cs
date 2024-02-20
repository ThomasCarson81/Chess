using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Piece
{
    void Start()
    {
        type = PieceType.QUEEN;
        material = 9;
    }
}
