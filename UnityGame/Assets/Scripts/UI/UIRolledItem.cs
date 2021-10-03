using Network;
using Network.Messages;
using System;
using UnityEngine;
using TMPro;

namespace UI
{
    public class UIRolledItem : MonoBehaviour
    {
        public GameObject NewItemRoot;
        public TextMeshProUGUI[] NewItemPower;
        public TextMeshProUGUI OldItemPower;

        private UIRoll _uiRoll;

        private void Awake()
        {
            _uiRoll = FindObjectOfType<UIRoll>();
            NewItemRoot.SetActive(false);
            Connection.Instance.MessageReceived.AddListener<ServerRollSuccess>(OnServerRollSuccess);
        }

        private void OnServerRollSuccess(ServerRollSuccess massage)
        {
            
            for (int i = 0; i < NewItemPower.Length; i++)
            {
                NewItemPower[i].text = massage.rolled_item.total_power.ToString();
            }
            NewItemRoot.SetActive(true);
        }

        public void HideRolledItem()
        {
            NewItemRoot.SetActive(false);
            _uiRoll.ActivateButton();
        }
    }
}