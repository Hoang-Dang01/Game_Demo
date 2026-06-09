using System;
using Raylib_cs;

namespace CSharpGame
{
    public class Item
    {
        public Guid InstanceId { get; set; } = Guid.NewGuid();
        public ItemData Data { get; set; } = new ItemData();

        // Dynamic stats (prefix/suffix modifiers, upgrade levels)
        public string Prefix { get; set; } = "";
        public string Suffix { get; set; } = "";
        public int UpgradeLevel { get; set; } = 0;

        // Effective Stats (base data + upgrades + affixes)
        public string Name
        {
            get => $"{(string.IsNullOrEmpty(Prefix) ? "" : Prefix + " ")}{Data.Name}{(string.IsNullOrEmpty(Suffix) ? "" : " của " + Suffix)}{(UpgradeLevel > 0 ? $" +{UpgradeLevel}" : "")}";
            set => Data.Name = value;
        }

        public string ItemType
        {
            get => Data.ItemType;
            set => Data.ItemType = value;
        }

        public string Rarity
        {
            get => Data.Rarity;
            set => Data.Rarity = value;
        }

        public string SlotType
        {
            get => Data.SlotType;
            set => Data.SlotType = value;
        }

        public string AttackType
        {
            get => Data.AttackType;
            set => Data.AttackType = value;
        }

        public string AllowedClass
        {
            get => Data.AllowedClass;
            set => Data.AllowedClass = value;
        }

        public int Damage => (int)Math.Round((Data.BaseDamage + UpgradeLevel * 3) * (Prefix == "Sắc Bén" || Suffix == "Hủy Diệt" ? 1.2f : 1.0f));
        public float Defense => Data.BaseDefense + UpgradeLevel * 1f;
        public float CritChance => Data.BaseCritChance + (Suffix == "Chí Mạng" ? 0.05f : 0f);
        public int Range => Data.BaseRange;
        public float Speed => Data.BaseSpeed;
        public int SplashRadius => Data.BaseSplashRadius;

        public int VigorBonus => Data.VigorBonus;
        public int StrengthBonus => Data.StrengthBonus;
        public int DexterityBonus => Data.DexterityBonus;
        public int IntelligenceBonus => Data.IntelligenceBonus;
        public int VitalityBonus => Data.VitalityBonus;

        public int Val { get; set; } = 0; // gold, exp value or heart heal amount

        // World Position (when dropped)
        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public float Radius { get; set; } = 8f;

        public Color RarityColor => Config.GetRarityColor(Rarity);

        public Item()
        {
        }

        public Item(ItemData data)
        {
            Data = data;
        }

        public Item Clone()
        {
            var clone = (Item)this.MemberwiseClone();
            clone.InstanceId = Guid.NewGuid();
            return clone;
        }
    }
}
