using Unity.Netcode;
using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] private PlayerObject playerObject;
    public bool swinging;
    public bool parrying;
    public bool blocking;

    private float timer = 0;
    private CapsuleCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<CapsuleCollider>();
    }

    public void Activate()
    {
        gameObject.layer = LayerMask.NameToLayer("Hitbox");
    }

    public void Deactivate()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else if (parrying)
        {
            parrying = false;
        }
    }

    public void StartSwing()
    {
        EndParryForced();
        swinging = true;
    }

    public void EndSwing()
    {
        swinging = false;
    }

    public void StartParry()
    {
        parrying = true;
        blocking = true;
        timer = 0.5f;
        _collider.radius = 0.05f;
        gameObject.layer = LayerMask.NameToLayer("Hurtbox");
    }

    public void EndParry()
    {
        parrying = false;
        blocking = false;
        _collider.radius = 0.04f;
        gameObject.layer = LayerMask.NameToLayer("Hitbox");
    }

    private void EndParryForced()
    {
        EndParry();
        timer = 0.0f;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (parrying)
        {
            if (other.TryGetComponent(out Bullet bullet))
            {
                Destroy(bullet);
                bullet.GetComponent<NetworkObject>().Despawn();
            }
            else if (other.TryGetComponent(out Sword sword))
            {
                playerObject.BecomeInvincibleServerRpc(0.5f);
            }
        }
        else if (swinging)
        {
            if (other.TryGetComponent(out Limb limb) && playerObject.team.Value != limb.GetTeam())
            {
                limb.TakeDamage(10);
            }
        }
    }
}
