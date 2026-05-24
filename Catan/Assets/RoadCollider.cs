using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RoadCollider : Collider
{
    NetworkVariable<int> tileSide = new NetworkVariable<int>();
    HashSet<StructCollider> structNeigbours = new HashSet<StructCollider>();
    void Start()
    {
        base.targetIndex = 6;
    }

    void Update()
    {
        
    }

    public void SetTileSide(int TileSide)
    {
        tileSide.Value = TileSide;
    }
    public int GetTileSide()
    {
        return tileSide.Value;
    }

    public void AddStructNeigbour(StructCollider structCollider)
    {
        structNeigbours.Add(structCollider);
    }
    public HashSet<StructCollider> GetStructColliders()
    {
        return structNeigbours;
    }
}
