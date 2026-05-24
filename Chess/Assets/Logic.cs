using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Logic : MonoBehaviour
{

    [SerializeField] private GameObject tile;
    [SerializeField] private GameObject piece;

    private List<List<Tile>> tile_list = new List<List<Tile>>();
    private List<Piece> piece_list = new List<Piece>();
    void Awake()
    {
        SetBoard();

    }
    void Start()
    {
        SetPawns();
        SetRooks();
        SetBishop();
        SetQueens();
        SetKnights();
        SetKings();
    }


    void Update()
    {

    }

    public List<List<Tile>> getTileList()
    {
        return tile_list;
    }
    public List<Piece> getPieceList()
    {
        return piece_list;
    }

    private void SetBoard()
    {
        bool white = true;
        int m = 0, n;
        for (float i = 6; i > -7.2f; i -= 1.65f)
        {
            tile_list.Add(new List<Tile>());
            n = 0;
            for (float j = -8; j < 3.48f; j += 1.64f)
            {
                GameObject obj = Instantiate(tile, new Vector3(j, i, 0), tile.transform.rotation);
                Tile tile_ = obj.GetComponent<Tile>();
                tile_.SetCordinate(new Cordinates(m, n));
                tile_list[m].Add(tile_);
                if (white)
                {
                    tile_.SetSprite(Resources.Load<Sprite>("White_tile"));
                    white = false;
                }
                else
                {
                    tile_.SetSprite(Resources.Load<Sprite>("Black_tile"));
                    white = true;
                }
                n++;
            }
            white = !white;
            m++;
        }
    }

    private void SetPawns()
    {
        bool white = false;
        for (int i = 1; i < 8; i += 5)
        {
            for (int j = 0; j < 8; j++)
            {

                GameObject obj = Instantiate(piece.transform.Find("Pawn").gameObject, tile_list[i][j].transform.position, gameObject.transform.rotation);
                Pawn pawn = obj.GetComponent<Pawn>();
                pawn.SetPosition(tile_list[i][j].transform.position);
                pawn.SetCordinates(new Cordinates(i, j));

                if (white)
                {
                    pawn.SetColor(PieceColor.White);
                    pawn.SetSprite(Resources.Load<Sprite>("Pawn"));
                    tile_list[i][j].SetIsOccupied(true);
                    tile_list[i][j].SetOcuppier(pawn);
                }
                else
                {
                    pawn.SetColor(PieceColor.Black);
                    pawn.SetSprite(Resources.Load<Sprite>("Pawn_black"));
                    tile_list[i][j].SetIsOccupied(true);
                    tile_list[i][j].SetOcuppier(pawn);
                }
                piece_list.Add(pawn);
            }
            white = true;
        }
    }

    private void SetRooks()
    {
        List<Cordinates> list = new List<Cordinates>();
        Sprite whitePawn = Resources.Load<Sprite>("Rook_white");
        Sprite blackPawn = Resources.Load<Sprite>("Rook_black");
        list.Add(new Cordinates(0, 0)); list.Add(new Cordinates(7, 0)); list.Add(new Cordinates(0, 7)); list.Add(new Cordinates(7, 7));
        bool white = false;
        foreach (Cordinates cord in list)
        {
            GameObject obj = Instantiate(piece.transform.Find("Rook").gameObject, tile_list[cord.GetX()][cord.GetY()].transform.position, gameObject.transform.rotation);
            Rook rook = obj.GetComponent<Rook>();
            rook.SetPosition(tile_list[cord.GetX()][cord.GetY()].transform.position);
            rook.SetCordinates(cord);
            if (white)
            {
                rook.SetColor(PieceColor.White);
                rook.SetSprite(whitePawn);
                tile_list[cord.GetX()][cord.GetY()].SetIsOccupied(true);
                tile_list[cord.GetX()][cord.GetY()].SetOcuppier(rook);
            }
            else
            {
                rook.SetColor(PieceColor.Black);
                rook.SetSprite(blackPawn);
                tile_list[cord.GetX()][cord.GetY()].SetIsOccupied(true);
                tile_list[cord.GetX()][cord.GetY()].SetOcuppier(rook);
            }
            piece_list.Add(rook);
            white = !white;
        }
    }

    private void SetBishop()
    {
        List<Cordinates> list = new List<Cordinates>();
        Sprite whitePawn = Resources.Load<Sprite>("Bishop_white");
        Sprite blackPawn = Resources.Load<Sprite>("Bishop_black");
        list.Add(new Cordinates(0, 2)); list.Add(new Cordinates(7, 2)); list.Add(new Cordinates(0, 5)); list.Add(new Cordinates(7, 5));
        bool white = false;
        foreach (Cordinates cord in list)
        {
            GameObject obj = Instantiate(piece.transform.Find("Bishop").gameObject, tile_list[cord.GetX()][cord.GetY()].transform.position, gameObject.transform.rotation);
            Bishop bishop = obj.GetComponent<Bishop>();
            bishop.SetPosition(tile_list[cord.GetX()][cord.GetY()].transform.position);
            bishop.SetCordinates(cord);
            if (white)
            {
                bishop.SetColor(PieceColor.White);
                bishop.SetSprite(whitePawn);
                tile_list[cord.GetX()][cord.GetY()].SetIsOccupied(true);
                tile_list[cord.GetX()][cord.GetY()].SetOcuppier(bishop);
            }
            else
            {
                bishop.SetColor(PieceColor.Black);
                bishop.SetSprite(blackPawn);
                tile_list[cord.GetX()][cord.GetY()].SetIsOccupied(true);
                tile_list[cord.GetX()][cord.GetY()].SetOcuppier(bishop);
            }
            piece_list.Add(bishop);
            white = !white;
        }
    }

    private void SetQueens()
    {
        List<Cordinates> list = new List<Cordinates>();
        Sprite whiteQueen = Resources.Load<Sprite>("Queen_white");
        Sprite blackQueen = Resources.Load<Sprite>("Queen_black");

        list.Add(new Cordinates(0, 3)); list.Add(new Cordinates(7, 3));
        bool white = false;
        foreach (Cordinates cord in list)
        {
            GameObject obj = Instantiate(piece.transform.Find("Queen").gameObject, tile_list[cord.GetX()][cord.GetY()].transform.position, gameObject.transform.rotation);
            Queen queen = obj.GetComponent<Queen>();
            queen.SetPosition(tile_list[cord.GetX()][cord.GetY()].transform.position);
            queen.SetCordinates(cord);
            if (white)
            {
                queen.SetColor(PieceColor.White);
                queen.SetSprite(whiteQueen);
                tile_list[cord.GetX()][cord.GetY()].SetIsOccupied(true);
                tile_list[cord.GetX()][cord.GetY()].SetOcuppier(queen);
            }
            else
            {
                queen.SetColor(PieceColor.Black);
                queen.SetSprite(blackQueen);
                tile_list[cord.GetX()][cord.GetY()].SetIsOccupied(true);
                tile_list[cord.GetX()][cord.GetY()].SetOcuppier(queen);
            }
            piece_list.Add(queen);
            white = !white;
        }
    }

    private void SetKnights()
    {
        List<Cordinates> list = new List<Cordinates>();
        Sprite whiteknight = Resources.Load<Sprite>("Knight_white");
        Sprite blackknight = Resources.Load<Sprite>("Knight_black");

        list.Add(new Cordinates(0, 1)); list.Add(new Cordinates(7, 1)); list.Add(new Cordinates(0, 6)); list.Add(new Cordinates(7, 6));
        bool white = false;
        foreach (Cordinates cord in list)
        {
            GameObject obj = Instantiate(piece.transform.Find("Knight").gameObject, tile_list[cord.GetX()][cord.GetY()].transform.position, gameObject.transform.rotation);
            Knight knight = obj.GetComponent<Knight>();
            knight.SetPosition(tile_list[cord.GetX()][cord.GetY()].transform.position);
            knight.SetCordinates(cord);
            if (white)
            {
                knight.SetColor(PieceColor.White);
                knight.SetSprite(whiteknight);
                tile_list[cord.GetX()][cord.GetY()].SetIsOccupied(true);
                tile_list[cord.GetX()][cord.GetY()].SetOcuppier(knight);
            }
            else
            {
                knight.SetColor(PieceColor.Black);
                knight.SetSprite(blackknight);
                tile_list[cord.GetX()][cord.GetY()].SetIsOccupied(true);
                tile_list[cord.GetX()][cord.GetY()].SetOcuppier(knight);
            }
            piece_list.Add(knight);
            white = !white;
        }
    }

    private void SetKings()
    {
        List<Cordinates> list = new List<Cordinates>();
        Sprite whiteQueen = Resources.Load<Sprite>("King_white");
        Sprite blackQueen = Resources.Load<Sprite>("King_black");

        list.Add(new Cordinates(0, 4)); list.Add(new Cordinates(7, 4));
        bool white = false;
        foreach (Cordinates cord in list)
        {
            GameObject obj = Instantiate(piece.transform.Find("King").gameObject, tile_list[cord.GetX()][cord.GetY()].transform.position, gameObject.transform.rotation);
            King king = obj.GetComponent<King>();
            king.SetPosition(tile_list[cord.GetX()][cord.GetY()].transform.position);
            king.SetCordinates(cord);
            if (white)
            {
                king.SetColor(PieceColor.White);
                king.SetSprite(whiteQueen);
                tile_list[cord.GetX()][cord.GetY()].SetIsOccupied(true);
                tile_list[cord.GetX()][cord.GetY()].SetOcuppier(king);
            }
            else
            {
                king.SetColor(PieceColor.Black);
                king.SetSprite(blackQueen);
                tile_list[cord.GetX()][cord.GetY()].SetIsOccupied(true);
                tile_list[cord.GetX()][cord.GetY()].SetOcuppier(king);
            }
            piece_list.Add(king);
            white = !white;
        }
    }

    [ContextMenu("Print")]
    private void PrintAttackers()
    {
        foreach (List<Tile> list in getTileList())
        {
            foreach (Tile tile in list)
            {
                String s = tile.GetCordinates().GetX().ToString() + ", " + tile.GetCordinates().GetY().ToString() + ": ";
                foreach (Piece piece in tile.GetAttacker())
                {
                    s += piece.ToString() + "; ";
                }
                Debug.Log(s);
                s = "";
            }
        }
    }
    [ContextMenu("PrintOccupiers")]
    private void PrintOccupiers()
    {
        foreach (List<Tile> list in getTileList())
        {
            foreach (Tile tile in list)
            {
                String s = tile.GetCordinates().GetX().ToString() + ", " + tile.GetCordinates().GetY().ToString() + ": " + tile.GetOccupier();
                Debug.Log(s);
            }
        }
    }

    [ContextMenu("Print figures")]
    private void PrintFigures()
    {
        foreach (Piece piece in piece_list)
        {
            Debug.Log(piece);
        }
    }
}
