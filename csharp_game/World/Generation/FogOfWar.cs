namespace CSharpGame
{
    /// <summary>
    /// Fog of War — exploration-based map revelation.
    ///
    /// Per-tile states:
    ///   0 = Unexplored  → render as solid black
    ///   1 = Explored    → render as dark tint (dim overlay)
    ///   2 = Visible     → no overlay (full brightness)
    ///
    /// Sanctuary (level == 0) always uses full visibility.
    /// </summary>
    public class FogOfWar
    {
        // ── Constants ────────────────────────────────────────────────────
        /// <summary>Player vision radius in tiles.</summary>
        public const int VisionRadius = 7;

        // Overlay alpha values (0–255)
        private const int AlphaUnexplored = 255;    // fully black
        private const int AlphaExplored   = 180;    // dark tint — you've been here
        // Visible tiles get 0 overlay drawn (full colour)

        // ── State ────────────────────────────────────────────────────────
        private byte[,] _state = new byte[0, 0]; // 0/1/2 per tile
        private int _mapW;
        private int _mapH;

        public bool IsInitialised => _mapW > 0 && _mapH > 0;

        // ── Init / Reset ─────────────────────────────────────────────────
        public void Init(int mapWidth, int mapHeight)
        {
            _mapW  = mapWidth;
            _mapH  = mapHeight;
            _state = new byte[mapHeight, mapWidth]; // all 0 = unexplored
        }

        public void Reset() => Init(_mapW, _mapH);

        // ── Update visibility around a world-space position ───────────────
        /// <summary>
        /// Mark tiles within VisionRadius of (worldX, worldY) as Visible (2),
        /// and previously-visible tiles as Explored (1).
        /// </summary>
        public void Update(float worldX, float worldY, DungeonData dungeon)
        {
            int tileSize = Config.TileSize;
            int cx = (int)(worldX / tileSize);
            int cy = (int)(worldY / tileSize);

            // Step 1: downgrade all currently Visible → Explored
            for (int y = System.Math.Max(0, cy - VisionRadius - 1);
                     y < System.Math.Min(_mapH, cy + VisionRadius + 2); y++)
            {
                for (int x = System.Math.Max(0, cx - VisionRadius - 1);
                         x < System.Math.Min(_mapW, cx + VisionRadius + 2); x++)
                {
                    if (_state[y, x] == 2) _state[y, x] = 1;
                }
            }

            // Step 2: reveal tiles in radius — simple circle, no LOS for now
            int r2 = VisionRadius * VisionRadius;
            for (int dy = -VisionRadius; dy <= VisionRadius; dy++)
            {
                for (int dx = -VisionRadius; dx <= VisionRadius; dx++)
                {
                    if (dx * dx + dy * dy > r2) continue;

                    int tx = cx + dx;
                    int ty = cy + dy;

                    if (tx < 0 || tx >= _mapW || ty < 0 || ty >= _mapH) continue;

                    _state[ty, tx] = 2; // Visible
                }
            }
        }

        // ── Query ─────────────────────────────────────────────────────────
        public byte GetState(int tileX, int tileY)
        {
            if (tileX < 0 || tileX >= _mapW || tileY < 0 || tileY >= _mapH)
                return 0;
            return _state[tileY, tileX];
        }

        /// <summary>Returns the alpha (0–255) of the darkness overlay for a tile.</summary>
        public int GetOverlayAlpha(int tileX, int tileY)
        {
            return GetState(tileX, tileY) switch
            {
                0 => AlphaUnexplored,
                1 => AlphaExplored,
                _ => 0             // Visible — no overlay
            };
        }

        /// <summary>True if a tile has been revealed at all (explored or visible).</summary>
        public bool IsRevealed(int tileX, int tileY) => GetState(tileX, tileY) > 0;

        /// <summary>True if currently in the player's sight cone.</summary>
        public bool IsVisible(int tileX, int tileY) => GetState(tileX, tileY) == 2;
    }
}
