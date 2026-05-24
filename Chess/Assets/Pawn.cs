using System;
using UnityEngine;

public class Pawn : Piece
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
        if (this.GetColor() == PieceColor.White)
        {
            if ((this.GetCordinates().GetX() - 1 >= 0) && (!logic.getTileList()[this.GetCordinates().GetX() - 1][this.GetCordinates().GetY()].GetIsOccupied()))
            {
                base.cord_list.Add(new Cordinates(this.GetCordinates().GetX() - 1, this.GetCordinates().GetY()));
            }

            if ((this.GetCordinates().GetX() - 1 >= 0) && (this.GetCordinates().GetY() - 1 >= 0) &&
               (logic.getTileList()[this.GetCordinates().GetX() - 1][this.GetCordinates().GetY() - 1].GetIsOccupied()) &&
               (logic.getTileList()[this.GetCordinates().GetX() - 1][this.GetCordinates().GetY() - 1].GetOccupier().GetColor() == PieceColor.Black))
            {
                base.cord_list.Add(new Cordinates(this.GetCordinates().GetX() - 1, this.GetCordinates().GetY() - 1));
            }

            if ((this.GetCordinates().GetX() - 1 >= 0) && (this.GetCordinates().GetY() + 1 <= 7) &&
               (logic.getTileList()[this.GetCordinates().GetX() - 1][this.GetCordinates().GetY() + 1].GetIsOccupied()) &&
               (logic.getTileList()[this.GetCordinates().GetX() - 1][this.GetCordinates().GetY() + 1].GetOccupier().GetColor() == PieceColor.Black))
            {
                base.cord_list.Add(new Cordinates(this.GetCordinates().GetX() - 1, this.GetCordinates().GetY() + 1));
            }
        }
        else
        {
            if ((this.GetCordinates().GetX() + 1 <= 7) && !logic.getTileList()[this.GetCordinates().GetX() + 1][this.GetCordinates().GetY()].GetIsOccupied())
            {
                base.cord_list.Add(new Cordinates(this.GetCordinates().GetX() + 1, this.GetCordinates().GetY()));
            }

            if ((this.GetCordinates().GetX() + 1 <= 7) && (this.GetCordinates().GetY() - 1 >= 0) &&
               (logic.getTileList()[this.GetCordinates().GetX() + 1][this.GetCordinates().GetY() - 1].GetIsOccupied()) &&
               (logic.getTileList()[this.GetCordinates().GetX() + 1][this.GetCordinates().GetY() - 1].GetOccupier().GetColor() == PieceColor.White))
            {
                base.cord_list.Add(new Cordinates(this.GetCordinates().GetX() + 1, this.GetCordinates().GetY() - 1));
            }

            if ((this.GetCordinates().GetX() + 1 <= 7) && (this.GetCordinates().GetY() + 1 <= 7) &&
               (logic.getTileList()[this.GetCordinates().GetX() + 1][this.GetCordinates().GetY() + 1].GetIsOccupied()) &&
               (logic.getTileList()[this.GetCordinates().GetX() + 1][this.GetCordinates().GetY() + 1].GetOccupier().GetColor() == PieceColor.White))
            {
                base.cord_list.Add(new Cordinates(this.GetCordinates().GetX() + 1, this.GetCordinates().GetY() + 1));
            }
        }
    }

    public override void LoadTileList()
    {
        if (this.GetColor() == PieceColor.White)
        {
            if (this.GetCordinates().GetX() - 1 >= 0 && this.GetCordinates().GetY() - 1 >= 0)
            {
                base.tile_list.Add(new Cordinates(this.GetCordinates().GetX() - 1, this.GetCordinates().GetY() - 1));
            }
            if (this.GetCordinates().GetX() - 1 >= 0 && this.GetCordinates().GetY() + 1 <= 7)
            {
                base.tile_list.Add(new Cordinates(this.GetCordinates().GetX() - 1, this.GetCordinates().GetY() + 1));
            }
        }
        if (this.GetColor() == PieceColor.Black)
        {
            if (this.GetCordinates().GetX() + 1 <= 7 && this.GetCordinates().GetY() - 1 >= 0)
            {
                base.tile_list.Add(new Cordinates(this.GetCordinates().GetX() + 1, this.GetCordinates().GetY() - 1));
            }
            if (this.GetCordinates().GetX() + 1 <= 7 && this.GetCordinates().GetY() + 1 <= 7)
            {
                base.tile_list.Add(new Cordinates(this.GetCordinates().GetX() + 1, this.GetCordinates().GetY() + 1));
            }
        }
    }

    public override String ToString()
    {
        if (this.GetColor() == PieceColor.White)
        {
            return "White Pawn";
        }
        return "Black Pawn";
    }
}
