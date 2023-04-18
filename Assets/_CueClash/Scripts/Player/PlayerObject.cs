using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private Animator _animator;
    private PlayerMovement _playerMovement;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        _inputHandler.OnSwitchedWeapons.AddListener(SwitchWeapons);
        _inputHandler.OnSwitchedAmmo.AddListener(SwitchAmmo);
    }

    private void OnDisable()
    {
        _inputHandler.OnSwitchedWeapons.RemoveListener(SwitchWeapons);
        _inputHandler.OnSwitchedAmmo.RemoveListener(SwitchAmmo);
    }

    private void Update()
    {
        _playerMovement.Move(_inputHandler.Movement);
    }

    private void SwitchWeapons()
    {

    }

    private void SwitchAmmo()
    {

    }
}
