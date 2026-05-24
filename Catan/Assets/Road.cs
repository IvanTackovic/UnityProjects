using UnityEngine;

public class Road : Structure
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        base.structIndex=2;
    }

    void Update()
    {
        
    }
}
