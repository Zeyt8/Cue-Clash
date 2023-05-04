using UnityEngine;

public class Sword : MonoBehaviour
{
    [SerializeField] private PlayerObject playerObject;
    public bool parrying;
    public bool blocking;

    private float timer = 0;
    private CapsuleCollider collider;

    private void Awake()
    {
        collider = GetComponent<CapsuleCollider>();
    }
    
    public void Activate()
    {

    }

    public void Deactivate()
    {

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

    public void StartParry()
    {
        parrying = true;
        blocking = true;
        timer = 0.5f;
        collider.radius = 0.05f;
        gameObject.layer = LayerMask.NameToLayer("Hurtbox");
    }

    public void EndParry()
    {
        blocking = false;
        collider.radius = 0.04f;
        gameObject.layer = LayerMask.NameToLayer("Hitbox");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!parrying) return;
        if (!other.CompareTag("HitBox")) return;
        if (other.TryGetComponent(out Bullet bullet))
        {
            Destroy(bullet);
        }
        else if (other.TryGetComponent(out Sword sword))
        {
            playerObject.BecomeInvincibleServerRpc(0.5f);
        }
    }
}
