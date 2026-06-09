namespace CSharpGame
{
    /// <summary>
    /// Represents a modular portal destination (usable for Dungeon, Boss Arena, Secret Rooms, etc.)
    /// </summary>
    public class PortalDestination
    {
        public int Floor { get; set; } = 1;
        public uint Seed { get; set; } = 0;
        public float X { get; set; } = 0f;
        public float Y { get; set; } = 0f;
        public bool IsActive { get; set; } = false;

        public void Clear()
        {
            Floor = 1;
            Seed = 0;
            X = 0f;
            Y = 0f;
            IsActive = false;
        }

        public void Set(int floor, uint seed, float x, float y)
        {
            Floor = floor;
            Seed = seed;
            X = x;
            Y = y;
            IsActive = true;
        }
    }
}
