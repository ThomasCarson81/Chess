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
    public static List<GameObject> pieces = new();
    public static string startFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
    public Sprite[] pawnSprites;
    public Sprite[] rookSprites;
    public Sprite[] knightSprites;
    public Sprite[] bishopSprites;
    public Sprite[] queenSprites;
    public Sprite[] kingSprites;
    [SerializeField] GameObject pawnPrefab;
    [SerializeField] GameObject rookPrefab;
    [SerializeField] GameObject knightPrefab;
    [SerializeField] GameObject bishopPrefab;
    [SerializeField] GameObject queenPrefab;
    [SerializeField] GameObject kingPrefab;
    void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        SetupBoard();
    }

    void SetupBoard()
    {
        GetPositionFromFEN(startFEN, ref pieces);
        // put them on the screen and stuff ig
    }

    void ModifyPieceComponent(GameObject piece, PlayerColour colour, PieceType type, int x, int y)
    {
        int index;
        if (colour == PlayerColour.WHITE)
        {
            index = 0;
        }
        else if (colour == PlayerColour.BLACK)
        {
            index = 1;
        }
        else
        {
             index = 2;
        }
        piece.GetComponent<SpriteRenderer>().sprite = type switch
        {
            PieceType.PAWN => pawnSprites[index],
            PieceType.ROOK => rookSprites[index],
            PieceType.KING => kingSprites[index],
            PieceType.BISHOP => bishopSprites[index],
            PieceType.QUEEN => queenSprites[index],
            PieceType.KNIGHT => knightSprites[index],
            _ => pawnSprites[index],
        };
        piece.GetComponent<Piece>().player = colour;
        piece.transform.position = new Vector3(x-4.5f, y-4.5f, 0);
    }

    void GetPositionFromFEN(string fen, ref List<GameObject> result)
    {
        string[] splitFEN = fen.Split("/");
        splitFEN[^1] = splitFEN[^1].Split(" ")[0];
        int x;
        int y = 9;
        char pieceChar;
        PlayerColour colour;
        PieceType pieceType;
        foreach (string rank in splitFEN)
        {
            y--;
            x = 0;
            foreach (char c in rank)
            {
                x++;
                if (char.IsUpper(c))
                {
                    colour = PlayerColour.WHITE;
                }
                else
                {
                    colour = PlayerColour.BLACK;
                }
                pieceChar = char.ToLower(c);
                GameObject obj;
                
                switch (pieceChar)
                {
                    case 'p':
                        obj = Instantiate(pawnPrefab);
                        pieceType = PieceType.PAWN;
                        break;
                    case 'r':
                        obj = Instantiate(rookPrefab);
                        pieceType = PieceType.ROOK;
                        break;
                    case 'n':
                        obj = Instantiate(knightPrefab);
                        pieceType = PieceType.KNIGHT;
                        break;
                    case 'b':
                        obj = Instantiate(bishopPrefab);
                        pieceType = PieceType.BISHOP;
                        break;
                    case 'q':
                        obj = Instantiate(queenPrefab);
                        pieceType = PieceType.QUEEN;
                        break;
                    case 'k':
                        obj = Instantiate(kingPrefab);
                        pieceType = PieceType.KING;
                        break;
                    default:
                        obj = null;
                        pieceType = PieceType.PAWN;
                        break;
                }
                if (obj != null)
                {
                    result.Add(obj);
                    ModifyPieceComponent(obj, colour, pieceType, x, y);
                }
            }
            
        }
    }
}
