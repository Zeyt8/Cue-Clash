using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Limb : MonoBehaviour
{
    [SerializeField]
    private NetworkVariable<int> health = new NetworkVariable<int>(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public void TakeDamage(int damage)
    {
        health.Value -= damage;
        if(health.Value <= 0)
        {
            Destroy(gameObject);
        }
    }
}
