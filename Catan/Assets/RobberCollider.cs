using UnityEngine;

public class RobberCollider :Collider
{
    void Start()
    {
        base.targetIndex = 7;   
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == targetIndex)
        {
            Robber robber = collision.gameObject.GetComponent<Robber>();
            robber.SetSentPosition(transform.position);
            robber.SetLastInteractedColl(this);
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.layer == targetIndex)
        {
            Robber robber = collision.gameObject.GetComponent<Robber>();
            robber.SetSentPosition(null);
            robber.SetLastInteractedColl(null);
        }
    }

    public override void Disable(bool oldValue, bool newValue)
    {
        return;
    }
}
