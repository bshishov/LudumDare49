using Network;
using Network.Messages;
using UnityEngine;
using UnityEngine.UI;
using Audio;

namespace UI
{
    public class UIAcceptRoll : MonoBehaviour
    {
        public UIRolledItem RolledItem;
        [SerializeField] private SoundAsset ClickSound;
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(AcceptRoll);
        }

        private void AcceptRoll()
        {
            Connection.Instance.Send(new ClientAcceptRoll());
            RolledItem.HideRolledItem();
            SoundManager.Instance.Play(ClickSound);
        }
    }
}