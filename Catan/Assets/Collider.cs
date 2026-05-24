using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Collider : NetworkBehaviour
{
    private HashSet<Collider> neighbours = new HashSet<Collider>();
    [SerializeField] private CircleCollider2D coll;
    protected int targetIndex;
    NetworkVariable<bool> isoccupied = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private Structure occupier;
    protected override void OnNetworkPostSpawn()
    {
        base.OnNetworkPostSpawn();
        isoccupied.OnValueChanged += Disable;
    }

    void Update()
    {
        
    }

    public void AddNeighbour(Collider collider)
    {
        neighbours.Add(collider);
    }

    public HashSet<Collider> GetNeighbours(){
        return neighbours;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == targetIndex)
        {
            Structure structure = collision.gameObject.GetComponent<Structure>();
            structure.SetSentPosition(gameObject.transform.position, gameObject.GetComponent<Collider>());
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.layer == targetIndex)
        {
            Structure structure = collision.gameObject.GetComponent<Structure>();
            structure.SetSentPosition(null, null);
        }
    }

    public virtual void Disable(bool oldValue, bool newValue)
    {
        coll.enabled= !newValue;
    } 

    public bool IsOccupied()
    {
        return isoccupied.Value;
    }
    public void SetIsOccupied(bool isoccupied)
    {
        this.isoccupied.Value=isoccupied;
    }
    [ServerRpc(RequireOwnership =false)]
    public void SetIsOccupiedServerRpc(bool isoccupied)
    {
        this.isoccupied.Value = isoccupied;
    }
    public void SetOccupier(Structure structure)
    {
        this.occupier = structure;
    }
    public Structure GetOccupier()
    {
        return occupier;
    }
}
