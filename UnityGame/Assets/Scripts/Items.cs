using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Items", menuName = "Items")]
public class Items : ScriptableObject
{
    public List<Item> AllItems = new List<Item>();
   // public List<TestItem> TestAllItems = new List<TestItem>();
}

[System.Serializable]
public class TestItem
{
    public string Name;
    public string ID;
    public int Power;
    public ItemQuality Quality;
    public ItemType Type;
    public Sprite Sprite;
}