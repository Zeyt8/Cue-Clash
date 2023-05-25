using TMPro;
using Unity.Netcode;

public class BallsMovingUI : NetworkBehaviour
{
    public NetworkVariable<bool> isActive = new NetworkVariable<bool>(false);
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public override void OnNetworkSpawn()
    {
        isActive.OnValueChanged += Activate;
    }

    private void Activate(bool previous, bool current)
    {
        text.text = current ? "Balls moving" : "";
    }
}
