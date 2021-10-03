using TMPro;
using UnityEngine;

namespace UI
{
    public class UIGold : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI Gold;

        private void Awake()
        {
            PlayerStats.Instance.PlayerStatsChanged += SetGold;
            SetGold();
        }

        public void SetGold()
        {
            Gold.text = PlayerStats.Instance.Gold.ToString();
        }
    }
}