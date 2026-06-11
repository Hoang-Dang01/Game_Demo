# BÁO CÁO CHI TIẾT PHASE 1 - DATA ARCHITECTURE (KHUNG DỮ LIỆU)

> Trạng thái Phase: **Hoàn thành ~70%**  
> Dự án: **Core of Ruin (Web Co-op Edition)**  
> Người báo cáo: **Cố vấn Phát triển AI**  

---

## 🎯 1. MỤC TIÊU CỦA PHASE 1
Mục tiêu cốt lõi của Phase 1 là xây dựng **Kiến trúc Dữ liệu đồng bộ (Authoritative Data Model)** cho trò chơi. Trò chơi sử dụng kiến trúc máy chủ chịu trách nhiệm quyết định mọi logic (Authoritative Server) để ngăn chặn gian lận và đảm bảo tính đồng bộ hoàn toàn giữa các người chơi trong chế độ Co-op nhiều người qua WebSocket.
Khung dữ liệu này được thiết kế để dễ dàng mở rộng khi phát triển các tính năng phức tạp hơn ở các Phase sau như Chế tạo (Crafting), Sách kỹ năng (Skills), và các ô trang bị phụ trợ (Equipment slots).

---

## ⚙️ 2. CÁCH THỨC HOẠT ĐỘNG VÀ LUỒNG DỮ LIỆU (DATA FLOW)

Luồng đồng bộ hóa dữ liệu hoạt động dựa trên cơ chế Client-Server thời gian thực:

```
[ Client Input ] (60Hz) ──> Gửi qua WebSocket ──> [ Server Logic Tick ] (20Hz)
                                                           │ (Tính toán tọa độ, HP, sát thương)
[ Render Canvas ] (60Hz) <── Nhận Snapshot 10Hz <──────────┘
 (Nội suy LERP mượt mà)
```

1. **Client Input (60Hz):** Trình duyệt gửi gói tin `input` chứa hướng đi (`dx`, `dy`), góc quay của chuột (`angle`), trạng thái tấn công (`attack`), lướt (`dash`), và tương tác (`interact`).
2. **Server Tick Rate (20Hz / 50ms):** Máy chủ cập nhật logic vật lý, kiểm tra va chạm tường, va chạm thực thể, tính toán trừ máu HP/MP, cộng điểm EXP và xử lý tương tác vật thể.
3. **Network Sync Rate (10Hz / 100ms):** Máy chủ đóng gói toàn bộ trạng thái thực thể của phòng (Players, Enemies, Projectiles, Items, Chests) gửi về dạng một `snapshot` đến tất cả người chơi trong phòng.
4. **Client Interpolation (LERP):** Client nhận snapshot và dùng phép nội suy tuyến tính (Linear Interpolation) để di chuyển nhân vật và quái vật mượt mà từ vị trí cũ sang vị trí mới ở tốc độ 60 FPS, khắc phục hiện tượng giật lag mạng.

---

## 🗂️ 3. CÁC KIỂU DỮ LIỆU CỐT LÕI (DATA STRUCTURES)

### 3.1. Đối Tượng Người Chơi (Player Entity)
Được quản lý trong danh sách `room.entities` trên Server. Mỗi Player có cấu trúc dữ liệu sau:
```javascript
playerEntity = {
    id: "p_" + connectionId,        // Mã định danh kết nối duy nhất
    type: "player",
    nickname: nickname,             // Mặc định: "Explorer"
    classType: classType,           // knight, assassin, archer, mage, support, summoner
    x: 0, y: 0,                     // Tọa độ thực trên bản đồ
    radius: 16,                     // Bán kính va chạm vật lý
    stats: {                        // Chỉ số thuộc tính của nhân vật
        vigor: 10,
        strength: 10,
        dexterity: 10,
        intelligence: 10,
        vitality: 10
    },
    freeStatPoints: 0,              // Điểm chỉ số chưa phân bổ (Nhận +5 mỗi cấp)
    level: 1,                       // Cấp độ hiện tại
    xp: 0,                          // EXP hiện tại
    gold: 0,                        // Số vàng tích lũy
    weapon: baseWeapon,             // Đối tượng vũ khí đang trang bị
    portalStoneCount: 3,            // Số Đá Dịch Chuyển đang sở hữu (Dùng về Sanctuary)
    storageItems: [],               // Kho lưu trữ dùng chung (Tối đa 20 vật phẩm)
    isDashing: false,               // Trạng thái lướt nhanh
    dashTimer: 0,                   
    dashCooldown: 0,
    attackCooldown: 0,
    angle: 0,                       // Hướng nhìn/quay nhân vật
    input: { dx: 0, dy: 0, angle: 0, attack: false, dash: false, interact: false },
    socket: ws                      // Socket kết nối trực tiếp
}
```

### 3.2. Đối Tượng Vũ Khí (Weapon Object)
Quyết định các đòn đánh và sát thương của người chơi, giới hạn trang bị theo Class:
```javascript
weapon = {
    name: "Kiếm Tập Sự",            // Tên vũ khí (hỗ trợ hậu tố cường hóa như +1, +2)
    damage: 12,                     // Sát thương cơ bản
    attackCooldown: 15,             // Tốc độ đánh (tính bằng frame tick)
    speed: 6.5,                     // Tốc độ bay của đạn (nếu là vũ khí bắn xa)
    range: 40,                      // Tầm đánh tối đa
    classLimit: "knight",           // Giới hạn class được phép đeo
    upgradeLevel: 0,                // Cấp độ cường hóa (+1, +2...)
    rarity: "Common",               // Phẩm chất: Common, Rare, Epic, Legendary
    color: "#f0f0f5"                // Mã màu hiển thị phẩm chất phát sáng trên UI
}
```

### 3.3. Thực Thể Quái Vật & Boss (Enemy / Boss Entity)
```javascript
enemy = {
    id: "e_" + nextEntityId,
    type: "enemy" || "boss",
    enemyType: "melee" || "ranged", // Quái cận chiến hoặc bắn xa
    x: 0, y: 0,
    radius: 14,
    hp: 50, maxHp: 50,
    damage: 10,
    speed: 2.0,
    kx: 0, ky: 0,                   // Lực đẩy giật lùi (Knockback) đang chịu
    knockback_decay: 0.85,          // Hệ số giảm dần lực đẩy theo thời gian
    shootCooldown: 0                // Cooldown đòn bắn xa
}
```

---

## 🧠 4. LOGIC LÕI XỬ LÝ DỮ LIỆU (CORE LOGIC)

1. **Tính Toán Lại Thuộc Tính (`recalculatePlayerAttributes`):**
   Mỗi khi người chơi lên cấp, phân bổ điểm chỉ số hoặc thay đổi vũ khí, Server sẽ tính lại máu tối đa (`maxHp`), năng lượng tối đa (`maxMp`) và sát thương gốc:
   * $MaxHP = 100 + Vitality \times 12$
   * $MaxMP = 50 + Intelligence \times 8$
   * Sát thương cơ bản cộng thêm từ chỉ số:
     * Knight/Assassin: cộng thêm sát thương dựa trên điểm `Strength`.
     * Mage/Support/Summoner: cộng thêm sát thương dựa trên điểm `Intelligence`.
     * Archer: cộng thêm sát thương dựa trên điểm `Dexterity`.

2. **Cơ Chế Va Chạm Vật Lý Với Tường (`isWallCollision`):**
   Trước khi chấp nhận vị trí `x, y` mới của người chơi hoặc quái vật, Server kiểm tra va chạm bằng cách quét bán kính nhân vật trên lưới gạch bản đồ (`mapData.grid`). Nếu gạch đích là tường (`grid[y][x] === 0`), tọa độ di chuyển sẽ bị chặn hoặc trượt dọc theo tường.

3. **Thu Thập Vật Phẩm & Lên Cấp:**
   Khi Player va chạm với các hạt rơi dưới đất (`itemType === "exp"`), lượng EXP sẽ tăng lên. Khi đạt mốc tích lũy cần thiết:
   * Công thức tính EXP yêu cầu để lên cấp: $EXP_{Needed} = \text{Round}(100 \times Level^{1.5})$
   * Khi lên cấp: $Level \leftarrow Level + 1$; nhận thêm **+5 điểm Attribute Points** (`freeStatPoints`); hồi phục hoàn toàn HP/MP.

---

## ⚡ 5. NHỮNG THỨ ĐÃ LÀM ĐƯỢC TRONG HỆ THỐNG DỮ LIỆU

1. **Chốt cấu trúc Client-Server snapshot**: Đồng bộ hoàn tất trạng thái di chuyển, lượng máu hiện tại, thông tin vũ khí, số lượng Đá Dịch Chuyển và dữ liệu hòm đồ.
2. **Khởi tạo dữ liệu lớp nhân vật và vũ khí ban đầu**: Các lớp nhân vật Knight, Assassin, Archer, Mage, Support, Summoner được gán các chỉ số khởi điểm đặc trưng phù hợp với định hướng thiết kế.
3. **Phát triển cơ chế ngẫu nhiên hóa thuộc tính vũ khí**: Cho phép rơi vũ khí ngẫu nhiên từ rương báu hoặc boss dựa trên hạt giống (seed) màn chơi và cấp độ tầng (Depth) hiện tại.

---

## 🚀 6. ĐỊNH HƯỚNG MỞ RỘNG (TODO PHÁT TRIỂN)

* **Thêm hệ thống Kho Đồ/Nguyên Liệu:** Mở rộng thuộc tính Player để lưu trữ danh sách nguyên liệu thô nhặt được (`materials`: `iron_fragment`, `beast_fur`, `magic_dust`) phục vụ cho nâng cấp và chế tạo.
* **Mở rộng các ô trang bị phụ trợ:** Bổ sung các slot trang bị như Giáp ngực, Mũ, Găng tay, Giày và Trang sức thay vì chỉ đeo vũ khí.
* **Xây dựng cấu trúc dữ liệu kỹ năng:** Thiết kế dữ liệu kỹ năng để nạp từ Sách kỹ năng, bao gồm thuộc tính mana tiêu tốn, tầm ảnh hưởng và cooldown.
