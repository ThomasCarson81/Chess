using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Pawn : Piece
{
    void Start()
    {
        type = PieceType.PAWN;
        material = 1;
    }

}
