using UnityEngine;

public class Cordinates
{
    private int x, y;

    public Cordinates(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void SetX(int x)
    {
        if (x < 0 || x > 7)
        {
            return;
        }
        this.x = x;
    }
    public void SetY(int y)
    {
        if (y < 0 || y > 7)
        {
            return;
        }
        this.y = y;
    }
    public int GetX()
    {
        return x;
    }
    public int GetY()
    {
        return y;
    }
}
