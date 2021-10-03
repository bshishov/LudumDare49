using System;
using UnityEngine;
using Utils;

public class PlayerStats : Singleton<PlayerStats>
{
    public string PlayerID { get; private set; }

    private string _playerIdPrefs = "PlayerID";

    public event Action IdReceived;

    public void Start()
    {
        LoadPlayerID();
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
        PlayerPrefs.Save();
    }
}
