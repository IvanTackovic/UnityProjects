
using System;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public abstract class Piece : MonoBehaviour
{
    private int index;
    private bool isClicked = false;
    private static bool isCheck = false, ismoved = false, isdestroyed=false;
    [SerializeField] private CapsuleCollider2D coll2D;
    private Vector3? SentPosition;
    private static Vector3? oldposition=null;
    private Vector3 position;
    private PieceColor color;
    private static PieceColor colorOnTurn = PieceColor.White;
    private static PieceColor? kingUnderCheck;
    protected Logic logic;
    private Cordinates cordinates, sentCordinates;
    private static Cordinates kingCordW, kingCordB, oldcordinates;
    private static Piece lastMovedPiece, pieceToBeDestroyed;
    protected List<Cordinates> cord_list = new List<Cordinates>();
    protected List<Cordinates> tile_list = new List<Cordinates>();
    void Start()
    {
        SentPosition = null;
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<Logic>();
    }

    void Update()
    {
    }

    public void Follow(InputAction.CallbackContext context)
    {

        if (isClicked)
        {
            Vector2 mouseposition = context.ReadValue<Vector2>();
            Vector3 position = Camera.main.ScreenToWorldPoint(new Vector3(mouseposition.x, mouseposition.y, 10f));
            transform.position = position;
        }
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        Vector3 mouseposition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseposition.z = 0;
        if (context.started && coll2D.bounds.Contains(mouseposition))
        {
            isClicked = true;
            LoadCordList();
            EnableTiles(cord_list, true);
            position = transform.position;
        }
        if (context.canceled)
        {
            isClicked = false;
            if (SentPosition != null)
            {
                if (SentPosition != position)
                {
                    ChangeColorOnTurn();
                    lastMovedPiece = this;
                    oldcordinates = cordinates;
                    oldposition = position;
                    ismoved = true;
                }
                position = SentPosition.Value;
            }
            if (sentCordinates != null)
            {
                logic.getTileList()[cordinates.GetX()][cordinates.GetY()].SetIsOccupied(false);
                logic.getTileList()[cordinates.GetX()][cordinates.GetY()].SetOcuppier(null);
                if (logic.getTileList()[sentCordinates.GetX()][sentCordinates.GetY()].GetIsOccupied())
                {
                    pieceToBeDestroyed = logic.getTileList()[sentCordinates.GetX()][sentCordinates.GetY()].GetOccupier();
                    Debug.Log(pieceToBeDestroyed);
                    SetInactive(pieceToBeDestroyed);
                    isdestroyed = true;
                }
                cordinates = sentCordinates;
            }
            transform.position = position;
            if (this is King)
            {
                if (this.color == PieceColor.White)
                {
                    kingCordW = cordinates;
                }
                else
                {
                    kingCordB = cordinates;
                }
            }

            logic.getTileList()[cordinates.GetX()][cordinates.GetY()].SetOcuppier(this);
            logic.getTileList()[cordinates.GetX()][cordinates.GetY()].SetIsOccupied(true);
            EnableTiles(cord_list, false);

            cord_list.Clear();
            tile_list.Clear();
            if (this is King && this.GetColor() == PieceColor.White)
            {
                SetAttackList();
                Debug.Log(lastMovedPiece);
                if (CheckForCheck())
                {
                    if (isCheck && ismoved)
                    {
                        Undo();
                    }
                    else
                    {
                        isCheck = true;
                        CheckForMate();
                    }
                    Debug.Log("Check");
                }
                else
                {
                    isCheck = false;
                }
                SetAttackList();

                Debug.Log(isdestroyed);
                if (isdestroyed)
                {
                    DestoryPiece(pieceToBeDestroyed);
                    isdestroyed = false;
                    pieceToBeDestroyed = null;
                }
                ismoved = false;
            }
        }
    }

    public void EnableTiles(List<Cordinates> cord, bool enabled)
    {
        foreach (Cordinates cor in cord)
        {
            logic.getTileList()[cor.GetX()][cor.GetY()].GetColl().enabled = enabled;
        }
    }

    public void SetAttacks(List<Cordinates> cord, bool attack)
    {
        foreach (Cordinates cor in cord)
        {
            if (attack)
            {
                logic.getTileList()[cor.GetX()][cor.GetY()].SetAttacker(this, true);
            }
            else
            {
                logic.getTileList()[cor.GetX()][cor.GetY()].SetAttacker(null, false);
            }
        }
    }

    public void SetAttackList()
    {
        logic.getTileList().ForEach(list => list.ForEach(tile => tile.GetAttacker().Clear()));
        foreach (Piece piece in logic.getPieceList())
        {
            piece.LoadTileList();
            piece.SetAttacks(piece.tile_list, true);
            piece.tile_list.Clear();
            piece.cord_list.Clear(); 
        }
    }

    public void DestoryPiece(Piece piece)
    {
        Destroy(piece.gameObject);
        Debug.Log("Destoryed");
    }

    public void SetInactive(Piece piece)
    {
        index = logic.getPieceList().IndexOf(piece);
        logic.getTileList()[piece.cordinates.GetX()][piece.cordinates.GetY()].SetOcuppier(null);
        logic.getTileList()[piece.cordinates.GetX()][piece.cordinates.GetY()].SetIsOccupied(false);
        logic.getPieceList().Remove(piece);
        piece.gameObject.SetActive(false);
        Debug.Log("Disabled");
    }

    public void SetActive(Piece piece)
    {
        logic.getPieceList().Insert(index, piece);
        logic.getTileList()[piece.cordinates.GetX()][piece.cordinates.GetY()].SetOcuppier(piece);
        logic.getTileList()[piece.cordinates.GetX()][piece.cordinates.GetY()].SetIsOccupied(true);
        piece.gameObject.SetActive(true);
        Debug.Log("Activated");
        return;
    }

    public static void ChangeColorOnTurn()
    {
        if (colorOnTurn == PieceColor.White)
        {
            colorOnTurn = PieceColor.Black;
        }
        else
        {
            colorOnTurn = PieceColor.White;
        }
        return;
    }

    public bool CheckForCheck()
    {
        if (kingCordB == null || kingCordW == null || lastMovedPiece==null)
        {
            return false;
        }
        if (lastMovedPiece.color == PieceColor.White)
        {
            if (Check(PieceColor.White, kingCordW))
            {
                Debug.Log("White check");
                kingUnderCheck = PieceColor.White;
                return true;
            }
            if (Check(PieceColor.Black, kingCordB))
            {
                Debug.Log("Black check");
                kingUnderCheck = PieceColor.Black;
                return true;
            }
            kingUnderCheck = null;
            return false;
        }
        else
        {
            if (Check(PieceColor.Black, kingCordB))
            {
                Debug.Log("Black check");
                kingUnderCheck = PieceColor.Black;
                return true;
            }
            if (Check(PieceColor.White, kingCordW))
            {
                Debug.Log("White check");
                kingUnderCheck = PieceColor.White;
                return true;
            }
            kingUnderCheck = null;
            return false;
        }
    }
    public bool Check(PieceColor color, Cordinates cord)
    {
        bool isUndoed = false;
        foreach (Piece piece in logic.getTileList()[cord.GetX()][cord.GetY()].GetAttacker())
        {
            if (piece.color != color)
            {
                if (lastMovedPiece.color == color)
                {
                    Undo();
                    if (isdestroyed)
                    {
                        SetActive(pieceToBeDestroyed);
                        isdestroyed = false;
                    }
                    isUndoed = true;
                    Debug.Log("Undo");
                }
                else
                {
                    return true;
                }
            }
        }
        if (isUndoed)
        {
            SetAttackList();
        }
        return false;
    }    
    public void Undo()
    {
        logic.getTileList()[lastMovedPiece.cordinates.GetX()][lastMovedPiece.cordinates.GetY()].SetIsOccupied(false);
        logic.getTileList()[lastMovedPiece.cordinates.GetX()][lastMovedPiece.cordinates.GetY()].SetOcuppier(null);
        lastMovedPiece.position = oldposition.Value;
        lastMovedPiece.transform.position = oldposition.Value;
        lastMovedPiece.cordinates = oldcordinates;
        logic.getTileList()[lastMovedPiece.cordinates.GetX()][lastMovedPiece.cordinates.GetY()].SetIsOccupied(true);
        logic.getTileList()[lastMovedPiece.cordinates.GetX()][lastMovedPiece.cordinates.GetY()].SetOcuppier(this);
        colorOnTurn = lastMovedPiece.color;
        return;
    }

    public bool CheckForMate()
    {
        Cordinates oldCordinates;
        bool ismate, isoccupied = false;
        int counter = 0;
        Piece Ocupator = null;

        if (kingUnderCheck == PieceColor.White)
        {
            foreach (Piece piece in logic.getPieceList())
            {
                if (piece is King && piece.color == PieceColor.White)
                {
                    piece.LoadCordList();
                    if (piece.cord_list.Count != 0)
                    {
                        piece.cord_list.Clear();
                        return false;
                    }
                }
            }
        }
        else
        {
           foreach (Piece piece in logic.getPieceList())
            {
                if (piece is King && piece.color == PieceColor.Black) 
                {
                    piece.LoadCordList();
                    if (piece.cord_list.Count != 0)
                    {
                        piece.cord_list.Clear();
                        return false;
                    }
                }
            } 
        }
        foreach (Piece piece in logic.getPieceList())
        {
            Debug.Log(piece);
            Debug.Log(counter);
            if (piece.color == kingUnderCheck)
            {
                oldCordinates = piece.cordinates;
                logic.getTileList()[oldCordinates.GetX()][oldCordinates.GetY()].SetIsOccupied(false);
                logic.getTileList()[oldCordinates.GetX()][oldCordinates.GetY()].SetOcuppier(null);
                piece.LoadCordList();
                Debug.Log("Cord list" + piece.cord_list.Count);
                List<Cordinates> list = new List<Cordinates>(piece.cord_list);
                for (int i = 0; i < list.Count; i++)
                {
                    if (logic.getTileList()[list[i].GetX()][list[i].GetY()].GetIsOccupied())
                    {
                        isoccupied = true;
                        Ocupator = logic.getTileList()[list[i].GetX()][list[i].GetY()].GetOccupier();
                    }
                    logic.getTileList()[list[i].GetX()][list[i].GetY()].SetIsOccupied(true);
                    logic.getTileList()[list[i].GetX()][list[i].GetY()].SetOcuppier(piece);
                    SetAttackList();
                    if (kingUnderCheck == PieceColor.White)
                    {
                        ismate = Check(PieceColor.White, kingCordW);
                    }
                    else
                    {
                        ismate = Check(PieceColor.Black, kingCordB);
                    }

                    logic.getTileList()[list[i].GetX()][list[i].GetY()].SetIsOccupied(false);
                    logic.getTileList()[list[i].GetX()][list[i].GetY()].SetOcuppier(null);
                    if (isoccupied)
                    {
                        logic.getTileList()[list[i].GetX()][list[i].GetY()].SetIsOccupied(true);
                        logic.getTileList()[list[i].GetX()][list[i].GetY()].SetOcuppier(Ocupator);
                    }
                    else
                    {
                        logic.getTileList()[list[i].GetX()][list[i].GetY()].SetIsOccupied(false);
                        logic.getTileList()[list[i].GetX()][list[i].GetY()].SetOcuppier(null);
                    }
                    isoccupied = false;
                    Ocupator = null;
                    if (!ismate)
                    {
                        piece.cordinates = oldCordinates;
                        logic.getTileList()[oldCordinates.GetX()][oldCordinates.GetY()].SetIsOccupied(true);
                        logic.getTileList()[oldCordinates.GetX()][oldCordinates.GetY()].SetOcuppier(piece);
                        piece.cord_list.Clear();
                        SetAttackList();
                        return false;
                    }
                }
                piece.cordinates = oldCordinates;
                logic.getTileList()[oldCordinates.GetX()][oldCordinates.GetY()].SetIsOccupied(true);
                logic.getTileList()[oldCordinates.GetX()][oldCordinates.GetY()].SetOcuppier(piece);
                piece.cord_list.Clear();
            }
            counter++;
        }
        SetAttackList();
        Debug.Log("Mate!");
        return true;
    }

    public abstract void LoadCordList();
    public abstract void LoadTileList();

    public void SetSentPosition(Vector3? SentPosition)
    {
        this.SentPosition = SentPosition;
    }
    public void SetPosition(Vector3 position)
    {
        this.position = position;
    }
    public void SetColor(PieceColor color)
    {
        this.color = color;
    }

    public void SetCordinates(Cordinates cord)
    {
        this.cordinates = cord;
    }

    public void SetSentCordinates(Cordinates cord)
    {
        this.sentCordinates = cord;
    }

    public PieceColor GetColor()
    {
        return color;
    }
    public PieceColor GetColorOnTurn()
    {
        return colorOnTurn;
    }
    public Cordinates GetCordinates()
    {
        return cordinates;
    }
    public bool GetIsCheck()
    {
        return isCheck;
    }

}
