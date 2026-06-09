using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace CSharpGame
{
    public class Player : Entity
    {
        public string Name { get; set; } = "Kirito";
        public int Level { get; set; } = 1;
        public float Xp { get; set; } = 0f;
        public int Gold { get; set; } = 0;

        // Attributes
        public int Vigor { get; set; } = 10;
        public int Strength { get; set; } = 10;
        public int Dexterity { get; set; } = 10;
        public int Intelligence { get; set; } = 10;
        public int Vitality { get; set; } = 10;

        // Effective combat stats (base + equipment)
        public int EffVigor { get; private set; }
        public int EffStrength { get; private set; }
        public int EffDexterity { get; private set; }
        public int EffIntelligence { get; private set; }
        public int EffVitality { get; private set; }

        public float Mp { get; set; } = 100f;
        public float MaxMp { get; set; } = 100f;
        public int FreeStatPoints { get; set; } = 0;

        public float DamageMultiplier { get; set; } = 1f;
        public float CritChance { get; set; } = 0.05f;
        public float Defense { get; set; } = 0f;

        public int AttackCooldown { get; set; } = 0;
        public int DashCooldown { get; set; } = 0;
        public int DashTimer { get; set; } = 0;
        public bool IsDashing { get; set; } = false;
        public Vector2 DashDir { get; set; } = Vector2.Zero;

        public float Angle { get; set; } = 0f;

        public Inventory Inventory { get; set; } = new Inventory();
        public Equipment Equipment { get; set; } = new Equipment();

        public Player()
        {
            Radius = 16f;
            RecalculateAttributes();
            Hp = MaxHp;
            Mp = MaxMp;
        }

        public void RecalculateAttributes()
        {
            // Clear bonuses and reset to base stats
            int addVigor = 0;
            int addStrength = 0;
            int addDexterity = 0;
            int addIntelligence = 0;
            int addVitality = 0;

            float addDefense = 0f;
            float addCrit = 0f;
            float addSpeed = 0f;

            // Scan equipped items
            void AddItemStats(Item? item)
            {
                if (item == null) return;
                addVigor += item.VigorBonus;
                addStrength += item.StrengthBonus;
                addDexterity += item.DexterityBonus;
                addIntelligence += item.IntelligenceBonus;
                addVitality += item.VitalityBonus;

                addDefense += item.Defense;
                addCrit += item.CritChance;
                addSpeed += item.Speed;
            }

            AddItemStats(Equipment.Weapon);
            AddItemStats(Equipment.Helmet);
            AddItemStats(Equipment.Armor);
            AddItemStats(Equipment.Gloves);
            AddItemStats(Equipment.Boots);
            AddItemStats(Equipment.Ring1);
            AddItemStats(Equipment.Ring2);
            AddItemStats(Equipment.Necklace);

            // Compute effective stats
            EffVigor = Vigor + addVigor;
            EffStrength = Strength + addStrength;
            EffDexterity = Dexterity + addDexterity;
            EffIntelligence = Intelligence + addIntelligence;
            EffVitality = Vitality + addVitality;

            // Stats formulas
            MaxHp = 50 + EffVigor * 10;
            MaxMp = 40 + EffIntelligence * 8;
            DamageMultiplier = 1.0f + EffStrength * 0.02f;
            CritChance = 0.05f + EffDexterity * 0.005f + addCrit;
            Defense = EffVitality * 0.5f + addDefense;
            Speed = 4.0f + EffDexterity * 0.02f + addSpeed;
        }

        public void AllocateStat(string stat)
        {
            if (FreeStatPoints <= 0) return;

            switch (stat.ToLower())
            {
                case "vigor": Vigor++; break;
                case "strength": Strength++; break;
                case "dexterity": Dexterity++; break;
                case "intelligence": Intelligence++; break;
                case "vitality": Vitality++; break;
                default: return; // Invalid stat
            }

            FreeStatPoints--;
            float oldMaxHp = MaxHp;
            float oldMaxMp = MaxMp;

            RecalculateAttributes();

            // Carry over HP and MP increases
            if (MaxHp > oldMaxHp) Hp += (MaxHp - oldMaxHp);
            if (MaxMp > oldMaxMp) Mp += (MaxMp - oldMaxMp);
        }

        public void GainXp(float amount)
        {
            Xp += amount;
            float needed = (float)Math.Round(100 * Math.Pow(Level, 1.5));
            if (Xp >= needed)
            {
                Level++;
                Xp -= needed;
                FreeStatPoints += 5;
                RecalculateAttributes();
                Hp = MaxHp;
                Mp = MaxMp;
            }
        }

        public override void TakeDamage(float amount, float knockbackX = 0f, float knockbackY = 0f)
        {
            float finalDmg = Math.Max(1f, amount - Defense);
            Hp = Math.Max(0f, Hp - finalDmg);
            Kx += knockbackX;
            Ky += knockbackY;
        }

        public override void Update(DungeonData dungeon)
        {
            base.Update(dungeon);

            if (AttackCooldown > 0) AttackCooldown--;
            if (DashCooldown > 0) DashCooldown--;

            if (IsDashing)
            {
                DashTimer--;
                float dashSpeed = 11f;
                float newX = X + DashDir.X * dashSpeed;
                float newY = Y + DashDir.Y * dashSpeed;

                if (!dungeon.IsWallCollision(newX, Y, Radius)) X = newX;
                if (!dungeon.IsWallCollision(X, newY, Radius)) Y = newY;

                if (DashTimer <= 0) IsDashing = false;
            }
        }
    }
}
