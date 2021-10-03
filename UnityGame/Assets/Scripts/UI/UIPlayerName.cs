using TMPro;
using UnityEngine;

namespace UI
{
    public class UIPlayerName : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI InputField;
        public string GetTextFromInput()
        {
            return InputField.text;
        }

    }
}