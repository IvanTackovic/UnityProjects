using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StructCollider : Collider
{
    private List<ResourceType> resourceTypes = new List<ResourceType>();
    private HashSet<RoadCollider> roadNeigbours= new HashSet<RoadCollider>();
    //private NetworkList<int> tileNumbers = new NetworkList<int>();
    void Start()
    {
        base.targetIndex=3;
    }
    void Update()
    {
        
    }

    public void AddResourceType(ResourceType resourceType)
    {
        resourceTypes.Add(resourceType);
    }

    public List<ResourceType> GetResourceType()
    {
        return resourceTypes;
    }


    public void AddRoadNeigbours(RoadCollider roadCollider)
    {
        roadNeigbours.Add(roadCollider);
    }

    public HashSet<RoadCollider> GetRoadNeighbours()
    {
        return roadNeigbours;
    }
}
