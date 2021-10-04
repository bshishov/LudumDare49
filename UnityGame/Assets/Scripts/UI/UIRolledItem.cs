using Network;
using Network.Messages;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

namespace UI
{
    public class UIRolledItem : MonoBehaviour
    {
        public ItemsSprite ItemsSprite;
        public GameObject NewItemRoot;

        public PlayerEquip PlayerEquip;

        public GameObject[] RarityBackgrounds;
        public TextMeshProUGUI[] NewItemPower;
        public TextMeshProUGUI OldItemPower;
        public Image[] NewItemImage;
        public Image OldItemImage;

        public Material RarityMaterial;

        private UIRoll _uiRoll;
        private GameObject _rarityBackgrounds;

        private void Awake()
        {
            _uiRoll = FindObjectOfType<UIRoll>();
            NewItemRoot.SetActive(false);
            Connection.Instance.MessageReceived.AddListener<ServerRollSuccess>(OnServerRollSuccess);
        }

        private void OnServerRollSuccess(ServerRollSuccess massage)
        {
            NewItemRoot.SetActive(true);

            ActivateBackground(massage);
            SetNewItemPower(massage);
            SetSprites(massage);
        }

        private void SetSprites(ServerRollSuccess massage)
        {
            for (int j = 0; j < ItemsSprite.AllItems.Count; j++)
            {
                for (int k = 0; k < ItemsSprite.AllItems[j].ID.Length; k++)
                {
                    if (ItemsSprite.AllItems[j].ID[k] == massage.rolled_item.item.id)
                    {

                        for (int i = 0; i < NewItemImage.Length; i++)
                        {
                            var indexOfRarity = (int)Enum.Parse(typeof(ItemRarity), massage.rolled_item.item.rarity);
                            NewItemImage[i].color = ItemsSprite.RarityColor[indexOfRarity];

                            NewItemImage[i].sprite = ItemsSprite.AllItems[j].Image;
                            NewItemImage[i].material = RarityMaterial;
                        }

                    }
                }
            }

            OldItemImage.sprite = PlayerEquip.GetItemSprite(massage.rolled_item.item.slot);
            OldItemImage.color = PlayerEquip.GetItemMaterialColor(massage.rolled_item.item.slot);
            OldItemPower.text = PlayerEquip.GetItemPower(massage.rolled_item.item.slot);
        }

        private void SetNewItemPower(ServerRollSuccess massage)
        {
            for (int i = 0; i < NewItemPower.Length; i++)
            {
                NewItemPower[i].text = massage.rolled_item.total_power.ToString();
            }
        }

        private void ActivateBackground(ServerRollSuccess massage)
        {
            var indexOfRarity = (int)Enum.Parse(typeof(ItemRarity), massage.rolled_item.item.rarity);

            _rarityBackgrounds = RarityBackgrounds[indexOfRarity];
            _rarityBackgrounds.SetActive(true);
        }

        public void HideRolledItem()
        {
            _rarityBackgrounds.SetActive(false);
            NewItemRoot.SetActive(false);
            _uiRoll.ActivateButton();
        }
    }
}