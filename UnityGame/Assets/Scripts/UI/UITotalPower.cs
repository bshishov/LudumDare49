using TMPro;
using UnityEngine;

namespace UI
{
    public class UITotalPower : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI PowerText;

        public void SetTotalPower(int power)
        {
            PowerText.text = power.ToString();
        }
    }
}