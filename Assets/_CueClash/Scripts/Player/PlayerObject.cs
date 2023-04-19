using Unity.Netcode;
using UnityEngine;

public enum PlayerState
{
    Billiard,
    Melee,
    Ranged
}

public class PlayerObject : NetworkBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private Animator _animator;
    [SerializeField] private Weapon _weapon;
    private PlayerMovement _playerMovement;
    private PlayerState _playerState;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        _inputHandler.OnShootWeapon.AddListener(Shoot);
        _inputHandler.OnSwitchedWeapons.AddListener(SwitchWeapons);
        _inputHandler.OnSwitchedAmmo.AddListener(SwitchAmmo);
    }

    private void OnDisable()
    {
        _inputHandler.OnShootWeapon.RemoveListener(Shoot);
        _inputHandler.OnSwitchedWeapons.RemoveListener(SwitchWeapons);
        _inputHandler.OnSwitchedAmmo.RemoveListener(SwitchAmmo);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (_inputHandler.Movement != Vector3.zero)
        {
            _animator.SetBool("Walking", true);
        }
        else
        {
            _animator.SetBool("Walking", false);
        }
        _playerMovement.Move(_inputHandler);

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (_playerState == PlayerState.Billiard)
            {
                _playerState = PlayerState.Ranged;
                _animator.SetInteger("Phase", 1);
            }
            else
            {
                _playerState = PlayerState.Billiard;
                _animator.SetInteger("Phase", 0);
            }
        }
    }

    private void Shoot()
    {
        if (!IsOwner || _playerState != PlayerState.Ranged) return;
        _weapon.Shoot();
    }

    private void SwitchWeapons()
    {

    }

    private void SwitchAmmo()
    {

    }
}
