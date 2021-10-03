using Network;
using Network.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIDeclineRoll : MonoBehaviour
    {
        public UIRolledItem RolledItem;
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(DeclineItem);
        }

        private void DeclineItem()
        {
            Connection.Instance.Send(new ClientDeclineRoll());
            RolledItem.HideRolledItem();
        }
    }
}