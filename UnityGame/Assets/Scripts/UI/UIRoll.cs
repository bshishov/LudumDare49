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
            Connection.Instance.MessageReceived.AddListener<ServerHello>(OnServerHello);
         
        }

        private void OnServerHello(ServerHello massage)
        {
            if (massage.player.gold > 100)
            {
                NotEnough.SetActive(false);
            }
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
                SoundManager.Instance.Play(NoMoneySound);
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
            if (PlayerStats.Instance.Gold < 100)
            {
                NotEnough.SetActive(true);
            }
            Connection.Instance.Send(new ClientRoll { merchant = "first" });
        }

        public void ActivateButton()
        {
            RollButton.SetActive(true);
        }

    }
}