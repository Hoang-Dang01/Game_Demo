using System;

namespace CSharpGame
{
    public class Boss : Enemy
    {
        public bool Enraged { get; set; } = false;
        public int AttackPattern { get; set; } = 0;

        public Boss()
        {
            Radius = 28f;
            Hp = 500f;
            MaxHp = 500f;
            Speed = 1.8f;
            EnemyType = "melee";
            ShootCooldown = 80;
        }

        public override void UpdateAI(Player player, DungeonData dungeon)
        {
            if (Hp < MaxHp / 2f && !Enraged)
            {
                Enraged = true;
                Speed = 2.3f;
            }

            float dx = player.X - X;
            float dy = player.Y - Y;
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);

            float ax = dist > 0.1f ? dx / dist : 0f;
            float ay = dist > 0.1f ? dy / dist : 0f;

            // Continuous collision contact damage
            if (dist <= (Radius + player.Radius) && !player.IsDashing)
            {
                float finalBossDmg = Math.Max(0.3f, 1.5f - player.Defense * 0.05f);
                player.TakeDamage(finalBossDmg, ax * 3f, ay * 3f);
            }

            float mx = ax * Speed;
            float my = ay * Speed;

            float newX = X + mx + Kx;
            float newY = Y + my + Ky;

            if (!dungeon.IsWallCollision(newX, Y, Radius)) X = newX;
            if (!dungeon.IsWallCollision(X, newY, Radius)) Y = newY;

            if (ShootCooldown > 0) ShootCooldown--;

            // Decay knockbacks
            Kx *= KnockbackDecay;
            Ky *= KnockbackDecay;
            if (Math.Abs(Kx) < 0.05f) Kx = 0f;
            if (Math.Abs(Ky) < 0.05f) Ky = 0f;
        }
    }
}
