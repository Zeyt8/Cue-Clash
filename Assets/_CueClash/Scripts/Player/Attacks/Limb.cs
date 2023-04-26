using Unity.Netcode;
using UnityEngine;

public class Limb : NetworkBehaviour
{
    [SerializeField]
    private int health = 100;

    public void TakeDamage(int damage)
    {
        TakeDamageServerRpc(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(int damage)
    {
        TakeDamageClientRpc(damage);
    }

    [ClientRpc]
    private void TakeDamageClientRpc(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            //Destroy(gameObject);
        }
    }
}
