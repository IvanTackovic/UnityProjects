using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Dice : NetworkBehaviour
{
    [SerializeField] private Rigidbody myrigidbody;
    private Button button;
    private bool isrolled = false;
    private float timer=0, timestop=3.5f;
    private MainLogic mainLogic;

    void Awake()
    {
        button = GameObject.FindGameObjectWithTag("RollButton").GetComponent<Button>();
        button.onClick.AddListener(()=>RollDiceServerRpc());
        mainLogic = GameObject.FindGameObjectWithTag("MainLogic").GetComponent<MainLogic>();
    }
    void Start()
    {
        myrigidbody.useGravity=false;
        button.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if(!IsHost) return;
        myrigidbody.AddForce(new Vector3(0, 0, 9.81f), ForceMode.Acceleration);
        if(timer<timestop) timer+=Time.deltaTime;
        else{
            if(isrolled && myrigidbody.linearVelocity.magnitude < 0.01f)
            {
                mainLogic.SetTileNumServerRpc(DiceSide(), gameObject.name, true);
            }
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void RollDiceServerRpc()
    {
        SetActiveClientRpc(true);
        myrigidbody.isKinematic = true;
        if(gameObject.name.Contains("Blue")) gameObject.transform.position = new Vector3(-3, -3, -2);
        else gameObject.transform.position = new Vector3(3, 3, -2);
        StartCoroutine(Roll());
    }

    private IEnumerator Roll()
    {
        yield return new WaitForSecondsRealtime(2);
        myrigidbody.isKinematic=false;
        Vector3 force = new Vector3(UnityEngine.Random.Range(-300, 300), UnityEngine.Random.Range(-300, 300), -UnityEngine.Random.Range(200, 500));
        Vector3 torque = new Vector3(UnityEngine.Random.Range(-250, 250), UnityEngine.Random.Range(-250, 250), UnityEngine.Random.Range(-200, 280));
        myrigidbody.AddForce(force, ForceMode.Acceleration);
        myrigidbody.AddTorque(torque, ForceMode.Acceleration);
        isrolled=true;
        timer=0;
        button.enabled=false;
    }

    [ClientRpc()]
    public void SetActiveClientRpc(bool state)
    {
        gameObject.SetActive(state);
    }
    [ServerRpc(RequireOwnership =false)]
    public void SetActiveServerRpc(bool state)
    {
        SetActiveClientRpc(state);
    }
    private int DiceSide()
    {   
        float maxdot = -1f, rez;
        int side=0;
        Vector3[] rotationVectorField = new Vector3[6] {transform.forward, transform.right, -transform.up, transform.up, -transform.right, -transform.forward};
        Vector3 cameraDice = new Vector3(0, 0, -1);
        for(int i=0; i<rotationVectorField.Length; i++)
        {
            rez = Vector3.Dot(cameraDice, rotationVectorField[i]);
            if(rez > maxdot)
            {
                maxdot=rez;
                side=i+1;
            }
        }
        Debug.Log(side);
        isrolled=false;
        timer=0;
        button.enabled=true;
        return side;
    }
}

