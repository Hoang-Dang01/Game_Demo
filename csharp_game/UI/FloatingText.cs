using Raylib_cs;

namespace CSharpGame
{
    public class FloatingText
    {
        public float X { get; set; }
        public float Y { get; set; }
        public string Text { get; set; } = string.Empty;
        public Color Color { get; set; }
        public int Lifetime { get; set; }
        public int MaxLifetime { get; set; }
    }
}
