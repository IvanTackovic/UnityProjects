using System.Collections.Generic;
using UnityEngine;

public class Village : Structure
{

    private  HashSet<int> tileNumbers = new HashSet<int>();
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        base.structIndex = 1;
    }

    void Update()
    {
        
    }
}
