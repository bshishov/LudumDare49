using Network;
using Network.Messages;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UI
{
    public class UIRolledItem : MonoBehaviour
    {
        public ItemsSprite ItemsSprite;
        public GameObject NewItemRoot;

        public PlayerEquip PlayerEquip;

        public TextMeshProUGUI[] NewItemPower;
        public TextMeshProUGUI OldItemPower;
        public Image[] NewItemImage;
        public Image OldItemImage;

        public Material RarityMaterial;

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

            for (int j = 0; j < ItemsSprite.AllItems.Count; j++)
            {
                for (int k = 0; k < ItemsSprite.AllItems[j].ID.Length; k++)
                {

                    if (ItemsSprite.AllItems[j].ID[k] == massage.rolled_item.item.id)
                    {
                     
                        for (int i = 0; i < NewItemImage.Length; i++)
                        {
                            NewItemImage[i].sprite = ItemsSprite.AllItems[j].Image;
                            NewItemImage[i].material = RarityMaterial;
                        }

                    }
                }
            }
            OldItemImage.sprite = PlayerEquip.GetItemImage(massage.rolled_item.item.slot);
            OldItemPower.text = PlayerEquip.GetItemPower(massage.rolled_item.item.slot);
        }

        public void HideRolledItem()
        {
            NewItemRoot.SetActive(false);
            _uiRoll.ActivateButton();
        }
    }
}