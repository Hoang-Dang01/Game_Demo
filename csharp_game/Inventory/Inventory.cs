using System.Collections.Generic;

namespace CSharpGame
{
    public class Inventory
    {
        public const int MaxSlots = 20;
        public List<Item> Items { get; set; } = new List<Item>();

        public bool AddItem(Item item)
        {
            if (Items.Count >= MaxSlots) return false;
            Items.Add(item);
            return true;
        }

        public void RemoveItem(Item item)
        {
            Items.Remove(item);
        }

        public int PortalStoneCount
        {
            get
            {
                int count = 0;
                foreach (var item in Items)
                {
                    if (item.ItemType == "portal_stone") count++;
                }
                return count;
            }
        }

        public bool ConsumePortalStone()
        {
            var stone = Items.Find(i => i.ItemType == "portal_stone");
            if (stone != null)
            {
                Items.Remove(stone);
                return true;
            }
            return false;
        }
    }
}
