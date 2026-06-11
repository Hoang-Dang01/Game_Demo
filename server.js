// server.js
const express = require('express');
const http = require('http');
const WebSocket = require('ws');
const path = require('path');
const { generateDungeon, generateSanctuary, SeededRandom } = require('./shared/dungeon_generator.js');
const { generateRandomWeapon } = require('./shared/weapon_system.js');

const app = express();
const server = http.createServer(app);
const wss = new WebSocket.Server({ server });

const PORT = 3000;
const TILE_SIZE = 48;

// Cấu hình Express phục vụ thư mục tĩnh public
app.use(express.static(path.join(__dirname, 'public')));
app.use('/shared', express.static(path.join(__dirname, 'shared')));

// Quản lý các phòng chơi (Rooms/Lobbies)
const rooms = new Map();
let nextConnectionId = 1;
let logicTickCount = 0;

const BaseClassStats = {
    knight: { vigor: 10, strength: 12, dexterity: 8, intelligence: 5, vitality: 15 },
    assassin: { vigor: 5, strength: 10, dexterity: 15, intelligence: 6, vitality: 8 },
    archer: { vigor: 5, strength: 8, dexterity: 14, intelligence: 8, vitality: 10 },
    mage: { vigor: 3, strength: 5, dexterity: 8, intelligence: 15, vitality: 6 },
    support: { vigor: 7, strength: 6, dexterity: 8, intelligence: 12, vitality: 12 },
    summoner: { vigor: 6, strength: 7, dexterity: 8, intelligence: 11, vitality: 10 }
};

function recalculatePlayerAttributes(p) {
    p.maxHp = 50 + p.stats.vigor * 10;
    p.maxMp = 40 + p.stats.intelligence * 8;
    p.damageMultiplier = 1.0 + p.stats.strength * 0.02;
    p.critChance = 0.05 + p.stats.dexterity * 0.005;
    p.defense = p.stats.vitality * 0.5;
}

// Hàm tạo phòng mới
function createRoom(roomId, level = 0) {
    const seed = Math.floor(Math.random() * 999999);
    const mapData = level === 0 ? generateSanctuary() : generateDungeon(seed, level);
    
    const room = {
        id: roomId,
        level: level,
        seed: seed,
        mapData: mapData,
        entities: new Map(), // key: entityId, value: entityObject
        playerCount: 0,
        nextEntityId: 1,
        state: "PLAYING", // "PLAYING", "GAMEOVER"
        stairsLocked: level !== 0 && level % 5 === 0, // Tầng Boss sẽ khóa cổng thoát
        returnPortal: { floor: 1, seed: 0, x: 0, y: 0, active: false }
    };

    // Spawn các thực thể ban đầu trong hầm ngục
    spawnDungeonEntities(room);
    rooms.set(roomId, room);
    return room;
}

function spawnDungeonEntities(room) {
    room.entities.clear();
    
    if (room.level === 0) {
        return;
    }
    
    // 1. Spawning Rương báu (Chests)
    room.mapData.chests.forEach(pos => {
        const id = "c_" + room.nextEntityId++;
        room.entities.set(id, {
            id, type: "chest", x: pos.x, y: pos.y, radius: 16, opened: false
        });
    });

    // 2. Chuyển đổi các bục Cửa hàng thành Rương báu an toàn (như đã đồng ý tối giản shop)
    room.mapData.shopPedestals.forEach(pos => {
        const id = "c_" + room.nextEntityId++;
        room.entities.set(id, {
            id, type: "chest", x: pos.x, y: pos.y, radius: 16, opened: false
        });
    });

    // 3. Spawning Quái vật thường
    room.mapData.enemies.forEach(pos => {
        const id = "e_" + room.nextEntityId++;
        const speed = pos.type === "melee" ? 2.0 : 1.4;
        const maxHp = pos.type === "melee" ? 50 : 35;
        const damage = pos.type === "melee" ? 10 : 8;
        
        room.entities.set(id, {
            id, type: "enemy", enemyType: pos.type,
            x: pos.x, y: pos.y, radius: 14,
            hp: maxHp, maxHp: maxHp, damage: damage, speed: speed,
            kx: 0, ky: 0, // lực knockback
            knockback_decay: 0.85,
            shootCooldown: pos.type === "ranged" ? Math.floor(Math.random() * 50) + 30 : 0
        });
    });

    // 4. Spawning Boss khổng lồ (nếu có)
    if (room.mapData.bossSpawn) {
        const id = "e_boss";
        room.entities.set(id, {
            id, type: "boss",
            x: room.mapData.bossSpawn.x, y: room.mapData.bossSpawn.y, radius: 28,
            hp: 500, maxHp: 500, damage: 15, speed: 1.8,
            kx: 0, ky: 0,
            knockback_decay: 0.85,
            shootCooldown: 80,
            attackPattern: 0,
            enraged: false
        });
    }
}

// Xử lý kết nối WebSocket
wss.on('connection', (ws) => {
    const connectionId = nextConnectionId++;
    let playerEntity = null;
    let currentRoom = null;

    ws.on('message', (message) => {
        try {
            const packet = JSON.parse(message);
            
            // 1. Packet yêu cầu tham gia phòng (join)
            if (packet.type === "join") {
                const nickname = packet.nickname || "Player_" + connectionId;
                const classType = packet.classType || "knight";
                let reqRoomId = packet.roomId || "dungeon_1";
                
                // Tìm phòng có sẵn hoặc tạo mới
                currentRoom = rooms.get(reqRoomId);
                if (!currentRoom || currentRoom.playerCount >= 4) {
                    if (currentRoom && currentRoom.playerCount >= 4) {
                        reqRoomId = "dungeon_" + (rooms.size + 1);
                    }
                    currentRoom = createRoom(reqRoomId, 0);
                }

                // Thiết lập nhân vật
                const baseStats = BaseClassStats[classType] || BaseClassStats["knight"];
                const rand = SeededRandom(currentRoom.seed + connectionId);
                const baseWeapon = generateRandomWeapon(classType, 1, rand);
                
                const baseWeaponNames = {
                    knight: "Kiếm Tập Sự",
                    assassin: "Dao Găm Tập Sự",
                    archer: "Cung Tập Sự",
                    mage: "Trượng Tập Sự",
                    support: "Sách Phép Tập Sự",
                    summoner: "Ngọc Tập Sự"
                };
                baseWeapon.name = baseWeaponNames[classType] || "Kiếm Tập Sự";
                baseWeapon.rarity = "Common";
                baseWeapon.color = "#f0f0f5";

                playerEntity = {
                    id: "p_" + connectionId,
                    type: "player",
                    nickname: nickname,
                    classType: classType,
                    x: currentRoom.mapData.playerSpawn.x,
                    y: currentRoom.mapData.playerSpawn.y,
                    radius: 16,
                    stats: { ...baseStats },
                    freeStatPoints: 0,
                    weapon: baseWeapon,
                    gold: 0,
                    xp: 0,
                    level: 1,
                    
                    // Sanctuary state
                    portalStoneCount: 3,
                    storageItems: [],
                    
                    isDashing: false,
                    dashTimer: 0,
                    dashDir: { x: 0, y: 0 },
                    dashCooldown: 0,
                    attackCooldown: 0,
                    angle: 0,
                    
                    input: { dx: 0, dy: 0, angle: 0, attack: false, dash: false, interact: false },
                    socket: ws
                };

                recalculatePlayerAttributes(playerEntity);
                playerEntity.hp = playerEntity.maxHp;
                playerEntity.mp = playerEntity.maxMp;

                currentRoom.entities.set(playerEntity.id, playerEntity);
                currentRoom.playerCount++;

                // Phản hồi đã tham gia
                ws.send(JSON.stringify({
                    type: "joined",
                    playerId: playerEntity.id,
                    roomId: currentRoom.id,
                    level: currentRoom.level,
                    seed: currentRoom.seed,
                    stairsLocked: currentRoom.stairsLocked
                }));
            }
            
            // 2. Packet cập nhật Input bàn phím & chuột
            else if (packet.type === "input" && playerEntity) {
                playerEntity.input.dx = typeof packet.dx === 'number' && !isNaN(packet.dx) ? packet.dx : 0;
                playerEntity.input.dy = typeof packet.dy === 'number' && !isNaN(packet.dy) ? packet.dy : 0;
                playerEntity.input.angle = typeof packet.angle === 'number' && !isNaN(packet.angle) ? packet.angle : 0;
                playerEntity.input.attack = !!packet.attack;
                playerEntity.input.dash = !!packet.dash;
                playerEntity.input.interact = !!packet.interact;
                playerEntity.angle = playerEntity.input.angle;
            }
            
            // 3. Packet phân bổ điểm chỉ số
            else if (packet.type === "allocate_stat" && playerEntity) {
                const stat = packet.stat;
                if (playerEntity.freeStatPoints > 0 && playerEntity.stats && playerEntity.stats[stat] !== undefined) {
                    playerEntity.freeStatPoints--;
                    playerEntity.stats[stat]++;
                    
                    const oldMaxHp = playerEntity.maxHp;
                    const oldMaxMp = playerEntity.maxMp;
                    
                    recalculatePlayerAttributes(playerEntity);
                    
                    if (playerEntity.maxHp > oldMaxHp) {
                        playerEntity.hp += (playerEntity.maxHp - oldMaxHp);
                    }
                    if (playerEntity.maxMp > oldMaxMp) {
                        playerEntity.mp += (playerEntity.maxMp - oldMaxMp);
                    }
                    
                    sendPrivateText(playerEntity, `+1 ${stat.toUpperCase()}!`, "#f0be1e");
                }
            }
            // 4. Packet gian lận để thêm EXP (chỉ chạy trong DEV)
            else if (packet.type === "cheat_exp" && playerEntity) {
                playerEntity.xp += packet.amount || 100;
                const expNeeded = Math.round(100 * Math.pow(playerEntity.level, 1.5));
                if (playerEntity.xp >= expNeeded) {
                    playerEntity.level++;
                    playerEntity.xp -= expNeeded;
                    playerEntity.freeStatPoints += 5;
                    
                    recalculatePlayerAttributes(playerEntity);
                    playerEntity.hp = playerEntity.maxHp;
                    playerEntity.mp = playerEntity.maxMp;
                    
                    sendPrivateText(playerEntity, `LÊN CẤP! Cấp ${playerEntity.level}`, "#f0c81e");
                }
            }
            // 5. Packet nâng cấp vũ khí (Blacksmith)
            else if (packet.type === "upgrade_weapon" && playerEntity && currentRoom) {
                if (currentRoom.level !== 0 || !currentRoom.mapData.merchantSpawn) return;
                const bx = currentRoom.mapData.merchantSpawn.x;
                const by = currentRoom.mapData.merchantSpawn.y;
                const dist = Math.sqrt((playerEntity.x - bx) ** 2 + (playerEntity.y - by) ** 2);
                if (dist < 70) {
                    if (playerEntity.weapon && playerEntity.weapon.name !== "Tay Không") {
                        const upgLevel = playerEntity.weapon.upgradeLevel || 0;
                        const cost = 100 * (upgLevel + 1);
                        if (playerEntity.gold >= cost) {
                            playerEntity.gold -= cost;
                            playerEntity.weapon.upgradeLevel = upgLevel + 1;
                            playerEntity.weapon.damage += 3;
                            playerEntity.weapon.name = playerEntity.weapon.name.replace(/\s\+\d+$/, "") + " +" + playerEntity.weapon.upgradeLevel;
                            
                            recalculatePlayerAttributes(playerEntity);
                            
                            sendPrivateText(playerEntity, `${playerEntity.weapon.name} nâng lên +${playerEntity.weapon.upgradeLevel}!`, "#f0be1e");
                        } else {
                            sendPrivateText(playerEntity, "Không đủ Vàng!", "#ff3250");
                        }
                    } else {
                        sendPrivateText(playerEntity, "Không có vũ khí để nâng cấp!", "#ff3250");
                    }
                }
            }
            // 6. Packet cất vũ khí vào kho (Storage Chest)
            else if (packet.type === "deposit_weapon" && playerEntity && currentRoom) {
                if (currentRoom.level !== 0 || currentRoom.mapData.shopPedestals.length === 0) return;
                const sx = currentRoom.mapData.shopPedestals[0].x;
                const sy = currentRoom.mapData.shopPedestals[0].y;
                const dist = Math.sqrt((playerEntity.x - sx) ** 2 + (playerEntity.y - sy) ** 2);
                if (dist < 65) {
                    if (!playerEntity.storageItems) playerEntity.storageItems = [];
                    if (playerEntity.storageItems.length >= 20) {
                        sendPrivateText(playerEntity, "Kho đồ đầy!", "#ff3250");
                        return;
                    }
                    if (playerEntity.weapon && playerEntity.weapon.name !== "Tay Không") {
                        playerEntity.storageItems.push(playerEntity.weapon);
                        playerEntity.weapon = {
                            name: "Tay Không",
                            classLimit: "",
                            type: "melee",
                            rarity: "Common",
                            color: "#8a8a98",
                            damage: 5,
                            cooldown: 15,
                            range: 45,
                            speed: 0,
                            splashRadius: 0,
                            critChance: 0.05,
                            attackSpeedBonus: 0,
                            upgradeLevel: 0
                        };
                        sendPrivateText(playerEntity, "Đã cất vũ khí vào kho!", "#50dc50");
                    } else {
                        sendPrivateText(playerEntity, "Không có vũ khí để cất!", "#ff3250");
                    }
                }
            }
            // 7. Packet rút vũ khí khỏi kho (Storage Chest)
            else if (packet.type === "withdraw_weapon" && playerEntity && currentRoom) {
                if (currentRoom.level !== 0 || currentRoom.mapData.shopPedestals.length === 0) return;
                const sx = currentRoom.mapData.shopPedestals[0].x;
                const sy = currentRoom.mapData.shopPedestals[0].y;
                const dist = Math.sqrt((playerEntity.x - sx) ** 2 + (playerEntity.y - sy) ** 2);
                if (dist < 65) {
                    const idx = packet.index;
                    if (!playerEntity.storageItems || idx < 0 || idx >= playerEntity.storageItems.length) return;
                    const storedWeapon = playerEntity.storageItems[idx];
                    if (storedWeapon) {
                        if (playerEntity.weapon && playerEntity.weapon.name !== "Tay Không") {
                            playerEntity.storageItems[idx] = playerEntity.weapon;
                            playerEntity.weapon = storedWeapon;
                            sendPrivateText(playerEntity, `Đổi vũ khí với kho!`, "#50dc50");
                        } else {
                            playerEntity.weapon = storedWeapon;
                            playerEntity.storageItems.splice(idx, 1);
                            sendPrivateText(playerEntity, `Rút: ${storedWeapon.name}`, "#50dc50");
                        }
                    }
                }
            }
            // 8. Packet dùng Đá Dịch Chuyển (Portal Stone)
            else if (packet.type === "use_portal_stone" && playerEntity && currentRoom) {
                if (currentRoom.level === 0) return;
                if (playerEntity.hp <= 0) return;
                if ((playerEntity.portalStoneCount || 0) > 0) {
                    playerEntity.portalStoneCount--;
                    
                    currentRoom.returnPortal = {
                        floor: currentRoom.level,
                        seed: currentRoom.seed,
                        x: playerEntity.x,
                        y: playerEntity.y,
                        active: true
                    };
                    
                    currentRoom.level = 0;
                    currentRoom.stairsLocked = false;
                    currentRoom.mapData = generateSanctuary();
                    
                    const pList = Array.from(currentRoom.entities.values()).filter(e => e.type === "player");
                    pList.forEach(p => {
                        p.x = currentRoom.mapData.playerSpawn.x;
                        p.y = currentRoom.mapData.playerSpawn.y;
                    });
                    
                    spawnDungeonEntities(currentRoom);
                    
                    broadcastToRoom(currentRoom, {
                        type: "next_floor",
                        level: currentRoom.level,
                        seed: currentRoom.seed,
                        stairsLocked: false,
                        playerSpawn: currentRoom.mapData.playerSpawn
                    });
                    
                    broadcastToRoom(currentRoom, {
                        type: "combat_text", x: playerEntity.x, y: playerEntity.y - 30,
                        text: "Dịch chuyển về Sanctuary!", color: "#b464ff"
                    });
                } else {
                    sendPrivateText(playerEntity, "Không có Đá Dịch Chuyển!", "#ff3250");
                }
            }
            // 9. Packet tương tác với Portal để quay lại Dungeon
            else if (packet.type === "interact_portal" && playerEntity && currentRoom) {
                if (currentRoom.level !== 0 || currentRoom.mapData.shopPedestals.length < 2) return;
                const px = currentRoom.mapData.shopPedestals[1].x;
                const py = currentRoom.mapData.shopPedestals[1].y;
                const dist = Math.sqrt((playerEntity.x - px) ** 2 + (playerEntity.y - py) ** 2);
                if (dist < 70) {
                    const ret = currentRoom.returnPortal;
                    if (ret && ret.active) {
                        currentRoom.level = ret.floor;
                        currentRoom.seed = ret.seed;
                        currentRoom.stairsLocked = currentRoom.level % 5 === 0;
                        currentRoom.mapData = generateDungeon(currentRoom.seed, currentRoom.level);
                        
                        const pList = Array.from(currentRoom.entities.values()).filter(e => e.type === "player");
                        pList.forEach(p => {
                            p.x = ret.x;
                            p.y = ret.y;
                        });
                        
                        spawnDungeonEntities(currentRoom);
                        ret.active = false;
                        
                        broadcastToRoom(currentRoom, {
                            type: "next_floor",
                            level: currentRoom.level,
                            seed: currentRoom.seed,
                            stairsLocked: currentRoom.stairsLocked,
                            playerSpawn: { x: ret.x, y: ret.y }
                        });
                    } else {
                        currentRoom.level = 1;
                        currentRoom.seed = Math.floor(Math.random() * 999999);
                        currentRoom.stairsLocked = false;
                        currentRoom.mapData = generateDungeon(currentRoom.seed, currentRoom.level);
                        
                        const pList = Array.from(currentRoom.entities.values()).filter(e => e.type === "player");
                        pList.forEach(p => {
                            p.x = currentRoom.mapData.playerSpawn.x;
                            p.y = currentRoom.mapData.playerSpawn.y;
                        });
                        
                        spawnDungeonEntities(currentRoom);
                        
                        broadcastToRoom(currentRoom, {
                            type: "next_floor",
                            level: currentRoom.level,
                            seed: currentRoom.seed,
                            stairsLocked: false,
                            playerSpawn: currentRoom.mapData.playerSpawn
                        });
                    }
                }
            }
        } catch (e) {
            console.error("Error processing packet: ", e);
        }
    });

    ws.on('close', () => {
        if (currentRoom && playerEntity) {
            currentRoom.entities.delete(playerEntity.id);
            currentRoom.playerCount--;
            if (currentRoom.playerCount <= 0) {
                rooms.delete(currentRoom.id);
            }
        }
    });
});

// VA CHẠM TƯỜNG (AABB check)
function isWallCollision(x, y, radius, grid) {
    const points = [
        { x: x - radius, y: y - radius },
        { x: x + radius, y: y - radius },
        { x: x - radius, y: y + radius },
        { x: x + radius, y: y + radius }
    ];

    for (let i = 0; i < points.length; i++) {
        const gx = Math.floor(points[i].x / TILE_SIZE);
        const gy = Math.floor(points[i].y / TILE_SIZE);
        
        if (gy < 0 || gy >= grid.length || gx < 0 || gx >= grid[0].length) {
            return true;
        }
        if (grid[gy][gx] === 0) {
            return true;
        }
    }
    return false;
}

// 20 HZ SERVER GAME TICK (Mỗi 50ms)
function logicTick() {
    logicTickCount++;
    const doRegen = (logicTickCount % 20 === 0); // 1Hz regen tick

    rooms.forEach(room => {
        if (room.playerCount <= 0) return;

        const players = Array.from(room.entities.values()).filter(e => e.type === "player");
        
        // --- 1. MOVEMENT & COLLISION SYSTEM ---
        players.forEach(p => {
            if (p.hp <= 0) return;

            // Xử lý nạp năng lượng lướt
            if (p.dashCooldown > 0) p.dashCooldown--;
            if (p.attackCooldown > 0) p.attackCooldown--;

            // Kích hoạt Lướt (Dash)
            if (p.input.dash && p.dashCooldown === 0 && !p.isDashing && (p.input.dx !== 0 || p.input.dy !== 0)) {
                p.isDashing = true;
                p.dashTimer = 8; // 0.4 giây ở 20Hz
                p.dashCooldown = 30; // 1.5 giây hồi chiêu
                const len = Math.sqrt(p.input.dx*p.input.dx + p.input.dy*p.input.dy);
                p.dashDir = { x: p.input.dx / len, y: p.input.dy / len };
            }

            if (p.isDashing) {
                p.dashTimer--;
                const speed = 11;
                const newX = p.x + p.dashDir.x * speed;
                const newY = p.y + p.dashDir.y * speed;
                
                if (!isWallCollision(newX, p.y, p.radius, room.mapData.grid)) p.x = newX;
                if (!isWallCollision(p.x, newY, p.radius, room.mapData.grid)) p.y = newY;
                
                if (p.dashTimer <= 0) p.isDashing = false;
            } else {
                // Di chuyển thường (Tốc chạy chịu ảnh hưởng bởi Dexterity)
                const speed = 4 + p.stats.dexterity * 0.02;
                const dx = p.input.dx;
                const dy = p.input.dy;
                
                if (dx !== 0 || dy !== 0) {
                    const len = Math.sqrt(dx*dx + dy*dy);
                    const newX = p.x + (dx / len) * speed;
                    const newY = p.y + (dy / len) * speed;
                    
                    if (!isWallCollision(newX, p.y, p.radius, room.mapData.grid)) p.x = newX;
                    if (!isWallCollision(p.x, newY, p.radius, room.mapData.grid)) p.y = newY;
                }
            }
        });

        // Hồi phục HP/MP thụ động (mỗi tick ở Sanctuary, 1Hz ở Dungeon)
        if (room.level === 0) {
            players.forEach(p => {
                if (p.hp > 0) {
                    p.hp = Math.min(p.maxHp, p.hp + 0.5);
                    p.mp = Math.min(p.maxMp, p.mp + 0.75);
                }
            });
        } else if (doRegen) {
            players.forEach(p => {
                if (p.hp > 0) {
                    const hpRegen = 1 + p.stats.vigor * 0.1;
                    const mpRegen = 2 + p.stats.intelligence * 0.15;
                    p.hp = Math.min(p.maxHp, parseFloat((p.hp + hpRegen).toFixed(2)));
                    p.mp = Math.min(p.maxMp, parseFloat((p.mp + mpRegen).toFixed(2)));
                }
            });
        }

        // --- 2. MONSTER AI SYSTEM ---
        const enemies = Array.from(room.entities.values()).filter(e => e.type === "enemy" || e.type === "boss");
        enemies.forEach(e => {
            if (e.hp <= 0) return;

            // Tìm người chơi gần nhất
            let nearestPlayer = null;
            let minDist = 350; // detect range
            players.forEach(p => {
                if (p.hp <= 0) return;
                const dx = p.x - e.x;
                const dy = p.y - e.y;
                const dist = Math.sqrt(dx*dx + dy*dy);
                if (dist < minDist) {
                    minDist = dist;
                    nearestPlayer = p;
                }
            });

            let ax = 0, ay = 0;
            if (nearestPlayer) {
                const dx = nearestPlayer.x - e.x;
                const dy = nearestPlayer.y - e.y;
                if (minDist > 0.1) {
                    ax = dx / minDist;
                    ay = dy / minDist;
                }
                if (e.type === "enemy") {
                    if (e.enemyType === "melee") {
                        // Cận chiến đuổi trực tiếp
                        const mx = ax * e.speed;
                        const my = ay * e.speed;
                        
                        // Gây sát thương khi va chạm cận chiến
                        if (minDist <= (e.radius + nearestPlayer.radius) && !nearestPlayer.isDashing) {
                            const finalDmg = Math.max(1, e.damage - nearestPlayer.defense);
                            nearestPlayer.hp = Math.max(0, nearestPlayer.hp - finalDmg);
                            // Đẩy lùi player nhẹ
                            nearestPlayer.x += ax * 10;
                            nearestPlayer.y += ay * 10;
                        }
                        
                        // Di chuyển có knockback
                        const finalX = e.x + mx + e.kx;
                        const finalY = e.y + my + e.ky;
                        if (!isWallCollision(finalX, e.y, e.radius, room.mapData.grid)) e.x = finalX;
                        if (!isWallCollision(e.x, finalY, e.radius, room.mapData.grid)) e.y = finalY;
                    } else {
                        // Tầm xa duy trì khoảng cách bắn đạn
                        let mx = 0, my = 0;
                        if (minDist > 180) {
                            mx = ax * e.speed;
                            my = ay * e.speed;
                        } else if (minDist < 140) {
                            mx = -ax * e.speed;
                            my = -ay * e.speed;
                        }
                        
                        const finalX = e.x + mx + e.kx;
                        const finalY = e.y + my + e.ky;
                        if (!isWallCollision(finalX, e.y, e.radius, room.mapData.grid)) e.x = finalX;
                        if (!isWallCollision(e.x, finalY, e.radius, room.mapData.grid)) e.y = finalY;

                        // Bắn phép
                        if (e.shootCooldown > 0) e.shootCooldown--;
                        if (e.shootCooldown === 0) {
                            e.shootCooldown = Math.floor(Math.random() * 40) + 40; // ~2-3 giây bắn một phát
                            const angle = Math.atan2(dy, dx);
                            const pid = "proj_" + room.nextEntityId++;
                            room.entities.set(pid, {
                                id: pid, type: "projectile", isPlayer: false,
                                x: e.x, y: e.y, radius: 6,
                                angle: angle, speed: 6.5, damage: e.damage, color: "#dc64ff"
                            });
                        }
                    }
                } else if (e.type === "boss") {
                    // Cập nhật Boss AI
                    if (e.hp < e.maxHp / 2 && !e.enraged) {
                        e.enraged = true;
                        e.speed = 2.3;
                    }

                    const mx = ax * e.speed;
                    const my = ay * e.speed;

                    // Gây sát thương húc liên tục (Giảm theo Vitality defense)
                    if (minDist <= (e.radius + nearestPlayer.radius) && !nearestPlayer.isDashing) {
                        const finalBossDmg = Math.max(0.3, 1.5 - nearestPlayer.defense * 0.05);
                        nearestPlayer.hp = Math.max(0, nearestPlayer.hp - finalBossDmg);
                    }

                    const finalX = e.x + mx + e.kx;
                    const finalY = e.y + my + e.ky;
                    if (!isWallCollision(finalX, e.y, e.radius, room.mapData.grid)) e.x = finalX;
                    if (!isWallCollision(e.x, finalY, e.radius, room.mapData.grid)) e.y = finalY;

                    if (e.shootCooldown > 0) e.shootCooldown--;
                    if (e.shootCooldown === 0) {
                        e.shootCooldown = e.enraged ? 35 : 60; // cooldown bắn nhanh gấp đôi
                        
                        if (e.attackPattern === 0) {
                            // Bắn đạn xoay tròn 360 độ (10 tia)
                            const numTia = 10;
                            for (let idx = 0; idx < numTia; idx++) {
                                const angle = (idx * 2 * Math.PI) / numTia;
                                const pid = "proj_" + room.nextEntityId++;
                                room.entities.set(pid, {
                                    id: pid, type: "projectile", isPlayer: false,
                                    x: e.x, y: e.y, radius: 7,
                                    angle: angle, speed: 4.5, damage: e.damage, color: "#c832ff"
                                });
                            }
                            e.attackPattern = 1;
                        } else {
                            // Phun chùm 4 đạn đuổi theo hướng player
                            const angleToPlayer = Math.atan2(dy, dx);
                            for (let idx = 0; idx < 4; idx++) {
                                const angleOffset = (idx - 1.5) * 0.2;
                                const pid = "proj_" + room.nextEntityId++;
                                room.entities.set(pid, {
                                    id: pid, type: "projectile", isPlayer: false,
                                    x: e.x, y: e.y, radius: 6,
                                    angle: angleToPlayer + angleOffset, speed: 5.5, damage: e.damage, color: "#ff78c8"
                                });
                            }
                            e.attackPattern = 0;
                        }
                    }
                }
            }

            // Giảm knockback
            e.kx *= e.knockback_decay;
            e.ky *= e.knockback_decay;
            if (Math.abs(e.kx) < 0.1) e.kx = 0;
            if (Math.abs(e.ky) < 0.1) e.ky = 0;
        });

        // --- 3. COMBAT & PROJECTILE SYSTEM ---
        // Xử lý tấn công của người chơi
        players.forEach(p => {
            if (p.hp <= 0) return;

            if (p.input.attack && p.attackCooldown === 0) {
                // Dexterity reduces attack cooldown
                p.attackCooldown = Math.max(5, Math.round(p.weapon.cooldown * (1 - p.stats.dexterity * 0.003)));
                
                if (p.weapon.type === "melee") {
                    // Tấn công quét góc chém cận chiến (Sweep angle)
                    let hitAny = false;
                    enemies.forEach(e => {
                        if (e.hp <= 0) return;
                        const dx = e.x - p.x;
                        const dy = e.y - p.y;
                        const dist = Math.sqrt(dx*dx + dy*dy);
                        
                        if (dist <= p.weapon.range) {
                            const enemyAngle = Math.atan2(dy, dx);
                            let diff = (enemyAngle - p.angle + Math.PI) % (2 * Math.PI) - Math.PI;
                            
                            // Góc chém lan khoảng 150 độ trước mặt
                            if (Math.abs(diff) <= Math.radians(75)) {
                                const crit = Math.random() < p.weapon.critChance;
                                const dmg = crit ? p.weapon.damage * 2 : p.weapon.damage;
                                const finalDmg = Math.round(dmg * p.damageMultiplier);
                                
                                e.hp -= finalDmg;
                                hitAny = true;
                                
                                // Áp dụng phản lực knockback đẩy lùi quái
                                e.kx += Math.cos(enemyAngle) * 5;
                                e.ky += Math.sin(enemyAngle) * 5;
                                
                                // Gửi packet thông báo chữ nổi sát thương
                                broadcastToRoom(room, {
                                    type: "combat_text", x: e.x, y: e.y - 15,
                                    text: crit ? `CRIT! -${finalDmg}` : `-${finalDmg}`,
                                    color: crit ? "#ff2828" : "#ffffff"
                                });

                                if (e.hp <= 0) handleEnemyDeath(room, e, p);
                            }
                        }
                    });
                } else {
                    // Bắn đạn tầm xa (phép thuật lửa nổ lan, hoặc cung tên làm chậm)
                    const pid = "proj_" + room.nextEntityId++;
                    room.entities.set(pid, {
                        id: pid, type: "projectile", isPlayer: true,
                        x: p.x, y: p.y, radius: p.weapon.splashRadius > 0 ? 8 : 5,
                        angle: p.angle, speed: p.weapon.speed, damage: Math.round(p.weapon.damage * p.damageMultiplier),
                        color: p.weapon.color, splashRadius: p.weapon.splashRadius, critChance: p.weapon.critChance
                    });
                }
            }
        });

        // Cập nhật Projectiles (Đạn bay)
        const projectiles = Array.from(room.entities.values()).filter(p => p.type === "projectile");
        projectiles.forEach(p => {
            p.x += Math.cos(p.angle) * p.speed;
            p.y += Math.sin(p.angle) * p.speed;
            
            // Va chạm tường
            if (isWallCollision(p.x, p.y, p.radius, room.mapData.grid)) {
                if (p.isPlayer && p.splashRadius > 0) {
                    triggerExplosion(room, p);
                }
                room.entities.delete(p.id);
                return;
            }

            // Kiểm tra va chạm thực thể
            if (p.isPlayer) {
                // Đạn người chơi bắn trúng quái
                for (let i = 0; i < enemies.length; i++) {
                    const e = enemies[i];
                    if (e.hp <= 0) continue;
                    const dx = e.x - p.x;
                    const dy = e.y - p.y;
                    const dist = Math.sqrt(dx*dx + dy*dy);
                    
                    if (dist <= (e.radius + p.radius)) {
                        room.entities.delete(p.id);
                        
                        if (p.splashRadius > 0) {
                            triggerExplosion(room, p);
                        } else {
                            const crit = Math.random() < p.critChance;
                            const dmg = crit ? p.damage * 2 : p.damage;
                            e.hp -= dmg;
                            e.kx += Math.cos(p.angle) * 7;
                            e.ky += Math.sin(p.angle) * 7;
                            
                            broadcastToRoom(room, {
                                type: "combat_text", x: e.x, y: e.y - 15,
                                text: crit ? `CRIT! -${dmg}` : `-${dmg}`,
                                color: crit ? "#ff2828" : "#ffffff"
                            });

                            if (p.color === "#50dc50" || p.color === "#3c96ff" || p.color === "#50b4ff") {
                                // Ice Slow (làm chậm tốc độ)
                                e.speed *= 0.5;
                            }
                            
                            if (e.hp <= 0) handleEnemyDeath(room, e, players[0]); // gán tạm người giết
                        }
                        break;
                    }
                }
            } else {
                // Đạn quái bắn trúng người chơi
                for (let i = 0; i < players.length; i++) {
                    const player = players[i];
                    if (player.hp <= 0 || player.isDashing) continue;
                    const dx = player.x - p.x;
                    const dy = player.y - p.y;
                    const dist = Math.sqrt(dx*dx + dy*dy);
                    
                    if (dist <= (player.radius + p.radius)) {
                        const finalDmg = Math.max(1, p.damage - player.defense);
                        player.hp = Math.max(0, player.hp - finalDmg);
                        player.x += Math.cos(p.angle) * 6;
                        player.y += Math.sin(p.angle) * 6;
                        
                        room.entities.delete(p.id);
                        
                        broadcastToRoom(room, {
                            type: "combat_text", x: player.x, y: player.y - 15,
                            text: `-${finalDmg}`, color: "#ff3250"
                        });
                        break;
                    }
                }
            }
        });

        // --- 4. INTERACTION & LOOT PICKUP SYSTEM ---
        const items = Array.from(room.entities.values()).filter(e => e.type === "item" || e.type === "chest");
        
        // Nhặt vật phẩm (Gold, Trái tim, Vũ khí)
        players.forEach(p => {
            if (p.hp <= 0) return;
            
            // Xử lý nút E tương tác mở rương
            if (p.input.interact) {
                items.forEach(item => {
                    if (item.type === "chest" && !item.opened) {
                        const dx = item.x - p.x;
                        const dy = item.y - p.y;
                        const dist = Math.sqrt(dx*dx + dy*dy);
                        if (dist < 46) {
                            item.opened = true;
                            // Tạo hiệu ứng nổ rương rơi ra 1-2 vũ khí và vàng
                            openChest(room, item, p);
                        }
                    }
                });
            }

            // Va chạm hút vật phẩm nhặt đồ
            items.forEach(item => {
                if (item.type === "item") {
                    const dx = item.x - p.x;
                    const dy = item.y - p.y;
                    const dist = Math.sqrt(dx*dx + dy*dy);
                    
                    if (dist <= (p.radius + item.radius)) {
                        // Nhặt thành công
                        room.entities.delete(item.id);
                        
                        if (item.itemType === "gold") {
                            p.gold += item.val;
                            sendPrivateText(p, `+${item.val} Vàng`, "#ffc828");
                        } else if (item.itemType === "heart") {
                            p.hp = Math.min(p.maxHp, p.hp + 25);
                            sendPrivateText(p, "+25 HP", "#ff3250");
                        } else if (item.itemType === "exp") {
                            p.xp += item.val;
                            sendPrivateText(p, `+${item.val} EXP`, "#32dc64");
                            
                            // Lên cấp (Level Up)
                            const expNeeded = Math.round(100 * Math.pow(p.level, 1.5));
                            if (p.xp >= expNeeded) {
                                p.level++;
                                p.xp -= expNeeded;
                                p.freeStatPoints += 5;
                                
                                recalculatePlayerAttributes(p);
                                p.hp = p.maxHp;
                                p.mp = p.maxMp;
                                sendPrivateText(p, `LÊN CẤP! Cấp ${p.level}`, "#f0c81e");
                            }
                        } else if (item.itemType === "portal_stone") {
                            p.portalStoneCount = (p.portalStoneCount || 0) + 1;
                            sendPrivateText(p, `+1 Đá Dịch Chuyển`, "#b464ff");
                        } else if (item.itemType === "weapon") {
                            // Phải cùng Class mới cho nhặt vũ khí
                            if (item.weapon.classLimit === p.classType) {
                                p.weapon = item.weapon;
                                sendPrivateText(p, `Nhặt được: ${item.weapon.name}!`, item.weapon.color);
                            }
                        }
                    }
                }
            });

                // 5. Kiểm tra xuống hầm sâu hơn (Cầu thang)
                if (!room.stairsLocked && room.level !== 0) {
                    const sx = room.mapData.stairsSpawn.x;
                    const sy = room.mapData.stairsSpawn.y;
                    const dx = p.x - sx;
                    const dy = p.y - sy;
                    const dist = Math.sqrt(dx*dx + dy*dy);
                    if (dist < 24) {
                        nextLevel(room);
                    }
                }
            });
        });
    }

// Hỗ trợ hàm chuyển đổi góc chéo
Math.radians = function(degrees) {
  return degrees * Math.PI / 180;
};

// Sát thương nổ lan của trượng lửa
function triggerExplosion(room, proj) {
    const enemies = Array.from(room.entities.values()).filter(e => e.type === "enemy" || e.type === "boss");
    enemies.forEach(e => {
        if (e.hp <= 0) return;
        const dx = e.x - proj.x;
        const dy = e.y - proj.y;
        const dist = Math.sqrt(dx*dx + dy*dy);
        
        if (dist <= proj.splashRadius) {
            e.hp -= proj.damage;
            if (dist > 0) {
                e.kx += (dx / dist) * 7.5;
                e.ky += (dy / dist) * 7.5;
            }
            broadcastToRoom(room, {
                type: "combat_text", x: e.x, y: e.y - 15,
                text: `-${proj.damage}`, color: "#ffa028"
            });
            if (e.hp <= 0) handleEnemyDeath(room, e, { damageMultiplier: 1.0 });
        }
    });
    // Gửi tín hiệu vụ nổ cho client vẽ hạt lửa
    broadcastToRoom(room, {
        type: "explosion_fx", x: proj.x, y: proj.y, radius: proj.splashRadius
    });
}

function handleEnemyDeath(room, enemy, killer) {
    room.entities.delete(enemy.id);
    
    // Nếu quái thường chết
    if (enemy.id !== "e_boss") {
        // 1. Rơi EXP (100% tỷ lệ)
        const expId = "i_" + room.nextEntityId++;
        room.entities.set(expId, {
            id: expId, type: "item", itemType: "exp", x: enemy.x, y: enemy.y, radius: 8, val: 15 + room.level * 3
        });
        
        // 2. Rơi Vàng (40%)
        if (Math.random() < 0.4) {
            const goldId = "i_" + room.nextEntityId++;
            room.entities.set(goldId, {
                id: goldId, type: "item", itemType: "gold", x: enemy.x + 10, y: enemy.y, radius: 8, val: Math.floor(Math.random() * 10) + 5
            });
        }
        // 3. Rơi Hạt hồi HP (15%)
        if (Math.random() < 0.15) {
            const hpId = "i_" + room.nextEntityId++;
            room.entities.set(hpId, {
                id: hpId, type: "item", itemType: "heart", x: enemy.x - 10, y: enemy.y, radius: 8
            });
        }
    } else {
        // Boss chết: Rơi EXP khủng, rương báu, hoặc vũ khí ngẫu nhiên siêu hiếm
        room.stairsLocked = false;
        
        const expId = "i_" + room.nextEntityId++;
        room.entities.set(expId, {
            id: expId, type: "item", itemType: "exp", x: enemy.x, y: enemy.y, radius: 8, val: 250
        });

        // Sinh vũ khí hiếm ngay vị trí Boss chết
        const rand = SeededRandom(room.seed + room.nextEntityId++);
        const classTarget = (killer && killer.classType) || "knight";
        const dropWeapon = generateRandomWeapon(classTarget, room.level + 1, rand); // +1 tầng nâng cấp phẩm
        
        const itemWpId = "i_" + room.nextEntityId++;
        room.entities.set(itemWpId, {
            id: itemWpId, type: "item", itemType: "weapon", x: enemy.x, y: enemy.y + 15, radius: 8, weapon: dropWeapon
        });
    }
}

function openChest(room, chest, player) {
    // Tạo 1 vũ khí ngẫu nhiên phù hợp với class của người chơi mở rương
    const rand = SeededRandom(room.seed + room.nextEntityId++);
    const weapon = generateRandomWeapon(player.classType, room.level, rand);
    
    const itemWpId = "i_" + room.nextEntityId++;
    room.entities.set(itemWpId, {
        id: itemWpId, type: "item", itemType: "weapon", x: chest.x, y: chest.y + 16, radius: 8, weapon: weapon
    });

    // Cộng thêm ít vàng văng ra xung quanh
    for (let i = 0; i < 2; i++) {
        const goldId = "i_" + room.nextEntityId++;
        room.entities.set(goldId, {
            id: goldId, type: "item", itemType: "gold", 
            x: chest.x + (i === 0 ? -12 : 12), y: chest.y - 10, radius: 8, val: Math.floor(Math.random() * 15) + 10
        });
    }

    // Rơi Đá Dịch Chuyển (25% cơ hội)
    if (Math.random() < 0.25) {
        const portalStoneId = "i_" + room.nextEntityId++;
        room.entities.set(portalStoneId, {
            id: portalStoneId, type: "item", itemType: "portal_stone", x: chest.x, y: chest.y - 16, radius: 8, val: 1
        });
    }
}

// Đổi tầng khi bước vào cầu thang
function nextLevel(room) {
    room.level++;
    room.stairsLocked = room.level % 5 === 0;
    room.seed = Math.floor(Math.random() * 999999);
    room.mapData = generateDungeon(room.seed, room.level);
    
    // Đặt vị trí tất cả player về điểm xuất phát
    const players = Array.from(room.entities.values()).filter(e => e.type === "player");
    players.forEach(p => {
        p.x = room.mapData.playerSpawn.x;
        p.y = room.mapData.playerSpawn.y;
    });

    // Reset quái vật & rương
    spawnDungeonEntities(room);
    
    // Gửi thông báo chuyển tầng tới tất cả client
    broadcastToRoom(room, {
        type: "next_floor",
        level: room.level,
        seed: room.seed,
        stairsLocked: room.stairsLocked,
        playerSpawn: room.mapData.playerSpawn
    });
}

function broadcastToRoom(room, data) {
    const json = JSON.stringify(data);
    room.entities.forEach(e => {
        if (e.type === "player" && e.socket.readyState === WebSocket.OPEN) {
            e.socket.send(json);
        }
    });
}

function sendPrivateText(player, text, color) {
    if (player.socket.readyState === WebSocket.OPEN) {
        player.socket.send(JSON.stringify({
            type: "combat_text", x: player.x, y: player.y - 30, text: text, color: color
        }));
    }
}

// 10 HZ NETWORK BROADCAST SYNCHRONIZATION (Mỗi 100ms)
function networkSync() {
    rooms.forEach(room => {
        if (room.playerCount <= 0) return;

        const snapshot = {
            type: "snapshot",
            level: room.level,
            stairsLocked: room.stairsLocked,
            players: Array.from(room.entities.values())
                .filter(e => e.type === "player")
                .map(p => ({
                    id: p.id, nickname: p.nickname, classType: p.classType,
                    x: p.x, y: p.y, hp: p.hp, maxHp: p.maxHp,
                    mp: Math.round(p.mp), maxMp: p.maxMp,
                    freeStatPoints: p.freeStatPoints, stats: p.stats,
                    weapon: p.weapon, gold: p.gold, xp: p.xp, level: p.level,
                    isDashing: p.isDashing, angle: p.angle,
                    portalStoneCount: p.portalStoneCount || 0,
                    storageItems: p.storageItems || []
                })),
            enemies: Array.from(room.entities.values())
                .filter(e => e.type === "enemy" || e.type === "boss")
                .map(e => ({
                    id: e.id, type: e.type, enemyType: e.enemyType || null,
                    x: e.x, y: e.y, hp: e.hp, maxHp: e.maxHp,
                    enraged: e.enraged || false
                })),
            projectiles: Array.from(room.entities.values())
                .filter(e => e.type === "projectile")
                .map(p => ({
                    id: p.id, x: p.x, y: p.y, angle: p.angle, color: p.color
                })),
            items: Array.from(room.entities.values())
                .filter(e => e.type === "item" || e.type === "chest")
                .map(i => ({
                    id: i.id, type: i.type, itemType: i.itemType || null,
                    x: i.x, y: i.y, weapon: i.weapon || null,
                    opened: i.opened || false, val: i.val || 0
                }))
        };

        broadcastToRoom(room, snapshot);
    });
}

// Đăng ký vòng lặp
setInterval(logicTick, 50); // 20 Hz
setInterval(networkSync, 100); // 10 Hz

server.listen(PORT, () => {
    console.log(`[SERVER] Dungeon RPG Authoritative Server đang chạy tại http://localhost:${PORT}`);
});
