using Network;
using Network.Messages;
using System;
using UnityEngine;

namespace UI
{
    public class UIRolledItem : MonoBehaviour
    {
        public GameObject NewItemRoot;

        private UIRoll _uiRoll;

        private void Awake()
        {
            _uiRoll = FindObjectOfType<UIRoll>();
            NewItemRoot.SetActive(false);
            Connection.Instance.MessageReceived.AddListener<ServerRollSuccess>(OnServerRollSuccess);
        }

        private void OnServerRollSuccess(ServerRollSuccess obj)
        {
            NewItemRoot.SetActive(true);
        }

        public void HideRolledItem()
        {
            NewItemRoot.SetActive(false);
            _uiRoll.ActivateButton();
        }
    }
}