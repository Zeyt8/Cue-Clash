using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonPersistent<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance { get; private set; } = null;

    public virtual void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = GetComponent<T>();
            DontDestroyOnLoad(gameObject);
        }
    }
}
