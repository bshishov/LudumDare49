using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UI;

public class PlayerEquip : MonoBehaviour
{
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
        var rolledItems = PlayerStats.Instance.playerItems;
        foreach (var rolledItem in rolledItems)
        {
            for (int i = 0; i < SlotsTypeOrder.Count; i++)
            {
                if (rolledItem.item.slot == SlotsTypeOrder[i].ToString())
                {
                    SlotsPower[i].text = rolledItem.item.power.ToString();

                }
            }
        }
    }

}
