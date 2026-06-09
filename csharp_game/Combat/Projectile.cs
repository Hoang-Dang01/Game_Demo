using System;
using Raylib_cs;

namespace CSharpGame
{
    public class Projectile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public float X { get; set; }
        public float Y { get; set; }
        public float Angle { get; set; }
        public float Speed { get; set; }
        public float Damage { get; set; }
        public bool IsPlayer { get; set; }
        public float Radius { get; set; } = 5f;
        public Color Color { get; set; } = Color.White;
        public int SplashRadius { get; set; } = 0;
        public float CritChance { get; set; } = 0.05f;
        public bool Active { get; set; } = true;

        public void Update(DungeonData dungeon)
        {
            X += (float)Math.Cos(Angle) * Speed;
            Y += (float)Math.Sin(Angle) * Speed;

            if (dungeon.IsWallCollision(X, Y, Radius))
            {
                Active = false;
            }
        }
    }
}
