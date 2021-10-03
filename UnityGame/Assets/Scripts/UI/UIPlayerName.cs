using TMPro;
using UnityEngine;

namespace UI
{
    public class UIPlayerName : MonoBehaviour
    {
        [SerializeField] private TMP_InputField InputField;

        private void Start()
        {
            InputField.text = PlayerStats.Instance.Username;
        }

        public string GetTextFromInput()
        {
            return InputField.text.Trim();
        }
    }
}