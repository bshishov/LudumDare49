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

        private void Update()
        {
            if (PlayerStats.Instance.Gold < 100)
            {
                NotEnough.SetActive(true);
            }
            else { NotEnough.SetActive(false); }
        }
    }
}