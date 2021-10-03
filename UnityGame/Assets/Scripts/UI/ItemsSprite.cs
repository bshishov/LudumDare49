using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    [CreateAssetMenu(fileName = "New Items", menuName = "Item Images")]
    public class ItemsSprite : ScriptableObject
    {
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
}