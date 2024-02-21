using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Purchasing;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }
    public Sprite[] pawnSprites;
    public Sprite[] rookSprites;
    public Sprite[] knightSprites;
    public Sprite[] bishopSprites;
    public Sprite[] queenSprites;
    public Sprite[] kingSprites;

    //GameObject PieceAt(int x, int y)
    //{
    //    Vector3 pos = new(x - 4.5f, y - 4.5f, 0);
    //    foreach (GameObject piece in pieces)
    //    {
    //        if (piece.transform.position == pos)
    //        {
    //            return piece;
    //        }
    //    }
    //    return null;
    //}
}
