using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerEquip : MonoBehaviour
{
    public List<ItemType> SlotsTypeOrder = new List<ItemType>();
    public List<Image> ItemIcons = new List<Image>();
    public List<Image> SlotIcons = new List<Image>();
    public List<TextMeshProUGUI>  SlotsPower = new List<TextMeshProUGUI>();

    private void Start()
    {
        PlayerStats.Instance.PlayerStatsChanged += SetStartPLayerItem;
    }

    private void SetStartPLayerItem()
    {
    }

    public void SetNewPlayerItem(Item item)
    {
    
    }

}
