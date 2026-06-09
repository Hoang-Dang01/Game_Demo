namespace CSharpGame
{
    public static class PlayerSave
    {
        public static void Save(Player player, SaveData data)
        {
            data.Level = player.Level;
            data.Xp = player.Xp;
            data.Gold = player.Gold;
            data.ClassType = player.ClassType;
            data.Vigor = player.Vigor;
            data.Strength = player.Strength;
            data.Dexterity = player.Dexterity;
            data.Intelligence = player.Intelligence;
            data.Vitality = player.Vitality;
            data.FreeStatPoints = player.FreeStatPoints;
            data.Inventory = player.Inventory.Items;
            data.Weapon = player.Equipment.Weapon;
            data.Helmet = player.Equipment.Helmet;
            data.Armor = player.Equipment.Armor;
            data.Gloves = player.Equipment.Gloves;
            data.Boots = player.Equipment.Boots;
            data.Ring1 = player.Equipment.Ring1;
            data.Ring2 = player.Equipment.Ring2;
            data.Necklace = player.Equipment.Necklace;
        }

        public static void Load(Player player, SaveData data)
        {
            player.Level = data.Level;
            player.Xp = data.Xp;
            player.Gold = data.Gold;
            player.ClassType = data.ClassType ?? "knight";
            player.Vigor = data.Vigor;
            player.Strength = data.Strength;
            player.Dexterity = data.Dexterity;
            player.Intelligence = data.Intelligence;
            player.Vitality = data.Vitality;
            player.FreeStatPoints = data.FreeStatPoints;
            player.Inventory.Items = data.Inventory ?? new();
            player.Equipment.Weapon = data.Weapon;
            player.Equipment.Helmet = data.Helmet;
            player.Equipment.Armor = data.Armor;
            player.Equipment.Gloves = data.Gloves;
            player.Equipment.Boots = data.Boots;
            player.Equipment.Ring1 = data.Ring1;
            player.Equipment.Ring2 = data.Ring2;
            player.Equipment.Necklace = data.Necklace;
            player.RecalculateAttributes();
        }
    }
}
