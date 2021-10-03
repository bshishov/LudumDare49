using Network;
using Network.Messages;
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
        }

        private void TryRollItem()
        {
            RollButton.SetActive(false);
            Connection.Instance.Send(new ClientRoll { merchant = "first" });
        }

        public void ActivateButton()
        {
            RollButton.SetActive(true);
        }
    }
}