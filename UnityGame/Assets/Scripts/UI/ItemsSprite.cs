using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu(fileName = "New Items", menuName = "Item Images")]
    public class ItemsSprite : ScriptableObject
    {
        public List<Color> RarityColor = new List<Color>();
        public List<ItemsImage> AllItems = new List<ItemsImage>();
    }

    [System.Serializable]
    public class ItemsImage
    {

        public Sprite Image;
        public string[] ID;
    }

    public enum ItemType
    {
        head,
        body,
        weapon,
        trinket,
        boots
    }

    public enum ItemRarity
    {
        common,
        uncommon,
        rare,
        epic,
        legendary
    }

    public enum ItemQuality
    {
        shitty,
        bad,
        normal,
        good,
        divine
    }
}