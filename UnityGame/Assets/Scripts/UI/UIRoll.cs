using Network;
using Network.Messages;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Audio;

namespace UI
{
    public class UIRoll : MonoBehaviour
    {
        public GameObject RollButton;
        public GameObject NotEnough;
        public Merchant Merchant;

        [SerializeField] private SoundAsset RollSound;
        [SerializeField] private SoundAsset NoMoneySound;

        private void Start()
        {
            RollButton.SetActive(true);
            RollButton.GetComponent<Button>().onClick.AddListener(TryRollItem);
            Connection.Instance.MessageReceived.AddListener<ServerRollSuccess>(OnServerRollSuccess);
            Connection.Instance.MessageReceived.AddListener<ServerError>(OnServerError);
            Connection.Instance.MessageReceived.AddListener<ServerGoldUpdated>(OnServerGoldUpdated);
        }

        private void OnServerGoldUpdated(ServerGoldUpdated massage)
        {
            if (massage.new_gold > 100 && NotEnough.activeSelf)
            {
                NotEnough.SetActive(false);
            }
        }

        private void OnServerError(ServerError obj)
        {
            if (obj.error == "not_enough_gold")
            {
                NotEnough.SetActive(true);
            }
        }


        private void OnServerRollSuccess(ServerRollSuccess obj)
        {
            SoundManager.Instance.Play(RollSound);
            Merchant.BeginCharge();
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