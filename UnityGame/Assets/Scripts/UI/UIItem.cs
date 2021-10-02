using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    public class UIItem : MonoBehaviour
    {
        [SerializeField] private Image ItemImage;
        [SerializeField] private TextMeshProUGUI PowerText;

        public void SetImage(Sprite sprite)
        {
            ItemImage.sprite = sprite;
        }

        public void SetPowerText(int power)
        {
            PowerText.text = power.ToString();
        }

    }
}