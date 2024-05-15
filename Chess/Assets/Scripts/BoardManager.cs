using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }
    public Board board;
    public GameObject piecePrefab;
    public GameObject moveDotPrefab;
    public Sprite[] pawnSprites;
    public Sprite[] rookSprites;
    public Sprite[] knightSprites;
    public Sprite[] bishopSprites;
    public Sprite[] queenSprites;
    public Sprite[] kingSprites;
    public Sprite errorSprite;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI turnText;
    public GameObject enPassentPiece;
    public int enPassantIndex = -1;
    public AudioSource moveSound;
    public AudioSource captureSound;
    public AudioSource checkSound;
    public AudioSource castleSound;
    public AudioSource promoteSound;
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
