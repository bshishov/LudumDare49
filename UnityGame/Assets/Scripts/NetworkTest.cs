using Network;
using Network.Messages;
using UnityEngine;
using UnityEngine.UI;

public class NetworkTest : MonoBehaviour
{
    public InputField Host;
    public Button Connect;
    public Button Disconnect;
    public Button Roll;
    public Button Accept;
    public Button Decline;
    public Button Hello;
    public Text LastMessage;

    void Start()
    {
        Connect.onClick.AddListener(OnConnectClicked);
        Disconnect.onClick.AddListener(OnDisconnectClicked);
        
        Hello.onClick.AddListener(() =>
        {
            Connection.Instance.Send(new ClientHello { token = "123", username = "test"} );
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
            Connection.Instance.Send(new ClientRoll { merchant = "default" });
        });
        
        Connection.Instance.Connected += OnConnected;
        Connection.Instance.Disconnected += OnDisconnected;
        
        Connection.Instance.MessageReceived.AddListener<ServerHello>(OnServerHello);
        Connection.Instance.MessageReceived.AddListener<ServerGoldUpdated>(OnServerGoldUpdated);
    }

    private void OnServerGoldUpdated(ServerGoldUpdated message)
    {
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

    void OnConnectClicked()
    {
        Debug.Log("Connect clicked");
        var host = Host.text;
        Connection.Instance.Connect(host);
    }

    void OnDisconnectClicked()
    {
        Debug.Log("Disconnect clicked");
        Connection.Instance.Close();
    }

    void OnServerHello(ServerHello hello)
    {
        LastMessage.text = $"Server sent hello {hello.player}";
    }

    void Update()
    {
    }
}
