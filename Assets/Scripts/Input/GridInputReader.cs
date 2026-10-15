using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridInputReader : MonoBehaviour, GridInputActions.IGridActions
{

    private GridInputActions inputActions;

    public Vector2 PointerPosition { get; private set; }
    public Vector2 MoveInput { get; private set; }

    public event Action<Vector2> SelectPressed;
    public  event Action<Vector2> MoveRequested;
    public event Action<Vector2> ScrollRequested;
    public event Action SpawnPressed;


    private void Awake()
    {
        inputActions = new GridInputActions();
    }

    private void OnEnable()
    {
        inputActions.Grid.SetCallbacks(this);
        inputActions.Grid.Enable();
    }

    private void OnDisable()
    {
        inputActions.Grid.Disable();
        inputActions.Grid.SetCallbacks(null);
        MoveInput = Vector2.zero;
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            MoveInput = Vector2.zero;
            return;
        }

        if(!context.performed) return;

        MoveInput = context.ReadValue<Vector2>();

        if(MoveInput != Vector2.zero)
        {
            MoveRequested?.Invoke(MoveInput);
        }
    }

    public void OnPoint(InputAction.CallbackContext context)
    {
        if(context.performed || context.canceled) PointerPosition = context.ReadValue<Vector2>();
    }

    public void OnScroll(InputAction.CallbackContext context)
    {
        if(!context.performed) return;

        Vector2 scroll = context.ReadValue<Vector2>();

        if(scroll != Vector2.zero)
        {
            ScrollRequested?.Invoke(scroll);
        }
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        PointerPosition = inputActions.Grid.Point.ReadValue<Vector2>();

        SelectPressed?.Invoke(PointerPosition);
    }

    public void OnSpawn(InputAction.CallbackContext context)
    {
        if(!context.performed) return;

        SpawnPressed?.Invoke();
    }
}
