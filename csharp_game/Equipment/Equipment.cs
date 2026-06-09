namespace CSharpGame
{
    public class Equipment
    {
        public Item? Weapon { get; set; }
        public Item? Helmet { get; set; }
        public Item? Armor { get; set; }
        public Item? Gloves { get; set; }
        public Item? Boots { get; set; }
        public Item? Ring1 { get; set; }
        public Item? Ring2 { get; set; }
        public Item? Necklace { get; set; }

        /// <summary>
        /// Equip an item to the given slot. Returns the previously equipped item (or null).
        /// </summary>
        public Item? Equip(Item item, string slot)
        {
            Item? oldItem = slot.ToLower() switch
            {
                "weapon"   => Weapon,
                "helmet"   => Helmet,
                "armor"    => Armor,
                "gloves"   => Gloves,
                "boots"    => Boots,
                "ring1"    => Ring1,
                "ring2"    => Ring2,
                "necklace" => Necklace,
                _          => null
            };

            switch (slot.ToLower())
            {
                case "weapon":   Weapon   = item; break;
                case "helmet":   Helmet   = item; break;
                case "armor":    Armor    = item; break;
                case "gloves":   Gloves   = item; break;
                case "boots":    Boots    = item; break;
                case "ring1":    Ring1    = item; break;
                case "ring2":    Ring2    = item; break;
                case "necklace": Necklace = item; break;
            }

            return oldItem;
        }

        /// <summary>
        /// Unequip the item in the given slot. Returns the removed item (or null if slot was empty).
        /// </summary>
        public Item? Unequip(string slot)
        {
            Item? oldItem = slot.ToLower() switch
            {
                "weapon"   => Weapon,
                "helmet"   => Helmet,
                "armor"    => Armor,
                "gloves"   => Gloves,
                "boots"    => Boots,
                "ring1"    => Ring1,
                "ring2"    => Ring2,
                "necklace" => Necklace,
                _          => null
            };

            switch (slot.ToLower())
            {
                case "weapon":   Weapon   = null; break;
                case "helmet":   Helmet   = null; break;
                case "armor":    Armor    = null; break;
                case "gloves":   Gloves   = null; break;
                case "boots":    Boots    = null; break;
                case "ring1":    Ring1    = null; break;
                case "ring2":    Ring2    = null; break;
                case "necklace": Necklace = null; break;
            }

            return oldItem;
        }

        /// <summary>Total defense from all equipped pieces.</summary>
        public float GetTotalDefense()
        {
            float def = 0f;
            if (Helmet   != null) def += Helmet.Defense;
            if (Armor    != null) def += Armor.Defense;
            if (Gloves   != null) def += Gloves.Defense;
            if (Boots    != null) def += Boots.Defense;
            if (Ring1    != null) def += Ring1.Defense;
            if (Ring2    != null) def += Ring2.Defense;
            if (Necklace != null) def += Necklace.Defense;
            return def;
        }
    }
}
