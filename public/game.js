// public/game.js
const SCREEN_WIDTH = 1024;
const SCREEN_HEIGHT = 768;
const TILE_SIZE = 48;

// Canvas setup
const canvas = document.getElementById("gameCanvas");
const ctx = canvas.getContext("2d");

// Socket & Game States
let socket = null;
let myId = null;
let myPlayer = null;
let currentLevel = 1;
let stairsLocked = false;

// Seeded Map Caching
let dungeonCanvas = null;
let dungeonCanvasCtx = null;
let localMapData = null;

// LERP interpolation lists
let renderPlayers = new Map(); // key: id, value: {x, y, hp, maxHp, isDashing, angle, ...}
let renderEnemies = new Map();
let renderProjectiles = new Map();
let renderItems = new Map();

// Local Visual Effects (Client-side only)
let particles = [];
let floatingTexts = [];
let screenShake = { intensity: 0, decay: 0.88 };
let camera = { x: 0, y: 0 };
let floorBannerTimer = 0;

// Input listeners
const keys = { w: false, a: false, s: false, d: false, ArrowUp: false, ArrowDown: false, ArrowLeft: false, ArrowRight: false, space: false, e: false };
let mouseX = 0;
let mouseY = 0;
let mouseLeft = false;
let mouseRight = false;

// Setup lobby DOM
const lobbyScreen = document.getElementById("lobbyScreen");
const gameContainer = document.getElementById("gameContainer");
const btnConnect = document.getElementById("btnConnect");
const nicknameInput = document.getElementById("nickname");
const roomIdInput = document.getElementById("roomId");
const classCards = document.querySelectorAll(".class-card");
const gameOverScreen = document.getElementById("gameOverScreen");
const btnRespawn = document.getElementById("btnRespawn");
const statsPanel = document.getElementById("statsPanel");
const blacksmithPanel = document.getElementById("blacksmithPanel");
const storagePanel = document.getElementById("storagePanel");
const btnExitBlacksmith = document.getElementById("btnExitBlacksmith");
const btnExitStorage = document.getElementById("btnExitStorage");
const tabCraft = document.getElementById("tabCraft");
const tabUpgrade = document.getElementById("tabUpgrade");
const craftTabContent = document.getElementById("craftTabContent");
const upgradeTabContent = document.getElementById("upgradeTabContent");
const btnUpgradeWeapon = document.getElementById("btnUpgradeWeapon");
const storageGrid = document.getElementById("storageGrid");
const equipWeaponSlot = document.getElementById("equipWeaponSlot");

let selectedClass = "knight";
let blacksmithUIOpen = false;
let storageUIOpen = false;
let blacksmithActiveTab = 0; // 0 = Craft, 1 = Upgrade

// Class select card listeners
classCards.forEach(card => {
    card.addEventListener("click", () => {
        classCards.forEach(c => c.classList.remove("selected"));
        card.classList.add("selected");
        selectedClass = card.getAttribute("data-class");
    });
});

btnConnect.addEventListener("click", startConnection);
btnRespawn.addEventListener("click", () => {
    gameOverScreen.classList.add("hidden");
    socket.send(JSON.stringify({ type: "join", nickname: nicknameInput.value, classType: selectedClass, roomId: roomIdInput.value }));
});

// Bind click events to stat allocation buttons
const plusButtons = document.querySelectorAll(".btn-stat-plus");
plusButtons.forEach(btn => {
    btn.addEventListener("click", () => {
        const stat = btn.getAttribute("data-stat");
        if (socket && socket.readyState === WebSocket.OPEN && myPlayer && myPlayer.freeStatPoints > 0) {
            socket.send(JSON.stringify({
                type: "allocate_stat",
                stat: stat
            }));
        }
    });
});

// Close buttons
btnExitBlacksmith.addEventListener("click", () => {
    blacksmithPanel.classList.add("hidden");
    blacksmithUIOpen = false;
});
btnExitStorage.addEventListener("click", () => {
    storagePanel.classList.add("hidden");
    storageUIOpen = false;
});

// Blacksmith tabs switching
tabCraft.addEventListener("click", () => {
    blacksmithActiveTab = 0;
    tabCraft.classList.add("active");
    tabUpgrade.classList.remove("active");
    craftTabContent.classList.remove("hidden");
    upgradeTabContent.classList.add("hidden");
});
tabUpgrade.addEventListener("click", () => {
    blacksmithActiveTab = 1;
    tabUpgrade.classList.add("active");
    tabCraft.classList.remove("active");
    upgradeTabContent.classList.remove("hidden");
    craftTabContent.classList.add("hidden");
});

// Blacksmith Upgrade action
btnUpgradeWeapon.addEventListener("click", () => {
    if (socket && socket.readyState === WebSocket.OPEN) {
        socket.send(JSON.stringify({ type: "upgrade_weapon" }));
    }
});

function startConnection() {
    const nick = nicknameInput.value.trim() || "Explorer";
    const room = roomIdInput.value.trim() || "dungeon_1";
    
    lobbyScreen.classList.add("hidden");
    gameContainer.classList.remove("hidden");

    // Khởi tạo WebSocket kết nối Server
    const protocol = window.location.protocol === "https:" ? "wss://" : "ws://";
    socket = new WebSocket(protocol + window.location.host);

    socket.onopen = () => {
        socket.send(JSON.stringify({
            type: "join",
            nickname: nick,
            classType: selectedClass,
            roomId: room
        }));
    };

    socket.onmessage = (event) => {
        const data = JSON.parse(event.data);

        // 1. Nhận thông báo đã vào phòng
        if (data.type === "joined") {
            myId = data.playerId;
            currentLevel = data.level;
            stairsLocked = data.stairsLocked;
            buildLocalDungeon(data.seed, data.level);
            showFloorBanner(data.level);
        }

        // 2. Nhận thông báo chuyển tầng mới
        else if (data.type === "next_floor") {
            currentLevel = data.level;
            stairsLocked = data.stairsLocked;
            buildLocalDungeon(data.seed, data.level);
            showFloorBanner(data.level);
            
            // Xóa đạn và quái cũ trên client để tránh nhảy giật hình
            renderEnemies.clear();
            renderProjectiles.clear();
            renderItems.clear();
        }

        // 3. Nhận đồng bộ mạng Snapshot (10Hz)
        else if (data.type === "snapshot") {
            stairsLocked = data.stairsLocked;
            currentLevel = data.level;
            processSnapshot(data);
        }

        // 4. Nhận hiệu ứng nổ lan của trượng phép hỏa thuật
        else if (data.type === "explosion_fx") {
            spawnExplosionParticles(data.x, data.y, data.radius);
            triggerScreenShake(8);
        }

        // 5. Nhận hiệu ứng chữ nổi sát thương
        else if (data.type === "combat_text") {
            floatingTexts.push({
                x: data.x, y: data.y, text: data.text, color: data.color,
                timer: 45, maxTimer: 45
            });
        }
    };
}

// BỘ PHÁT SINH PHÒNG & THIẾT KẾ GẠCH ĐÁ PIXEL ĐỒNG BỘ
function buildLocalDungeon(seed, level) {
    localMapData = level === 0 ? DungeonGenerator.generateSanctuary() : DungeonGenerator.generateDungeon(seed, level);
    
    // Khởi tạo Canvas ẩn để cache bản đồ gạch đá pixel
    dungeonCanvas = document.createElement("canvas");
    dungeonCanvas.width = localMapData.width * TILE_SIZE;
    dungeonCanvas.height = localMapData.height * TILE_SIZE;
    dungeonCanvasCtx = dungeonCanvas.getContext("2d");
    
    const rand = DungeonGenerator.SeededRandom(seed);
    
    // Vẽ từng ô gạch đá lên canvas bộ nhớ đệm
    for (let y = 0; y < localMapData.height; y++) {
        for (let x = 0; x < localMapData.width; x++) {
            const tile = localMapData.grid[y][x];
            const rx = x * TILE_SIZE;
            const ry = y * TILE_SIZE;
            
            if (tile === 0) {
                // Vẽ tường đá pixel
                drawWallPixel(dungeonCanvasCtx, rx, ry, rand);
            } else if (tile === 3) {
                // Vẽ sàn gỗ Sanctuary pixel
                drawSafeFloorPixel(dungeonCanvasCtx, rx, ry, y, rand);
            } else {
                // Vẽ sàn gạch pixel
                drawFloorPixel(dungeonCanvasCtx, rx, ry, rand);
            }
        }
    }
}

// Vẽ sàn gỗ Sanctuary pixel
function drawSafeFloorPixel(ctx, x, y, gy, rand) {
    const baseColor = "#4b3527";
    const border = "#302016";
    const grain = "#5a4434";
    
    ctx.fillStyle = baseColor;
    ctx.fillRect(x, y, TILE_SIZE, TILE_SIZE);
    
    // Đường ván ngang
    ctx.fillStyle = border;
    ctx.fillRect(x, y, TILE_SIZE, 2);
    ctx.fillRect(x, y + TILE_SIZE - 2, TILE_SIZE, 2);
    
    // Mạch gỗ dọc so le
    if (gy % 2 === 0) {
        ctx.fillRect(x + TILE_SIZE / 2 - 1, y, 2, TILE_SIZE);
    } else {
        ctx.fillRect(x + TILE_SIZE / 4 - 1, y, 2, TILE_SIZE);
        ctx.fillRect(x + 3 * TILE_SIZE / 4 - 1, y, 2, TILE_SIZE);
    }
    
    // Vân gỗ mờ
    ctx.fillStyle = grain;
    ctx.fillRect(x + 4, y + 12, TILE_SIZE - 8, 1);
    ctx.fillRect(x + 6, y + 24, TILE_SIZE - 12, 1);
}

// Vẽ gạch tường đá pixel nứt nẻ
function drawWallPixel(ctx, x, y, rand) {
    const baseColor = "#2d2d34";
    const highlight = "#4b4b55";
    const shadow = "#1e1e23";
    const border = "#141419";
    
    ctx.fillStyle = baseColor;
    ctx.fillRect(x, y, TILE_SIZE, TILE_SIZE);
    
    // Vẽ các đường biên phân gạch
    ctx.fillStyle = border;
    ctx.fillRect(x, y, TILE_SIZE, 1);
    ctx.fillRect(x, y + 16, TILE_SIZE, 1);
    ctx.fillRect(x, y + 32, TILE_SIZE, 1);
    ctx.fillRect(x, y + TILE_SIZE - 1, TILE_SIZE, 1);
    
    // Khe dọc hàng gạch chéo
    ctx.fillRect(x + 24, y, 1, 16);
    ctx.fillRect(x + 12, y + 16, 1, 16);
    ctx.fillRect(x + 36, y + 16, 1, 16);
    ctx.fillRect(x + 24, y + 32, 1, 16);
    
    // Tạo hiệu ứng xiên 3D
    ctx.fillStyle = highlight;
    ctx.fillRect(x + 1, y + 1, 22, 1);
    ctx.fillRect(x + 1, y + 1, 1, 14);
    ctx.fillRect(x + 25, y + 1, 21, 1);
    ctx.fillRect(x + 25, y + 1, 1, 14);

    ctx.fillStyle = shadow;
    ctx.fillRect(x + 23, y + 2, 1, 14);
    ctx.fillRect(x + 2, y + 15, 22, 1);
    ctx.fillRect(x + TILE_SIZE - 2, y + 2, 1, 14);
    ctx.fillRect(x + 26, y + 15, 21, 1);
    
    // Nứt đá ngẫu nhiên
    for (let i = 0; i < 4; i++) {
        if (rand.random() < 0.25) {
            ctx.fillStyle = shadow;
            ctx.fillRect(x + rand.randint(4, TILE_SIZE - 4), y + rand.randint(4, TILE_SIZE - 4), 1, 1);
        }
    }
}

// Vẽ sàn đá pixel u tối
function drawFloorPixel(ctx, x, y, rand) {
    const baseColor = "#18181c";
    const border = "#121216";
    const speckle = "#202026";
    
    ctx.fillStyle = baseColor;
    ctx.fillRect(x, y, TILE_SIZE, TILE_SIZE);
    
    ctx.strokeStyle = border;
    ctx.lineWidth = 1;
    ctx.strokeRect(x, y, TILE_SIZE, TILE_SIZE);
    
    // Hạt nhiễu đá
    ctx.fillStyle = speckle;
    for (let i = 0; i < 6; i++) {
        ctx.fillRect(x + rand.randint(2, TILE_SIZE - 2), y + rand.randint(2, TILE_SIZE - 2), 1, 1);
    }
}

// HIỂN THỊ BANNER VƯỢT TẦNG
function showFloorBanner(level) {
    const banner = document.getElementById("floorBanner");
    const floorText = document.getElementById("floorText");
    const floorSubText = document.getElementById("floorSubText");
    
    let sub = "Kẻ Thách Thức Hầm Ngục";
    if (level === 0) {
        floorText.innerText = "SANCTUARY";
        sub = "VÙNG AN TOÀN DUY NHẤT";
    } else {
        floorText.innerText = `FLOOR ${level}`;
        if (level === 5) sub = "LÃNH ĐỊA BOSS RỒNG XƯƠNG";
        else if (level > 5 && level < 10) sub = "MỎ ĐÁ BỎ HOANG";
        else if (level === 10) sub = "HẦM BĂNG VĨNH CỬU";
    }
    
    floorSubText.innerText = sub;
    
    banner.classList.remove("hidden");
    
    // Tự động ẩn banner sau 4 giây
    setTimeout(() => {
        banner.classList.add("hidden");
    }, 3900);
}

// XỬ LÝ DỮ LIỆU SNAPSHOT ĐỒNG BỘ MẠNG (10Hz)
function processSnapshot(data) {
    // 1. Đồng bộ người chơi khác
    const newPlayerIds = new Set(data.players.map(p => p.id));
    
    data.players.forEach(p => {
        if (p.id === myId) {
            myPlayer = p;
            updateHUD(p);
            
            if (p.hp <= 0 && gameOverScreen.classList.contains("hidden")) {
                // Hiện màn hình báo chết
                document.getElementById("statFloor").innerText = currentLevel;
                document.getElementById("statGold").innerText = p.gold;
                gameOverScreen.classList.remove("hidden");
            }
        }
        
        let rp = renderPlayers.get(p.id);
        if (!rp) {
            rp = { x: p.x, y: p.y, hp: p.hp, maxHp: p.maxHp, gold: p.gold, angle: p.angle, nickname: p.nickname, classType: p.classType, level: p.level };
            renderPlayers.set(p.id, rp);
        } else {
            // Lưu mục tiêu đích để nội suy LERP
            rp.targetX = p.x;
            rp.targetY = p.y;
            rp.hp = p.hp;
            rp.maxHp = p.maxHp;
            rp.gold = p.gold;
            rp.angle = p.angle;
            rp.isDashing = p.isDashing;
            rp.weapon = p.weapon;
            rp.level = p.level;
        }
    });

    // Dọn dẹp người chơi thoát game
    renderPlayers.forEach((val, id) => {
        if (!newPlayerIds.has(id)) renderPlayers.delete(id);
    });

    // 2. Đồng bộ quái vật
    const newEnemyIds = new Set(data.enemies.map(e => e.id));
    let activeBoss = null;

    data.enemies.forEach(e => {
        if (e.type === "boss") activeBoss = e;

        let re = renderEnemies.get(e.id);
        if (!re) {
            re = { x: e.x, y: e.y, hp: e.hp, maxHp: e.maxHp, type: e.type, enemyType: e.enemyType, enraged: e.enraged };
            renderEnemies.set(e.id, re);
        } else {
            re.targetX = e.x;
            re.targetY = e.y;
            re.hp = e.hp;
            re.maxHp = e.maxHp;
            re.enraged = e.enraged;
        }
    });

    renderEnemies.forEach((val, id) => {
        if (!newEnemyIds.has(id)) renderEnemies.delete(id);
    });

    // Cập nhật Boss HP Bar hiển thị
    const bossHUD = document.getElementById("bossHUD");
    if (activeBoss) {
        bossHUD.classList.remove("hidden");
        document.getElementById("bossHpFill").style.width = (activeBoss.hp / activeBoss.maxHp * 100) + "%";
        document.getElementById("bossHpText").innerText = `${activeBoss.hp}/${activeBoss.maxHp}`;
        
        if (activeBoss.enraged) {
            document.getElementById("bossName").innerText = "!!! CHÚA TỂ XƯƠNG (CUỒNG NỘ) !!!";
            document.getElementById("bossName").style.color = "#ff3250";
        } else {
            document.getElementById("bossName").innerText = "!!! CHÚA TỂ XƯƠNG (BOSS) !!!";
            document.getElementById("bossName").style.color = "#b43cdc";
        }
    } else {
        bossHUD.classList.add("hidden");
    }

    // 3. Đồng bộ Projectiles
    const newProjIds = new Set(data.projectiles.map(p => p.id));
    data.projectiles.forEach(p => {
        let rp = renderProjectiles.get(p.id);
        if (!rp) {
            rp = { x: p.x, y: p.y, angle: p.angle, color: p.color };
            renderProjectiles.set(p.id, rp);
        } else {
            rp.targetX = p.x;
            rp.targetY = p.y;
            rp.angle = p.angle;
        }
    });
    renderProjectiles.forEach((val, id) => {
        if (!newProjIds.has(id)) renderProjectiles.delete(id);
    });

    // 4. Đồng bộ Rương và Vật phẩm rơi
    const newItemIds = new Set(data.items.map(i => i.id));
    data.items.forEach(i => {
        let ri = renderItems.get(i.id);
        if (!ri) {
            ri = { x: i.x, y: i.y, type: i.type, itemType: i.itemType, weapon: i.weapon, opened: i.opened, val: i.val };
            renderItems.set(i.id, ri);
        } else {
            ri.opened = i.opened;
        }
    });
    renderItems.forEach((val, id) => {
        if (!newItemIds.has(id)) renderItems.delete(id);
    });
    
    // Cập nhật Party HUD hiển thị HP đồng đội
    updatePartyHUD();
}

// CẬP NHẬT HUD PLAYER
function updateHUD(p) {
    document.getElementById("hudName").innerText = p.nickname;
    document.getElementById("hudLevel").innerText = "LV." + p.level;
    document.getElementById("goldCount").innerText = p.gold;
    
    // Cập nhật tầng hiện tại và đá dịch chuyển
    const floorHUD = document.getElementById("hudFloor");
    if (currentLevel === 0) {
        floorHUD.innerText = "SANCTUARY";
        floorHUD.className = "color-gold";
    } else {
        floorHUD.innerText = "TẦNG " + currentLevel;
        floorHUD.className = "";
    }
    
    const portalHUD = document.getElementById("hudPortalStones");
    portalHUD.innerText = `[T] Portal (${p.portalStoneCount || 0})`;
    if ((p.portalStoneCount || 0) > 0 && currentLevel !== 0) {
        portalHUD.classList.remove("disabled");
    } else {
        portalHUD.classList.add("disabled");
    }

    // Thanh HP
    const hpPct = (p.hp / p.maxHp) * 100;
    document.getElementById("hpFill").style.width = hpPct + "%";
    document.getElementById("hpText").innerText = `${Math.round(p.hp)}/${p.maxHp}`;
    
    // Thanh MP
    const mpPct = (p.mp / p.maxMp) * 100;
    document.getElementById("mpFill").style.width = mpPct + "%";
    document.getElementById("mpText").innerText = `${Math.round(p.mp)}/${p.maxMp}`;
    
    // Thanh EXP
    const expNeeded = Math.round(100 * Math.pow(p.level, 1.5));
    const xpPct = (p.xp / expNeeded) * 100;
    document.getElementById("xpFill").style.width = xpPct + "%";
    document.getElementById("xpText").innerText = `${p.xp}/${expNeeded} XP`;
    
    // Nhãn vũ khí
    const wpTag = document.getElementById("weaponName");
    wpTag.innerText = p.weapon.name;
    wpTag.style.color = p.weapon.color;
    wpTag.style.borderColor = p.weapon.color;

    // Cập nhật Bảng chỉ số (Stats Panel)
    if (p.stats) {
        document.getElementById("statPointsCount").innerText = p.freeStatPoints;
        document.getElementById("statVigorVal").innerText = p.stats.vigor;
        document.getElementById("statStrengthVal").innerText = p.stats.strength;
        document.getElementById("statDexterityVal").innerText = p.stats.dexterity;
        document.getElementById("statIntelligenceVal").innerText = p.stats.intelligence;
        document.getElementById("statVitalityVal").innerText = p.stats.vitality;

        // Bật/tắt nút cộng chỉ số tùy thuộc vào điểm còn lại
        const plusButtons = document.querySelectorAll(".btn-stat-plus");
        plusButtons.forEach(btn => {
            if (p.freeStatPoints > 0) {
                btn.removeAttribute("disabled");
                btn.classList.remove("disabled");
            } else {
                btn.setAttribute("disabled", "true");
                btn.classList.add("disabled");
            }
        });
    }

    // Cập nhật thông số bảng Thợ rèn
    if (blacksmithUIOpen) {
        document.getElementById("upgWeaponName").innerText = p.weapon.name;
        document.getElementById("upgWeaponName").style.color = p.weapon.color;
        document.getElementById("upgWeaponLevel").innerText = "+" + (p.weapon.upgradeLevel || 0);
        document.getElementById("upgWeaponDmg").innerText = p.weapon.damage;
        
        const cost = 100 * ((p.weapon.upgradeLevel || 0) + 1);
        document.getElementById("upgWeaponCost").innerText = cost;
        
        const upgBtn = document.getElementById("btnUpgradeWeapon");
        if (p.weapon.name === "Tay Không") {
            upgBtn.innerText = "KHÔNG CÓ VŨ KHÍ";
            upgBtn.setAttribute("disabled", "true");
            upgBtn.classList.add("disabled");
        } else if (p.gold < cost) {
            upgBtn.innerText = `THIẾU VÀNG (${cost} Vàng)`;
            upgBtn.setAttribute("disabled", "true");
            upgBtn.classList.add("disabled");
        } else {
            upgBtn.innerText = `NÂNG CẤP (+3 SÁT THƯƠNG)`;
            upgBtn.removeAttribute("disabled");
            upgBtn.classList.remove("disabled");
        }
    }

    // Cập nhật thông số bảng Kho chứa đồ
    if (storageUIOpen) {
        const sItems = p.storageItems || [];
        document.getElementById("storageCount").innerText = sItems.length;
        
        // Repopulate Storage Grid
        storageGrid.innerHTML = "";
        for (let i = 0; i < 20; i++) {
            const slot = document.createElement("div");
            slot.className = "item-slot";
            if (i < sItems.length) {
                const item = sItems[i];
                slot.style.borderColor = item.color;
                slot.innerHTML = `
                    <span class="slot-name" style="color: ${item.color}">${item.name}</span>
                    <span class="slot-rarity" style="color: ${item.color}">${item.rarity}</span>
                `;
                slot.addEventListener("click", () => {
                    if (socket && socket.readyState === WebSocket.OPEN) {
                        socket.send(JSON.stringify({ type: "withdraw_weapon", index: i }));
                    }
                });
            } else {
                slot.classList.add("empty");
                slot.innerHTML = `<span class="empty-text">(TRỐNG)</span>`;
            }
            storageGrid.appendChild(slot);
        }
        
        // Repopulate Equip weapon slot
        const weaponSlot = document.getElementById("equipWeaponSlot");
        if (p.weapon && p.weapon.name !== "Tay Không") {
            weaponSlot.classList.remove("empty");
            weaponSlot.style.borderColor = p.weapon.color;
            weaponSlot.innerHTML = `
                <span class="slot-name" style="color: ${p.weapon.color}">${p.weapon.name}</span>
                <span class="slot-rarity" style="color: ${p.weapon.color}">${p.weapon.rarity}</span>
            `;
            weaponSlot.onclick = () => {
                if (socket && socket.readyState === WebSocket.OPEN) {
                    socket.send(JSON.stringify({ type: "deposit_weapon" }));
                }
            };
            weaponSlot.style.cursor = "pointer";
        } else {
            weaponSlot.classList.add("empty");
            weaponSlot.style.borderColor = "#282830";
            weaponSlot.innerHTML = `<span class="empty-text">(TRỐNG)</span>`;
            weaponSlot.onclick = null;
            weaponSlot.style.cursor = "default";
        }
    }
}

// CẬP NHẬT TỔ ĐỘI (PARTY STATS)
function updatePartyHUD() {
    const listDiv = document.getElementById("partyList");
    listDiv.innerHTML = "";
    
    let teammateCount = 0;
    renderPlayers.forEach((p, id) => {
        if (id === myId) return; // bỏ qua bản thân
        teammateCount++;
        
        const memberDiv = document.createElement("div");
        memberDiv.className = "party-member";
        
        const hpPct = (p.hp / p.maxHp) * 100;
        
        memberDiv.innerHTML = `
            <div class="party-member-header">
                <span>${p.nickname} (LV.${p.level || 1})</span>
                <span>${p.hp}/${p.maxHp}</span>
            </div>
            <div class="party-hp-bg">
                <div class="party-hp-fill" style="width: ${hpPct}%;"></div>
            </div>
        `;
        listDiv.appendChild(memberDiv);
    });
    
    if (teammateCount === 0) {
        listDiv.innerHTML = `<p class="empty-msg">Không có đồng đội nào...</p>`;
    }
}

// HIỆU ỨNG RUNG MÀN HÌNH
function triggerScreenShake(force) {
    screenShake.intensity = Math.max(screenShake.intensity, force);
}

// HIỆU ỨNG HẠT LỬA PHÉP THUẬT LẦY LỘI
function spawnExplosionParticles(ex, ey, radius) {
    const count = 20;
    for (let i = 0; i < count; i++) {
        const angle = Math.random() * 2 * Math.PI;
        const dist = Math.random() * radius * 0.7;
        const px = ex + Math.cos(angle) * dist;
        const py = ey + Math.sin(angle) * dist;
        
        const speed = Math.random() * 4 + 1;
        const dx = Math.cos(angle) * speed;
        const dy = Math.sin(angle) * speed;
        
        particles.push({
            x: px, y: py,
            dx: dx, dy: dy,
            color: Math.random() < 0.6 ? "#ff5a14" : "#ffbe28",
            size: Math.random() * 4 + 2,
            life: Math.floor(Math.random() * 20) + 15
        });
    }
}

// Bắt phím bàn phím
window.addEventListener("keydown", (e) => {
    const key = e.key.toLowerCase();
    if (key === "w" || e.key === "ArrowUp") keys.w = true;
    if (key === "s" || e.key === "ArrowDown") keys.s = true;
    if (key === "a" || e.key === "ArrowLeft") keys.a = true;
    if (key === "d" || e.key === "ArrowRight") keys.d = true;
    if (e.key === " ") keys.space = true;
    if (key === "e") keys.e = true;
    if (e.key === "1") keys["1"] = true;
    if (e.key === "2") keys["2"] = true;
    if (key === "c") {
        statsPanel.classList.toggle("hidden");
    }
    if (e.key === "Escape") {
        statsPanel.classList.add("hidden");
        blacksmithPanel.classList.add("hidden");
        storagePanel.classList.add("hidden");
        blacksmithUIOpen = false;
        storageUIOpen = false;
    }
    if (key === "t" && currentLevel !== 0) {
        if (socket && socket.readyState === WebSocket.OPEN) {
            socket.send(JSON.stringify({ type: "use_portal_stone" }));
        }
    }
    if (blacksmithUIOpen) {
        if (e.key === "1") {
            tabCraft.click();
            e.preventDefault();
        }
        if (e.key === "2") {
            tabUpgrade.click();
            e.preventDefault();
        }
    }
    if (key === "l") {
        if (socket && socket.readyState === WebSocket.OPEN) {
            socket.send(JSON.stringify({ type: "cheat_exp", amount: 100 }));
        }
    }
});

window.addEventListener("keyup", (e) => {
    const key = e.key.toLowerCase();
    if (key === "w" || e.key === "ArrowUp") keys.w = false;
    if (key === "s" || e.key === "ArrowDown") keys.s = false;
    if (key === "a" || e.key === "ArrowLeft") keys.a = false;
    if (key === "d" || e.key === "ArrowRight") keys.d = false;
    if (e.key === " ") keys.space = false;
    if (key === "e") keys.e = false;
    if (e.key === "1") keys["1"] = false;
    if (e.key === "2") keys["2"] = false;
});

// Bắt tọa độ chuột
canvas.addEventListener("mousemove", (e) => {
    const rect = canvas.getBoundingClientRect();
    mouseX = e.clientX - rect.left;
    mouseY = e.clientY - rect.top;
});

canvas.addEventListener("mousedown", (e) => {
    if (e.button === 0) mouseLeft = true;
    if (e.button === 2) mouseRight = true;
});

canvas.addEventListener("mouseup", (e) => {
    if (e.button === 0) mouseLeft = false;
    if (e.button === 2) mouseRight = false;
});

// Chống mở chuột phải menu trình duyệt
canvas.addEventListener("contextmenu", e => e.preventDefault());

// CLIENT TICK RATE (Vòng gửi input - 60Hz)
function inputTick() {
    if (!socket || socket.readyState !== WebSocket.OPEN || !myPlayer) return;

    let dx = 0;
    let dy = 0;
    if (keys.w) dy = -1;
    if (keys.s) dy = 1;
    if (keys.a) dx = -1;
    if (keys.d) dx = 1;

    // Tính góc quay nhân vật tương đối trên màn hình
    const sx = Math.round(myPlayer.x - camera.x);
    const sy = Math.round(myPlayer.y - camera.y);
    const angle = Math.atan2(mouseY - sy, mouseX - sx);

    // Gửi packet input
    socket.send(JSON.stringify({
        type: "input",
        dx: dx,
        dy: dy,
        angle: angle,
        attack: mouseLeft,
        dash: keys.space,
        interact: keys.e
    }));
    
    // Uống thuốc hp/mp trực tiếp gửi packet (Server auto-trigger khi bấm 1/2)
    if (keys["1"]) {
        socket.send(JSON.stringify({ type: "input", dx, dy, angle, attack: false, dash: false, interact: false })); // trigger update
        keys["1"] = false;
    }
    if (keys["2"]) {
        socket.send(JSON.stringify({ type: "input", dx, dy, angle, attack: false, dash: false, interact: false }));
        keys["2"] = false;
    }
    
    // reset nút tương tác E
    if (keys.e) {
        if (currentLevel === 0 && localMapData) {
            const distB = myPlayer && localMapData.merchantSpawn ? Math.sqrt((myPlayer.x - localMapData.merchantSpawn.x)**2 + (myPlayer.y - localMapData.merchantSpawn.y)**2) : Infinity;
            const distS = myPlayer && localMapData.shopPedestals[0] ? Math.sqrt((myPlayer.x - localMapData.shopPedestals[0].x)**2 + (myPlayer.y - localMapData.shopPedestals[0].y)**2) : Infinity;
            const distP = myPlayer && localMapData.shopPedestals[1] ? Math.sqrt((myPlayer.x - localMapData.shopPedestals[1].x)**2 + (myPlayer.y - localMapData.shopPedestals[1].y)**2) : Infinity;
            
            if (distB < 60) {
                blacksmithUIOpen = !blacksmithUIOpen;
                blacksmithPanel.classList.toggle("hidden");
                storagePanel.classList.add("hidden");
                storageUIOpen = false;
                statsPanel.classList.add("hidden");
                keys.e = false;
            } else if (distS < 55) {
                storageUIOpen = !storageUIOpen;
                storagePanel.classList.toggle("hidden");
                blacksmithPanel.classList.add("hidden");
                blacksmithUIOpen = false;
                statsPanel.classList.add("hidden");
                keys.e = false;
            } else if (distP < 60) {
                socket.send(JSON.stringify({ type: "interact_portal" }));
                keys.e = false;
            }
        }
        if (keys.e) keys.e = false;
    }
}

// 60 FPS CLIENT RENDER LOOP
function renderLoop() {
    requestAnimationFrame(renderLoop);
    
    // Gửi input
    inputTick();

    // 1. CẬP NHẬT NỘI SUY LERP (10Hz -> 60Hz)
    renderPlayers.forEach(p => {
        if (p.targetX !== undefined) {
            p.x += (p.targetX - p.x) * 0.15;
            p.y += (p.targetY - p.y) * 0.15;
        }
    });

    renderEnemies.forEach(e => {
        if (e.targetX !== undefined) {
            e.x += (e.targetX - e.x) * 0.15;
            e.y += (e.targetY - e.y) * 0.15;
        }
    });

    renderProjectiles.forEach(p => {
        if (p.targetX !== undefined) {
            p.x += (p.targetX - p.x) * 0.25;
            p.y += (p.targetY - p.y) * 0.25;
        }
    });

    // Cập nhật hạt bụi vẽ cục bộ
    particles.forEach(p => {
        p.x += p.dx;
        p.y += p.dy;
        p.dx *= 0.94;
        p.dy *= 0.94;
        p.life--;
        p.size *= 0.96;
    });
    particles = particles.filter(p => p.life > 0 && p.size > 0.5);

    // Cập nhật chữ nổi bay
    floatingTexts.forEach(ft => {
        ft.y -= 0.8;
        ft.timer--;
    });
    floatingTexts = floatingTexts.filter(ft => ft.timer > 0);

    // Cập nhật camera trượt mượt
    if (myPlayer) {
        // Rung màn hình
        if (screenShake.intensity > 0.1) screenShake.intensity *= screenShake.decay;
        else screenShake.intensity = 0;
        
        const shakeX = (Math.random() - 0.5) * screenShake.intensity;
        const shakeY = (Math.random() - 0.5) * screenShake.intensity;

        const targetCamX = myPlayer.x - SCREEN_WIDTH / 2;
        const targetCamY = myPlayer.y - SCREEN_HEIGHT / 2;
        
        camera.x += (targetCamX - camera.x) * 0.1;
        camera.y += (targetCamY - camera.y) * 0.1;
        
        camera.shakeX = camera.x + shakeX;
        camera.shakeY = camera.y + shakeY;
    } else {
        camera.shakeX = 0;
        camera.shakeY = 0;
    }

    // 2. VẼ MÀN HÌNH CHÍNH
    ctx.fillStyle = "#0f0f14";
    ctx.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);

    if (dungeonCanvas) {
        // Vẽ bản đồ đá đã cache với offset camera
        ctx.drawImage(
            dungeonCanvas,
            camera.shakeX, camera.shakeY, SCREEN_WIDTH, SCREEN_HEIGHT,
            0, 0, SCREEN_WIDTH, SCREEN_HEIGHT
        );
        
        // Vẽ Cổng cầu thang xuống tầng dưới (nếu nằm trong camera)
        if (localMapData && localMapData.stairsSpawn) {
            const sx = localMapData.stairsSpawn.x - camera.shakeX;
            const sy = localMapData.stairsSpawn.y - camera.shakeY;
            if (sx > -TILE_SIZE && sx < SCREEN_WIDTH + TILE_SIZE && sy > -TILE_SIZE && sy < SCREEN_HEIGHT + TILE_SIZE) {
                // Đã vẽ nền cổng trên map, ở đây có thể vẽ thêm quầng hạt lấp lánh màu xanh lục nếu mở khóa
                if (!stairsLocked) {
                    ctx.fillStyle = "rgba(100, 255, 150, 0.08)";
                    ctx.beginPath();
                    ctx.arc(sx, sy, 20 + Math.sin(Date.now() * 0.005) * 4, 0, 2*Math.PI);
                    ctx.fill();
                }
            }
        }
    }

    // Vẽ các thực thể trong Sanctuary (Thợ Rèn, Kho Đồ, Portal)
    if (currentLevel === 0 && localMapData) {
        // 1. Thợ rèn
        if (localMapData.merchantSpawn) {
            const bx = localMapData.merchantSpawn.x - camera.shakeX;
            const by = localMapData.merchantSpawn.y - camera.shakeY;
            
            ctx.fillStyle = "rgba(10, 10, 15, 0.5)";
            ctx.beginPath();
            ctx.ellipse(bx, by + 18, 18, 5, 0, 0, 2*Math.PI);
            ctx.fill();
            
            ctx.fillStyle = "#3c2814"; // viền gỗ
            ctx.fillRect(bx - 18, by - 22, 36, 40);
            ctx.fillStyle = "#78501e"; // gỗ sáng
            ctx.fillRect(bx - 14, by - 18, 28, 28);
            
            ctx.fillStyle = "#f0c81e";
            ctx.font = "20px Outfit";
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillText("⚒", bx, by - 4);
            
            ctx.fillStyle = "#f0c81e";
            ctx.font = "bold 9px Outfit";
            ctx.fillText("THỢ RÈN", bx, by + 28);
            
            // Chỉ dẫn khoảng cách
            const distB = myPlayer ? Math.sqrt((myPlayer.x - localMapData.merchantSpawn.x)**2 + (myPlayer.y - localMapData.merchantSpawn.y)**2) : Infinity;
            if (distB < 60) {
                ctx.fillStyle = "#ffffff";
                ctx.font = "10px Outfit";
                ctx.fillText("[E] Thực Hiện", bx, by - 32);
            }
        }
        
        // 2. Kho đồ
        if (localMapData.shopPedestals.length > 0) {
            const sx = localMapData.shopPedestals[0].x - camera.shakeX;
            const sy = localMapData.shopPedestals[0].y - camera.shakeY;
            
            ctx.fillStyle = "rgba(10, 10, 15, 0.5)";
            ctx.beginPath();
            ctx.ellipse(sx, sy + 14, 16, 5, 0, 0, 2*Math.PI);
            ctx.fill();
            
            ctx.fillStyle = "#1e1e23"; // viền sắt
            ctx.fillRect(sx - 16, sy - 12, 32, 26);
            ctx.fillStyle = "#28643c"; // màu xanh lục cho hòm đồ Sanctuary
            ctx.fillRect(sx - 14, sy - 10, 28, 22);
            
            ctx.fillStyle = "#50dc78";
            ctx.font = "14px Outfit";
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillText("■", sx, sy + 1);
            
            ctx.fillStyle = "#50dc78";
            ctx.font = "bold 9px Outfit";
            ctx.fillText("KHO ĐỒ", sx, sy + 22);
            
            const distS = myPlayer ? Math.sqrt((myPlayer.x - localMapData.shopPedestals[0].x)**2 + (myPlayer.y - localMapData.shopPedestals[0].y)**2) : Infinity;
            if (distS < 55) {
                ctx.fillStyle = "#ffffff";
                ctx.font = "10px Outfit";
                ctx.fillText("[E] Mở Kho", sx, sy - 22);
            }
        }
        
        // 3. Portal
        if (localMapData.shopPedestals.length > 1) {
            const px = localMapData.shopPedestals[1].x - camera.shakeX;
            const py = localMapData.shopPedestals[1].y - camera.shakeY;
            
            const pulse = Math.abs(Math.sin(Date.now() * 0.003)) * 0.5 + 0.5;
            const portalColor = `rgba(180, 100, 255, ${pulse})`;
            
            ctx.fillStyle = "rgba(20, 10, 40, 0.4)";
            ctx.beginPath();
            ctx.arc(px, py, 22, 0, 2*Math.PI);
            ctx.fill();
            
            ctx.strokeStyle = portalColor;
            ctx.lineWidth = 2;
            ctx.beginPath();
            ctx.arc(px, py, 22, 0, 2*Math.PI);
            ctx.stroke();
            ctx.beginPath();
            ctx.arc(px, py, 16, 0, 2*Math.PI);
            ctx.stroke();
            
            ctx.fillStyle = portalColor;
            ctx.font = "18px Outfit";
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillText("★", px, py);
            
            const distP = myPlayer ? Math.sqrt((myPlayer.x - localMapData.shopPedestals[1].x)**2 + (myPlayer.y - localMapData.shopPedestals[1].y)**2) : Infinity;
            if (distP < 60) {
                ctx.fillStyle = "#ffffff";
                ctx.font = "10px Outfit";
                ctx.fillText("[E] Hầm Ngục", px, py - 30);
            }
        }
    }

    // Vẽ Rương báu (Chests)
    renderItems.forEach(item => {
        const rx = item.x - camera.shakeX;
        const ry = item.y - camera.shakeY;
        if (rx < -32 || rx > SCREEN_WIDTH + 32 || ry < -32 || ry > SCREEN_HEIGHT + 32) return;

        // Vẽ bóng đổ rương
        ctx.fillStyle = "rgba(10, 10, 15, 0.5)";
        ctx.beginPath();
        ctx.ellipse(rx, ry + 12, 14, 4, 0, 0, 2*Math.PI);
        ctx.fill();

        if (item.type === "chest") {
            // Rương gỗ viền sắt nẹp
            ctx.fillStyle = "#1e1e23"; // outline
            ctx.fillRect(rx - 16, ry - 16, 32, 26);
            ctx.fillStyle = "#78461e"; // gỗ
            ctx.fillRect(rx - 14, ry - 14, 28, 22);
            ctx.fillStyle = "#41414b"; // viền sắt
            ctx.fillRect(rx - 14, ry - 14, 4, 22);
            ctx.fillRect(rx + 10, ry - 14, 4, 22);
            ctx.fillRect(rx - 14, ry - 6, 28, 3); // nắp dọc

            if (!item.opened) {
                // Khóa vàng
                ctx.fillStyle = "#f0be1e";
                ctx.fillRect(rx - 2, ry - 4, 4, 6);
            } else {
                // Rương rỗng mở
                ctx.fillStyle = "#1e1e23";
                ctx.font = "bold 9px Consolas";
                ctx.fillStyle = "#8a8a98";
                ctx.fillText("OPENED", rx - 16, ry - 18);
            }
        }
    });

    // Vẽ Vật phẩm trôi nổi (Gold, HP Heart, Weapon)
    renderItems.forEach(item => {
        if (item.type === "chest") return;
        const rx = item.x - camera.shakeX;
        const ry = item.y - camera.shakeY;
        if (rx < -32 || rx > SCREEN_WIDTH + 32 || ry < -32 || ry > SCREEN_HEIGHT + 32) return;

        const bob = Math.sin(Date.now() * 0.004 + item.x) * 4;
        const itemY = ry + bob;

        ctx.fillStyle = "rgba(10, 10, 15, 0.4)";
        ctx.beginPath();
        ctx.ellipse(rx, ry + 10, 6, 2, 0, 0, 2*Math.PI);
        ctx.fill();

        if (item.itemType === "gold") {
            ctx.fillStyle = "#f0be1e";
            ctx.strokeStyle = "#1e1a05";
            ctx.lineWidth = 1;
            ctx.beginPath();
            ctx.arc(rx, itemY, 5, 0, 2*Math.PI);
            ctx.fill();
            ctx.stroke();
        } else if (item.itemType === "heart") {
            // Vẽ hạt HP hình tim đỏ
            ctx.fillStyle = "#ff2846";
            ctx.beginPath();
            ctx.arc(rx - 3, itemY - 2, 3, 0, 2*Math.PI);
            ctx.arc(rx + 3, itemY - 2, 3, 0, 2*Math.PI);
            ctx.lineTo(rx, itemY + 5);
            ctx.fill();
        } else if (item.itemType === "exp") {
            // Vẽ hạt EXP hình thoi xanh lá
            ctx.fillStyle = "#50dc50";
            ctx.beginPath();
            ctx.moveTo(rx, itemY - 6);
            ctx.lineTo(rx + 5, itemY);
            ctx.lineTo(rx, itemY + 6);
            ctx.lineTo(rx - 5, itemY);
            ctx.fill();
        } else if (item.itemType === "weapon" && item.weapon) {
            // Vẽ vũ khí trôi nổi có tag tên phát sáng
            ctx.strokeStyle = item.weapon.color;
            ctx.lineWidth = 2;
            ctx.beginPath();
            ctx.arc(rx, itemY, 11, 0, 2*Math.PI);
            ctx.stroke();
            
            ctx.fillStyle = "#ffffff";
            ctx.beginPath();
            ctx.arc(rx, itemY, 3, 0, 2*Math.PI);
            ctx.fill();

            // Tag tên vũ khí
            ctx.font = "bold 10px Consolas";
            const textWidth = ctx.measureText(item.weapon.name).width;
            
            // Vẽ nền đen sau chữ tag
            ctx.fillStyle = "rgba(10, 10, 15, 0.8)";
            ctx.fillRect(rx - textWidth/2 - 4, itemY - 24, textWidth + 8, 12);
            
            ctx.fillStyle = item.weapon.color;
            ctx.fillText(item.weapon.name, rx - textWidth/2, itemY - 15);
        } else if (item.itemType === "portal_stone") {
            // Vẽ Đá dịch chuyển trôi nổi (màu tím sáng nhấp nháy, hình ngôi sao bên trong)
            const pulse = Math.abs(Math.sin(Date.now() * 0.005)) * 0.4 + 0.6;
            ctx.fillStyle = `rgba(180, 100, 255, ${pulse})`;
            ctx.strokeStyle = "#4b1e78";
            ctx.lineWidth = 1.5;
            ctx.beginPath();
            ctx.arc(rx, itemY, 7, 0, 2*Math.PI);
            ctx.fill();
            ctx.stroke();

            ctx.fillStyle = "#ffffff";
            ctx.font = "bold 9px Outfit";
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillText("★", rx, itemY);

            // Nhãn tên "Đá Dịch Chuyển"
            ctx.font = "bold 9px Consolas";
            const textWidth = ctx.measureText("Đá Dịch Chuyển").width;
            ctx.fillStyle = "rgba(10, 10, 15, 0.8)";
            ctx.fillRect(rx - textWidth/2 - 4, itemY - 22, textWidth + 8, 12);
            ctx.fillStyle = "#b464ff";
            ctx.textAlign = "left";
            ctx.textBaseline = "alphabetic";
            ctx.fillText("Đá Dịch Chuyển", rx - textWidth/2, itemY - 13);
        }
    });

    // Vẽ Quái vật
    renderEnemies.forEach(e => {
        const rx = e.x - camera.shakeX;
        const ry = e.y - camera.shakeY;
        if (rx < -48 || rx > SCREEN_WIDTH + 48 || ry < -48 || ry > SCREEN_HEIGHT + 48) return;

        // Vẽ bóng chân
        ctx.fillStyle = "rgba(10, 10, 15, 0.5)";
        ctx.beginPath();
        ctx.arc(rx, ry + 12, e.radius, 0, 2*Math.PI);
        ctx.fill();

        if (e.type === "enemy") {
            // Vẽ Skeleton hoặc Mage
            if (e.enemyType === "melee") {
                // Skeleton: Đầu lâu xám trắng mắt đỏ
                ctx.fillStyle = "#d2d2d7"; // xương
                ctx.beginPath();
                ctx.arc(rx, ry, 11, 0, 2*Math.PI);
                ctx.fill();
                
                // Mắt đỏ
                ctx.fillStyle = "#ff0000";
                ctx.beginPath();
                ctx.arc(rx - 3, ry - 1, 1.5, 0, 2*Math.PI);
                ctx.arc(rx + 3, ry - 1, 1.5, 0, 2*Math.PI);
                ctx.fill();
            } else {
                // Mage: Áo choàng tím huyền bí
                ctx.fillStyle = "#642d9b";
                ctx.beginPath();
                ctx.moveTo(rx - 12, ry + 14);
                ctx.lineTo(rx + 12, ry + 14);
                ctx.lineTo(rx, ry - 12);
                ctx.fill();
                
                // Mắt vàng sáng
                ctx.fillStyle = "#ffff00";
                ctx.beginPath();
                ctx.arc(rx - 2, ry, 1.5, 0, 2*Math.PI);
                ctx.arc(rx + 2, ry, 1.5, 0, 2*Math.PI);
                ctx.fill();
            }
        } else if (e.type === "boss") {
            // Vẽ Boss Khổng Lồ
            ctx.fillStyle = e.enraged ? "#e150ff" : "#5a5564"; // xương xám đen
            ctx.beginPath();
            ctx.arc(rx, ry, e.radius, 0, 2*Math.PI);
            ctx.fill();
            
            // Viền ngoài của Boss sừng
            ctx.strokeStyle = "#2d2837";
            ctx.lineWidth = 3;
            ctx.stroke();

            // Mắt tím phát sáng của Boss Dragon
            ctx.fillStyle = "#c832ff";
            ctx.beginPath();
            ctx.arc(rx - 8, ry - 4, 4, 0, 2*Math.PI);
            ctx.arc(rx + 8, ry - 4, 4, 0, 2*Math.PI);
            ctx.fill();
            ctx.fillStyle = "#ffffff";
            ctx.beginPath();
            ctx.arc(rx - 8, ry - 4, 1.5, 0, 2*Math.PI);
            ctx.arc(rx + 8, ry - 4, 1.5, 0, 2*Math.PI);
            ctx.fill();
        }

        // Vẽ HP Bar nhỏ của quái trên đầu
        if (e.hp < e.maxHp && e.type !== "boss") {
            const barW = 24;
            const barH = 3;
            const bx = rx - barW/2;
            const by = ry - e.radius - 6;
            
            ctx.fillStyle = "#3c1414";
            ctx.fillRect(bx, by, barW, barH);
            ctx.fillStyle = "#ff2828";
            ctx.fillRect(bx, by, barW * (e.hp / e.maxHp), barH);
        }
    });

    // Vẽ Projectiles (Đạn phép bay phát sáng)
    renderProjectiles.forEach(proj => {
        const rx = proj.x - camera.shakeX;
        const ry = proj.y - camera.shakeY;
        if (rx < -20 || rx > SCREEN_WIDTH + 20 || ry < -20 || ry > SCREEN_HEIGHT + 20) return;

        // Quầng phát sáng
        ctx.fillStyle = proj.color + "50"; // add alpha
        ctx.beginPath();
        ctx.arc(rx, ry, 9, 0, 2*Math.PI);
        ctx.fill();
        
        ctx.fillStyle = proj.color;
        ctx.beginPath();
        ctx.arc(rx, ry, 5, 0, 2*Math.PI);
        ctx.fill();
        
        // Nhân trắng
        ctx.fillStyle = "#ffffff";
        ctx.beginPath();
        ctx.arc(rx, ry, 2, 0, 2*Math.PI);
        ctx.fill();
    });

    // Vẽ Players (Đồng đội và Bản thân)
    renderPlayers.forEach(p => {
        const rx = p.x - camera.shakeX;
        const ry = p.y - camera.shakeY;
        if (rx < -48 || rx > SCREEN_WIDTH + 48 || ry < -48 || ry > SCREEN_HEIGHT + 48) return;

        // Vẽ vệt bóng mờ (Afterimages) nếu đang lướt
        if (p.isDashing) {
            ctx.fillStyle = "rgba(0, 180, 255, 0.15)";
            ctx.beginPath();
            ctx.arc(rx - Math.cos(p.angle)*15, ry - Math.sin(p.angle)*15, 12, 0, 2*Math.PI);
            ctx.fill();
        }

        // Vẽ bóng đổ chân
        ctx.fillStyle = "rgba(10, 10, 15, 0.5)";
        ctx.beginPath();
        ctx.ellipse(rx, ry + 12, 12, 3, 0, 0, 2*Math.PI);
        ctx.fill();

        // 1. Thân người chơi (Neon orbs)
        ctx.fillStyle = "#0f192d"; // outline viền đen
        ctx.beginPath();
        ctx.arc(rx, ry, 14, 0, 2*Math.PI);
        ctx.fill();
        
        let bodyColor = "#3c96ff"; // Knight blue
        if (p.classType === "assassin") bodyColor = "#ff7800"; // Assassin orange
        else if (p.classType === "archer") bodyColor = "#50dc50"; // Archer green
        else if (p.classType === "mage") bodyColor = "#b43cdc"; // Mage purple
        else if (p.classType === "support") bodyColor = "#00ffd8"; // Support teal
        else if (p.classType === "summoner") bodyColor = "#aaff00"; // Summoner lime green
        
        ctx.fillStyle = bodyColor;
        ctx.beginPath();
        ctx.arc(rx, ry, 12, 0, 2*Math.PI);
        ctx.fill();
        
        // Lõi phát sáng
        ctx.fillStyle = "#f0ffff";
        ctx.beginPath();
        ctx.arc(rx, ry, 4, 0, 2*Math.PI);
        ctx.fill();

        // Vẽ đốm hướng xoay nhân vật (Đèn chỉ hướng)
        const indX = rx + Math.cos(p.angle) * 10;
        const indY = ry + Math.sin(p.angle) * 10;
        ctx.fillStyle = "#00f0ff";
        ctx.beginPath();
        ctx.arc(indX, indY, 3, 0, 2*Math.PI);
        ctx.fill();

        // Tên biệt danh trên đầu
        ctx.font = "bold 10px Consolas";
        ctx.fillStyle = p.id === myId ? "#f0c81e" : "#ffffff";
        ctx.textAlign = "center";
        ctx.fillText(p.nickname, rx, ry - 20);
    });

    // Vẽ hạt bụi phép thuật
    particles.forEach(p => {
        const rx = p.x - camera.shakeX;
        const ry = p.y - camera.shakeY;
        ctx.fillStyle = p.color;
        ctx.beginPath();
        ctx.arc(rx, ry, p.size, 0, 2*Math.PI);
        ctx.fill();
    });

    // Vẽ chữ nổi bay (Combat Damage)
    floatingTexts.forEach(ft => {
        const rx = ft.x - camera.shakeX;
        const ry = ft.y - camera.shakeY;
        ctx.font = "bold 12px Consolas";
        ctx.textAlign = "center";
        
        // Vẽ viền đen cho chữ nổi
        ctx.fillStyle = "#0a0a0f";
        ctx.fillText(ft.text, rx + 1, ry + 1);
        ctx.fillStyle = ft.color;
        ctx.fillText(ft.text, rx, ry);
    });

    // 3. VẼ HỆ THỐNG ÁNH SÁNG & BÓNG TỐI (Darkness Light Mask)
    if (myPlayer) {
        // Tạo bóng tối
        const fogCanvas = document.createElement("canvas");
        fogCanvas.width = SCREEN_WIDTH;
        fogCanvas.height = SCREEN_HEIGHT;
        const fogCtx = fogCanvas.getContext("2d");
        
        fogCtx.fillStyle = "rgba(10, 10, 15, 0.96)"; // Bóng tối hầm ngục
        fogCtx.fillRect(0, 0, SCREEN_WIDTH, SCREEN_HEIGHT);
        
        // Đục lỗ sáng bằng Destination-Out
        fogCtx.globalCompositeOperation = "destination-out";
        
        // Quầng sáng quanh player
        const psx = myPlayer.x - camera.shakeX;
        const psy = myPlayer.y - camera.shakeY;
        drawLightHole(fogCtx, psx, psy, 190);

        // Quầng sáng quanh các đèn lồng tĩnh
        if (localMapData && localMapData.lanterns) {
            localMapData.lanterns.forEach(lantern => {
                const lsx = lantern.x - camera.shakeX;
                const lsy = lantern.y - camera.shakeY;
                if (lsx > -120 && lsx < SCREEN_WIDTH + 120 && lsy > -120 && lsy < SCREEN_HEIGHT + 120) {
                    drawLightHole(fogCtx, lsx, lsy, 110);
                }
            });
        }

        // Vẽ đè fog lên màn hình chính
        ctx.drawImage(fogCanvas, 0, 0);
    }
    
    // Quét tương tác hiện chữ nhắc E
    let nearChest = false;
    if (myPlayer) {
        renderItems.forEach(item => {
            if (item.type === "chest" && !item.opened) {
                const dx = item.x - myPlayer.x;
                const dy = item.y - myPlayer.y;
                if (Math.sqrt(dx*dx + dy*dy) < 46) nearChest = true;
            }
        });
    }
    const interactPrompt = document.getElementById("interactPrompt");
    if (nearChest) interactPrompt.classList.remove("hidden");
    else interactPrompt.classList.add("hidden");
}

function drawLightHole(ctx, x, y, radius) {
    const gradient = ctx.createRadialGradient(x, y, 0, x, y, radius);
    gradient.addColorStop(0, "rgba(0,0,0,1.0)");
    gradient.addColorStop(0.3, "rgba(0,0,0,0.85)");
    gradient.addColorStop(0.7, "rgba(0,0,0,0.3)");
    gradient.addColorStop(1.0, "rgba(0,0,0,0.0)");
    
    ctx.fillStyle = gradient;
    ctx.beginPath();
    ctx.arc(x, y, radius, 0, 2*Math.PI);
    ctx.fill();
}

// Gọi bắt đầu vòng lặp đồ họa
requestAnimationFrame(renderLoop);
