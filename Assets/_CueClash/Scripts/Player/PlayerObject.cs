using Cinemachine;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerState
{
    Billiard,
    Sword,
    Gun
}

public class PlayerObject : NetworkBehaviour
{
    [SerializeField] private InputHandler inputHandler;
    [Header("Children")]
    [SerializeField] private Transform cueTransform;
    [SerializeField] private Transform head;
    [SerializeField] private Transform animatorTransform;
    [SerializeField] private FollowTransform headLookAt;
    [Header("Prefabs")]
    [SerializeField] private CinemachineVirtualCamera cameraPrefab;

    private PlayerMovement playerMovement;
    private PlayerState playerState = PlayerState.Billiard;

    private Cue cue;
    private Gun gun;
    private Sword sword;

    private Animator animator;
    private PlayerAnimations playerAnimations;
    private CinemachinePOV pov;

    private bool aimCue;

    private readonly int maxNrOfPoolShots = 3;
    private int nrOfPoolShotsBeforeBattle = 3;

    private readonly float maxDurationOfBattle = 6;
    private float battleTimer = 0;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        animator = animatorTransform.GetComponent<Animator>();
        playerAnimations = animatorTransform.GetComponent<PlayerAnimations>();
        cue = cueTransform.GetComponent<Cue>();
        gun = cueTransform.GetComponent<Gun>();
        sword = cueTransform.GetComponent<Sword>();
        playerAnimations.PlayerState = PlayerState.Billiard;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        CinemachineVirtualCamera camera = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);
        camera.Follow = head;
        camera.LookAt = head;
        pov = camera.GetCinemachineComponent<CinemachinePOV>();
        playerMovement.pov = pov;
        headLookAt.followTransform = camera.transform;
        playerAnimations.camera = camera;
    }

    private void OnEnable()
    {
        inputHandler.OnCueRelease.AddListener(HitWithCue);
        inputHandler.OnShootWeapon.AddListener(Shoot);
        inputHandler.OnSwitchedWeapons.AddListener(SwitchWeapons);
        inputHandler.OnSwitchedAmmo.AddListener(SwitchAmmo);
        inputHandler.AimCueStateChanged.AddListener(AimCueChangedState);
    }

    private void OnDisable()
    {
        inputHandler.OnCueRelease.RemoveListener(HitWithCue);
        inputHandler.OnShootWeapon.RemoveListener(Shoot);
        inputHandler.OnSwitchedWeapons.RemoveListener(SwitchWeapons);
        inputHandler.OnSwitchedAmmo.RemoveListener(SwitchAmmo);
        inputHandler.AimCueStateChanged.RemoveListener(AimCueChangedState);
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
                playerState = PlayerState.Gun;
                animator.SetInteger("Phase", 1);
                gun.Activate();
                cue.Deactivate();
                playerAnimations.PlayerState = PlayerState.Gun;
            }
            else
            {
                playerState = PlayerState.Billiard;
                animator.SetInteger("Phase", 0);
                gun.Deactivate();
                cue.Activate();
                playerAnimations.PlayerState = PlayerState.Billiard;
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
            else if (aimCue)
            {
                Vector2 pos = new Vector2(
                    inputHandler.MousePosition.x.Remap(0, Screen.width, -1, 1),
                    inputHandler.MousePosition.y.Remap(0, Screen.height, -1, 1)
                );
                playerAnimations.AlignBilliardAim(pos);
            }
        }

        else
        {
            battleTimer += Time.deltaTime;
            if (battleTimer > maxDurationOfBattle)
            {
                playerState = PlayerState.Billiard;
                battleTimer = 0;
            }
        }
    }

    private void HitWithCue()
    {
        if (!IsOwner || playerState != PlayerState.Billiard) return;
        cue.Shoot();
        nrOfPoolShotsBeforeBattle--;
        if (nrOfPoolShotsBeforeBattle == 0)
        {
            playerState = PlayerState.Gun;
            nrOfPoolShotsBeforeBattle = maxNrOfPoolShots;
            battleTimer = 0;
        }
        playerAnimations.HitWithCue();
    }

    private void Shoot()
    {
        if (!IsOwner || playerState != PlayerState.Gun) return;
        gun.Shoot();

        /// TODO: For when Sword is implemented
        // if (gun.nrOfBullets.Value < 1)
        //     SwitchWeapons();
    }

    private void SwitchWeapons()
    {
        if (!IsOwner) return;
        if (playerState == PlayerState.Gun)
        {
            playerState = PlayerState.Sword;
            gun.Deactivate();
            sword.Activate();
            playerAnimations.PlayerState = PlayerState.Sword;
        }
        else if (playerState == PlayerState.Sword)
        {
            playerState = PlayerState.Gun;
            gun.Activate();
            sword.Deactivate();
            playerAnimations.PlayerState = PlayerState.Gun;
        }
    }

    private void SwitchAmmo()
    {

    }

    private void AimCueChangedState(bool state)
    {
        if (state == true)
        {
            Cursor.lockState = CursorLockMode.Confined;
            pov.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            pov.enabled = true;
        }
        aimCue = state;
    }
}
