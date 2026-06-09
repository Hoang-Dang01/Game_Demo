// shared/dungeon_generator.js
(function(exports) {

    // Seeded Random Number Generator (Mulberry32)
    function SeededRandom(seed) {
        let state = seed;
        return {
            random: function() {
                let t = state += 0x6D2B79F5;
                t = Math.imul(t ^ (t >>> 15), t | 1);
                t ^= t + Math.imul(t ^ (t >>> 7), t | 61);
                return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
            },
            randint: function(min, max) {
                return Math.floor(this.random() * (max - min + 1)) + min;
            },
            choice: function(arr) {
                if (!arr || arr.length === 0) return null;
                const idx = Math.floor(this.random() * arr.length);
                return arr[idx];
            }
        };
    }

    // Dungeon Generator class
    function generateDungeon(seed, level, width = 45, height = 45) {
        const rand = SeededRandom(seed);
        const grid = Array(height).fill(null).map(() => Array(width).fill(0)); // 0 = Wall, 1 = Floor, 2 = Stairs
        const rooms = [];
        
        const minRoomSize = 6;
        const maxRoomSize = 10;
        const maxRooms = 12;
        const tileSize = 48;

        // Helper to check if two rects overlap
        function rectsOverlap(r1, r2) {
            // Inflate by 1 tile buffer to keep rooms separated
            return !(r1.x + r1.w + 1 < r2.x - 1 || 
                     r1.x - 1 > r2.x + r2.w + 1 || 
                     r1.y + r1.h + 1 < r2.y - 1 || 
                     r1.y - 1 > r2.y + r2.h + 1);
        }

        // Generate rooms
        for (let i = 0; i < maxRooms; i++) {
            const w = rand.randint(minRoomSize, maxRoomSize);
            const h = rand.randint(minRoomSize, maxRoomSize);
            const x = rand.randint(2, width - w - 3);
            const y = rand.randint(2, height - h - 3);

            const room = {
                x: x, y: y, w: w, h: h,
                cx: Math.floor(x + w / 2),
                cy: Math.floor(y + h / 2)
            };

            let overlap = false;
            for (let j = 0; j < rooms.length; j++) {
                if (rectsOverlap(room, rooms[j])) {
                    overlap = true;
                    break;
                }
            }

            if (!overlap) {
                rooms.push(room);
                // Carve room
                for (let ry = room.y; ry < room.y + room.h; ry++) {
                    for (let rx = room.x; rx < room.x + room.w; rx++) {
                        grid[ry][rx] = 1;
                    }
                }

                // Connect rooms
                if (rooms.length > 1) {
                    const prevRoom = rooms[rooms.length - 2];
                    connectRooms(prevRoom, room, grid, rand);
                }
            }
        }

        // Carver corridors
        function connectRooms(r1, r2, grid, rand) {
            let x1 = r1.cx, y1 = r1.cy;
            let x2 = r2.cx, y2 = r2.cy;

            if (rand.random() < 0.5) {
                carveHLine(x1, x2, y1, grid);
                carveVLine(y1, y2, x2, grid);
            } else {
                carveVLine(y1, y2, x1, grid);
                carveHLine(x1, x2, y2, grid);
            }
        }

        function carveHLine(x1, x2, y, grid) {
            for (let x = Math.min(x1, x2); x <= Math.max(x1, x2); x++) {
                grid[y][x] = 1;
            }
        }

        function carveVLine(y1, y2, x, grid) {
            for (let y = Math.min(y1, y2); y <= Math.max(y1, y2); y++) {
                grid[y][x] = 1;
            }
        }

        // Set spawns and metadata
        const result = {
            width: width,
            height: height,
            grid: grid,
            playerSpawn: { x: 0, y: 0 },
            stairsSpawn: { x: 0, y: 0 },
            bossSpawn: null,
            merchantSpawn: null,
            shopPedestals: [],
            chests: [],
            enemies: [],
            lanterns: []
        };

        if (rooms.length >= 3) {
            // Room 0: Player spawn
            const first = rooms[0];
            result.playerSpawn = {
                x: first.cx * tileSize + tileSize / 2,
                y: first.cy * tileSize + tileSize / 2
            };

            // Room 1: Shop (Town/Merchant) room (Safe zone, no enemies)
            const shop = rooms[1];
            result.merchantSpawn = {
                x: shop.cx * tileSize + tileSize / 2,
                y: (shop.cy - 1) * tileSize + tileSize / 2
            };
            result.shopPedestals = [
                { x: (shop.cx - 1) * tileSize + tileSize / 2, y: (shop.cy + 1) * tileSize + tileSize / 2, type: "hp_upgrade" },
                { x: shop.cx * tileSize + tileSize / 2, y: (shop.cy + 1) * tileSize + tileSize / 2, type: "weapon" },
                { x: (shop.cx + 1) * tileSize + tileSize / 2, y: (shop.cy + 1) * tileSize + tileSize / 2, type: "mp_upgrade" } // even without mana, we can sell damage upgrades!
            ];
            result.lanterns.push({ x: shop.cx * tileSize + tileSize / 2, y: shop.cy * tileSize + tileSize / 2 });

            // Room N-1: Exit Stairs & Boss room (if Boss Floor)
            const last = rooms[rooms.length - 1];
            result.stairsSpawn = {
                x: last.cx * tileSize + tileSize / 2,
                y: last.cy * tileSize + tileSize / 2
            };
            grid[last.cy][last.cx] = 2; // Mark stairs down
            result.lanterns.push({ x: last.cx * tileSize + tileSize / 2, y: last.cy * tileSize + tileSize / 2 });

            // Check if Boss Floor (every 5th floor)
            if (level % 5 === 0) {
                result.bossSpawn = {
                    x: last.cx * tileSize + tileSize / 2,
                    y: last.cy * tileSize + tileSize / 2
                };
            }

            // Room 2 to N-2: Combat rooms, Chests, Loot
            for (let i = 2; i < rooms.length - 1; i++) {
                const room = rooms[i];
                
                // Spawn Chest (60% chance)
                if (rand.random() < 0.6) {
                    result.chests.push({
                        x: room.cx * tileSize + tileSize / 2,
                        y: room.cy * tileSize + tileSize / 2
                    });
                }

                // Spawn Enemies (1 to 3)
                const numEnemies = rand.randint(1, 3);
                for (let k = 0; k < numEnemies; k++) {
                    const ex = rand.randint(room.x + 1, room.x + room.w - 2);
                    const ey = rand.randint(room.y + 1, room.y + room.h - 2);
                    
                    // Don't spawn exactly on center if chest is there
                    if (ex !== room.cx || ey !== room.cy) {
                        const enemyType = rand.random() < 0.7 ? "melee" : "ranged";
                        result.enemies.push({
                            x: ex * tileSize + tileSize / 2,
                            y: ey * tileSize + tileSize / 2,
                            type: enemyType
                        });
                    }
                }
            }
        }

        return result;
    }

    // Export generator function and randomizer
    exports.SeededRandom = SeededRandom;
    exports.generateDungeon = generateDungeon;

})(typeof exports === 'undefined' ? this.DungeonGenerator = {} : exports);
