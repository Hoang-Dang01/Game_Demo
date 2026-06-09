using System;
using System.Collections.Generic;

namespace CSharpGame
{
    public class SeededRandom
    {
        private uint _state;

        public SeededRandom(uint seed)
        {
            _state = seed == 0 ? 1 : seed;
        }

        public float NextFloat()
        {
            unchecked
            {
                uint t = _state += 0x6D2B79F5;
                t = (t ^ (t >> 15)) * (t | 1);
                t ^= t + (t ^ (t >> 7)) * (t | 61);
                return (float)(t ^ (t >> 14)) / 4294967296.0f;
            }
        }

        public int NextInt(int min, int max)
        {
            return (int)Math.Floor(NextFloat() * (max - min + 1)) + min;
        }

        public T? Choice<T>(List<T> list)
        {
            if (list == null || list.Count == 0) return default;
            int idx = NextInt(0, list.Count - 1);
            return list[idx];
        }
    }

    public static class DungeonGenerator
    {
        public static DungeonData GenerateDungeon(uint seed, int level, int width = 45, int height = 45)
        {
            SeededRandom rand = new SeededRandom(seed);
            int[,] grid = new int[height, width];
            List<Room> rooms = new();

            int minRoomSize = 6;
            int maxRoomSize = 10;
            int maxRooms = 12;
            int tileSize = Config.TileSize;

            bool RectsOverlap(Room r1, Room r2)
            {
                return !(r1.X + r1.W + 1 < r2.X - 1 || 
                         r1.X - 1 > r2.X + r2.W + 1 || 
                         r1.Y + r1.H + 1 < r2.Y - 1 || 
                         r1.Y - 1 > r2.Y + r2.H + 1);
            }

            void CarveHLine(int x1, int x2, int y)
            {
                for (int x = Math.Min(x1, x2); x <= Math.Max(x1, x2); x++)
                {
                    grid[y, x] = 1;
                }
            }

            void CarveVLine(int y1, int y2, int x)
            {
                for (int y = Math.Min(y1, y2); y <= Math.Max(y1, y2); y++)
                {
                    grid[y, x] = 1;
                }
            }

            void ConnectRooms(Room r1, Room r2)
            {
                int x1 = r1.Cx, y1 = r1.Cy;
                int x2 = r2.Cx, y2 = r2.Cy;

                if (rand.NextFloat() < 0.5f)
                {
                    CarveHLine(x1, x2, y1);
                    CarveVLine(y1, y2, x2);
                }
                else
                {
                    CarveVLine(y1, y2, x1);
                    CarveHLine(x1, x2, y2);
                }
            }

            // Generate rooms
            for (int i = 0; i < maxRooms; i++)
            {
                int w = rand.NextInt(minRoomSize, maxRoomSize);
                int h = rand.NextInt(minRoomSize, maxRoomSize);
                int x = rand.NextInt(2, width - w - 3);
                int y = rand.NextInt(2, height - h - 3);

                Room room = new Room(x, y, w, h);

                bool overlap = false;
                foreach (var existingRoom in rooms)
                {
                    if (RectsOverlap(room, existingRoom))
                    {
                        overlap = true;
                        break;
                    }
                }

                if (!overlap)
                {
                    rooms.push_back_equivalent(room);
                    // Carve
                    for (int ry = room.Y; ry < room.Y + room.H; ry++)
                    {
                        for (int rx = room.X; rx < room.X + room.W; rx++)
                        {
                            grid[ry, rx] = 1;
                        }
                    }

                    // Connect
                    if (rooms.Count > 1)
                    {
                        Room prevRoom = rooms[rooms.Count - 2];
                        ConnectRooms(prevRoom, room);
                    }
                }
            }

            DungeonData result = new DungeonData
            {
                Width = width,
                Height = height,
                Grid = grid
            };

            if (rooms.Count >= 3)
            {
                // Room 0: Player
                Room first = rooms[0];
                result.PlayerSpawn = new Position(
                    first.Cx * tileSize + tileSize / 2,
                    first.Cy * tileSize + tileSize / 2
                );

                // Room 1: Sanctuary/Shop room (Safe zone, no enemies)
                Room shop = rooms[1];
                result.MerchantSpawn = new Position(
                    shop.Cx * tileSize + tileSize / 2,
                    (shop.Cy - 1) * tileSize + tileSize / 2
                );
                result.ShopPedestals.Add(new Position((shop.Cx - 1) * tileSize + tileSize / 2, (shop.Cy + 1) * tileSize + tileSize / 2));
                result.ShopPedestals.Add(new Position(shop.Cx * tileSize + tileSize / 2, (shop.Cy + 1) * tileSize + tileSize / 2));
                result.ShopPedestals.Add(new Position((shop.Cx + 1) * tileSize + tileSize / 2, (shop.Cy + 1) * tileSize + tileSize / 2));
                result.Lanterns.Add(new Position(shop.Cx * tileSize + tileSize / 2, shop.Cy * tileSize + tileSize / 2));

                // Room N-1: Exit Stairs & Boss room
                Room last = rooms[rooms.Count - 1];
                result.StairsSpawn = new Position(
                    last.Cx * tileSize + tileSize / 2,
                    last.Cy * tileSize + tileSize / 2
                );
                grid[last.Cy, last.Cx] = 2; // Stairs
                result.Lanterns.Add(new Position(last.Cx * tileSize + tileSize / 2, last.Cy * tileSize + tileSize / 2));

                if (level % 5 == 0)
                {
                    result.BossSpawn = new Position(
                        last.Cx * tileSize + tileSize / 2,
                        last.Cy * tileSize + tileSize / 2
                    );
                }

                // Room 2 to N-2: Combat rooms, Chests, Loot
                for (int i = 2; i < rooms.Count - 1; i++)
                {
                    Room room = rooms[i];
                    
                    if (rand.NextFloat() < 0.6f)
                    {
                        result.Chests.Add(new Position(
                            room.Cx * tileSize + tileSize / 2,
                            room.Cy * tileSize + tileSize / 2
                        ));
                    }

                    int numEnemies = rand.NextInt(1, 3);
                    for (int k = 0; k < numEnemies; k++)
                    {
                        int ex = rand.NextInt(room.X + 1, room.X + room.W - 2);
                        int yVal = rand.NextInt(room.Y + 1, room.Y + room.H - 2);

                        if (ex != room.Cx || yVal != room.Cy)
                        {
                            string enemyType = rand.NextFloat() < 0.7f ? "melee" : "ranged";
                            result.Enemies.Add(new EnemySpawn(
                                ex * tileSize + tileSize / 2,
                                yVal * tileSize + tileSize / 2,
                                enemyType
                            ));
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Generates the Sanctuary (Floor 0) — one safe room, no enemies.
        /// Tile 3 = safe floor (grass/wood colour, rendered differently).
        /// ShopPedestals[0] = Storage Chest position
        /// ShopPedestals[1] = Portal pedestal position
        /// MerchantSpawn    = Blacksmith position
        /// </summary>
        public static DungeonData GenerateSanctuary(int width = 25, int height = 25)
        {
            int[,] grid = new int[height, width];
            int tileSize = Config.TileSize;

            // Carve a single large room that fills most of the map
            int margin = 3;
            for (int y = margin; y < height - margin; y++)
                for (int x = margin; x < width - margin; x++)
                    grid[y, x] = 3; // tile 3 = safe floor

            int cx = width  / 2;
            int cy = height / 2;

            DungeonData result = new DungeonData
            {
                Width  = width,
                Height = height,
                Grid   = grid
            };

            // Player spawns in the bottom center (Spawn)
            result.PlayerSpawn = new Position(cx * tileSize + tileSize / 2,
                                              (cy + 4) * tileSize + tileSize / 2);

            // Blacksmith — left side
            result.MerchantSpawn = new Position((cx - 5) * tileSize + tileSize / 2,
                                                cy * tileSize + tileSize / 2);

            // Storage Chest — right side (ShopPedestals[0])
            result.ShopPedestals.Add(new Position((cx + 5) * tileSize + tileSize / 2,
                                                  cy * tileSize + tileSize / 2));

            // Portal pedestal — top center (ShopPedestals[1])
            result.ShopPedestals.Add(new Position(cx * tileSize + tileSize / 2,
                                                  (cy - 4) * tileSize + tileSize / 2));

            // Lanterns for ambience
            if (result.MerchantSpawn.HasValue)
                result.Lanterns.Add(result.MerchantSpawn.Value);
            result.Lanterns.Add(new Position(result.ShopPedestals[0].X, result.ShopPedestals[0].Y));
            result.Lanterns.Add(new Position(result.ShopPedestals[1].X, result.ShopPedestals[1].Y));

            // StairsSpawn unused in sanctuary; point it to player spawn as safe default
            result.StairsSpawn = result.PlayerSpawn;

            return result;
        }

        private static void push_back_equivalent(this List<Room> rooms, Room r)
        {
            rooms.Add(r);
        }
    }
}
