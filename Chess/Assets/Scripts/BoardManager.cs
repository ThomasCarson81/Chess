using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }
    public Board board;
    public GameObject piecePrefab;
    public GameObject moveDotPrefab;
    public GameObject highlightPrefab;
    public Button queenButton;
    public Button knightButton;
    public Button rookButton;
    public Button bishopButton;
    public Image queenImg;
    public Image knightImg;
    public Image rookImg;
    public Image bishopImg;
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
    public GameObject highlight;
    public int enPassantIndex = -1;
    public AudioSource moveSound;
    public AudioSource captureSound;
    public AudioSource checkSound;
    public AudioSource castleSound;
    public AudioSource promoteSound;
    public byte chosenPiece = Piece.None;
    public Piece promotingPiece = null;
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
        HideButtons();
    }
    public void OnChooseQueen()
    {
        chosenPiece = Piece.Queen;
        Promote();
    }
    public void OnChooseKnight()
    {
        chosenPiece = Piece.Knight;
        Promote();
    }
    public void OnChooseRook()
    {
        chosenPiece = Piece.Rook;
        Promote();
    }
    public void OnChooseBishop()
    {
        chosenPiece = Piece.Bishop;
        Promote();
    }
    public void DisplayButtons()
    {
        if (promotingPiece == null)
        {
            Debug.Log("Promoting piece is null!");
            return;
        }
        if (Utility.ColourCode(promotingPiece.pieceCode) == Piece.White)
        {
            queenImg.sprite = queenSprites[0];
            knightImg.sprite = knightSprites[0];
            rookImg.sprite = rookSprites[0];
            bishopImg.sprite = bishopSprites[0];
        }
        else
        {
            queenImg.sprite = queenSprites[1];
            knightImg.sprite = knightSprites[1];
            rookImg.sprite = rookSprites[1];
            bishopImg.sprite = bishopSprites[1];
        }
        queenImg.enabled = true;
        queenButton.enabled = true;
        knightImg.enabled = true;
        knightButton.enabled = true;
        rookImg.enabled = true;
        rookButton.enabled = true;
        bishopImg.enabled = true;
        bishopButton.enabled = true;
    }
    public void HideButtons()
    {
        queenImg.enabled = false;
        queenButton.enabled = false;
        knightImg.enabled = false;
        knightButton.enabled = false;
        rookImg.enabled = false;
        rookButton.enabled = false;
        bishopImg.enabled = false;
        bishopButton.enabled = false;
    }
    public void Promote()
    {
        HideButtons();
        if (promotingPiece == null)
        {
            Debug.LogError("Promoting Piece is null!");
            chosenPiece = Piece.None;
            return;
        }
        promotingPiece.pieceCode &= 248;
        promotingPiece.pieceCode |= chosenPiece;
        promotingPiece.sr.sprite = promotingPiece.GetSpriteFromPieceCode(Utility.RemoveMetadata(promotingPiece.pieceCode));
        promoteSound.Play();
    }
}
