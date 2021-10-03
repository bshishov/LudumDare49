using TMPro;
using UnityEngine;

namespace UI
{
    public class UITotalPower : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI PowerText;

        private void Awake()
        {
            PlayerStats.Instance.PlayerStatsChanged += SetTotalPower;
            SetTotalPower();
        }

        public void SetTotalPower()
        {
            PowerText.text = PlayerStats.Instance.Power.ToString();
        }
    }
}