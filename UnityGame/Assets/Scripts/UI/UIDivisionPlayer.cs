using Network.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIDivisionPlayer : MonoBehaviour
    {
        [Header("References")] 
        public TextMeshProUGUI UserName;
        public TextMeshProUGUI Rank;
        public TextMeshProUGUI Power;
        public Image Background;

        public void Setup(DivisionPlayer divisionPlayer)
        {
            UserName.text = divisionPlayer.username;
            Rank.text = divisionPlayer.rank.ToString();
            Power.text = divisionPlayer.power.ToString();
            Background.color = RankToColor(divisionPlayer.rank);
        }

        private static Color RankToColor(int rank)
        {
            return rank switch
            {
                1 => new Color(0.99f, 0.48f, 0f),
                2 => new Color(0f, 0.3f, 0.56f),
                3 => new Color(0.42f, 0.12f, 0f),
                _ => new Color(0.19f, 0.18f, 0.19f)
            };
        }
    }
}