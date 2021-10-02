using TMPro;
using UnityEngine;

namespace UI
{
    public class UIGold : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI Gold;

        public void SetGold(int gold)
        {
            Gold.text = gold.ToString();
        }
    }
}