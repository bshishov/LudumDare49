using Network;
using Network.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIRoll : MonoBehaviour
    {
        public GameObject RollButton;
        public Merchant Merchant;
        public GameObject NotEnough;
        private void Start()
        {
            NotEnough.SetActive(false);
            RollButton.SetActive(true);
            RollButton.GetComponent<Button>().onClick.AddListener(TryRollItem);
            Connection.Instance.MessageReceived.AddListener<ServerRollSuccess>(OnServerRollSuccess);
        }

        private void OnServerRollSuccess(ServerRollSuccess obj)
        {
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