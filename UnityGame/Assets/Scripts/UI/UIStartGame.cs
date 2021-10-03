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
            var name = _uiPlayerName.GetTextFromInput();

            Connection.Instance.Send(new ClientHello { token = PlayerStats.Instance.PlayerID, username = name });
            GameManager.Instance.TransitionToGame();
        }
    }
}
