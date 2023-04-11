using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputHandler", menuName = "Cue Clash/InputHandler", order = 0)]
public class InputHandler : ScriptableObject, ControlSchemes.IPlayerActions
{
    public Vector3 Movement { get; private set; }
    public Vector2 Look { get; private set; }
    public bool Cue { get; private set; }
    public bool Attack { get; private set; }
    public bool Parry { get; private set; }
    public UnityEvent OnSwitchedWeapons = new UnityEvent();
    public UnityEvent OnSwitchedAmmo = new UnityEvent();

    private ControlSchemes _playerControls;

    public void OnEnable()
    {
        if (_playerControls == null)
        {
            _playerControls = new ControlSchemes();
            _playerControls.Player.SetCallbacks(this);
            _playerControls.Player.Enable();
        }
    }

    public void OnDisable()
    {
        _playerControls.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 m = context.ReadValue<Vector2>();
        Movement = new Vector3(m.x, 0, m.y);
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Look = context.ReadValue<Vector2>();
    }

    public void OnCue(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Cue = true;
        }
        else if (context.canceled)
        {
            Cue = false;
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Attack = true;
        }
        else if (context.canceled)
        {
            Attack = false;
        }
    }

    public void OnParry(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Parry = true;
        }
        else if (context.canceled)
        {
            Parry = false;
        }
    }

    public void OnSwitchWeapons(InputAction.CallbackContext context)
    {
        OnSwitchedWeapons.Invoke();
    }

    public void OnSwitchAmmo(InputAction.CallbackContext context)
    {
        OnSwitchedAmmo.Invoke();
    }
}
