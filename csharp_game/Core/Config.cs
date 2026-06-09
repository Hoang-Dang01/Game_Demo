using Raylib_cs;

namespace CSharpGame
{
    public static class Config
    {
        public const int ScreenWidth = 1024;
        public const int ScreenHeight = 768;
        public const int TileSize = 48;
        public const int TargetFps = 60;

        // Custom UI & Entity Colors
        public static readonly Color ColorBg = new Color(15, 15, 20, 255);
        public static readonly Color ColorWall = new Color(45, 45, 52, 255);
        public static readonly Color ColorWallHighlight = new Color(75, 75, 85, 255);
        public static readonly Color ColorWallShadow = new Color(30, 30, 35, 255);
        public static readonly Color ColorWallBorder = new Color(20, 20, 25, 255);
        
        public static readonly Color ColorFloor = new Color(24, 24, 28, 255);
        public static readonly Color ColorFloorSpeckle = new Color(32, 32, 38, 255);
        public static readonly Color ColorFloorBorder = new Color(18, 18, 22, 255);

        // HUD & Bars
        public static readonly Color ColorHp = new Color(220, 40, 40, 255);
        public static readonly Color ColorHpBg = new Color(60, 20, 20, 255);
        public static readonly Color ColorMana = new Color(33, 150, 243, 255);
        public static readonly Color ColorManaBg = new Color(15, 40, 60, 255);
        public static readonly Color ColorXp = new Color(76, 175, 80, 255);
        public static readonly Color ColorXpBg = new Color(20, 50, 25, 255);
        public static readonly Color ColorGold = new Color(240, 200, 30, 255);
        public static readonly Color ColorTextWhite = Color.White;

        // Item Rarities
        public static readonly Color RarityCommon = new Color(240, 240, 245, 255);
        public static readonly Color RarityUncommon = new Color(80, 220, 80, 255);
        public static readonly Color RarityRare = new Color(60, 150, 255, 255);
        public static readonly Color RarityEpic = new Color(180, 60, 220, 255);
        public static readonly Color RarityLegendary = new Color(255, 130, 40, 255);
        public static readonly Color RarityUnique = new Color(240, 200, 30, 255);

        public static Color GetRarityColor(string rarity)
        {
            return rarity switch
            {
                "Uncommon" => RarityUncommon,
                "Rare" => RarityRare,
                "Epic" => RarityEpic,
                "Legendary" => RarityLegendary,
                "Unique" => RarityUnique,
                _ => RarityCommon
            };
        }
    }
}
