using System;
using System.Collections.Generic;
using UnityEngine;

public class Knight : Piece
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

    private void MoveOnBoard()
    {
        int x = this.GetCordinates().GetX();
        int y = this.GetCordinates().GetY();
        for (int i = x - 2; i <= x + 2; i++)
        {
            for (int j = y - 2; j <= y + 2; j++)
            {
                if (i >= 0 && i <= 7 && j >= 0 && j <= 7 && (Math.Abs(i - x) + Math.Abs(j - y) == 3))
                {
                    if (logic.getTileList()[i][j].GetIsOccupied())
                    {
                        if (logic.getTileList()[i][j].GetOccupier().GetColor() != this.GetColor())
                        {
                            base.cord_list.Add(new Cordinates(i, j));
                        }
                    }
                    else
                    {
                        base.cord_list.Add(new Cordinates(i, j));
                    }
                    base.tile_list.Add(new Cordinates(i, j));
                }
            }
        }
    }
    
    public override String ToString()
    {
        if (this.GetColor() == PieceColor.White)
        {
            return "White Knight";
        }
        return "Black Knight";
    }
}
