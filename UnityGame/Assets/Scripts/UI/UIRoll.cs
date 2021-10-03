using Network;
using Network.Messages;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIRoll : MonoBehaviour
    {
        public GameObject RollButton;

        private void Start()
        {
            RollButton.SetActive(true);
            RollButton.GetComponent<Button>().onClick.AddListener(TryRollItem);
            Connection.Instance.MessageReceived.AddListener<ServerRollSuccess>(OnServerRollSuccess);
        }

        private void OnServerRollSuccess(ServerRollSuccess obj)
        { 
            RollButton.SetActive(false);
        }

        private void TryRollItem()
        {
            Connection.Instance.Send(new ClientRoll { merchant = "first" });
        }

        public void ActivateButton()
        {
            RollButton.SetActive(true);
        }
    }
}