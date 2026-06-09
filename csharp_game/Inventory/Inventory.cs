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
    }
}
