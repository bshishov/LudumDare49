using Network;
using Network.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIStartGame : MonoBehaviour
    {
        private UIPlayerName _uiPlayerName;

        private void Awake()
        {
            PlayerStats.Instance.IdReceived += ActivateStartButton;
        }

        private void Start()
        {
            _uiPlayerName = FindObjectOfType<UIPlayerName>();
        }

        private void ActivateStartButton()
        {
            GetComponent<Button>().onClick.AddListener(SendHello);
        }

        private void SendHello()
        {
            PlayerStats.Instance.Username = _uiPlayerName.GetTextFromInput();

            if (Connection.Instance.IsConnected)
            {
                Connection.Instance.Send(new ClientHello
                {
                    token = PlayerStats.Instance.PlayerID, 
                    username = PlayerStats.Instance.Username
                });
            
                GameManager.Instance.TransitionToGame(); 
            }
        }
    }
}
