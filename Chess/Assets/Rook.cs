using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class Rook : Piece
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
        MoveOnBoard(i + 1, j, 1, true, true, base.cord_list);
        MoveOnBoard(i - 1, j, -1, true, true, base.cord_list);
        MoveOnBoard(i, j + 1, 1, false, true, base.cord_list);
        MoveOnBoard(i, j - 1, -1, false, true, base.cord_list);
    }

    public override void LoadTileList()
    {
        int i = this.GetCordinates().GetX();
        int j = this.GetCordinates().GetY();
        MoveOnBoard(i + 1, j, 1, true, false, base.tile_list);
        MoveOnBoard(i - 1, j, -1, true, false, base.tile_list);
        MoveOnBoard(i, j + 1, 1, false, false, base.tile_list);
        MoveOnBoard(i, j - 1, -1, false, false, base.tile_list);
    }

    public void MoveOnBoard(int numX, int numY, int diff, bool x, bool pom, List<Cordinates> list)
    {
        if (x)
        {

            while ((diff > 0) ? numX <= 7 : numX >= 0)
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
                numX += diff;
            }
        }
        else
        {
            while ((diff > 0) ? numY <= 7 : numY >= 0)
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
                numY += diff;
            }
        }
    }
    
    public override String ToString()
    {
        if (this.GetColor() == PieceColor.White)
        {
            return "White Rook";
        }
        return "Black Rook";
    }
}
