using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Town : Structure
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        base.structIndex = 3;
    }

    void Update()
    {
        
    }


}
