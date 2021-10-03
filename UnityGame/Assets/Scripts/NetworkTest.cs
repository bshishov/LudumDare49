using System.Linq;
using System.Text;
using Network;
using Network.Game;
using Network.Messages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkTest : MonoBehaviour
{
    public Button ConnectLocal;
    public Button ConnectProd;
    public Button Disconnect;
    public Button Roll;
    public Button Accept;
    public Button Decline;
    public Button Hello;
    public Text LastMessage;
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI PowerText;
    public TextMeshProUGUI ItemsText;
    public TextMeshProUGUI RolledItem;
    public TMP_InputField Token;

    void Start()
    {
        Disconnect.onClick.AddListener(OnDisconnectClicked);

        ConnectLocal.onClick.AddListener(() =>
        {
            Connection.Instance.Connect("ws://localhost:6789/");
        });
        
        ConnectProd.onClick.AddListener(() =>
        {
            Connection.Instance.Connect("ws://unsmith.shishov.me/");
        });
        
        Hello.onClick.AddListener(() =>
        {
            Connection.Instance.Send(new ClientHello { token = Token.text, username = "test"} );
        });

        Accept.onClick.AddListener(() =>
        {
            Connection.Instance.Send(new ClientAcceptRoll() );
        });
        
        Decline.onClick.AddListener(() =>
        {
            Connection.Instance.Send(new ClientDeclineRoll() );
        });
        
        Roll.onClick.AddListener(() =>
        {
            Connection.Instance.Send(new ClientRoll { merchant = "first" });
        });
        
        Connection.Instance.Connected += OnConnected;
        Connection.Instance.Disconnected += OnDisconnected;
        
        Connection.Instance.Send(new ClientRoll());
        
        
        Connection.Instance.MessageReceived.AddListener<ServerRollSuccess>(OnServerRollSuccess);
        Connection.Instance.MessageReceived.AddListener<ServerHello>(OnServerHello);
        Connection.Instance.MessageReceived.AddListener<ServerGoldUpdated>(OnServerGoldUpdated);
        Connection.Instance.MessageReceived.AddListener<ServerError>(OnServerError);
        Connection.Instance.MessageReceived.AddListener<ServerRollDecided>(OnServerRollDecided);
    }

    private void OnServerRollDecided(ServerRollDecided message)
    {
        UpdatePlayer(message.player);
    }

    private void OnServerError(ServerError message)
    {
        Debug.LogWarning(message.error);
    }

    private void OnServerRollSuccess(ServerRollSuccess message)
    {
        UpdatePlayer(message.player);
    }

    private void OnServerGoldUpdated(ServerGoldUpdated message)
    {
        GoldText.text = message.new_gold.ToString();
        LastMessage.text = $"New gold: {message.new_gold}";
    }

    private void OnDisconnected()
    {
        Debug.Log("Disconnected");
    }

    private void OnConnected()
    {
        Debug.Log("Connected");
    }

    void OnDisconnectClicked()
    {
        Debug.Log("Disconnect clicked");
        Connection.Instance.Close();
    }

    void OnServerHello(ServerHello hello)
    {
        UpdatePlayer(hello.player);
        LastMessage.text = $"Server sent hello {hello.player}";
        UpdateItemsText(hello.player);
    }

    private int CalculatePower(Player player)
    {
        return player.items.Sum(ItemPower);
    }

    private void UpdateItemsText(Player player)
    {
        var sb = new StringBuilder();

        sb.AppendLine("Items:");

        var sortedItems = player.items.OrderBy(i => i.item.slot);

        foreach (var rolledItem in sortedItems)
        {
            sb.Append(rolledItem.item.slot);
            sb.Append(": ");
            sb.Append($"<color=grey>{rolledItem.quality}</color>");
            sb.Append(" ");
            sb.Append(rolledItem.item.id);
            sb.Append(" ");
            sb.Append($"<color=yellow>{ItemPower(rolledItem)}</color>");
            sb.Append("\n");
        }
        
        ItemsText.text = sb.ToString();
    }

    private int ItemPower(RolledItem item)
    {
        return item.total_power;
    }

    private void UpdatePlayer(Player player)
    {
        GoldText.text = player.gold.ToString();
        PowerText.text = CalculatePower(player).ToString();
        UpdateItemsText(player);

        if (player.current_undecided_roll_item.HasValue)
        {
            var i = player.current_undecided_roll_item.Value;
            RolledItem.text = $"<color=grey>{i.quality}</color> {i.item.id} <color=yellow>{ItemPower(i)}</color>";
        }
        else
        {
            RolledItem.text = "";
        }
    }
}
