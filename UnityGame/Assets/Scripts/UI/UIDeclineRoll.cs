using Network;
using Network.Messages;
using UnityEngine;
using UnityEngine.UI;
using Audio;


namespace UI
{
    public class UIDeclineRoll : MonoBehaviour
    {
        public UIRolledItem RolledItem;
        [SerializeField] private SoundAsset ClickSound;
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(DeclineItem);
        }

        private void DeclineItem()
        {
            Connection.Instance.Send(new ClientDeclineRoll());
            RolledItem.HideRolledItem();
            SoundManager.Instance.Play(ClickSound);
        }
    }
}