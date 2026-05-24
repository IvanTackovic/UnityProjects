using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public class Player : NetworkBehaviour
{
    private Color? color;
    private string color_name;
    private NetworkVariable<FixedString64Bytes> color_online = new NetworkVariable<FixedString64Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private Dictionary<ResourceType, int> resources= new Dictionary<ResourceType, int>();
    private Dictionary<String, int> structures = new Dictionary<string, int>();
    public event Action<int, string> OnStructNumChanged;
    private int TotalPoints=0; 
    private NetworkVariable<FixedString512Bytes> playerName = new NetworkVariable<FixedString512Bytes>("");
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        DontDestroyOnLoad(gameObject);
        color = null;
        color_name = null;
        resources.Add(ResourceType.Wheat, 0);
        resources.Add(ResourceType.Sheep, 0);
        resources.Add(ResourceType.Forest, 0);
        resources.Add(ResourceType.Brick, 0);
        resources.Add(ResourceType.Ore, 0);
        structures.Add("Road", 0);
        structures.Add("Village", 0);
        structures.Add("Town",0);
        SetName();
    }

    public string GetColor()
    {
        return color_name;
    }
    public string GetColorOnline()
    {
        return color_online.Value.ToSafeString();
    }
    public void SetColor(Color color, String color_name)
    {
        this.color = color;
        this.color_name = color_name;
        this.color_online.Value = color_name;
    }

    public void AddResource(ResourceType resourceType, int quantity)
    {
        if(!resources.ContainsKey(resourceType)) return;
        resources[resourceType] += quantity;
        GameObject.FindGameObjectWithTag(resourceType.ToString()).GetComponent<TMP_Text>().text = resources[resourceType].ToString();
    }

    public int GetQuantityOfResource(ResourceType resourceType)
    {
        return resources[resourceType];
    }

    public void AddStructure(String structure, int quan)
    {
        structures[structure] += quan;
        OnStructNumChanged?.Invoke(structures[structure], structure);
    }
    public int GetQuantityOfStruct(String structure)
    {
        return structures[structure];
    }

    public void AddPoints(int point)
    {
        TotalPoints += point;
    }
    public int GetPoints()
    {
        return TotalPoints;
    }

    public void SetName()
    {
        if(IsOwner){
        SetNameServerRpc(PlayerPrefs.GetString("name"));
        }
    }
    [ServerRpc(RequireOwnership =false)]
    public void SetNameServerRpc(FixedString512Bytes name)
    {
        playerName.Value = name;
    }
    public string GetName()
    {
        return playerName.Value.ToSafeString();
    }
    [ContextMenu("Print")]
    public void Print()
    {
        Debug.Log(playerName + " " + color_online.Value);
    }
}