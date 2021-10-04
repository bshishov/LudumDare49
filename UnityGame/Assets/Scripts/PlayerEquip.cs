using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UI;
using System;

public class PlayerEquip : MonoBehaviour
{
    public ItemsSprite ItemsSprite;
    public Material RarityMaterial;
    public List<ItemType> SlotsTypeOrder = new List<ItemType>();
    public List<Image> ItemIcons = new List<Image>();
    public List<Image> SlotIcons = new List<Image>();
    public List<TextMeshProUGUI> SlotsPower = new List<TextMeshProUGUI>();

    private void Start()
    {
        PlayerStats.Instance.PlayerStatsChanged += SetPlayerItems;
        if (PlayerStats.Instance.playerItems.Length != 0)
        {
            SetPlayerItems();
        }
    }

    private void SetPlayerItems()
    {
        var rolledItems = PlayerStats.Instance.playerItems;
        foreach (var rolledItem in rolledItems)
        {
            var slotIndex = SlotsTypeOrder.IndexOf((ItemType)Enum.Parse(typeof(ItemType), rolledItem.item.slot));
            SlotsPower[slotIndex].text = rolledItem.total_power.ToString();

            for (int j = 0; j < ItemsSprite.AllItems.Count; j++)
            {
                for (int k = 0; k < ItemsSprite.AllItems[j].ID.Length; k++)
                {
                    if (ItemsSprite.AllItems[j].ID[k] == rolledItem.item.id)
                    {
                        ItemIcons[slotIndex].sprite = ItemsSprite.AllItems[j].Image;
                        ItemIcons[slotIndex].material = RarityMaterial;

                        var indexOfRarity = (int)Enum.Parse(typeof(ItemRarity), rolledItem.item.rarity);
                        ItemIcons[slotIndex].color = ItemsSprite.RarityColor[indexOfRarity];
                    }
                }

            }
        }
    }

    public Color GetItemMaterialColor(string slot)
    {
        var itemIndex = SlotsTypeOrder.IndexOf((ItemType)Enum.Parse(typeof(ItemType), slot));
        return ItemIcons[itemIndex].color;
    }

    public Sprite GetItemSprite(string slot)
    {
        var itemIndex = SlotsTypeOrder.IndexOf((ItemType)Enum.Parse(typeof(ItemType), slot));
        return ItemIcons[itemIndex].sprite;
    }

    public string GetItemPower(string slot)
    {
        var itemIndex = SlotsTypeOrder.IndexOf((ItemType)Enum.Parse(typeof(ItemType), slot));
        return SlotsPower[itemIndex].text;
    }

    internal Sprite GetSlotSprite(string slot)
    {
        var itemIndex = SlotsTypeOrder.IndexOf((ItemType)Enum.Parse(typeof(ItemType), slot));
        return SlotIcons[itemIndex].sprite;
    }
}
