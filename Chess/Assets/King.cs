using System;
using System.Linq;
using UnityEngine;

public class King : Piece
{
    [SerializeField] private SpriteRenderer sprite;
    void Update()
    {

    }

    public void SetSprite(Sprite sprite)
    {
        this.sprite.sprite = sprite;
        this.sprite.sortingOrder = 1;
    }

    public override void LoadCordList()
    {
        if (base.GetColorOnTurn() != this.GetColor())
        {
            return;
        }
        MoveOnBoard();
    }

    public override void LoadTileList()
    {
        MoveOnBoard();
    }
    public void MoveOnBoard()
    {
        int x = this.GetCordinates().GetX();
        int y = this.GetCordinates().GetY();
        bool pom = false;
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (i >= 0 && i <= 7 && j >= 0 && j <= 7)
                {
                    base.tile_list.Add(new Cordinates(i, j));
                    if (logic.getTileList()[i][j].GetIsOccupied() && logic.getTileList()[i][j].GetOccupier().GetColor() == this.GetColor())
                    {
                        continue;
                    }
                    if (logic.getTileList()[i][j].GetIsUnderAttack())
                    {
                        foreach (Piece piece in logic.getTileList()[i][j].GetAttacker())
                        {
                            if (piece.GetColor() != this.GetColor())
                            {
                                pom = true;
                                break;
                            }
                        }
                        if (pom)
                        {
                            pom = false;
                            continue;
                        }
                    }
                    base.cord_list.Add(new Cordinates(i, j));
                }
            }
        }
    }

    public override String ToString()
    {
        if (this.GetColor() == PieceColor.White)
        {
            return "White King";
        }
        return "Black King";
    }
}
