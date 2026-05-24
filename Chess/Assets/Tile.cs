using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprit;
    [SerializeField] private BoxCollider2D coll;
    private bool isOccupied, isUnderAttack;
    private Piece occupier;
    private List<Piece> attackers = new List<Piece>();
    private Cordinates cord;

    void Awake()
    {
        SetIsOccupied(false);
        coll.enabled = false;
        sprit.sortingOrder = 0;
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetSprite(Sprite sprite)
    {
        sprit.sprite = sprite;
    }

    public void SetIsOccupied(bool occupation)
    {
        this.isOccupied = occupation;
    }

    public void SetCordinate(Cordinates cord)
    {
        this.cord = cord;
    }
    public void SetOcuppier(Piece occupier)
    {
        this.occupier = occupier;
    }

    public void SetAttacker(Piece attacker, bool add)
    {
        if (add)
        {
            attackers.Add(attacker);
        }
        else
        {
            attackers.Clear();
        }
    }
    public bool GetIsOccupied()
    {
        return isOccupied;
    }

    public bool GetIsUnderAttack()
    {
        if (attackers.Count > 0)
        {
            isUnderAttack = true;
        }
        else
        {
            isUnderAttack = false;
        }
        return isUnderAttack;
    }

    public BoxCollider2D GetColl()
    {
        return coll;
    }

    public Cordinates GetCordinates()
    {
        return cord;
    }

    public Piece GetOccupier()
    {
        return occupier;
    }

    public List<Piece> GetAttacker()
    {
        return attackers;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            Piece piece = (Piece)collision.gameObject.GetComponent<Piece>();
            piece.SetSentPosition(transform.position);
            piece.SetSentCordinates(cord);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 3)
        {
            Piece piece = (Piece)collision.gameObject.GetComponent<Piece>();
            piece.SetSentPosition(null);
            piece.SetSentCordinates(null);
        }
    }

}
