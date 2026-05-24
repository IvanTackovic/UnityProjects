using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Tile : NetworkBehaviour
{
    private NetworkVariable<ResourceType> resourceType = new NetworkVariable<ResourceType>(ResourceType.None, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private NetworkVariable<int> number = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private StructCollider[] structColliders = new StructCollider[6];
    private RoadCollider[] roadColliders = new RoadCollider[6];
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject StructCollider;
    [SerializeField] private GameObject RoadCollider;
    [SerializeField] private GameObject RobberCollider, robberColl;

    private const float sizeOfTile = 2.9f;

    public override void OnNetworkSpawn()
    {
        transform.Find("Number").transform.localPosition = Vector3.zero;
        resourceType.OnValueChanged += SetSprite;
        number.OnValueChanged += SetNumberText;
        if (IsHost)
        {
            CreateStructColl();
            AddStructNeigbours();
            CreateRoadColl();
            CreateRobberCollider();
        }
    }

    void Update()
    {
        
    }

    public void SetresourceType(ResourceType resourceType)
    {
        this.resourceType.Value = resourceType;
    }

    public ResourceType GetResourceType()
    {
        return resourceType.Value;
    }

    public void SetNumber(int number)
    {
        this.number.Value = number;
    }

    private void SetSprite(ResourceType oldValue, ResourceType newValue)
    {
        if(newValue == ResourceType.None) return;
        string sprite_ = newValue.ToString() + "_tile";
        Sprite sprite = Resources.Load<Sprite>(sprite_);
        spriteRenderer.sprite=sprite;
    }

    private void SetNumberText(int oldValue, int newValue)
    {
        transform.Find("Number").GetComponent<TextMeshPro>().text = newValue.ToString();
        /*foreach(StructCollider sc in structColliders)
        {
            sc.AddTileNumber(newValue);
        }*/
    }

    public StructCollider[] GetStructColliders()
    {
        return structColliders;
    }

    private void CreateStructColl()
    {
        Vector2 position = new Vector2();
        float x, y;
        int index=0;
        for(int angle=0; angle < 360; angle += 60)
        {
            x = (float)(gameObject.transform.position.x + sizeOfTile * Mathf.Cos(angle * Mathf.Deg2Rad));
            y = (float)(gameObject.transform.position.y + sizeOfTile * Mathf.Sin(angle * Mathf.Deg2Rad));
            position.x = (float)Math.Round(x, 2);
            position.y = (float)Math.Round(y, 2);
            if (!MainLogic.GetSColl().ContainsKey(position))
            {
                GameObject _gameObject = Instantiate(StructCollider, new Vector3(position.x, position.y, 0), gameObject.transform.rotation);
                Transform transform = _gameObject.transform;
                transform.GetComponent<NetworkObject>().Spawn(true);
                StructCollider sc = _gameObject.GetComponent<StructCollider>();
                MainLogic.GetSColl().Add(position, sc);
                sc.AddResourceType(resourceType.Value);
                //if(number.Value!=0) sc.AddTileNumber(number.Value);
                structColliders[index] = sc;
            }
            else
            {
                StructCollider sc = MainLogic.GetSColl()[position];
                sc.AddResourceType(resourceType.Value);
                //if(number.Value!=0) sc.AddTileNumber(number.Value);
                structColliders[index] = sc;
            }
            index++;
        }
    }

    private void CreateRoadColl()
    {
        Vector2 position = new Vector2();
        float x, y;
        int index=0;
        for(int angle = 30; angle <360; angle += 60)
        {
            x = (float)(gameObject.transform.position.x + sizeOfTile *Mathf.Sqrt(3)* Mathf.Cos(angle * Mathf.Deg2Rad)/2);
            y = (float)(gameObject.transform.position.y + sizeOfTile * Mathf.Sqrt(3)*Mathf.Sin(angle * Mathf.Deg2Rad)/2);
            position.x = (float)Math.Round(x, 1);
            position.y = (float)Math.Round(y, 1);
            if (!MainLogic.GetRColl().ContainsKey(position))
            {
                GameObject _gameObject = Instantiate(RoadCollider, new Vector3(position.x, position.y, 0), gameObject.transform.rotation);
                Transform transform = _gameObject.transform;
                transform.GetComponent<NetworkObject>().Spawn(true);
                RoadCollider rc = _gameObject.GetComponent<RoadCollider>();
                MainLogic.GetRColl().Add(position, rc);
                rc.SetTileSide(index%3);
                structColliders[index].AddRoadNeigbours(rc);
                structColliders[(index+1)%6].AddRoadNeigbours(rc);
                rc.AddStructNeigbour(structColliders[index]);
                rc.AddStructNeigbour(structColliders[(index+1)%6]);
                roadColliders[index] = rc;
            }
            else
            {
                RoadCollider rc = MainLogic.GetRColl()[position];
                rc.SetTileSide(index%3);
                structColliders[index].AddRoadNeigbours(rc);
                structColliders[(index+1)%6].AddRoadNeigbours(rc);
                rc.AddStructNeigbour(structColliders[index]);
                rc.AddStructNeigbour(structColliders[(index+1)%6]);
                roadColliders[index] = rc;
            }
            index++;
        }
    }

    private void CreateRobberCollider()
    {
        GameObject _gameobject = Instantiate(RobberCollider, gameObject.transform.position, gameObject.transform.rotation);
        robberColl = _gameobject;
        Transform transform = _gameobject.transform;
        transform.GetComponent<NetworkObject>().Spawn(true);
    }

    public GameObject GetRobberCollider()
    {
        return robberColl;
    }

    private void AddStructNeigbours()
    {
        for(int i=0; i<structColliders.Length; i++)
        {
            structColliders[i].AddNeighbour(structColliders[(i+1)%6]);
            structColliders[i].AddNeighbour(structColliders[(i+5)%6]);
        }
    }
}
