using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }
    public Board board;
    public GameObject piecePrefab;
    public Sprite[] pawnSprites;
    public Sprite[] rookSprites;
    public Sprite[] knightSprites;
    public Sprite[] bishopSprites;
    public Sprite[] queenSprites;
    public Sprite[] kingSprites;
    public Sprite errorSprite;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        board = new();
    }
}
