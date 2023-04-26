using Cinemachine;
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
    [SerializeField] private InputHandler inputHandler;
    [Header("Children")]
    [SerializeField] private Weapon weapon;
    [SerializeField] private Cue cue;
    [SerializeField] private Transform head;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerAnimations playerAnimations;
    [SerializeField] private FollowTransform headLookAt;
    [Header("Prefabs")]
    [SerializeField] private CinemachineVirtualCamera cameraPrefab;

    private PlayerMovement playerMovement;
    private PlayerState playerState = PlayerState.Billiard;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        CinemachineVirtualCamera camera = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);
        camera.Follow = head;
        camera.LookAt = head;
        playerMovement.Camera = camera;
        headLookAt.followTransform = camera.transform;
        playerAnimations.camera = camera;
    }

    private void OnEnable()
    {
        inputHandler.OnCueRelease.AddListener(HitWithCue);
        inputHandler.OnShootWeapon.AddListener(Shoot);
        inputHandler.OnSwitchedWeapons.AddListener(SwitchWeapons);
        inputHandler.OnSwitchedAmmo.AddListener(SwitchAmmo);
    }

    private void OnDisable()
    {
        inputHandler.OnCueRelease.RemoveListener(HitWithCue);
        inputHandler.OnShootWeapon.RemoveListener(Shoot);
        inputHandler.OnSwitchedWeapons.RemoveListener(SwitchWeapons);
        inputHandler.OnSwitchedAmmo.RemoveListener(SwitchAmmo);
    }

    private void Update()
    {
        if (!IsOwner) return;

        animator.SetBool("Walking", inputHandler.Movement != Vector3.zero);
        playerMovement.Move(inputHandler);

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (playerState == PlayerState.Billiard)
            {
                playerState = PlayerState.Ranged;
                animator.SetInteger("Phase", 1);
            }
            else
            {
                playerState = PlayerState.Billiard;
                animator.SetInteger("Phase", 0);
            }
        }
        
        // Start charging cue
        if (playerState == PlayerState.Billiard)
        {
            cue.charging = inputHandler.Cue;
            if (inputHandler.Cue)
            {
                playerAnimations.ChargeCue(cue.cueForce);
            }
        }
    }

    private void HitWithCue()
    {
        if (!IsOwner || playerState != PlayerState.Billiard) return;
        cue.Shoot();
        playerAnimations.HitWithCue();
    }

    private void Shoot()
    {
        if (!IsOwner || playerState != PlayerState.Ranged) return;
        weapon.Shoot();
    }

    private void SwitchWeapons()
    {

    }

    private void SwitchAmmo()
    {

    }
}
