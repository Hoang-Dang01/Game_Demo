using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace CSharpGame
{
    class Program
    {
        enum GameState { Lobby, Playing, GameOver }

        static void Main(string[] args)
        {
            // Init Window
            Raylib.InitWindow(Config.ScreenWidth, Config.ScreenHeight, "Dungeon Survivor: C# RPG Framework");
            Raylib.SetTargetFPS(Config.TargetFps);

            // States
            GameState gameState = GameState.Lobby;
            string nickname = "Explorer";
            string selectedWeaponType = "sword"; // "sword", "bow", "wand"

            // Game Session Variables
            Player player = new Player();
            int currentLevel = 1;
            int highestFloorReached = 1;
            uint dungeonSeed = (uint)new Random().Next(100000, 999999);
            DungeonData? dungeon = null;

            // Entities lists
            List<Enemy> enemies = new List<Enemy>();
            List<Projectile> projectiles = new List<Projectile>();
            List<Item> droppedItems = new List<Item>(); // Represents items floating on ground

            // Ground chest tracking
            List<ChestState> chests = new List<ChestState>();

            // Floating Combat texts
            List<FloatingText> floatingTexts = new List<FloatingText>();

            // Visual effects
            List<Particle> particles = new List<Particle>();

            // Screen shake
            float shakeIntensity = 0f;
            float shakeDecay = 0.88f;

            // Camera setup
            Camera2D camera = new Camera2D
            {
                Rotation = 0f,
                Zoom = 1f
            };

            // Inventory Screen Toggle
            bool showCharacterScreen = false;

            // ── Sanctuary & Portal ────────────────────────────────────────
            SanctuaryManager sanctuary = new SanctuaryManager();
            PortalSystem portal = new PortalSystem();
            Blacksmith blacksmith = new Blacksmith();
            bool showStorageUI = false;

            // ── Fog of War ─────────────────────────────────────────
            FogOfWar fog = new FogOfWar();

            // Load saved game if exists
            void TryLoadSavedGame()
            {
                if (SaveManager.LoadGame(player, sanctuary, portal, out int loadedFloor, out int loadedHighest))
                {
                    currentLevel = loadedFloor;
                    highestFloorReached = loadedHighest;
                    if (currentLevel == 0)
                    {
                        InitSanctuary();
                    }
                    else
                    {
                        dungeonSeed = portal.ReturnPortal.Seed != 0 ? portal.ReturnPortal.Seed
                                     : (uint)new Random().Next(100000, 999999);
                        InitLevel();
                    }
                    gameState = GameState.Playing;
                }
            }

            // Init Sanctuary (Floor 0)
            void InitSanctuary()
            {
                currentLevel = 0;
                dungeon = DungeonGenerator.GenerateSanctuary();
                player.X = dungeon.PlayerSpawn.X;
                player.Y = dungeon.PlayerSpawn.Y;

                enemies.Clear();
                projectiles.Clear();
                droppedItems.Clear();
                chests.Clear();

                // Sanctuary: fully revealed, no fog
                fog.Init(dungeon.Width, dungeon.Height);
                // Mark every tile as Visible so overlay = 0
                for (int fy = 0; fy < dungeon.Height; fy++)
                    for (int fx = 0; fx < dungeon.Width; fx++)
                        fog.Update(fx * Config.TileSize + Config.TileSize / 2f,
                                   fy * Config.TileSize + Config.TileSize / 2f, dungeon);

                sanctuary.EnterSanctuary(player);
                blacksmith.CloseUI();
                showStorageUI = false;
            }

            // Init Level
            void InitLevel()
            {
                dungeon = DungeonGenerator.GenerateDungeon(dungeonSeed, currentLevel);
                player.X = dungeon.PlayerSpawn.X;
                player.Y = dungeon.PlayerSpawn.Y;

                // Fresh fog for new dungeon — all tiles start unexplored
                fog.Init(dungeon.Width, dungeon.Height);
                // Reveal player spawn immediately
                fog.Update(player.X, player.Y, dungeon);
                enemies.Clear();
                projectiles.Clear();
                droppedItems.Clear();
                chests.Clear();

                // Build chests
                foreach (var c in dungeon.Chests)
                {
                    chests.Add(new ChestState { Pos = c, Opened = false });
                }

                // Spawn enemies
                foreach (var e in dungeon.Enemies)
                {
                    if (e.Type == "melee")
                    {
                        enemies.Add(new Enemy
                        {
                            X = e.X,
                            Y = e.Y,
                            EnemyType = "melee",
                            Speed = 2.0f,
                            Hp = 50f + currentLevel * 5,
                            MaxHp = 50f + currentLevel * 5
                        });
                    }
                    else
                    {
                        enemies.Add(new Enemy
                        {
                            X = e.X,
                            Y = e.Y,
                            EnemyType = "ranged",
                            Speed = 1.4f,
                            Hp = 35f + currentLevel * 3,
                            MaxHp = 35f + currentLevel * 3
                        });
                    }
                }

                // Spawn boss
                if (dungeon.BossSpawn.HasValue)
                {
                    enemies.Add(new Boss
                    {
                        X = dungeon.BossSpawn.Value.X,
                        Y = dungeon.BossSpawn.Value.Y,
                        Hp = 500f + currentLevel * 20,
                        MaxHp = 500f + currentLevel * 20
                    });
                }
            }

            // Open Chest
            void OpenChest(ChestState chest)
            {
                chest.Opened = true;
                SeededRandom r = new SeededRandom((uint)(dungeonSeed + chest.Pos.X + chest.Pos.Y));

                // 1. Drop gold văng ra
                droppedItems.Add(new Item
                {
                    Name = "Vàng",
                    ItemType = "gold",
                    Val = r.NextInt(15, 30),
                    X = chest.Pos.X - 15,
                    Y = chest.Pos.Y - 15,
                    Radius = 6f
                });
                droppedItems.Add(new Item
                {
                    Name = "Vàng",
                    ItemType = "gold",
                    Val = r.NextInt(15, 30),
                    X = chest.Pos.X + 15,
                    Y = chest.Pos.Y - 15,
                    Radius = 6f
                });

                // 2. Drop 1 ngẫu nhiên trang bị
                Item dropEq = ItemDatabase.GenerateRandomEquipment((uint)r.NextInt(1, 9999), currentLevel);
                dropEq.X = chest.Pos.X;
                dropEq.Y = chest.Pos.Y + 16;
                dropEq.Radius = 10f;
                droppedItems.Add(dropEq);

                // 3. Drop Portal Stone (25% chance)
                if (r.NextFloat() < 0.25f)
                {
                    Item portalStone = ItemDatabase.GeneratePortalStone();
                    portalStone.X = chest.Pos.X;
                    portalStone.Y = chest.Pos.Y - 16;
                    portalStone.Radius = 8f;
                    droppedItems.Add(portalStone);
                }

                shakeIntensity = Math.Max(shakeIntensity, 6f);
            }

            // Main Loop
            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();

                // --- UPDATE STATE ---
                if (gameState == GameState.Lobby)
                {
                    // Handle keyboard typing for nickname
                    int charPressed = Raylib.GetCharPressed();
                    while (charPressed > 0)
                    {
                        if (nickname.Length < 12 && charPressed >= 32 && charPressed <= 125)
                        {
                            nickname += (char)charPressed;
                        }
                        charPressed = Raylib.GetCharPressed();
                    }
                    if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && nickname.Length > 0)
                    {
                        nickname = nickname.Substring(0, nickname.Length - 1);
                    }

                    // Check Clicks for selections
                    Vector2 mousePos = Raylib.GetMousePosition();

                    // Weapon Select boxes
                    Rectangle rectSword = new Rectangle(Config.ScreenWidth / 2 - 190, 330, 100, 100);
                    Rectangle rectBow = new Rectangle(Config.ScreenWidth / 2 - 50, 330, 100, 100);
                    Rectangle rectWand = new Rectangle(Config.ScreenWidth / 2 + 90, 330, 100, 100);

                    if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                    {
                        if (Raylib.CheckCollisionPointRec(mousePos, rectSword)) selectedWeaponType = "sword";
                        if (Raylib.CheckCollisionPointRec(mousePos, rectBow)) selectedWeaponType = "bow";
                        if (Raylib.CheckCollisionPointRec(mousePos, rectWand)) selectedWeaponType = "wand";

                        // Connect Button Click
                        Rectangle btnStart = new Rectangle(Config.ScreenWidth / 2 - 140, 480, 280, 50);
                        if (Raylib.CheckCollisionPointRec(mousePos, btnStart))
                        {
                            player.Name = string.IsNullOrWhiteSpace(nickname) ? "Explorer" : nickname;
                            
                            // Generate starting weapon
                            Item startingWeapon = ItemDatabase.GenerateStartingWeapon(selectedWeaponType);
                            player.Equipment.Equip(startingWeapon, "Weapon");

                            // Start with 3 Portal Stones
                            player.Inventory.AddItem(ItemDatabase.GeneratePortalStone());
                            player.Inventory.AddItem(ItemDatabase.GeneratePortalStone());
                            player.Inventory.AddItem(ItemDatabase.GeneratePortalStone());

                            player.RecalculateAttributes();
                            player.Hp = player.MaxHp;
                            player.Mp = player.MaxMp;

                            // New game always starts in Sanctuary
                            InitSanctuary();
                            gameState = GameState.Playing;
                        }

                        // Load Game Button
                        Rectangle btnLoad = new Rectangle(Config.ScreenWidth / 2 - 140, 550, 280, 45);
                        if (Raylib.CheckCollisionPointRec(mousePos, btnLoad))
                        {
                            TryLoadSavedGame();
                        }
                    }
                }
                else if (gameState == GameState.Playing)
                {
                    // 1. Tự động hồi HP/MP (1Hz) — Dungeon only
                    if (!sanctuary.IsActive && Raylib.GetTime() % 1.0 < dt)
                    {
                        if (player.Hp > 0)
                        {
                            float hpReg = 1f + player.EffVigor * 0.1f;
                            float mpReg = 2f + player.EffIntelligence * 0.15f;
                            player.Hp = Math.Min(player.MaxHp, player.Hp + hpReg);
                            player.Mp = Math.Min(player.MaxMp, player.Mp + mpReg);
                        }
                    }

                    // 1b. Sanctuary fast regen (every frame) and sync portal stone count
                    sanctuary.PortalStoneCount = player.Inventory.PortalStoneCount;
                    if (sanctuary.IsActive)
                        sanctuary.Update(player);

                    // 2. Keyboard Inputs
                    if (Raylib.IsKeyPressed(KeyboardKey.C) || Raylib.IsKeyPressed(KeyboardKey.I))
                    {
                        showCharacterScreen = !showCharacterScreen;
                        if (showCharacterScreen) { blacksmith.CloseUI(); showStorageUI = false; }
                    }

                    // ESC closes any UI
                    if (Raylib.IsKeyPressed(KeyboardKey.Escape))
                    {
                        showCharacterScreen = false;
                        blacksmith.CloseUI();
                        showStorageUI = false;
                    }

                    // [T] — Portal: Dungeon → Sanctuary (Consumes Portal Stone)
                    if (Raylib.IsKeyPressed(KeyboardKey.T) && !sanctuary.IsActive && player.Hp > 0)
                    {
                        if (portal.PortalToSanctuary(player, sanctuary, currentLevel, dungeonSeed))
                        {
                            floatingTexts.Add(new FloatingText
                            {
                                X = player.X, Y = player.Y - 30,
                                Text = "Dịch chuyển về Sanctuary!",
                                Color = new Raylib_cs.Color(180, 100, 255, 255),
                                Lifetime = 50, MaxLifetime = 50
                            });
                            InitSanctuary();
                            SaveManager.SaveGame(player, 0, highestFloorReached, sanctuary, portal.ReturnPortal);
                        }
                        else
                        {
                            floatingTexts.Add(new FloatingText
                            {
                                X = player.X, Y = player.Y - 30,
                                Text = "Không có Đá Dịch Chuyển!",
                                Color = Color.Red,
                                Lifetime = 50, MaxLifetime = 50
                            });
                        }
                    }

                    // [E] — Interact in Sanctuary
                    if (Raylib.IsKeyPressed(KeyboardKey.E) && sanctuary.IsActive && dungeon != null)
                    {
                        sanctuary.CheckProximity(player, dungeon);

                        if (sanctuary.NearBlacksmith)
                        {
                            blacksmith.ToggleUI();
                            showStorageUI = false;
                            showCharacterScreen = false;
                        }
                        else if (sanctuary.NearStorageChest)
                        {
                            showStorageUI = !showStorageUI;
                            blacksmith.CloseUI();
                            showCharacterScreen = false;
                        }
                        else if (sanctuary.NearPortal)
                        {
                            if (portal.CanReturnToDungeon())
                            {
                                // Sanctuary → Dungeon (free, return to saved coordinates)
                                int rFloor = portal.ReturnPortal.Floor;
                                uint rSeed  = portal.ReturnPortal.Seed;
                                float rX    = portal.ReturnPortal.X;
                                float rY    = portal.ReturnPortal.Y;

                                portal.ReturnToDungeon(player, sanctuary);
                                currentLevel = rFloor;
                                dungeonSeed  = rSeed;
                                InitLevel();
                                player.X = rX;
                                player.Y = rY;
                            }
                            else
                            {
                                // New Game / No return state: Go to Floor 1
                                portal.ReturnToDungeon(player, sanctuary);
                                currentLevel = 1;
                                dungeonSeed = (uint)new Random().Next(100000, 999999);
                                InitLevel();
                            }

                            SaveManager.SaveGame(player, currentLevel, highestFloorReached, sanctuary, portal.ReturnPortal);
                        }
                    }

                    // Keyboard Cheat Key for EXP
                    if (Raylib.IsKeyPressed(KeyboardKey.L))
                    {
                        player.GainXp(100f);
                        floatingTexts.Add(new FloatingText
                        {
                            X = player.X,
                            Y = player.Y - 30,
                            Text = "CHEAT EXP +100!",
                            Color = Config.ColorGold,
                            Lifetime = 45,
                            MaxLifetime = 45
                        });
                    }

                    // Handle input movement
                    float dx = 0f;
                    float dy = 0f;
                    if (Raylib.IsKeyDown(KeyboardKey.W) || Raylib.IsKeyDown(KeyboardKey.Up)) dy = -1f;
                    if (Raylib.IsKeyDown(KeyboardKey.S) || Raylib.IsKeyDown(KeyboardKey.Down)) dy = 1f;
                    if (Raylib.IsKeyDown(KeyboardKey.A) || Raylib.IsKeyDown(KeyboardKey.Left)) dx = -1f;
                    if (Raylib.IsKeyDown(KeyboardKey.D) || Raylib.IsKeyDown(KeyboardKey.Right)) dx = 1f;

                    // Dash
                    if (Raylib.IsKeyPressed(KeyboardKey.Space) && player.DashCooldown == 0 && !player.IsDashing && (dx != 0 || dy != 0))
                    {
                        player.IsDashing = true;
                        player.DashTimer = 8;
                        player.DashCooldown = 30;
                        player.DashDir = Vector2.Normalize(new Vector2(dx, dy));
                    }

                    // Move player
                    if (!player.IsDashing && (dx != 0 || dy != 0))
                    {
                        Vector2 move = Vector2.Normalize(new Vector2(dx, dy)) * player.Speed;
                        float newX = player.X + move.X;
                        float newY = player.Y + move.Y;

                    if (dungeon != null)
                    {
                        if (!dungeon.IsWallCollision(newX, player.Y, player.Radius)) player.X = newX;
                        if (!dungeon.IsWallCollision(player.X, newY, player.Radius)) player.Y = newY;
                    }
                    }

                    if (dungeon != null)
                        player.Update(dungeon);

                    // Update Fog of War — reveal tiles around current player position
                    if (dungeon != null && fog.IsInitialised)
                        fog.Update(player.X, player.Y, dungeon);

                    // Aiming Angle
                    Vector2 screenMouse = Raylib.GetMousePosition();
                    Vector2 worldMouse = Raylib.GetScreenToWorld2D(screenMouse, camera);
                    player.Angle = (float)Math.Atan2(worldMouse.Y - player.Y, worldMouse.X - player.X);

                    // Attacks
                    if (Raylib.IsMouseButtonPressed(MouseButton.Left) && player.AttackCooldown == 0 && !showCharacterScreen)
                    {
                        Item? equippedWeapon = player.Equipment.Weapon;
                        if (equippedWeapon != null)
                        {
                            player.AttackCooldown = Math.Max(12, (int)Math.Round(50 * (1f - player.EffDexterity * 0.003f)));

                            if (equippedWeapon.AttackType == "melee")
                            {
                                foreach (var enemy in enemies)
                                {
                                    if (enemy.Hp <= 0) continue;
                                    float ex = enemy.X - player.X;
                                    float ey = enemy.Y - player.Y;
                                    float distance = (float)Math.Sqrt(ex * ex + ey * ey);

                                    if (distance <= equippedWeapon.Range)
                                    {
                                        float enemyAngle = (float)Math.Atan2(ey, ex);
                                        float diff = (enemyAngle - player.Angle + (float)Math.PI) % (2f * (float)Math.PI) - (float)Math.PI;

                                        if (Math.Abs(diff) <= 75f * (float)Math.PI / 180f)
                                        {
                                            bool isCrit = new Random().NextDouble() < player.CritChance;
                                            float dmg = isCrit ? equippedWeapon.Damage * 2f : equippedWeapon.Damage;
                                            float finalDmg = (float)Math.Round(dmg * player.DamageMultiplier);

                                            enemy.TakeDamage(finalDmg, (float)Math.Cos(enemyAngle) * 5f, (float)Math.Sin(enemyAngle) * 5f);
                                            shakeIntensity = Math.Max(shakeIntensity, isCrit ? 5f : 2f);

                                            floatingTexts.Add(new FloatingText
                                            {
                                                X = enemy.X,
                                                Y = enemy.Y - 15,
                                                Text = isCrit ? $"CRIT! -{finalDmg}" : $"-{finalDmg}",
                                                Color = isCrit ? Color.Red : Color.White,
                                                Lifetime = 45,
                                                MaxLifetime = 45
                                            });
                                        }
                                    }
                                }
                            }
                            else // Ranged
                            {
                                projectiles.Add(new Projectile
                                {
                                    X = player.X,
                                    Y = player.Y,
                                    Angle = player.Angle,
                                    Speed = equippedWeapon.Speed,
                                    Damage = (float)Math.Round(equippedWeapon.Damage * player.DamageMultiplier),
                                    IsPlayer = true,
                                    Color = equippedWeapon.RarityColor,
                                    SplashRadius = equippedWeapon.SplashRadius,
                                    CritChance = player.CritChance,
                                    Radius = equippedWeapon.SplashRadius > 0 ? 8f : 5f
                                });
                            }
                        }
                    }

                    // Open Chest interact (phím E)
                    if (Raylib.IsKeyPressed(KeyboardKey.E))
                    {
                        foreach (var chest in chests)
                        {
                            if (!chest.Opened)
                            {
                                float cx = chest.Pos.X - player.X;
                                float cy = chest.Pos.Y - player.Y;
                                float d = (float)Math.Sqrt(cx * cx + cy * cy);
                                if (d < 46f)
                                {
                                    OpenChest(chest);
                                    break;
                                }
                            }
                        }
                    }

                    // 3. Update Projectiles
                    for (int i = projectiles.Count - 1; i >= 0; i--)
                    {
                        var proj = projectiles[i];
                        proj.Update(dungeon!);

                        if (!proj.Active)
                        {
                            if (proj.IsPlayer && proj.SplashRadius > 0)
                            {
                                foreach (var enemy in enemies)
                                {
                                    if (enemy.Hp <= 0) continue;
                                    float splashDx = enemy.X - proj.X;
                                    float splashDy = enemy.Y - proj.Y;
                                    float splashDist = (float)Math.Sqrt(splashDx * splashDx + splashDy * splashDy);
                                    if (splashDist <= proj.SplashRadius)
                                    {
                                        enemy.TakeDamage(proj.Damage, splashDx / splashDist * 6f, splashDy / splashDist * 6f);
                                        floatingTexts.Add(new FloatingText
                                        {
                                            X = enemy.X, Y = enemy.Y - 15,
                                            Text = $"-{proj.Damage}", Color = Color.Orange,
                                            Lifetime = 45, MaxLifetime = 45
                                        });
                                    }
                                }
                                shakeIntensity = Math.Max(shakeIntensity, 6f);
                            }
                            projectiles.RemoveAt(i);
                            continue;
                        }

                        if (proj.IsPlayer)
                        {
                            foreach (var enemy in enemies)
                            {
                                if (enemy.Hp <= 0) continue;
                                float eProjDx = enemy.X - proj.X;
                                float eProjDy = enemy.Y - proj.Y;
                                float eProjDist = (float)Math.Sqrt(eProjDx * eProjDx + eProjDy * eProjDy);

                                if (eProjDist <= (enemy.Radius + proj.Radius))
                                {
                                    bool isCrit = new Random().NextDouble() < proj.CritChance;
                                    float dmg = isCrit ? proj.Damage * 2f : proj.Damage;
                                    enemy.TakeDamage(dmg, (float)Math.Cos(proj.Angle) * 6f, (float)Math.Sin(proj.Angle) * 6f);

                                    floatingTexts.Add(new FloatingText
                                    {
                                        X = enemy.X, Y = enemy.Y - 15,
                                        Text = isCrit ? $"CRIT! -{dmg}" : $"-{dmg}",
                                        Color = isCrit ? Color.Red : Color.White,
                                        Lifetime = 45, MaxLifetime = 45
                                    });

                                    proj.Active = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            float pProjDx = player.X - proj.X;
                            float pProjDy = player.Y - proj.Y;
                            float pProjDist = (float)Math.Sqrt(pProjDx * pProjDx + pProjDy * pProjDy);

                            if (pProjDist <= (player.Radius + proj.Radius))
                            {
                                player.TakeDamage(proj.Damage, (float)Math.Cos(proj.Angle) * 5f, (float)Math.Sin(proj.Angle) * 5f);
                                floatingTexts.Add(new FloatingText
                                {
                                    X = player.X, Y = player.Y - 15,
                                    Text = $"-{Math.Max(1f, proj.Damage - player.Defense)}", Color = Color.Red,
                                    Lifetime = 45, MaxLifetime = 45
                                });
                                proj.Active = false;
                            }
                        }
                    }

                    // 4. Update Enemies
                    for (int i = enemies.Count - 1; i >= 0; i--)
                    {
                        var enemy = enemies[i];
                        enemy.UpdateAI(player, dungeon!);
                        enemy.Update(dungeon!);

                        if (enemy.EnemyType == "ranged" && enemy.Hp > 0 && enemy.ShootCooldown == 60)
                        {
                            float edx = player.X - enemy.X;
                            float edy = player.Y - enemy.Y;
                            float eAngle = (float)Math.Atan2(edy, edx);
                            projectiles.Add(new Projectile
                            {
                                X = enemy.X,
                                Y = enemy.Y,
                                Angle = eAngle,
                                Speed = 6.5f,
                                Damage = 8f + currentLevel * 1f,
                                IsPlayer = false,
                                Color = Color.Purple
                            });
                        }

                        if (enemy.Hp <= 0)
                        {
                            SeededRandom r = new SeededRandom((uint)(dungeonSeed + enemy.X + enemy.Y));
                            
                            droppedItems.Add(new Item
                            {
                                Name = "Kinh Nghiệm",
                                ItemType = "exp",
                                Val = 15 + currentLevel * 3,
                                X = enemy.X,
                                Y = enemy.Y,
                                Radius = 8f
                            });

                            if (r.NextFloat() < 0.40f)
                            {
                                droppedItems.Add(new Item
                                {
                                    Name = "Vàng",
                                    ItemType = "gold",
                                    Val = r.NextInt(5, 12),
                                    X = enemy.X + 10,
                                    Y = enemy.Y,
                                    Radius = 6f
                                });
                            }

                            if (r.NextFloat() < 0.15f)
                            {
                                droppedItems.Add(new Item
                                {
                                    Name = "Máu",
                                    ItemType = "heart",
                                    Val = 25,
                                    X = enemy.X - 10,
                                    Y = enemy.Y,
                                    Radius = 8f
                                });
                            }

                            if (enemy is Boss)
                            {
                                Item bossEq = ItemDatabase.GenerateRandomEquipment((uint)r.NextInt(1, 9999), currentLevel + 1);
                                bossEq.X = enemy.X;
                                bossEq.Y = enemy.Y + 16;
                                bossEq.Radius = 10f;
                                droppedItems.Add(bossEq);

                                // Drop Portal Stone from Boss (100% chance)
                                Item portalStone = ItemDatabase.GeneratePortalStone();
                                portalStone.X = enemy.X - 16;
                                portalStone.Y = enemy.Y;
                                portalStone.Radius = 8f;
                                droppedItems.Add(portalStone);

                                if (currentLevel + 1 > highestFloorReached) highestFloorReached = currentLevel + 1;
                                SaveManager.SaveGame(player, currentLevel + 1, highestFloorReached,
                                    sanctuary, portal.ReturnPortal);
                            }

                            enemies.RemoveAt(i);
                        }
                    }

                    // 5. Update Dropped items
                    for (int i = droppedItems.Count - 1; i >= 0; i--)
                    {
                        var item = droppedItems[i];
                        float itemDx = item.X - player.X;
                        float itemDy = item.Y - player.Y;
                        float d = (float)Math.Sqrt(itemDx * itemDx + itemDy * itemDy);

                        if (d < 120f)
                        {
                            item.X -= (itemDx / d) * 3.5f;
                            item.Y -= (itemDy / d) * 3.5f;
                        }

                        if (d <= (player.Radius + item.Radius))
                        {
                            bool success = true;
                            if (item.ItemType == "gold")
                            {
                                player.Gold += item.Val;
                                floatingTexts.Add(new FloatingText { X = player.X, Y = player.Y - 25, Text = $"+{item.Val} Vàng", Color = Config.ColorGold, Lifetime = 40, MaxLifetime = 40 });
                            }
                            else if (item.ItemType == "heart")
                            {
                                player.Hp = Math.Min(player.MaxHp, player.Hp + item.Val);
                                floatingTexts.Add(new FloatingText { X = player.X, Y = player.Y - 25, Text = $"+{item.Val} HP", Color = Color.Red, Lifetime = 40, MaxLifetime = 40 });
                            }
                            else if (item.ItemType == "exp")
                            {
                                player.GainXp(item.Val);
                                floatingTexts.Add(new FloatingText { X = player.X, Y = player.Y - 25, Text = $"+{item.Val} EXP", Color = Config.ColorXp, Lifetime = 40, MaxLifetime = 40 });
                            }
                            else
                            {
                                success = player.Inventory.AddItem(item);
                                if (success)
                                {
                                    floatingTexts.Add(new FloatingText { X = player.X, Y = player.Y - 25, Text = $"Nhặt: {item.Name}", Color = item.RarityColor, Lifetime = 50, MaxLifetime = 50 });
                                }
                                else
                                {
                                    floatingTexts.Add(new FloatingText { X = player.X, Y = player.Y - 25, Text = "Hành trang đầy!", Color = Color.Red, Lifetime = 40, MaxLifetime = 40 });
                                }
                            }

                            if (success)
                            {
                                droppedItems.RemoveAt(i);
                            }
                        }
                    }

                    // 6. Camera Follow
                    if (shakeIntensity > 0.1f) shakeIntensity *= shakeDecay;
                    else shakeIntensity = 0f;

                    float shakeX = (float)(new Random().NextDouble() - 0.5) * shakeIntensity;
                    float shakeY = (float)(new Random().NextDouble() - 0.5) * shakeIntensity;

                    camera.Target = new Vector2(player.X + shakeX, player.Y + shakeY);
                    camera.Offset = new Vector2(Config.ScreenWidth / 2f, Config.ScreenHeight / 2f);

                    // 7. Check stairs
                    int playerGx = (int)(player.X / Config.TileSize);
                    int playerGy = (int)(player.Y / Config.TileSize);
                    if (dungeon!.Grid[playerGy, playerGx] == 2)
                    {
                        currentLevel++;
                        if (currentLevel > highestFloorReached) highestFloorReached = currentLevel;
                        dungeonSeed = (uint)new Random().Next(100000, 999999);
                        InitLevel();
                        SaveManager.SaveGame(player, currentLevel, highestFloorReached, sanctuary, portal.ReturnPortal);
                    }

                    if (player.Hp <= 0)
                    {
                        gameState = GameState.GameOver;
                    }
                }

                // Update Visual Particles
                for (int i = particles.Count - 1; i >= 0; i--)
                {
                    var p = particles[i];
                    p.X += p.Dx;
                    p.Y += p.Dy;
                    p.Dx *= 0.94f;
                    p.Dy *= 0.94f;
                    p.Size *= 0.96f;
                    p.Life--;
                    if (p.Life <= 0 || p.Size < 0.5f) particles.RemoveAt(i);
                }

                // Update floating combat texts
                for (int i = floatingTexts.Count - 1; i >= 0; i--)
                {
                    var ft = floatingTexts[i];
                    ft.Y -= 0.8f;
                    ft.Lifetime--;
                    if (ft.Lifetime <= 0) floatingTexts.RemoveAt(i);
                }


                // --- DRAWING STAGE ---
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Config.ColorBg);

                if (gameState == GameState.Lobby)
                {
                    Raylib.DrawText("CORE RPG FRAMEWORK", Config.ScreenWidth / 2 - 220, 100, 40, Config.ColorGold);
                    Raylib.DrawText("C# Native Edition (Raylib-cs)", Config.ScreenWidth / 2 - 190, 150, 18, Color.Gray);

                    Raylib.DrawText("NHẬP BIỆT DANH:", Config.ScreenWidth / 2 - 140, 220, 13, Color.LightGray);
                    Raylib.DrawRectangle(Config.ScreenWidth / 2 - 140, 245, 280, 40, new Color(20, 20, 25, 255));
                    Raylib.DrawRectangleLines(Config.ScreenWidth / 2 - 140, 245, 280, 40, Color.DarkGray);
                    Raylib.DrawText(nickname, Config.ScreenWidth / 2 - 120, 257, 18, Color.White);

                    Raylib.DrawText("CHỌN VŨ KHÍ KHỞI ĐẦU:", Config.ScreenWidth / 2 - 140, 305, 13, Color.LightGray);

                    Rectangle rectSword = new Rectangle(Config.ScreenWidth / 2 - 190, 330, 100, 100);
                    Rectangle rectBow = new Rectangle(Config.ScreenWidth / 2 - 50, 330, 100, 100);
                    Rectangle rectWand = new Rectangle(Config.ScreenWidth / 2 + 90, 330, 100, 100);

                    void DrawSelectBox(Rectangle r, string title, string sub, bool selected)
                    {
                        Raylib.DrawRectangleRec(r, selected ? new Color(60, 150, 255, 20) : new Color(20, 20, 25, 255));
                        Raylib.DrawRectangleLinesEx(r, 2, selected ? Config.ColorMana : Color.DarkGray);
                        Raylib.DrawText(title, (int)r.X + 15, (int)r.Y + 30, 15, Color.White);
                        Raylib.DrawText(sub, (int)r.X + 15, (int)r.Y + 55, 11, Color.Gray);
                    }

                    DrawSelectBox(rectSword, "KIẾM GỖ", "Cận chiến", selectedWeaponType == "sword");
                    DrawSelectBox(rectBow, "CUNG GỖ", "Tầm xa", selectedWeaponType == "bow");
                    DrawSelectBox(rectWand, "TRƯỢNG GỖ", "Phép thuật", selectedWeaponType == "wand");

                    Rectangle btnStart = new Rectangle(Config.ScreenWidth / 2 - 140, 480, 280, 50);
                    Raylib.DrawRectangleRec(btnStart, Config.ColorMana);
                    Raylib.DrawText("THÂM NHẬP HẦM NGỤC", Config.ScreenWidth / 2 - 95, 495, 16, Color.White);

                    Rectangle btnLoad = new Rectangle(Config.ScreenWidth / 2 - 140, 550, 280, 45);
                    Raylib.DrawRectangleRec(btnLoad, new Color(40, 40, 48, 255));
                    Raylib.DrawText("TẢI TIẾN TRÌNH CŨ (LOAD)", Config.ScreenWidth / 2 - 105, 565, 13, Color.LightGray);
                }
                else if (gameState == GameState.Playing || gameState == GameState.GameOver)
                {
                    Raylib.BeginMode2D(camera);

                    int startX = Math.Max(0, (int)((camera.Target.X - Config.ScreenWidth / 2f) / Config.TileSize) - 1);
                    int endX = Math.Min(dungeon!.Width, (int)((camera.Target.X + Config.ScreenWidth / 2f) / Config.TileSize) + 2);
                    int startY = Math.Max(0, (int)((camera.Target.Y - Config.ScreenHeight / 2f) / Config.TileSize) - 1);
                    int endY = Math.Min(dungeon.Height, (int)((camera.Target.Y + Config.ScreenHeight / 2f) / Config.TileSize) + 2);

                    for (int y = startY; y < endY; y++)
                    {
                        for (int x = startX; x < endX; x++)
                        {
                            int tile = dungeon.Grid[y, x];
                            int rx = x * Config.TileSize;
                            int ry = y * Config.TileSize;

                            if (tile == 0)
                            {
                                Raylib.DrawRectangle(rx, ry, Config.TileSize, Config.TileSize, Config.ColorWall);
                                Raylib.DrawLine(rx, ry, rx + Config.TileSize, ry, Config.ColorWallBorder);
                                Raylib.DrawLine(rx, ry + 16, rx + Config.TileSize, ry + 16, Config.ColorWallBorder);
                                Raylib.DrawLine(rx, ry + 32, rx + Config.TileSize, ry + 32, Config.ColorWallBorder);
                            }
                            else if (tile == 1)
                            {
                                Raylib.DrawRectangle(rx, ry, Config.TileSize, Config.TileSize, Config.ColorFloor);
                                Raylib.DrawRectangleLines(rx, ry, Config.TileSize, Config.TileSize, Config.ColorFloorBorder);
                            }
                            else if (tile == 2)
                            {
                                Raylib.DrawRectangle(rx, ry, Config.TileSize, Config.TileSize, new Color(40, 70, 50, 255));
                                Raylib.DrawRectangleLines(rx, ry, Config.TileSize, Config.TileSize, Color.Green);
                                Raylib.DrawText("STAIRS", rx + 6, ry + 18, 11, Config.ColorXp);
                            }
                            else if (tile == 3) // Sanctuary safe floor (sàn gỗ ấm áp)
                            {
                                Raylib.DrawRectangle(rx, ry, Config.TileSize, Config.TileSize, new Color(75, 53, 39, 255));
                                // Vẽ các đường nối ghép ván gỗ (ngang)
                                Raylib.DrawLine(rx, ry, rx + Config.TileSize, ry, new Color(48, 32, 22, 255));
                                Raylib.DrawLine(rx, ry + Config.TileSize - 1, rx + Config.TileSize, ry + Config.TileSize - 1, new Color(48, 32, 22, 255));
                                // Vẽ đường ghép dọc so le để giống ván gỗ thật
                                if ((y % 2) == 0)
                                {
                                    Raylib.DrawLine(rx + Config.TileSize / 2, ry, rx + Config.TileSize / 2, ry + Config.TileSize, new Color(48, 32, 22, 255));
                                }
                                else
                                {
                                    Raylib.DrawLine(rx + Config.TileSize / 4, ry, rx + Config.TileSize / 4, ry + Config.TileSize, new Color(48, 32, 22, 255));
                                    Raylib.DrawLine(rx + 3 * Config.TileSize / 4, ry, rx + 3 * Config.TileSize / 4, ry + Config.TileSize, new Color(48, 32, 22, 255));
                                }
                                // Vẽ một vài vân gỗ mờ để sinh động
                                Raylib.DrawLine(rx + 4, ry + 12, rx + Config.TileSize - 4, ry + 12, new Color(90, 68, 52, 60));
                                Raylib.DrawLine(rx + 6, ry + 24, rx + Config.TileSize - 6, ry + 24, new Color(90, 68, 52, 60));
                            }
                        }
                    }

                    // ── Sanctuary NPC & Object rendering ──────────────────────
                    if (sanctuary.IsActive && dungeon != null)
                    {
                        // ■ Blacksmith
                        if (dungeon.MerchantSpawn.HasValue)
                        {
                            int bx = dungeon.MerchantSpawn.Value.X;
                            int by = dungeon.MerchantSpawn.Value.Y;
                            Raylib.DrawRectangle(bx - 18, by - 22, 36, 40, new Color(60, 40, 20, 255));
                            Raylib.DrawRectangle(bx - 14, by - 18, 28, 28, new Color(120, 80, 30, 255));
                            Raylib.DrawText("⚒", bx - 8, by - 14, 20, Config.ColorGold);
                            Raylib.DrawText("THỢ RÈN", bx - Raylib.MeasureText("THỢ RÈN", 9) / 2, by + 24, 9, Config.ColorGold);
                            if (sanctuary.NearBlacksmith)
                                Raylib.DrawText("[E] Thực Hiện", bx - 40, by - 36, 10, Color.White);
                        }

                        // ■ Storage Chest
                        if (dungeon.ShopPedestals.Count > 0)
                        {
                            int sx = dungeon.ShopPedestals[0].X;
                            int sy = dungeon.ShopPedestals[0].Y;
                            Raylib.DrawRectangle(sx - 16, sy - 12, 32, 26, new Color(30, 30, 35, 255));
                            Raylib.DrawRectangle(sx - 14, sy - 10, 28, 22, new Color(40, 100, 60, 255));
                            Raylib.DrawText("■", sx - 5, sy - 6, 14, new Color(80, 200, 120, 255));
                            Raylib.DrawText("KHO ĐỒ", sx - Raylib.MeasureText("KHO ĐỒ", 9) / 2, sy + 18, 9, new Color(80, 200, 120, 255));
                            if (sanctuary.NearStorageChest)
                                Raylib.DrawText("[E] Mở Kho", sx - 36, sy - 26, 10, Color.White);
                        }

                        // ■ Portal Pedestal
                        if (dungeon.ShopPedestals.Count > 1)
                        {
                            int px = dungeon.ShopPedestals[1].X;
                            int py = dungeon.ShopPedestals[1].Y;
                            float pulse = (float)Math.Abs(Math.Sin(Raylib.GetTime() * 3.0)) * 0.5f + 0.5f;
                            var portalGlow = new Color((int)(100 * pulse), (int)(60 * pulse), (int)(255 * pulse), 220);
                            Raylib.DrawCircle(px, py, 22f, new Color(20, 10, 40, 200));
                            Raylib.DrawCircleLines(px, py, 22f, portalGlow);
                            Raylib.DrawCircleLines(px, py, 16f, portalGlow);
                            Raylib.DrawText("★", px - 7, py - 10, 18, portalGlow);
                            if (portal.CanReturnToDungeon())
                            {
                                Raylib.DrawText("[E] Quay Lại", px - Raylib.MeasureText("[E] Quay Lại", 10) / 2, py + 28, 10, portalGlow);
                                Raylib.DrawText($"Tầng {portal.ReturnPortal.Floor}", px - Raylib.MeasureText($"Tầng {portal.ReturnPortal.Floor}", 9) / 2, py + 40, 9, Color.Gray);
                            }
                            else
                            {
                                Raylib.DrawText("[E] Hầm Ngục", px - Raylib.MeasureText("[E] Hầm Ngục", 10) / 2, py + 28, 10, portalGlow);
                                Raylib.DrawText("Tầng 1", px - Raylib.MeasureText("Tầng 1", 9) / 2, py + 40, 9, Color.Gray);
                            }
                        }
                    }

                    foreach (var chest in chests)
                    {
                        // Don't reveal chest if tile not yet explored
                        int cTx = chest.Pos.X / Config.TileSize;
                        int cTy = chest.Pos.Y / Config.TileSize;
                        if (!fog.IsRevealed(cTx, cTy)) continue;

                        Raylib.DrawRectangle(chest.Pos.X - 16, chest.Pos.Y - 16, 32, 26, new Color(30, 30, 35, 255));
                        Raylib.DrawRectangle(chest.Pos.X - 14, chest.Pos.Y - 14, 28, 22, new Color(120, 70, 30, 255));
                        Raylib.DrawRectangle(chest.Pos.X - 14, chest.Pos.Y - 14, 4, 22, Color.DarkGray);
                        Raylib.DrawRectangle(chest.Pos.X + 10, chest.Pos.Y - 14, 4, 22, Color.DarkGray);
                        
                        if (!chest.Opened)
                        {
                            Raylib.DrawRectangle(chest.Pos.X - 2, chest.Pos.Y - 4, 4, 6, Config.ColorGold);
                        }
                        else
                        {
                            Raylib.DrawText("OPENED", chest.Pos.X - 16, chest.Pos.Y - 26, 8, Color.Gray);
                        }
                    }

                    foreach (var item in droppedItems)
                    {
                        Raylib.DrawEllipse((int)item.X, (int)item.Y + 8, 6, 2, new Color(10, 10, 15, 100));

                        float bob = (float)Math.Sin(Raylib.GetTime() * 6.0f + item.X) * 3f;
                        int iy = (int)(item.Y + bob);

                        if (item.ItemType == "gold")
                        {
                            Raylib.DrawCircle((int)item.X, iy, 5f, Config.ColorGold);
                        }
                        else if (item.ItemType == "heart")
                        {
                            Raylib.DrawCircle((int)item.X, iy, 6f, Color.Red);
                        }
                        else if (item.ItemType == "exp")
                        {
                            Raylib.DrawCircle((int)item.X, iy, 5f, Config.ColorXp);
                        }
                        else
                        {
                            Raylib.DrawCircleLines((int)item.X, iy, 11f, item.RarityColor);
                            Raylib.DrawCircle((int)item.X, iy, 3f, Color.White);
                            
                            int tw = Raylib.MeasureText(item.Name, 10);
                            Raylib.DrawRectangle((int)item.X - tw / 2 - 4, iy - 24, tw + 8, 12, new Color(10, 10, 15, 200));
                            Raylib.DrawText(item.Name, (int)item.X - tw / 2, iy - 23, 10, item.RarityColor);
                        }
                    }

                    foreach (var enemy in enemies)
                    {
                        // Only render enemies on currently visible tiles
                        int eTx = (int)enemy.X / Config.TileSize;
                        int eTy = (int)enemy.Y / Config.TileSize;
                        if (!fog.IsVisible(eTx, eTy)) continue;
                        Raylib.DrawCircle((int)enemy.X, (int)enemy.Y + 12, enemy.Radius, new Color(10, 10, 15, 100));

                        if (enemy is Boss boss)
                        {
                            Raylib.DrawCircle((int)boss.X, (int)boss.Y, boss.Radius, boss.Enraged ? Color.Magenta : Color.DarkPurple);
                            Raylib.DrawCircleLines((int)boss.X, (int)boss.Y, boss.Radius, Color.Black);
                            Raylib.DrawCircle((int)boss.X - 8, (int)boss.Y - 4, 3f, Color.Purple);
                            Raylib.DrawCircle((int)boss.X + 8, (int)boss.Y - 4, 3f, Color.Purple);
                        }
                        else
                        {
                            Raylib.DrawCircle((int)enemy.X, (int)enemy.Y, enemy.Radius, enemy.EnemyType == "melee" ? Color.LightGray : new Color(100, 45, 150, 255));
                            Raylib.DrawCircleLines((int)enemy.X, (int)enemy.Y, enemy.Radius, Color.Black);
                            Raylib.DrawCircle((int)enemy.X - 3, (int)enemy.Y - 1, 1.5f, enemy.EnemyType == "melee" ? Color.Red : Color.Yellow);
                            Raylib.DrawCircle((int)enemy.X + 3, (int)enemy.Y - 1, 1.5f, enemy.EnemyType == "melee" ? Color.Red : Color.Yellow);
                        }

                        if (enemy.Hp < enemy.MaxHp)
                        {
                            int barW = 24;
                            int barH = 3;
                            int bx = (int)enemy.X - barW / 2;
                            int by = (int)(enemy.Y - enemy.Radius - 6f);
                            Raylib.DrawRectangle(bx, by, barW, barH, Color.Black);
                            Raylib.DrawRectangle(bx, by, (int)(barW * (enemy.Hp / enemy.MaxHp)), barH, Color.Red);
                        }
                    }

                    foreach (var proj in projectiles)
                    {
                        Raylib.DrawCircle((int)proj.X, (int)proj.Y, proj.Radius + 3f, new Color(proj.Color.R, proj.Color.G, proj.Color.B, (byte)80));
                        Raylib.DrawCircle((int)proj.X, (int)proj.Y, proj.Radius, proj.Color);
                        Raylib.DrawCircle((int)proj.X, (int)proj.Y, proj.Radius - 3f, Color.White);
                    }

                    if (player.Hp > 0)
                    {
                        Raylib.DrawEllipse((int)player.X, (int)player.Y + 12, 12, 3, new Color(10, 10, 15, 100));

                        if (player.IsDashing)
                        {
                            Raylib.DrawCircle((int)(player.X - player.DashDir.X * 15f), (int)(player.Y - player.DashDir.Y * 15f), player.Radius - 2f, new Color(33, 150, 243, 80));
                        }

                        Raylib.DrawCircle((int)player.X, (int)player.Y, player.Radius, new Color(15, 25, 45, 255));
                        Raylib.DrawCircle((int)player.X, (int)player.Y, player.Radius - 2f, Config.ColorMana);
                        Raylib.DrawCircle((int)player.X, (int)player.Y, 4f, Color.White);

                        float indX = player.X + (float)Math.Cos(player.Angle) * 11f;
                        float indY = player.Y + (float)Math.Sin(player.Angle) * 11f;
                        Raylib.DrawCircle((int)indX, (int)indY, 3f, new Color(0, 255, 255, 255));

                        int nameW = Raylib.MeasureText(player.Name, 11);
                        Raylib.DrawText(player.Name, (int)player.X - nameW / 2, (int)player.Y - 24, 11, Color.White);
                    }

                    foreach (var p in particles)
                    {
                        Raylib.DrawCircle((int)p.X, (int)p.Y, p.Size, p.Color);
                    }

                    foreach (var ft in floatingTexts)
                    {
                        int textW = Raylib.MeasureText(ft.Text, 13);
                        Raylib.DrawText(ft.Text, (int)ft.X - textW / 2 + 1, (int)ft.Y + 1, 13, Color.Black);
                        Raylib.DrawText(ft.Text, (int)ft.X - textW / 2, (int)ft.Y, 13, ft.Color);
                    }

                    // ── Fog of War overlay pass ─────────────────────────────────
                    // Sanctuary has full visibility (fog fully revealed in InitSanctuary)
                    if (!sanctuary.IsActive)
                    {
                        for (int y = startY; y < endY; y++)
                        {
                            for (int x = startX; x < endX; x++)
                            {
                                int fogAlpha = fog.GetOverlayAlpha(x, y);
                                if (fogAlpha <= 0) continue; // fully visible — skip

                                int rx = x * Config.TileSize;
                                int ry = y * Config.TileSize;

                                // Unexplored (255): solid black wall
                                // Explored  (180): dark tint — you can see the map but it's dim
                                Raylib.DrawRectangle(rx, ry, Config.TileSize, Config.TileSize,
                                    new Color(6, 6, 8, fogAlpha));
                            }
                        }
                    }

                    Raylib.EndMode2D();

                    Raylib.DrawRectangle(15, 15, 280, 140, new Color(18, 18, 24, 225));
                    Raylib.DrawRectangleLines(15, 15, 280, 140, Color.DarkGray);

                    Raylib.DrawText(player.Name, 25, 25, 16, Color.White);
                    Raylib.DrawText($"LV.{player.Level}", 230, 25, 12, Config.ColorGold);

                    Raylib.DrawText("HP", 25, 50, 10, Color.LightGray);
                    Raylib.DrawRectangle(50, 50, 220, 14, Config.ColorHpBg);
                    Raylib.DrawRectangle(50, 50, (int)(220 * (player.Hp / player.MaxHp)), 14, Config.ColorHp);
                    string hpText = $"{(int)player.Hp}/{player.MaxHp}";
                    Raylib.DrawText(hpText, 50 + 110 - Raylib.MeasureText(hpText, 9) / 2, 53, 9, Color.White);

                    Raylib.DrawText("MP", 25, 70, 10, Color.LightGray);
                    Raylib.DrawRectangle(50, 70, 220, 14, Config.ColorManaBg);
                    Raylib.DrawRectangle(50, 70, (int)(220 * (player.Mp / player.MaxMp)), 14, Config.ColorMana);
                    string mpText = $"{(int)player.Mp}/{player.MaxMp}";
                    Raylib.DrawText(mpText, 50 + 110 - Raylib.MeasureText(mpText, 9) / 2, 73, 9, Color.White);

                    Raylib.DrawText("XP", 25, 90, 10, Color.LightGray);
                    float xpNeeded = (float)Math.Round(100 * Math.Pow(player.Level, 1.5));
                    Raylib.DrawRectangle(50, 90, 220, 14, Config.ColorXpBg);
                    Raylib.DrawRectangle(50, 90, (int)(220 * (player.Xp / xpNeeded)), 14, Config.ColorXp);
                    string xpText = $"{(int)player.Xp}/{(int)xpNeeded} XP";
                    Raylib.DrawText(xpText, 50 + 110 - Raylib.MeasureText(xpText, 9) / 2, 93, 9, Color.White);

                    Raylib.DrawText($"$ {player.Gold} Vàng", 25, 120, 12, Config.ColorGold);
                    Item? curWp = player.Equipment.Weapon;
                    string wpNameStr = curWp != null ? curWp.Name : "Tay Không";
                    Color wpColor = curWp != null ? curWp.RarityColor : Color.LightGray;
                    Raylib.DrawText(wpNameStr, 290 - Raylib.MeasureText(wpNameStr, 11) - 15, 120, 11, wpColor);

                    // ── Top-right: Floor indicator or Sanctuary banner
                    if (sanctuary.IsActive)
                    {
                        Raylib.DrawText("SANCTUARY", Config.ScreenWidth - 145, 25, 15, new Color(80, 200, 100, 255));
                        Raylib.DrawText("Tầng An Toàn", Config.ScreenWidth - 150, 47, 10, new Color(60, 160, 80, 200));
                        Raylib.DrawText($"◆ Đá Portal: {sanctuary.PortalStoneCount}", Config.ScreenWidth - 155, 65, 10, new Color(180, 100, 255, 255));
                    }
                    else
                    {
                        Raylib.DrawText($"TẦNG {currentLevel}", Config.ScreenWidth - 110, 25, 15, Config.ColorGold);
                        Raylib.DrawText("HẦM NGỤC U TỐI", Config.ScreenWidth - 150, 47, 10, Color.Gray);
                        Raylib.DrawText($"[T] Portal ({sanctuary.PortalStoneCount})", Config.ScreenWidth - 155, 65, 10,
                            sanctuary.PortalStoneCount > 0 ? new Raylib_cs.Color(180, 100, 255, 200) : Raylib_cs.Color.DarkGray);
                    }

                    bool nearChestAny = !sanctuary.IsActive && false;
                    foreach (var chest in chests)
                    {
                        if (!chest.Opened && Vector2.Distance(new Vector2(player.X, player.Y), new Vector2(chest.Pos.X, chest.Pos.Y)) < 46f)
                        {
                            nearChestAny = true;
                            break;
                        }
                    }
                    if (nearChestAny)
                    {
                        Raylib.DrawText("NHẤN [E] ĐỂ MỞ RƯƠNG BÁU!", Config.ScreenWidth / 2 - 120, Config.ScreenHeight - 120, 15, Config.ColorGold);
                    }

                    // ── Sanctuary proximity hints
                    if (sanctuary.IsActive && dungeon != null)
                    {
                        sanctuary.CheckProximity(player, dungeon);
                        if (sanctuary.NearBlacksmith || sanctuary.NearStorageChest || sanctuary.NearPortal)
                        {
                            string hint = sanctuary.NearBlacksmith  ? "THỢ RÈN" :
                                          sanctuary.NearStorageChest ? "KHO ĐỒ" : "PORTAL";
                            Raylib.DrawText($"[E] Tương tác với {hint}",
                                Config.ScreenWidth / 2 - 100, Config.ScreenHeight - 120, 14, Color.White);
                        }
                    }

                    // ── Blacksmith UI ─────────────────────────────────────────────
                    if (blacksmith.UIOpen)
                    {
                        int bpX = Config.ScreenWidth / 2 - 300;
                        int bpY = Config.ScreenHeight / 2 - 200;
                        Raylib.DrawRectangle(bpX, bpY, 600, 400, new Color(12, 8, 4, 245));
                        Raylib.DrawRectangleLines(bpX, bpY, 600, 400, Config.ColorGold);
                        Raylib.DrawText("⚒  THỢ RÈN — SANCTUARY", bpX + 20, bpY + 15, 18, Config.ColorGold);
                        Raylib.DrawLine(bpX, bpY + 40, bpX + 600, bpY + 40, Config.ColorGold);

                        // Tabs
                        Color tabCraft  = blacksmith.ActiveTab == 0 ? Config.ColorGold : Color.DarkGray;
                        Color tabUpgr   = blacksmith.ActiveTab == 1 ? Config.ColorGold : Color.DarkGray;
                        Raylib.DrawText("[1] CHẾ TẠO",  bpX + 20,  bpY + 55, 13, tabCraft);
                        Raylib.DrawText("[2] NÂNG CẤP", bpX + 200, bpY + 55, 13, tabUpgr);

                        if (Raylib.IsKeyPressed(KeyboardKey.One))  blacksmith.SetTab(0);
                        if (Raylib.IsKeyPressed(KeyboardKey.Two))  blacksmith.SetTab(1);

                        Raylib.DrawLine(bpX, bpY + 75, bpX + 600, bpY + 75, Color.DarkGray);

                        if (blacksmith.ActiveTab == 0)
                        {
                            Raylib.DrawText("— Công thức chế tạo sẽ xuất hiện ở Phase 3 —",
                                bpX + 60, bpY + 180, 13, Color.Gray);
                            Raylib.DrawText("Nguyên liệu: Mảnh Sắt, Gỗ Cổ, Lông Thú...",
                                bpX + 60, bpY + 210, 11, Color.DarkGray);
                        }
                        else
                        {
                            Item? weapon = player.Equipment.Weapon;
                            if (weapon == null)
                            {
                                Raylib.DrawText("Không có vũ khí nào được trang bị!", bpX + 60, bpY + 140, 13, Color.Red);
                            }
                            else
                            {
                                Raylib.DrawText($"Vũ khí: {weapon.Name}", bpX + 60, bpY + 120, 14, Color.White);
                                Raylib.DrawText($"Cấp nâng cấp: +{weapon.UpgradeLevel}", bpX + 60, bpY + 150, 12, Color.LightGray);
                                Raylib.DrawText($"Sát thương hiện tại: {weapon.Damage}", bpX + 60, bpY + 175, 12, Color.LightGray);
                                
                                int cost = 100 * (weapon.UpgradeLevel + 1);
                                Raylib.DrawText($"Chi phí nâng cấp: {cost} Vàng", bpX + 60, bpY + 210, 13, Config.ColorGold);

                                Raylib.DrawText("Nhấn [U] Để Nâng Cấp (+3 Sát Thương)", bpX + 60, bpY + 250, 14, Color.Green);

                                if (Raylib.IsKeyPressed(KeyboardKey.U))
                                {
                                    if (player.Gold >= cost)
                                    {
                                        player.Gold -= cost;
                                        weapon.UpgradeLevel++;
                                        player.RecalculateAttributes();
                                        floatingTexts.Add(new FloatingText
                                        {
                                            X = player.X, Y = player.Y - 30,
                                            Text = $"{weapon.Name} nâng lên +{weapon.UpgradeLevel}!",
                                            Color = Config.ColorGold,
                                            Lifetime = 60, MaxLifetime = 60
                                        });
                                    }
                                    else
                                    {
                                        floatingTexts.Add(new FloatingText
                                        {
                                            X = player.X, Y = player.Y - 30,
                                            Text = "Không đủ Vàng!",
                                            Color = Color.Red,
                                            Lifetime = 45, MaxLifetime = 45
                                        });
                                    }
                                }
                            }
                        }

                        Raylib.DrawText("[ESC] Đóng", bpX + 480, bpY + 370, 10, Color.Gray);
                    }

                    // ── Storage Chest UI ─────────────────────────────────────────
                    if (showStorageUI)
                    {
                        int spWidth = 760;
                        int spHeight = 380;
                        int spX = Config.ScreenWidth / 2 - spWidth / 2;
                        int spY = Config.ScreenHeight / 2 - spHeight / 2;

                        // Draw background panel (glassmorphism look with green tint)
                        Raylib.DrawRectangle(spX, spY, spWidth, spHeight, new Color(10, 24, 15, 240));
                        Raylib.DrawRectangleLines(spX, spY, spWidth, spHeight, new Color(80, 200, 120, 255));
                        Raylib.DrawText("■  KHO CHỨA ĐỒ SANCTUARY", spX + 20, spY + 15, 16, new Color(80, 200, 120, 255));
                        Raylib.DrawLine(spX, spY + 38, spX + spWidth, spY + 38, new Color(80, 200, 120, 100));

                        // Left Side: Storage Items
                        int storageLimit = 20;
                        Raylib.DrawText($"Kho chứa đồ: {sanctuary.StorageItems.Count}/{storageLimit} vật phẩm", spX + 20, spY + 50, 12, Color.LightGray);

                        Vector2 mouseScrPos = Raylib.GetMousePosition();
                        Item? hoveredStorageItem = null;
                        int hoveredStorageIdx = -1;

                        for (int k = 0; k < storageLimit; k++)
                        {
                            int col = k % 4;
                            int row = k / 4;
                            int sx = spX + 20 + col * 85;
                            int sy = spY + 80 + row * 55;

                            Rectangle slotRec = new Rectangle(sx, sy, 78, 48);
                            Raylib.DrawRectangleRec(slotRec, new Color(20, 20, 25, 255));
                            Raylib.DrawRectangleLinesEx(slotRec, 1, Color.DarkGray);

                            if (k < sanctuary.StorageItems.Count)
                            {
                                Item item = sanctuary.StorageItems[k];
                                Raylib.DrawRectangleLinesEx(slotRec, 1.5f, item.RarityColor);

                                string shortName = item.Name.Length > 12 ? item.Name.Substring(0, 10) + ".." : item.Name;
                                Raylib.DrawText(shortName, sx + 6, sy + 18, 9, item.RarityColor);

                                if (Raylib.CheckCollisionPointRec(mouseScrPos, slotRec))
                                {
                                    hoveredStorageItem = item;
                                    hoveredStorageIdx = k;
                                    Raylib.DrawRectangleLinesEx(slotRec, 2f, Color.White);
                                }
                            }
                        }

                        // Right Side: Player Inventory
                        Raylib.DrawText($"Hành trang người chơi: {player.Inventory.Items.Count}/{Inventory.MaxSlots} vật phẩm", spX + 400, spY + 50, 12, Color.LightGray);

                        Item? hoveredPlayerItem = null;
                        int hoveredPlayerIdx = -1;

                        for (int k = 0; k < Inventory.MaxSlots; k++)
                        {
                            int col = k % 4;
                            int row = k / 4;
                            int sx = spX + 400 + col * 85;
                            int sy = spY + 80 + row * 55;

                            Rectangle slotRec = new Rectangle(sx, sy, 78, 48);
                            Raylib.DrawRectangleRec(slotRec, new Color(20, 20, 25, 255));
                            Raylib.DrawRectangleLinesEx(slotRec, 1, Color.DarkGray);

                            if (k < player.Inventory.Items.Count)
                            {
                                Item item = player.Inventory.Items[k];
                                Raylib.DrawRectangleLinesEx(slotRec, 1.5f, item.RarityColor);

                                string shortName = item.Name.Length > 12 ? item.Name.Substring(0, 10) + ".." : item.Name;
                                Raylib.DrawText(shortName, sx + 6, sy + 18, 9, item.RarityColor);

                                if (Raylib.CheckCollisionPointRec(mouseScrPos, slotRec))
                                {
                                    hoveredPlayerItem = item;
                                    hoveredPlayerIdx = k;
                                    Raylib.DrawRectangleLinesEx(slotRec, 2f, Color.White);
                                }
                            }
                        }

                        // Click actions:
                        if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                        {
                            if (hoveredStorageIdx >= 0 && hoveredStorageItem != null)
                            {
                                // Withdraw item
                                if (player.Inventory.AddItem(hoveredStorageItem))
                                {
                                    sanctuary.StorageItems.RemoveAt(hoveredStorageIdx);
                                    floatingTexts.Add(new FloatingText { X = player.X, Y = player.Y - 25, Text = $"Rút: {hoveredStorageItem.Name}", Color = hoveredStorageItem.RarityColor, Lifetime = 40, MaxLifetime = 40 });
                                }
                                else
                                {
                                    floatingTexts.Add(new FloatingText { X = player.X, Y = player.Y - 25, Text = "Hành trang đầy!", Color = Color.Red, Lifetime = 40, MaxLifetime = 40 });
                                }
                            }
                            else if (hoveredPlayerIdx >= 0 && hoveredPlayerItem != null)
                            {
                                // Deposit item
                                if (sanctuary.StorageItems.Count < storageLimit)
                                {
                                    sanctuary.StorageItems.Add(hoveredPlayerItem);
                                    player.Inventory.RemoveItem(hoveredPlayerItem);
                                    floatingTexts.Add(new FloatingText { X = player.X, Y = player.Y - 25, Text = $"Cất: {hoveredPlayerItem.Name}", Color = hoveredPlayerItem.RarityColor, Lifetime = 40, MaxLifetime = 40 });
                                }
                                else
                                {
                                    floatingTexts.Add(new FloatingText { X = player.X, Y = player.Y - 25, Text = "Kho đồ đầy!", Color = Color.Red, Lifetime = 40, MaxLifetime = 40 });
                                }
                            }
                        }

                        // Tooltip rendering for hovered items
                        Item? tooltipItem = hoveredStorageItem ?? hoveredPlayerItem;
                        if (tooltipItem != null)
                        {
                            int tX = (int)mouseScrPos.X + 15;
                            int tY = (int)mouseScrPos.Y + 15;

                            int tooltipW = 200;
                            int tooltipH = 140;

                            if (tX + tooltipW > Config.ScreenWidth) tX = (int)mouseScrPos.X - tooltipW - 15;
                            if (tY + tooltipH > Config.ScreenHeight) tY = (int)mouseScrPos.Y - tooltipH - 15;

                            Raylib.DrawRectangle(tX, tY, tooltipW, tooltipH, new Color(10, 10, 15, 255));
                            Raylib.DrawRectangleLines(tX, tY, tooltipW, tooltipH, tooltipItem.RarityColor);

                            Raylib.DrawText(tooltipItem.Name, tX + 12, tY + 12, 12, tooltipItem.RarityColor);
                            Raylib.DrawText($"Độ hiếm: {tooltipItem.Rarity}", tX + 12, tY + 30, 9, Color.Gray);
                            Raylib.DrawText($"Ô: {tooltipItem.SlotType}", tX + 12, tY + 42, 9, Color.Gray);

                            int statRow = 0;
                            void DrawStatLine(string label)
                            {
                                Raylib.DrawText(label, tX + 12, tY + 60 + statRow * 12, 9, Color.LightGray);
                                statRow++;
                            }

                            if (tooltipItem.Damage > 0) DrawStatLine($"+{tooltipItem.Damage} Sát thương");
                            if (tooltipItem.Defense > 0) DrawStatLine($"+{tooltipItem.Defense} Phòng thủ");
                            if (tooltipItem.CritChance > 0.001f) DrawStatLine($"+{(int)(tooltipItem.CritChance * 100)}% Chí mạng");
                            if (tooltipItem.VigorBonus > 0) DrawStatLine($"+{tooltipItem.VigorBonus} Sức sống");
                            if (tooltipItem.StrengthBonus > 0) DrawStatLine($"+{tooltipItem.StrengthBonus} Sức mạnh");
                            if (tooltipItem.DexterityBonus > 0) DrawStatLine($"+{tooltipItem.DexterityBonus} Khéo léo");
                            if (tooltipItem.IntelligenceBonus > 0) DrawStatLine($"+{tooltipItem.IntelligenceBonus} Trí tuệ");
                            if (tooltipItem.VitalityBonus > 0) DrawStatLine($"+{tooltipItem.VitalityBonus} Thể chất");
                        }

                        Raylib.DrawText("[ESC] Đóng", spX + spWidth - 90, spY + spHeight - 25, 10, Color.Gray);
                    }

                    if (showCharacterScreen)
                    {
                        Raylib.DrawRectangle(15, 170, 994, 440, new Color(12, 12, 16, 245));
                        Raylib.DrawRectangleLines(15, 170, 994, 440, Color.Gray);

                        Raylib.DrawText("BẢNG CHỈ SỐ NHÂN VẬT", 35, 190, 15, Config.ColorGold);
                        Raylib.DrawText($"Điểm chưa cộng: {player.FreeStatPoints}", 35, 215, 12, Color.LightGray);

                        string[] statNames = { "Vigor (Sức sống)", "Strength (Sức mạnh)", "Dexterity (Khéo léo)", "Intelligence (Trí tuệ)", "Vitality (Thể chất)" };
                        int[] statValues = { player.Vigor, player.Strength, player.Dexterity, player.Intelligence, player.Vitality };
                        string[] statKeys = { "vigor", "strength", "dexterity", "intelligence", "vitality" };

                        Vector2 mouseScrPos = Raylib.GetMousePosition();

                        for (int k = 0; k < statNames.Length; k++)
                        {
                            int rowY = 245 + k * 50;
                            Raylib.DrawRectangle(35, rowY, 260, 40, new Color(24, 24, 30, 255));
                            Raylib.DrawRectangleLines(35, rowY, 260, 40, Color.DarkGray);

                            Raylib.DrawText(statNames[k], 45, rowY + 6, 10, Color.Gray);
                            Raylib.DrawText(statValues[k].ToString(), 45, rowY + 20, 14, Color.White);

                            Rectangle plusBtnRec = new Rectangle(265, rowY + 8, 22, 22);
                            bool hasPoints = player.FreeStatPoints > 0;
                            Raylib.DrawRectangleRec(plusBtnRec, hasPoints ? Config.ColorMana : new Color(30, 30, 35, 255));
                            Raylib.DrawRectangleLinesEx(plusBtnRec, 1, Color.DarkGray);
                            Raylib.DrawText("+", 272, rowY + 12, 14, hasPoints ? Color.White : Color.Gray);

                            if (hasPoints && Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouseScrPos, plusBtnRec))
                            {
                                player.AllocateStat(statKeys[k]);
                            }
                        }

                        Raylib.DrawText("TRANG BỊ ĐANG MẶC", 340, 190, 15, Config.ColorGold);

                        string[] slotNames = { "Weapon", "Helmet", "Armor", "Gloves", "Boots", "Ring1", "Ring2", "Necklace" };
                        string[] slotLabels = { "VŨ KHÍ", "NÓN", "GIÁP NGỰC", "BAO TAY", "GIÀY", "NHẪN 1", "NHẪN 2", "DÂY CHUYỀN" };
                        Item?[] equippedItems = { player.Equipment.Weapon, player.Equipment.Helmet, player.Equipment.Armor, player.Equipment.Gloves, player.Equipment.Boots, player.Equipment.Ring1, player.Equipment.Ring2, player.Equipment.Necklace };

                        Rectangle[] slotRecs = new Rectangle[8];
                        for (int k = 0; k < 8; k++)
                        {
                            int sx = 340 + (k % 2) * 160;
                            int sy = 230 + (k / 2) * 85;
                            slotRecs[k] = new Rectangle(sx, sy, 140, 65);

                            Item? item = equippedItems[k];
                            Raylib.DrawRectangleRec(slotRecs[k], new Color(20, 20, 25, 255));
                            Raylib.DrawRectangleLinesEx(slotRecs[k], 1, item != null ? item.RarityColor : Color.DarkGray);

                            Raylib.DrawText(slotLabels[k], sx + 10, sy + 8, 9, Color.DarkGray);

                            if (item != null)
                            {
                                string shortName = item.Name.Length > 17 ? item.Name.Substring(0, 15) + ".." : item.Name;
                                Raylib.DrawText(shortName, sx + 10, sy + 28, 11, item.RarityColor);
                                Raylib.DrawText(item.Rarity, sx + 10, sy + 45, 9, Color.Gray);
                            }
                            else
                            {
                                Raylib.DrawText("(TRỐNG)", sx + 10, sy + 32, 11, Color.DarkGray);
                            }

                            if (item != null && Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouseScrPos, slotRecs[k]))
                            {
                                if (player.Inventory.AddItem(item))
                                {
                                    player.Equipment.Unequip(slotNames[k]);
                                    player.RecalculateAttributes();
                                    floatingTexts.Add(new FloatingText { X = player.X, Y = player.Y - 20, Text = $"Tháo: {item.Name}", Color = Color.Gray, Lifetime = 40, MaxLifetime = 40 });
                                }
                                else
                                {
                                    floatingTexts.Add(new FloatingText { X = player.X, Y = player.Y - 20, Text = "Hành trang đầy!", Color = Color.Red, Lifetime = 40, MaxLifetime = 40 });
                                }
                            }
                        }

                        Raylib.DrawText("TÚI ĐỒ HÀNH TRANG", 685, 190, 15, Config.ColorGold);
                        Raylib.DrawText($"Sức chứa: {player.Inventory.Items.Count}/{Inventory.MaxSlots}", 685, 215, 12, Color.LightGray);

                        Rectangle[] invRecs = new Rectangle[Inventory.MaxSlots];
                        Item? hoveredItem = null;

                        for (int k = 0; k < Inventory.MaxSlots; k++)
                        {
                            int col = k % 4;
                            int row = k / 4;
                            int sx = 685 + col * 75;
                            int sy = 245 + row * 65;

                            invRecs[k] = new Rectangle(sx, sy, 68, 58);

                            Raylib.DrawRectangleRec(invRecs[k], new Color(20, 20, 25, 255));
                            Raylib.DrawRectangleLinesEx(invRecs[k], 1, Color.DarkGray);

                            if (k < player.Inventory.Items.Count)
                            {
                                Item item = player.Inventory.Items[k];
                                Raylib.DrawRectangleLinesEx(invRecs[k], 1.5f, item.RarityColor);

                                string initial = item.SlotType.Substring(0, 2).ToUpper();
                                Raylib.DrawText(initial, sx + 8, sy + 8, 9, Color.DarkGray);

                                string shortName = item.Name.Length > 10 ? item.Name.Substring(0, 8) + ".." : item.Name;
                                Raylib.DrawText(shortName, sx + 8, sy + 25, 9, item.RarityColor);

                                if (Raylib.CheckCollisionPointRec(mouseScrPos, invRecs[k]))
                                {
                                    hoveredItem = item;
                                    Raylib.DrawRectangleLinesEx(invRecs[k], 2f, Color.White);
                                }

                                if (Raylib.IsMouseButtonPressed(MouseButton.Left) && Raylib.CheckCollisionPointRec(mouseScrPos, invRecs[k]))
                                {
                                    string equipSlot = item.SlotType;
                                    if (equipSlot == "Ring")
                                    {
                                        equipSlot = player.Equipment.Ring1 == null ? "Ring1" : "Ring2";
                                    }

                                    Item? oldItem = player.Equipment.Equip(item, equipSlot);
                                    player.Inventory.RemoveItem(item);

                                    if (oldItem != null)
                                    {
                                        player.Inventory.AddItem(oldItem);
                                    }

                                    player.RecalculateAttributes();
                                    floatingTexts.Add(new FloatingText { X = player.X, Y = player.Y - 20, Text = $"Trang bị: {item.Name}", Color = item.RarityColor, Lifetime = 40, MaxLifetime = 40 });
                                    break;
                                }
                            }
                        }

                        if (hoveredItem != null)
                        {
                            int tX = (int)mouseScrPos.X + 15;
                            int tY = (int)mouseScrPos.Y + 15;

                            int tooltipW = 200;
                            int tooltipH = 140;

                            if (tX + tooltipW > Config.ScreenWidth) tX = (int)mouseScrPos.X - tooltipW - 15;
                            if (tY + tooltipH > Config.ScreenHeight) tY = (int)mouseScrPos.Y - tooltipH - 15;

                            Raylib.DrawRectangle(tX, tY, tooltipW, tooltipH, new Color(10, 10, 15, 255));
                            Raylib.DrawRectangleLines(tX, tY, tooltipW, tooltipH, hoveredItem.RarityColor);

                            Raylib.DrawText(hoveredItem.Name, tX + 12, tY + 12, 12, hoveredItem.RarityColor);
                            Raylib.DrawText($"Độ hiếm: {hoveredItem.Rarity}", tX + 12, tY + 30, 9, Color.Gray);
                            Raylib.DrawText($"Ô: {hoveredItem.SlotType}", tX + 12, tY + 42, 9, Color.Gray);

                            int statRow = 0;
                            void DrawStatLine(string label)
                            {
                                Raylib.DrawText(label, tX + 12, tY + 60 + statRow * 12, 9, Color.LightGray);
                                statRow++;
                            }

                            if (hoveredItem.Damage > 0) DrawStatLine($"+{hoveredItem.Damage} Sát thương");
                            if (hoveredItem.Defense > 0) DrawStatLine($"+{hoveredItem.Defense} Phòng thủ");
                            if (hoveredItem.CritChance > 0.001f) DrawStatLine($"+{(int)(hoveredItem.CritChance * 100)}% Chí mạng");
                            if (hoveredItem.VigorBonus > 0) DrawStatLine($"+{hoveredItem.VigorBonus} Vigor (Sức sống)");
                            if (hoveredItem.StrengthBonus > 0) DrawStatLine($"+{hoveredItem.StrengthBonus} Strength (Sức mạnh)");
                            if (hoveredItem.DexterityBonus > 0) DrawStatLine($"+{hoveredItem.DexterityBonus} Dexterity (Khéo léo)");
                            if (hoveredItem.IntelligenceBonus > 0) DrawStatLine($"+{hoveredItem.IntelligenceBonus} Intelligence (Trí tuệ)");
                            if (hoveredItem.VitalityBonus > 0) DrawStatLine($"+{hoveredItem.VitalityBonus} Vitality (Thể chất)");
                        }
                    }

                    if (gameState == GameState.GameOver)
                    {
                        Raylib.DrawRectangle(0, 0, Config.ScreenWidth, Config.ScreenHeight, new Color(10, 10, 15, 200));
                        Raylib.DrawText("BẠN ĐÃ TỬ TRẬN", Config.ScreenWidth / 2 - 160, Config.ScreenHeight / 2 - 50, 36, Color.Red);
                        Raylib.DrawText("Nhấn SPACE (Dấu cách) để hồi sinh tại sảnh chờ", Config.ScreenWidth / 2 - 210, Config.ScreenHeight / 2 + 10, 14, Color.LightGray);

                        if (Raylib.IsKeyPressed(KeyboardKey.Space))
                        {
                            player = new Player();
                            currentLevel = 1;
                            gameState = GameState.Lobby;
                        }
                    }
                }

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
