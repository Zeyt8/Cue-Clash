using TMPro;
using Unity.Netcode;

public class InfoText : NetworkBehaviour
{
    public NetworkVariable<int> shotsLeft = new NetworkVariable<int>();
    public NetworkVariable<int> timeLeft = new NetworkVariable<int>();
    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    public override void OnNetworkSpawn()
    {
        shotsLeft.OnValueChanged += ChangeShotsLeft;
        timeLeft.OnValueChanged += ChangeTimer;
        shotsLeft.Value = 5;
        ChangeShotsLeft(0, 5);
    }

    private void ChangeShotsLeft(int previous, int current)
    {
        string name = LobbyManagerCustom.JoinedLobby.Players[PoolManager.Instance.currentPoolPlayer].Data["PlayerName"].Value;
        text.text = current + " shots left" + "\nTo Strike: " + name;
    }

    private void ChangeTimer(int previous, int current)
    {
        text.text = current + " seconds left";
    }
}
