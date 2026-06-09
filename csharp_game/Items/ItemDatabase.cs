using System;
using System.Collections.Generic;

namespace CSharpGame
{
    public static class ItemDatabase
    {
        private static readonly List<string> PrefixesCommon = new() { "Rỉ Sét", "Cũ Kỹ", "Bình Thường", "Sứt Mẻ" };
        private static readonly List<string> PrefixesUncommon = new() { "Sắc Bén", "Gia Cố", "Chiến Sĩ", "Sắt Luyện" };
        private static readonly List<string> PrefixesRare = new() { "Băng Giá", "U Tối", "Tinh Nhuệ", "Hào Quang" };
        private static readonly List<string> PrefixesEpic = new() { "Hỏa Ngục", "Hư Không", "Cuồng Bạo", "Cổ Đại" };
        private static readonly List<string> PrefixesLegendary = new() { "Rồng Thiêng", "Thần Thánh", "Vô Song", "Huyền Thoại" };
        private static readonly List<string> PrefixesUnique = new() { "Độc Nhất", "Chí Tôn", "Khải Huyền", "Vương Giả" };

        private static readonly List<string> Suffixes = new() { "Phục Hận", "Chí Mạng", "Cuồng Phong", "Bão Tố", "Hủy Diệt", "Nhà Vua", "Kẻ Thách Thức" };

        private static readonly List<string> EquipmentSlots = new() { "Helmet", "Armor", "Gloves", "Boots", "Ring", "Necklace", "Weapon" };

        public static Item GenerateStartingWeapon(string type)
        {
            ItemData data = type.ToLower() switch
            {
                "bow" => new ItemData
                {
                    Id = "starter_bow",
                    Name = "Cung Tập Sự",
                    SlotType = "Weapon",
                    AttackType = "ranged_arrow",
                    BaseDamage = 12,
                    BaseRange = 380,
                    BaseSpeed = 10f,
                    Rarity = "Common"
                },
                "wand" => new ItemData
                {
                    Id = "starter_wand",
                    Name = "Trượng Tập Sự",
                    SlotType = "Weapon",
                    AttackType = "ranged_magic",
                    BaseDamage = 15,
                    BaseRange = 320,
                    BaseSpeed = 6.5f,
                    BaseSplashRadius = 35,
                    Rarity = "Common"
                },
                _ => new ItemData
                {
                    Id = "starter_sword",
                    Name = "Kiếm Tập Sự",
                    SlotType = "Weapon",
                    AttackType = "melee",
                    BaseDamage = 20,
                    BaseRange = 65,
                    Rarity = "Common"
                }
            };
            return new Item(data);
        }

        public static Item GenerateRandomEquipment(uint seed, int floor)
        {
            SeededRandom rand = new SeededRandom(seed);
            
            // 1. Roll Rarity
            float roll = rand.NextFloat();
            string rarity = "Common";
            if (roll < 0.005f) rarity = "Unique";
            else if (roll < 0.03f) rarity = "Legendary";
            else if (roll < 0.12f) rarity = "Epic";
            else if (roll < 0.30f) rarity = "Rare";
            else if (roll < 0.55f) rarity = "Uncommon";

            // 2. Roll Slot
            string slot = rand.Choice(EquipmentSlots) ?? "Weapon";

            // 3. Multipliers based on floor and rarity
            float floorMult = 1.0f + (floor - 1) * 0.15f;
            float rarityMult = rarity switch
            {
                "Uncommon" => 1.15f,
                "Rare" => 1.35f,
                "Epic" => 1.6f,
                "Legendary" => 2.0f,
                "Unique" => 2.7f,
                _ => 1.0f
            };

            ItemData data = new ItemData
            {
                Id = $"{slot.ToLower()}_{rarity.ToLower()}_{seed}",
                Rarity = rarity,
                SlotType = slot
            };

            // Prefixes
            var prefixes = rarity switch
            {
                "Uncommon" => PrefixesUncommon,
                "Rare" => PrefixesRare,
                "Epic" => PrefixesEpic,
                "Legendary" => PrefixesLegendary,
                "Unique" => PrefixesUnique,
                _ => PrefixesCommon
            };

            string? prefix = rand.Choice(prefixes);
            string? suffix = rand.Choice(Suffixes);

            if (slot == "Weapon")
            {
                // Weapon type: 0 = Sword, 1 = Bow, 2 = Wand
                int wType = rand.NextInt(0, 2);
                if (wType == 0)
                {
                    data.Name = "Kiếm Ngắn";
                    data.AttackType = "melee";
                    data.BaseDamage = (int)(18 * floorMult * rarityMult);
                    data.BaseRange = 65;
                }
                else if (wType == 1)
                {
                    data.Name = "Đoản Cung";
                    data.AttackType = "ranged_arrow";
                    data.BaseDamage = (int)(11 * floorMult * rarityMult);
                    data.BaseRange = 380;
                    data.BaseSpeed = 10f;
                }
                else
                {
                    data.Name = "Vương Trượng";
                    data.AttackType = "ranged_magic";
                    data.BaseDamage = (int)(14 * floorMult * rarityMult);
                    data.BaseRange = 320;
                    data.BaseSpeed = 6.5f;
                    data.BaseSplashRadius = 40;
                }
            }
            else if (slot == "Helmet" || slot == "Armor" || slot == "Gloves" || slot == "Boots")
            {
                string baseName = slot switch
                {
                    "Helmet" => "Mũ",
                    "Armor" => "Giáp Ngực",
                    "Gloves" => "Bao Tay",
                    "Boots" => "Giày Bốt",
                    _ => "Trang Bị"
                };
                data.Name = baseName;
                data.BaseDefense = (1.0f + (floor - 1) * 0.5f) * rarityMult;
                
                if (slot == "Boots") data.BaseSpeed = 0.5f;
            }
            else // Accessories (Ring, Necklace)
            {
                string baseName = slot == "Ring" ? "Nhẫn Ngọc" : "Dây Chuyền";
                data.Name = baseName;
                data.BaseCritChance = 0.02f + rand.NextInt(1, 10) / 100f; // +2% -> +12%
            }

            // Stat Bonuses for non-common items
            if (rarity != "Common")
            {
                int statVal = (int)Math.Max(1, Math.Round(2 * floorMult * rarityMult));
                int rStat = rand.NextInt(0, 4);
                switch (rStat)
                {
                    case 0: data.VigorBonus = statVal; break;
                    case 1: data.StrengthBonus = statVal; break;
                    case 2: data.DexterityBonus = statVal; break;
                    case 3: data.IntelligenceBonus = statVal; break;
                    case 4: data.VitalityBonus = statVal; break;
                }
            }

            Item item = new Item(data)
            {
                Prefix = prefix ?? "",
                Suffix = suffix ?? ""
            };

            return item;
        }
    }
}
