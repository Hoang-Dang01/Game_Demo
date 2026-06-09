namespace CSharpGame
{
    public class ItemData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "Vật Phẩm";
        public string ItemType { get; set; } = "equipment"; // "equipment", "gold", "heart", "exp", "material"
        public string Rarity { get; set; } = "Common"; // Common, Uncommon, Rare, Epic, Legendary, Unique
        public string SlotType { get; set; } = "Weapon"; // Weapon, Helmet, Armor, Gloves, Boots, Ring, Necklace, Material, Heart
        public string AttackType { get; set; } = "melee"; // "melee", "ranged_arrow", "ranged_magic"
        public string AllowedClass { get; set; } = "";

        // Base Stats
        public int BaseDamage { get; set; } = 0;
        public float BaseDefense { get; set; } = 0f;
        public float BaseCritChance { get; set; } = 0f;
        public int BaseRange { get; set; } = 60;
        public float BaseSpeed { get; set; } = 0f; // Projectile base speed
        public int BaseSplashRadius { get; set; } = 0;

        // Base Attributes Bonuses
        public int VigorBonus { get; set; } = 0;
        public int StrengthBonus { get; set; } = 0;
        public int DexterityBonus { get; set; } = 0;
        public int IntelligenceBonus { get; set; } = 0;
        public int VitalityBonus { get; set; } = 0;
    }
}
