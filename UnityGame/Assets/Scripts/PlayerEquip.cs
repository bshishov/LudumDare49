using System.Collections.Generic;
using UnityEngine;
using UI;

public class PlayerEquip : MonoBehaviour
{
    public Item Head;
    public Item Weapon;
    public Item Trinket;
    public Item Body;
    public Item Boots;

    public GameObject Item;

    public List<ItemType> PlayerEquipeTypes = new List<ItemType>();

    public UITotalPower UITotalPower;

    private List<GameObject> _playerUIItems = new List<GameObject>();
    private List<Item> _playerItems = new List<Item>();

    private int _totalPower;

    private void Start()
    {
        SetStartPLayerItem();
    }

    private void SetStartPLayerItem()
    {
        foreach (var item in PlayerEquipeTypes)
        {
            var newItem = Instantiate(Item);
            newItem.transform.SetParent(transform);
            _playerUIItems.Add(newItem);
        }

        SetNewPlayerItem(Head);
        SetNewPlayerItem(Weapon);
        SetNewPlayerItem(Trinket);
        SetNewPlayerItem(Body);
        SetNewPlayerItem(Boots);
    }

    public void SetNewPlayerItem(Item item)
    {
        var setableItem = _playerUIItems[PlayerEquipeTypes.IndexOf(item.Type)];
        var uiItem = setableItem.GetComponent<UIItem>();
        uiItem.SetPowerText(item.Power);
        uiItem.SetImage(item.Sprite);

        _playerItems.Add(item);
        CalculateTotalPower();
    }

    private void CalculateTotalPower()
    {
        _totalPower = 0;

        foreach (var item in _playerItems)
        {
            _totalPower += item.Power;
        }

        UITotalPower.SetTotalPower(_totalPower);
    }
}
