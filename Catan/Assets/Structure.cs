using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;

public class Structure : NetworkBehaviour
{
    private Vector3? sentPosition;
    private Collider lastInteractedCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    protected int structIndex;
    protected List<ResourceType> resourceTypes = new List<ResourceType>();
    InputActionCam inputActions;
    private Player player;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsOwner) return;
        player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
        SetColorServerRpc(player.GetColor());
        inputActions = new InputActionCam();
        inputActions.Enable();
        inputActions.Piece.Follow.performed += Follow;
        inputActions.Piece.DisableFollowing.performed += Place;
    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public List<ResourceType> GetResourceType()
    {
        return resourceTypes;
    }

    public int GetStructIndex()
    {
        return structIndex;
    }

    [ServerRpc(RequireOwnership =false)]
    public void SetColorServerRpc(FixedString32Bytes color)
    {
        SetColorClientRpc(color);
    }

    [ClientRpc()]
    public void SetColorClientRpc(FixedString32Bytes colorname)
    {
        Color color;
        if(ColorUtility.TryParseHtmlString(colorname.Value, out color)) spriteRenderer.color = color;
    }

    public void SetSentPosition(Vector3? sentPosition, Collider collider)
    {
        this.sentPosition = sentPosition;
        this.lastInteractedCollider=collider;
    }

    private void Follow(InputAction.CallbackContext callbackContext)
    {
        Vector2 screenPos = callbackContext.ReadValue<Vector2>();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane));
        worldPos.z = 0;
        transform.position = worldPos;
    }

    public void Place(InputAction.CallbackContext context)
    {
        if (sentPosition == null)
        {
            DestroyServerRpc();
        }
        else
        {
            CheckForPlacementServerRpc(lastInteractedCollider.gameObject.transform.position.x, lastInteractedCollider.gameObject.transform.position.y);
        }
        inputActions.Disable();
    }

    [ServerRpc(RequireOwnership =false)]
    public void DestroyServerRpc()
    {
        gameObject.transform.GetComponent<NetworkObject>().Despawn(true);
        Destroy(gameObject);
    }

    [ServerRpc(RequireOwnership =false)]
    public void CheckForPlacementServerRpc(float x, float y, ServerRpcParams serverRpcParams = default)
    {
        if(structIndex == 1){
            Vector2 key = new Vector2(x, y);
            StructCollider collider = MainLogic.GetSColl()[key];

            foreach(Collider neighbour in collider.GetNeighbours())
            {
                if (neighbour.IsOccupied())
                {
                    PlaceClientRpc(false, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){serverRpcParams.Receive.SenderClientId}}});
                    return;
                }
            }
            if (MainLogic.IsFirstSecondTurnOver())
            {
                foreach(RoadCollider neighbourRoad in collider.GetRoadNeighbours())
                {
                    if (neighbourRoad.IsOccupied() && neighbourRoad.GetOccupier().OwnerClientId == this.OwnerClientId)
                    {
                        PlaceClientRpc(true, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){serverRpcParams.Receive.SenderClientId}}});
                        MainLogic.GetSColl()[key].SetIsOccupied(true);
                        MainLogic.GetSColl()[key].SetOccupier(this);
                        return;
                    }
                }
                PlaceClientRpc(false, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){serverRpcParams.Receive.SenderClientId}}});
            }else{
                PlaceClientRpc(true, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){serverRpcParams.Receive.SenderClientId}}});
                MainLogic.GetSColl()[key].SetIsOccupied(true);
                MainLogic.GetSColl()[key].SetOccupier(this);
            }
            return;
        }
        else if(structIndex == 2)
        {
            Vector2 key = new Vector2(x, y);
            RoadCollider collider = MainLogic.GetRColl()[key];
            foreach(StructCollider structNeigbour in collider.GetStructColliders())
            {
                if (structNeigbour.IsOccupied() && structNeigbour.GetOccupier().OwnerClientId == this.OwnerClientId)
                {
                    PlaceClientRpc(true, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){serverRpcParams.Receive.SenderClientId}}});
                    MainLogic.GetRColl()[key].SetIsOccupied(true);
                    MainLogic.GetRColl()[key].SetOccupier(this);
                    return;
                }
            }
            foreach(RoadCollider neighbour in collider.GetNeighbours())
            {
                if (neighbour.IsOccupied() && neighbour.GetOccupier().OwnerClientId == this.OwnerClientId)
                {
                    StructCollider commonneighbour = collider.GetStructColliders().Intersect(neighbour.GetStructColliders()).First();
                    if(commonneighbour.IsOccupied() && commonneighbour.GetOccupier().OwnerClientId != this.OwnerClientId) break;
                    
                    PlaceClientRpc(true, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){serverRpcParams.Receive.SenderClientId}}});
                    MainLogic.GetRColl()[key].SetIsOccupied(true);
                    MainLogic.GetRColl()[key].SetOccupier(this);
                    return;
                }
            }
            PlaceClientRpc(false, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){serverRpcParams.Receive.SenderClientId}}});
            return;
        }
        else if(structIndex == 3)
        {
            Vector2 key = new Vector2(x, y);
            StructCollider collider = MainLogic.GetSColl()[key];
            if(collider.IsOccupied() && collider.GetOccupier() is Village && collider.GetOccupier().OwnerClientId == this.OwnerClientId)
            {
                PlaceClientRpc(true, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){serverRpcParams.Receive.SenderClientId}}});
                return;
            }
            PlaceClientRpc(false, new ClientRpcParams{Send = new ClientRpcSendParams{TargetClientIds = new List<ulong>(){serverRpcParams.Receive.SenderClientId}}});
        }
    }

    [ClientRpc()]
    public void PlaceClientRpc(bool state, ClientRpcParams clientRpcParams)
    {
        if (state)
        {
            gameObject.transform.position = sentPosition.Value;
            if(structIndex == 1)
            {
                player.AddStructure("Village", -1);
                player.AddPoints(1);
            }
            else if (structIndex == 2)
            {
                RoadCollider roadCollider = (RoadCollider) lastInteractedCollider;
                if(roadCollider.GetTileSide()==0) gameObject.transform.rotation = Quaternion.Euler(0, 0, 30);
                else if(roadCollider.GetTileSide()==1) gameObject.transform.rotation = Quaternion.Euler(0, 0, 90);
                else gameObject.transform.rotation = Quaternion.Euler(0, 0, -30);
                player.AddStructure("Road", -1);
            }
            else
            {
                player.AddStructure("Town", -1);
                player.AddPoints(1);
            }
            /*else
            {
                StructCollider structCollider = (StructCollider) lastInteractedCollider;
                foreach(int i in structCollider.GetTileNumbers())
                {
                    if (MainLogic.GetTileNumber_Structure().ContainsKey(i))
                    {
                        MainLogic.GetTileNumber_Structure()[i].Add(this);
                    }
                    else
                    {
                        MainLogic.GetTileNumber_Structure().Add(i, new List<Structure>(){this});
                    }
                }
                resourceTypes = structCollider.GetResourceType();
            }*/
        }
        else
        {
            DestroyServerRpc();
        }
    }
}
