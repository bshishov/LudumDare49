using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UI;

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
    }

    private void SetPlayerItems()
    {
        //TODO fix this shit
        var rolledItems = PlayerStats.Instance.playerItems;
        foreach (var rolledItem in rolledItems)
        {
            for (int i = 0; i < SlotsTypeOrder.Count; i++)
            {
                if (rolledItem.item.slot == SlotsTypeOrder[i].ToString())
                {
                    SlotsPower[i].text = rolledItem.total_power.ToString();
                    for (int j = 0; j < ItemsSprite.AllItems.Count; j++)
                    {
                        for (int k = 0; k < ItemsSprite.AllItems[j].ID.Length; k++)
                        {

                            if (ItemsSprite.AllItems[j].ID[k] == rolledItem.item.id)
                            {
                                ItemIcons[i].sprite = ItemsSprite.AllItems[j].Image;
                                ItemIcons[i].material = RarityMaterial;
                            }
                        }

                    }
                }
            }
        }
    }

    public Sprite GetItemImage(string slot)
    {
        for (int i = 0; i < SlotsTypeOrder.Count; i++)
        {
            if (slot == SlotsTypeOrder[i].ToString())
            {
                return ItemIcons[i].sprite;
            }
        }
        return ItemIcons[0].sprite;
    }
    public string GetItemPower(string slot)
    {
        for (int i = 0; i < SlotsTypeOrder.Count; i++)
        {
            if (slot == SlotsTypeOrder[i].ToString())
            {
                return SlotsPower[i].text;
            }
        }
        return "";
    }

}
