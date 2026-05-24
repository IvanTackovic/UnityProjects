using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScript : MonoBehaviour
{
    private InputActionCam inputActions;
    private Camera _camera;
    private const float speed = 10f;
    void Start()
    {
        _camera = GetComponent<Camera>();
        inputActions = new InputActionCam();
        inputActions.Enable();
        inputActions.Moving.ClickForMoving.started += (InputAction.CallbackContext context)=> inputActions.Moving.Move.Enable();
        inputActions.Moving.ClickForMoving.canceled += (InputAction.CallbackContext context)=> inputActions.Moving.Move.Disable();
        inputActions.Moving.Move.performed += Move;
        inputActions.Moving.Move.Disable();
        inputActions.Moving.Resize.performed += Resize;
    }

    private void Move(InputAction.CallbackContext context)
    {
        Vector2 moved = context.ReadValue<Vector2>();
        Vector3 currentPosition = gameObject.transform.position;
        currentPosition.x -=moved.x*speed*Time.deltaTime;
        currentPosition.y -=moved.y*speed*Time.deltaTime;
        gameObject.transform.position=currentPosition;
    }

    private void Resize(InputAction.CallbackContext context)
    {
        float scroll = context.ReadValue<Vector2>().y;
        _camera.orthographicSize += scroll;
        if (_camera.orthographicSize < 5)
        {
            _camera.orthographicSize=5;
        }else if (_camera.orthographicSize > 25)
        {
            _camera.orthographicSize=25;
        }
    }

    public void DisableMovement()
    {
        inputActions.Moving.Disable();
    }
    public void EnableMovement()
    {
        inputActions.Moving.Enable();
        inputActions.Moving.Move.Disable();
    }
}
