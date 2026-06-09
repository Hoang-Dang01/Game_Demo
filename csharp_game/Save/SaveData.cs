using System.Collections.Generic;

namespace CSharpGame
{
    public class SaveData
    {
        // ── Player Stats ─────────────────────────────────────────────────
        public int Level { get; set; } = 1;
        public float Xp { get; set; } = 0f;
        public int Gold { get; set; } = 0;
        public string ClassType { get; set; } = "knight";

        public int Vigor { get; set; } = 10;
        public int Strength { get; set; } = 10;
        public int Dexterity { get; set; } = 10;
        public int Intelligence { get; set; } = 10;
        public int Vitality { get; set; } = 10;
        public int FreeStatPoints { get; set; } = 0;

        // ── World Progress ────────────────────────────────────────────────
        public int CurrentFloor { get; set; } = 1;
        public int HighestFloorReached { get; set; } = 1;

        // ── Portal Return State (saved when player uses Portal Stone) ─────
        public PortalDestination ReturnPortal { get; set; } = new PortalDestination();

        // ── Inventory & Equipment ─────────────────────────────────────────
        public List<Item> Inventory { get; set; } = new List<Item>();

        public Item? Weapon { get; set; }
        public Item? Helmet { get; set; }
        public Item? Armor { get; set; }
        public Item? Gloves { get; set; }
        public Item? Boots { get; set; }
        public Item? Ring1 { get; set; }
        public Item? Ring2 { get; set; }
        public Item? Necklace { get; set; }

        // ── Sanctuary Storage Chest ───────────────────────────────────────
        public List<Item> StorageItems { get; set; } = new List<Item>();

        // ── Portal Stones count (shortcut instead of scanning inventory) ──
        public int PortalStoneCount { get; set; } = 3; // start with 3 for testing
    }
}
