using System;
using System.Linq;
using System.Text;
using Network;
using Network.Game;
using Network.Messages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIDivisionFrame : MonoBehaviour
    {
        public UIDivisionPlayerList PlayerList;
        public TextMeshProUGUI LeagueName;
        public TextMeshProUGUI NextUpdateAt;
        public Button RefreshButton;

        private DateTime _targetUpdateTime = DateTime.Now;

        private void Start()
        {
            // Subscribe to division updates
            Connection.Instance.MessageReceived.AddListener<ServerDivisionInfo>(
                m => UpdateStandings(m.standings));
            
            RefreshButton.onClick.AddListener(RequestDivisionInfo);

            RequestDivisionInfo();
            PlayerList.Clear();
        }

        [ContextMenu("Request division")]
        private void RequestDivisionInfo()
        {
            Connection.Instance.Send(new ClientDivisionInfoRequest());
        }

        private void Update()
        {
            NextUpdateAt.text = SecondsToString(_targetUpdateTime - DateTime.Now);
        }

        public static double ConvertToUnixTimestamp(DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var diff = date.ToUniversalTime() - origin;
            return diff.TotalSeconds;
        }
        
        public static DateTime UnixTimestampToDateTime(double unixTime)
        {
            var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            var unixTimeStampInTicks = (long) (unixTime * TimeSpan.TicksPerSecond);
            return new DateTime(unixStart.Ticks + unixTimeStampInTicks, System.DateTimeKind.Utc);
        }

        private void UpdateStandings(DivisionStandings standings)
        {
            if (PlayerList)
            {
                PlayerList.Clear();
                foreach (var divisionPlayer in standings.players.OrderBy(p => p.rank))
                {
                    PlayerList.Add(divisionPlayer);
                }
            }

            LeagueName.text = $"{standings.league_id} League";
            _targetUpdateTime = UnixTimestampToDateTime(standings.next_update_at).ToLocalTime();
        }

        private string SecondsToString(TimeSpan span)
        {
            var s = span.Seconds;
            var m = span.Minutes;
            var h = (int)Math.Floor(span.TotalHours);

            var sb = new StringBuilder();
            
            if (h > 0)
            {
                sb.Append(h.ToString());
                sb.Append("h ");
            }

            if (m > 0)
            {
                sb.Append(m.ToString());
                sb.Append("m ");
            }

            if (s > 0)
            {
                sb.Append(s.ToString());
                sb.Append("s");
            }

            return sb.ToString();
        }
    }
}