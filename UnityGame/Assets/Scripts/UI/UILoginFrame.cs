using System;
using Network;
using Network.Messages;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UILoginFrame : MonoBehaviour
    {
        [SerializeField] private Button LoginButton;
        [SerializeField] private TMP_InputField UsernameField;

        private void Awake()
        {
            if (string.IsNullOrEmpty(PlayerStats.Instance.PlayerID))
            {
                LoginButton.interactable = false;
                PlayerStats.Instance.IdReceived += ActivateStartButton;
            }
        }
        
        private void Start()
        {
            UsernameField.text = PlayerStats.Instance.Username;
            LoginButton.onClick.AddListener(OnLoginClicked);
        }

        private void ActivateStartButton()
        {
            LoginButton.interactable = true;
        }
        
        private void OnLoginClicked()
        {
            PlayerStats.Instance.Username = UsernameField.text;

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