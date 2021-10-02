using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public string ID;
    public int Power;
    public ItemQuality Quality;
    public ItemType Type;
    public Sprite Sprite;
}

public enum ItemType
{
    head,
    body,
    weapon,
    trinket,
    boots
}

public enum ItemQuality
{
    shitty,
    bad,
    normal,
    good,
    divine
}