using Network;
using Network.Messages;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIGoldTime : MonoBehaviour
{
    public Slider Slider;

    private TimeSpan newGoldIncome;
    private void Start()
    {
        Connection.Instance.MessageReceived.AddListener<ServerHello>(OnServerHello);
        Connection.Instance.MessageReceived.AddListener<ServerGoldUpdated>(OnServerGoldUpdated);
    }

    private void OnServerGoldUpdated(ServerGoldUpdated obj)
    {
    }

    private void OnServerHello(ServerHello obj)
    {
        var now = DateTime.Now;
        var target = UnixTimestampToDateTime(obj.player.last_gold_update_time).ToLocalTime();
        newGoldIncome = target - now;
        Debug.Log(newGoldIncome);
    }

    public static DateTime UnixTimestampToDateTime(double unixTime)
    {
        var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        var unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
        return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
    }

    
}
