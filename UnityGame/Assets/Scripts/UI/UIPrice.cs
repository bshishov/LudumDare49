using TMPro;
using UnityEngine;

namespace UI
{
    public class UIPrice : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI Price;

        public void SetPrice(int price)
        {
            Price.text = price.ToString();
        }
    }
}