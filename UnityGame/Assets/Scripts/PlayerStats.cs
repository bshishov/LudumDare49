using Network;
using Network.Game;
using Network.Messages;
using System;
using System.Linq;
using UnityEngine;
using Utils;
using Network.Game;

public class PlayerStats : Singleton<PlayerStats>
{
    public event Action IdReceived;
    public event Action PlayerStatsChanged;


    public RolledItem[] playerItems;

    public string PlayerID { get; private set; }
    public string Username { get; set; }
    public int Gold { get; private set; }
    public int Power { get; private set; }

    private string _playerIdPrefs = "PlayerID";
    private const string UsernameKey = "username";

    private void Awake()
    {
        if (PlayerPrefs.HasKey(UsernameKey))
            Username = PlayerPrefs.GetString(UsernameKey);
    }
    
    public void Start()
    {
        LoadPlayerID();
        Connection.Instance.MessageReceived.AddListener<ServerHello>(OnServerHello);
        Connection.Instance.MessageReceived.AddListener<ServerRollSuccess>(OnServerRollSuccess);
        Connection.Instance.MessageReceived.AddListener<ServerRollDecided>(OnServerRollDecided);
        Connection.Instance.MessageReceived.AddListener<ServerGoldUpdated>(OnServerGoldUpdated);
    }

    private void OnServerGoldUpdated(ServerGoldUpdated massage)
    {
        Gold = massage.new_gold;
        PlayerStatsChanged?.Invoke();
    }

    private void LoadPlayerID()
    {
        if (PlayerPrefs.HasKey("PlayerID"))
        {
            PlayerID = PlayerPrefs.GetString("PlayerID");
            IdReceived?.Invoke();
        }
        else
        {
            GenerateId();
        }
    }

    private void GenerateId()
    {
        PlayerID = SystemInfo.deviceUniqueIdentifier; // Android and Windows only
        IdReceived?.Invoke();
        Save();
    }

    private void Save()
    {
        PlayerPrefs.SetString("PlayerID", PlayerID);
        PlayerPrefs.SetString(UsernameKey, Username);
        PlayerPrefs.Save();
    }

    private void OnServerHello(ServerHello hello)
    {
        Save();
        UpdatePlayer(hello.player);

        //player exit when rolled  
        if (hello.player.current_undecided_roll_item != null)
        {
            Connection.Instance.Send(new ClientDeclineRoll());
        }
    }
    private void OnServerRollDecided(ServerRollDecided message)
    {
        UpdatePlayer(message.player);
    }

    private void OnServerRollSuccess(ServerRollSuccess message)
    {
        UpdatePlayer(message.player);
    }

    private void UpdatePlayer(Player player)
    {
        Gold = player.gold;
        Power = CalculatePower(player);
      
        playerItems = player.items;
        PlayerStatsChanged?.Invoke();
    }

    private int CalculatePower(Player player)
    {
        return player.items.Sum(ItemPower);
    }

    private int ItemPower(RolledItem item)
    {
        return item.total_power;
    }
}
