using System;
using System.Collections.Generic;
using UnityEngine;

public class Queen : Piece
{
    [SerializeField] SpriteRenderer sprite;
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
        MoveOnBoardRook(i + 1, j, 1, true, true, base.cord_list);
        MoveOnBoardRook(i - 1, j, -1, true, true, base.cord_list);
        MoveOnBoardRook(i, j + 1, 1, false, true, base.cord_list);
        MoveOnBoardRook(i, j - 1, -1, false, true, base.cord_list);
        MoveOnBoardBishop(i - 1, j - 1, -1, -1, true, base.cord_list);
        MoveOnBoardBishop(i - 1, j + 1, -1, 1, true, base.cord_list);
        MoveOnBoardBishop(i + 1, j - 1, 1, -1, true, base.cord_list);
        MoveOnBoardBishop(i + 1, j + 1, 1, 1, true, base.cord_list);
    }

    public override void LoadTileList()
    {
        int i = this.GetCordinates().GetX();
        int j = this.GetCordinates().GetY();
        MoveOnBoardRook(i + 1, j, 1, true, false, base.tile_list);
        MoveOnBoardRook(i - 1, j, -1, true, false, base.tile_list);
        MoveOnBoardRook(i, j + 1, 1, false, false, base.tile_list);
        MoveOnBoardRook(i, j - 1, -1, false, false, base.tile_list);
        MoveOnBoardBishop(i - 1, j - 1, -1, -1, false, base.tile_list);
        MoveOnBoardBishop(i - 1, j + 1, -1, 1, false, base.tile_list);
        MoveOnBoardBishop(i + 1, j - 1, 1, -1, false, base.tile_list);
        MoveOnBoardBishop(i + 1, j + 1, 1, 1, false, base.tile_list);
    }

    private void MoveOnBoardRook(int numX, int numY, int diff, bool x, bool pom, List<Cordinates> list)
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

    private void MoveOnBoardBishop(int numX, int numY, int diffX, int diffY, bool pom, List<Cordinates> list)
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
            return "White Queen";
        }
        return "Black Queen";
    }
}
