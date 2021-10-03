using Network;
using Network.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIAcceptRoll : MonoBehaviour
    {
        public UIRolledItem RolledItem;
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(AcceptRoll);
        }

        private void AcceptRoll()
        {
            Connection.Instance.Send(new ClientAcceptRoll());
            RolledItem.HideRolledItem();
        }
    }
}