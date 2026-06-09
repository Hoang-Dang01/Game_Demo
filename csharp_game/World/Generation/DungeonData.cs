using System;
using System.Collections.Generic;

namespace CSharpGame
{
    public struct Position
    {
        public int X;
        public int Y;
        public Position(int x, int y) { X = x; Y = y; }
    }

    public struct EnemySpawn
    {
        public int X;
        public int Y;
        public string Type;
        public EnemySpawn(int x, int y, string type) { X = x; Y = y; Type = type; }
    }

    public class Room
    {
        public int X, Y, W, H;
        public int Cx, Cy;

        public Room(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
            Cx = x + w / 2;
            Cy = y + h / 2;
        }
    }

    public class ChestState 
    { 
        public Position Pos { get; set; } 
        public bool Opened { get; set; } 
    }

    public class DungeonData
    {
        public int Width;
        public int Height;
        public int[,] Grid = new int[0, 0]; // 0 = Wall, 1 = Floor, 2 = Stairs
        public Position PlayerSpawn;
        public Position StairsSpawn;
        public Position? BossSpawn;
        public Position? MerchantSpawn;
        public List<Position> ShopPedestals = new();
        public List<Position> Chests = new();
        public List<EnemySpawn> Enemies = new();
        public List<Position> Lanterns = new();

        public bool IsWallCollision(float x, float y, float radius)
        {
            Position[] points = new Position[]
            {
                new Position((int)(x - radius), (int)(y - radius)),
                new Position((int)(x + radius), (int)(y - radius)),
                new Position((int)(x - radius), (int)(y + radius)),
                new Position((int)(x + radius), (int)(y + radius))
            };

            foreach (var p in points)
            {
                int gx = p.X / Config.TileSize;
                int gy = p.Y / Config.TileSize;

                if (gy < 0 || gy >= Grid.GetLength(0) || gx < 0 || gx >= Grid.GetLength(1))
                    return true;

                if (Grid[gy, gx] == 0) // 0 is Wall
                    return true;
            }

            return false;
        }
    }
}
