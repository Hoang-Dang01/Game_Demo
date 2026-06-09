// shared/weapon_system.js
(function(exports) {

    const RarityColors = {
        "Common": "#f0f0f5",
        "Uncommon": "#50dc50",
        "Rare": "#3c96ff",
        "Epic": "#b43cdc",
        "Legendary": "#ff8228",
        "Mythic": "#ff2828",
        "Unique": "#f0c81e"
    };

    const Prefixes = {
        "Common": ["Rỉ Sét", "Cũ Kỹ", "Bình Thường", "Sứt Mẻ"],
        "Uncommon": ["Sắc Bén", "Gia Cố", "Chiến Sĩ", "Sắt Luyện"],
        "Rare": ["Băng Giá", "U Tối", "Tinh Nhuệ", "Hào Quang"],
        "Epic": ["Hỏa Ngục", "Hư Không", "Cuồng Bạo", "Cổ Đại"],
        "Legendary": ["Rồng Thiêng", "Thần Thánh", "Vô Song", "Huyền Thoại"],
        "Mythic": ["Diệt Vong", "Huyết Nguyệt", "Ác Quỷ", "Tử Thần"],
        "Unique": ["Độc Nhất", "Chí Tôn", "Khải Huyền", "Vương Giả"]
    };

    const Suffixes = ["Phục Hận", "Chí Mạng", "Cuồng Phong", "Bão Tố", "Hủy Diệt", "Nhà Vua", "Kẻ Thách Thức"];

    // Base weapon templates for classes
    const BaseWeapons = {
        "knight": {
            type: "melee",
            name: "Kiếm Sắt",
            baseDamage: 20,
            baseCooldown: 20, // frames (at 20 FPS, 1.0s)
            range: 65,
            manaCost: 0
        },
        "assassin": {
            type: "melee",
            name: "Dao Găm Rỉ",
            baseDamage: 14,
            baseCooldown: 12, // 0.6s
            range: 55,
            manaCost: 0
        },
        "mage": {
            type: "ranged_magic",
            name: "Trượng Gỗ",
            baseDamage: 16,
            baseCooldown: 25, // 1.25s
            range: 350,
            speed: 6.5,
            splashRadius: 40,
            manaCost: 0
        },
        "archer": {
            type: "ranged_arrow",
            name: "Cung Gỗ",
            baseDamage: 12,
            baseCooldown: 15, // 0.75s
            range: 400,
            speed: 10,
            splashRadius: 0,
            manaCost: 0
        },
        "support": {
            type: "ranged_magic",
            name: "Trượng Cũ",
            baseDamage: 10,
            baseCooldown: 20, // 1.0s
            range: 300,
            speed: 6.0,
            splashRadius: 0,
            manaCost: 0
        },
        "summoner": {
            type: "ranged_magic",
            name: "Ngọc Triệu Hồi",
            baseDamage: 11,
            baseCooldown: 22, // 1.1s
            range: 320,
            speed: 5.5,
            splashRadius: 0,
            manaCost: 0
        }
    };

    function generateRandomWeapon(classType, floor, rand) {
        const base = BaseWeapons[classType] || BaseWeapons["knight"];
        
        // 1. Xác định Độ hiếm (Rarity)
        const roll = rand.random();
        let rarity = "Common";
        if (roll < 0.001) rarity = "Unique";
        else if (roll < 0.005) rarity = "Mythic";
        else if (roll < 0.03) rarity = "Legendary";
        else if (roll < 0.10) rarity = "Epic";
        else if (roll < 0.25) rarity = "Rare";
        else if (roll < 0.50) rarity = "Uncommon";
        
        // 2. Nhân sát thương cơ bản theo Tầng Hầm (Floor)
        const floorMultiplier = 1 + (floor - 1) * 0.18; // +18% sát thương mỗi tầng
        let damage = Math.round(base.baseDamage * floorMultiplier);
        let cooldown = base.baseCooldown;
        let speed = base.speed || 0;
        let splashRadius = base.splashRadius || 0;
        let range = base.range;

        // 3. Nhân chỉ số theo Độ hiếm
        let rarityMultiplier = 1.0;
        if (rarity === "Uncommon") rarityMultiplier = 1.15;
        else if (rarity === "Rare") rarityMultiplier = 1.35;
        else if (rarity === "Epic") rarityMultiplier = 1.6;
        else if (rarity === "Legendary") rarityMultiplier = 2.0;
        else if (rarity === "Mythic") rarityMultiplier = 2.5;
        else if (rarity === "Unique") rarityMultiplier = 3.2;

        damage = Math.round(damage * rarityMultiplier);

        // 4. Sinh ngẫu nhiên Sub-stats (Tốc độ đánh, Chí mạng)
        let critChance = 0.05; // mặc định 5%
        let attackSpeedBonus = 0; // % tăng tốc độ đánh
        
        if (rarity !== "Common") {
            critChance += rand.randint(5, 25) / 100; // +5% -> +25%
            attackSpeedBonus += rand.randint(10, 50); // +10% -> +50%
            cooldown = Math.max(6, Math.round(cooldown / (1 + attackSpeedBonus / 100)));
            
            if (base.type === "ranged_magic" && splashRadius > 0) {
                splashRadius += rand.randint(10, 40);
            }
            if (base.type === "ranged_arrow") {
                speed += rand.randint(1, 4);
            }
        }

        // 5. Sinh tên vũ khí ngẫu nhiên
        const prefix = rand.choice(Prefixes[rarity]);
        const suffix = rand.choice(Suffixes);
        
        let weaponName = "";
        if (classType === "knight") {
            weaponName = `Đoản Kiếm ${prefix} của ${suffix}`;
        } else if (classType === "assassin") {
            weaponName = `Dao Găm ${prefix} của ${suffix}`;
        } else if (classType === "mage") {
            weaponName = `Gậy Phép ${prefix} của ${suffix}`;
        } else if (classType === "archer") {
            weaponName = `Trường Cung ${prefix} của ${suffix}`;
        } else if (classType === "support") {
            weaponName = `Vương Trượng ${prefix} của ${suffix}`;
        } else if (classType === "summoner") {
            weaponName = `Trượng Hồn ${prefix} của ${suffix}`;
        }

        return {
            name: weaponName,
            classLimit: classType,
            type: base.type,
            rarity: rarity,
            color: RarityColors[rarity],
            damage: damage,
            cooldown: cooldown,
            range: range,
            speed: speed,
            splashRadius: splashRadius,
            critChance: critChance,
            attackSpeedBonus: attackSpeedBonus
        };
    }

    exports.RarityColors = RarityColors;
    exports.generateRandomWeapon = generateRandomWeapon;

})(typeof exports === 'undefined' ? this.WeaponSystem = {} : exports);
