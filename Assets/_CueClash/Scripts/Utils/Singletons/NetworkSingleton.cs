using Unity.Netcode;

public class NetworkSingleton<T> : NetworkBehaviour where T : NetworkBehaviour
{
    public static T Instance { get; private set; } = null;

    public virtual void Awake()
    {
        Instance = GetComponent<T>();
    }
}
