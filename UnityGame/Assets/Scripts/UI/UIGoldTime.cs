using Network;
using Network.Messages;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UIGoldTime : MonoBehaviour
{
    public Slider Slider;
    private float _sliderSpeed;
    
    private void Start()
    {
        Connection.Instance.MessageReceived.AddListener<ServerHello>(OnServerHello);
        Connection.Instance.MessageReceived.AddListener<ServerGoldUpdated>(OnServerGoldUpdated);

        Slider.interactable = false;
    }

    private void Update()
    {
        var oldValue = Slider.value; 
        Slider.value = Mathf.Clamp01(oldValue + _sliderSpeed * Time.deltaTime);
    }

    private void OnServerGoldUpdated(ServerGoldUpdated message)
    {
        var nextUpdate = UnixTimestampToDateTime(message.next_update_time).ToLocalTime();
        var secondsRemaining = (float)(nextUpdate - DateTime.Now).TotalSeconds;
        if (secondsRemaining > 0)
        {
            _sliderSpeed = 1 / secondsRemaining;
        }
        else
        {
            _sliderSpeed = 0;
        }

        Slider.value = 0;
    }

    private void OnServerHello(ServerHello obj)
    {
        var secondsRemaining = 2f * 60;
        _sliderSpeed = 1 / secondsRemaining;
    }

    private static DateTime UnixTimestampToDateTime(double unixTime)
    {
        var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        var unixTimeStampInTicks = (long)(unixTime * TimeSpan.TicksPerSecond);
        return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
    }

    
}
