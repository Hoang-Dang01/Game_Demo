namespace CSharpGame
{
    /// <summary>
    /// Manages portal travel between Dungeon and Sanctuary.
    ///
    /// Rules (per SANCTUARY_DESIGN.md):
    ///   Dungeon → Sanctuary : free in v1 (key T)
    ///   Sanctuary → Dungeon : free, returns to saved floor/pos (key E at portal)
    /// </summary>
    public class PortalSystem
    {
        // ── Return State (written when Dungeon→Sanctuary) ─────────────────
        public PortalDestination ReturnPortal { get; private set; } = new PortalDestination();

        // ── Dungeon → Sanctuary ───────────────────────────────────────────
        /// <summary>
        /// Attempt to portal to Sanctuary. Consumes 1 Portal Stone from inventory.
        /// </summary>
        public bool PortalToSanctuary(Player player, SanctuaryManager sanctuary,
                                      int currentFloor, uint currentSeed)
        {
            // Try to consume portal stone
            if (!player.Inventory.ConsumePortalStone())
            {
                return false;
            }
            
            // Save return coordinates
            ReturnPortal.Set(currentFloor, currentSeed, player.X, player.Y);

            sanctuary.EnterSanctuary(player);
            return true;
        }

        // ── Sanctuary → Dungeon ───────────────────────────────────────────
        /// <summary>
        /// Returns true if there is a saved dungeon position to return to.
        /// </summary>
        public bool CanReturnToDungeon() => ReturnPortal.IsActive;

        public void ReturnToDungeon(Player player, SanctuaryManager sanctuary)
        {
            sanctuary.ExitSanctuary();
        }

        // ── Save/Load ──────────────────────────────────────────────────────
        public void LoadFromSaveData(SaveData data)
        {
            ReturnPortal.Floor = data.ReturnPortal.Floor;
            ReturnPortal.Seed = data.ReturnPortal.Seed;
            ReturnPortal.X = data.ReturnPortal.X;
            ReturnPortal.Y = data.ReturnPortal.Y;
            ReturnPortal.IsActive = data.ReturnPortal.IsActive;
        }
    }
}
