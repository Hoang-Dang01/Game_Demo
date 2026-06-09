using System.Collections.Generic;

namespace CSharpGame
{
    /// <summary>
    /// Runtime state for the Sanctuary hub (Floor 0).
    /// Tracks storage chest, portal stone count, and fast HP regen.
    /// </summary>
    public class SanctuaryManager
    {
        // ── State ─────────────────────────────────────────────────────────
        public bool IsActive { get; set; } = false;

        // Storage Chest — shared between runs
        public List<Item> StorageItems { get; set; } = new List<Item>();

        // Portal Stones held in sanctuary context (mirrors player count)
        public int PortalStoneCount { get; set; } = 3;

        // ── HP Regen ─────────────────────────────────────────────────────
        private const float HpRegenPerTick = 0.5f; // per frame (60fps ≈ 30HP/s)

        // ── NPC Interaction flags ─────────────────────────────────────────
        public bool NearBlacksmith { get; set; } = false;
        public bool NearStorageChest { get; set; } = false;
        public bool NearPortal { get; set; } = false;

        // ── Enter / Exit ─────────────────────────────────────────────────
        public void EnterSanctuary(Player player)
        {
            IsActive = true;
            // Full restore on arrival
            player.Hp = player.MaxHp;
            player.Mp = player.MaxMp;
        }

        public void ExitSanctuary()
        {
            IsActive = false;
            NearBlacksmith  = false;
            NearStorageChest = false;
            NearPortal      = false;
        }

        // ── Per-Frame Update (call every frame while IsActive) ────────────
        public void Update(Player player)
        {
            if (!IsActive || player.Hp <= 0) return;

            // Passive regen — faster than dungeon
            if (player.Hp < player.MaxHp)
                player.Hp = System.Math.Min(player.MaxHp, player.Hp + HpRegenPerTick);
            if (player.Mp < player.MaxMp)
                player.Mp = System.Math.Min(player.MaxMp, player.Mp + HpRegenPerTick * 1.5f);
        }

        // ── Proximity check (call after player moves) ────────────────────
        public void CheckProximity(Player player, DungeonData dungeon)
        {
            // Positions from dungeon data — stored in MerchantSpawn (Blacksmith)
            // ShopPedestals[0] = Storage Chest, ShopPedestals[1] = Portal Stone
            NearBlacksmith   = dungeon.MerchantSpawn.HasValue &&
                               System.Numerics.Vector2.Distance(
                                   new System.Numerics.Vector2(player.X, player.Y),
                                   new System.Numerics.Vector2(dungeon.MerchantSpawn.Value.X, dungeon.MerchantSpawn.Value.Y)
                               ) < 60f;

            NearStorageChest = dungeon.ShopPedestals.Count > 0 &&
                               System.Numerics.Vector2.Distance(
                                   new System.Numerics.Vector2(player.X, player.Y),
                                   new System.Numerics.Vector2(dungeon.ShopPedestals[0].X, dungeon.ShopPedestals[0].Y)
                               ) < 55f;

            NearPortal       = dungeon.ShopPedestals.Count > 1 &&
                               System.Numerics.Vector2.Distance(
                                   new System.Numerics.Vector2(player.X, player.Y),
                                   new System.Numerics.Vector2(dungeon.ShopPedestals[1].X, dungeon.ShopPedestals[1].Y)
                               ) < 60f;
        }
    }
}
