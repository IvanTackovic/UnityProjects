using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Bishop : Piece
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
        int i = this.GetCordinates().GetX();
        int j = this.GetCordinates().GetY();
        MoveOnBoard(i - 1, j - 1, -1, -1, true, base.cord_list);
        MoveOnBoard(i - 1, j + 1, -1, 1, true, base.cord_list);
        MoveOnBoard(i + 1, j - 1, 1, -1, true, base.cord_list);
        MoveOnBoard(i + 1, j + 1, 1, 1, true, base.cord_list);
        return;
    }

    public override void LoadTileList()
    {
        int i = this.GetCordinates().GetX();
        int j = this.GetCordinates().GetY();
        MoveOnBoard(i - 1, j - 1, -1, -1, false, base.tile_list);
        MoveOnBoard(i - 1, j + 1, -1, 1, false, base.tile_list);
        MoveOnBoard(i + 1, j - 1, 1, -1, false, base.tile_list);
        MoveOnBoard(i + 1, j + 1, 1, 1, false, base.tile_list);
    }

    public void MoveOnBoard(int numX, int numY, int diffX, int diffY, bool pom, List<Cordinates> list)
    {
        while (numX >= 0 && numX <= 7 && numY >= 0 && numY <= 7)
        {
            if (logic.getTileList()[numX][numY].GetIsOccupied())
            {
                if (logic.getTileList()[numX][numY].GetOccupier().GetColor() == this.GetColor() && pom)
                {
                    break;
                }
                else
                {
                    list.Add(new Cordinates(numX, numY));
                    break;
                }
            }
            list.Add(new Cordinates(numX, numY));
            numX += diffX;
            numY += diffY;
        }
    }

    public override String ToString()
    {
        if (this.GetColor() == PieceColor.White)
        {
            return "White Bishop";
        }
        return "Black Bishop";
    }
}
