using System;

namespace CSharpGame
{
    public abstract class Entity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public float X { get; set; }
        public float Y { get; set; }
        public float Radius { get; set; } = 16f;
        public float Hp { get; set; } = 100f;
        public float MaxHp { get; set; } = 100f;
        public float Speed { get; set; } = 4f;

        // Knockback physics
        public float Kx { get; set; } = 0f;
        public float Ky { get; set; } = 0f;
        public float KnockbackDecay { get; set; } = 0.85f;

        public virtual void Update(DungeonData dungeon)
        {
            // Apply knockback movement
            if (Math.Abs(Kx) > 0.05f || Math.Abs(Ky) > 0.05f)
            {
                float newX = X + Kx;
                float newY = Y + Ky;

                if (!dungeon.IsWallCollision(newX, Y, Radius)) X = newX;
                if (!dungeon.IsWallCollision(X, newY, Radius)) Y = newY;

                Kx *= KnockbackDecay;
                Ky *= KnockbackDecay;
            }
        }

        public virtual void TakeDamage(float amount, float knockbackX = 0f, float knockbackY = 0f)
        {
            Hp = Math.Max(0f, Hp - amount);
            Kx += knockbackX;
            Ky += knockbackY;
        }
    }
}
