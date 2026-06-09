using System;

namespace CSharpGame
{
    public class Enemy : Entity
    {
        public string EnemyType { get; set; } = "melee"; // "melee" or "ranged"
        public int ShootCooldown { get; set; } = 0;

        public Enemy()
        {
            Radius = 14f;
        }

        public virtual void UpdateAI(Player player, DungeonData dungeon)
        {
            // Find distance to player
            float dx = player.X - X;
            float dy = player.Y - Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            if (dist < 350f) // Detection range
            {
                float ax = dist > 0.1f ? dx / dist : 0f;
                float ay = dist > 0.1f ? dy / dist : 0f;

                if (EnemyType == "melee")
                {
                    // Chase directly
                    float mx = ax * Speed;
                    float my = ay * Speed;

                    // Contact damage
                    if (dist <= (Radius + player.Radius) && !player.IsDashing)
                    {
                        player.TakeDamage(10f, ax * 10f, ay * 10f);
                    }

                    float newX = X + mx + Kx;
                    float newY = Y + my + Ky;

                    if (!dungeon.IsWallCollision(newX, Y, Radius)) X = newX;
                    if (!dungeon.IsWallCollision(X, newY, Radius)) Y = newY;
                }
                else // Ranged
                {
                    float mx = 0f, my = 0f;
                    if (dist > 180f)
                    {
                        mx = ax * Speed;
                        my = ay * Speed;
                    }
                    else if (dist < 140f)
                    {
                        mx = -ax * Speed;
                        my = -ay * Speed;
                    }

                    float newX = X + mx + Kx;
                    float newY = Y + my + Ky;

                    if (!dungeon.IsWallCollision(newX, Y, Radius)) X = newX;
                    if (!dungeon.IsWallCollision(X, newY, Radius)) Y = newY;

                    // Shoot AI
                    if (ShootCooldown > 0) ShootCooldown--;
                    if (ShootCooldown == 0)
                    {
                        ShootCooldown = 80 + new SeededRandom((uint)DateTime.Now.Ticks).NextInt(0, 40);
                        // Shot will be handled in the main game loop
                    }
                }
            }

            // Decay knockbacks
            Kx *= KnockbackDecay;
            Ky *= KnockbackDecay;
            if (Math.Abs(Kx) < 0.05f) Kx = 0f;
            if (Math.Abs(Ky) < 0.05f) Ky = 0f;
        }
    }
}
