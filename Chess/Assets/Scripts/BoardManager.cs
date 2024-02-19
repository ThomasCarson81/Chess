using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager instance { get; private set; }
    public static Piece[] pieces;
    public static string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }
    void Start()
    {
        SetupBoard();
    }

    void SetupBoard()
    {
        pieces = GetPositionFromFEN(startFEN);
        // put them on the screen and stuff ig
    }

    Piece[] GetPositionFromFEN(string fen)
    {
        Piece[] result = new Piece[64];
        // do stuff
        return result;
    }
}
