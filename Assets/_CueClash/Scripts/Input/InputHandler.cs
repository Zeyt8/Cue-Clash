using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputHandler", menuName = "Cue Clash/InputHandler", order = 0)]
public class InputHandler : ScriptableObject, ControlSchemes.IPlayerActions
{
    public Vector3 Movement { get; private set; }
    public bool Jump { get; set; }
    public Vector2 Look { get; private set; }
    public bool Cue { get; private set; }
    public UnityEvent OnCueRelease = new UnityEvent();
    public bool Attack { get; private set; }
    public UnityEvent OnShootWeapon = new UnityEvent();
    public bool Parry { get; private set; }
    public UnityEvent OnSwitchedWeapons = new UnityEvent();
    public UnityEvent OnSwitchedAmmo = new UnityEvent();

    private ControlSchemes playerControls;

    public void OnEnable()
    {
        if (playerControls == null)
        {
            playerControls = new ControlSchemes();
            playerControls.Player.SetCallbacks(this);
            playerControls.Player.Enable();
        }
    }

    public void OnDisable()
    {
        playerControls.Player.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 m = context.ReadValue<Vector2>();
        Movement = new Vector3(m.x, 0, m.y);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Jump = true;
        }
        else if (context.canceled)
        {
            Jump = false;
        }
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
            OnCueRelease?.Invoke();
        }
    }

    public void OnSwing(InputAction.CallbackContext context)
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

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnShootWeapon?.Invoke();
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
        if (context.performed)
        {
            OnSwitchedWeapons.Invoke();
        }
    }

    public void OnSwitchAmmo(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnSwitchedAmmo.Invoke();
        }
    }
}
