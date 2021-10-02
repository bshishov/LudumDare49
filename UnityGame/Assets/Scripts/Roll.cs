using UnityEngine;
using UI;

public class Roll : MonoBehaviour
{
    public GameObject UIItem;
    public GameObject ItemSlot;
    public GameObject Price;
    public GameObject RollButton;

    private void Start()
    {
        ItemSlot.SetActive(false);
    }
    public void SetItem(Item item)
    {
        Price.SetActive(false);
        RollButton.SetActive(false);
        ItemSlot.SetActive(true);
        var uiItem = UIItem.GetComponent<UIItem>();
        uiItem.SetPowerText(item.Power);
        uiItem.SetImage(item.Sprite);
    }
}
