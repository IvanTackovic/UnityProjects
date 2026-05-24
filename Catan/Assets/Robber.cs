using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Robber : NetworkBehaviour
{
    [SerializeField] private BoxCollider2D coll;
    private MainLogic mainLogic;
    InputActionCam inputActions;
    NetworkVariable<bool> canBeMoved = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private Vector3 oldPosition;
    private Vector3? sentPosition;
    private RobberCollider lastInteractedColl = null;
    private RobberCollider lastPlacedColl=null;

    void Start()
    {
        mainLogic = GameObject.FindGameObjectWithTag("MainLogic").GetComponent<MainLogic>();
        inputActions = new InputActionCam();
        inputActions.Enable();
        inputActions.Piece.DisableFollowing.started += PickUp;
        inputActions.Piece.Follow.performed += Follow;
        inputActions.Piece.DisableFollowing.canceled += Drop;

        inputActions.Piece.Follow.Disable();
        oldPosition = transform.position;
    }

    private void PickUp(InputAction.CallbackContext callbackContext)
    {
        if(!canBeMoved.Value || mainLogic.GetPersonOnTurn() != (int)NetworkManager.LocalClient.ClientId) return;
        sentPosition = null;
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mousePosition.z = 0;
        if (coll.bounds.Contains(mousePosition)) {
            inputActions.Piece.Follow.Enable();
            Camera cam = Camera.main;
            CameraScript cameraScript = cam.GetComponent<CameraScript>();
            cameraScript.DisableMovement();
        }
    }

    private void Follow(InputAction.CallbackContext callbackContext)
    {
        Vector2 screenPos = callbackContext.ReadValue<Vector2>();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane));
        worldPos.z = 0;
        ChangePosServerRpc(worldPos);
    }

    [ServerRpc(RequireOwnership =false)]
    private void ChangePosServerRpc(Vector3 pos)
    {
        transform.position = pos;
    }

    private void Drop(InputAction.CallbackContext callbackContext)
    {
        if(!canBeMoved.Value || mainLogic.GetPersonOnTurn() != (int)NetworkManager.LocalClient.ClientId) return;
        inputActions.Piece.Follow.Disable();
        if(sentPosition != null)
        {
            oldPosition = sentPosition.Value;
            transform.position = oldPosition;
            if(lastPlacedColl!=null) lastPlacedColl.SetIsOccupiedServerRpc(false);
            lastPlacedColl=lastInteractedColl;
            lastInteractedColl.SetIsOccupiedServerRpc(true);
            SetCanBeMovedServerRpc(false);
        }
        else
        {
            transform.position = oldPosition;
        }
        Camera cam = Camera.main;
        CameraScript cameraScript = cam.GetComponent<CameraScript>();
        cameraScript.EnableMovement();
    }

    public void SetSentPosition(Vector3? position)
    {
        sentPosition = position;
    }

    public void SetLastInteractedColl(RobberCollider robberCollider)
    {
        lastInteractedColl = robberCollider;
    }

    [ServerRpc(RequireOwnership =false)]
    public void SetCanBeMovedServerRpc(bool moved)
    {
        canBeMoved.Value = moved;
    }
    public bool GetCanBeMoved()
    {
        return canBeMoved.Value;
    }
}
