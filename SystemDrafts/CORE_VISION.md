# CORE OF RUIN
## Core Game Vision & Design Document v1.0

### 1. Tầm nhìn dự án
Core of Ruin là một game Action RPG Dungeon Crawler tập trung vào:
- Chiến đấu với quái vật
- Thu thập nguyên liệu
- Chế tạo trang bị
- Nâng cấp sức mạnh
- Khám phá thế giới ngầm nhiều tầng

Trọng tâm của game **KHÔNG phải** MMORPG, PvP hay cốt truyện tuyến tính.
Trọng tâm của game là vòng lặp:
```
  Đánh Quái
     ↓
Nhặt Nguyên Liệu
     ↓
  Chế Tạo
     ↓
  Nâng Cấp
     ↓
 Xuống Sâu Hơn (Depth)
     ↓
Đánh Quái Mạnh Hơn
```

---

### 2. Bản sắc riêng
Game không đi theo hướng:
- Sword Art Online ❌
- 100 tầng tháp ❌
- Anime MMORPG ❌

Game đi theo hướng:
- Dungeon Exploration ✅
- Crafting RPG ✅
- Progression-based Adventure ✅
- Underground World Discovery ✅

Người chơi **không leo lên cao**, người chơi **đi xuống sâu hơn**.
Khái niệm sử dụng:
- Floor ❌
- Depth ✅ (Ví dụ: Depth 1, Depth 2, Depth 3...)

---

### 3. Lore cốt lõi
Từ rất lâu trước đây, một thảm họa cổ đại đã phá hủy nền văn minh cũ. Nguồn gốc của thảm họa được cho là đến từ một thực thể nằm sâu dưới lòng đất: **"Core of Ruin"**.

Nhân loại chỉ còn tồn tại trong Sanctuary — nơi trú ẩn an toàn cuối cùng. Bên dưới Sanctuary là vô số tầng hầm ngục, di tích cổ đại và những vùng đất bị lãng quên.

Người chơi là một **Explorer** với mục tiêu cuối cùng:
- Đi xuống tận cùng thế giới.
- Tìm ra sự thật về Core of Ruin.

---

### 4. Gameplay Loop
```
 Sanctuary ──> Portal ──> Dungeon Exploration ──> Combat ──> Loot Materials ──> Craft / Upgrade ──> Return Sanctuary ──> Descend Deeper
```
Đây là vòng lặp gameplay cốt lõi. Mọi hệ thống mới được đề xuất đều phải phục vụ và bổ trợ cho vòng lặp này.

---

### 5. Sanctuary (Depth 0)
Sanctuary là khu vực an toàn duy nhất trong thế giới game.
**Đặc điểm:**
- Không có quái vật.
- Không nhận sát thương.
- Hiển thị toàn bộ bản đồ, không sử dụng Fog of War.

**Thành phần:**
- Portal.
- Blacksmith.
- Storage Chest.
- Các NPC/Chức năng tương lai được mở khóa dần.

**Vai trò:**
Nghỉ ngơi, Chế tạo, Nâng cấp trang bị, Quản lý vật phẩm và chuẩn bị cho chuyến thám hiểm tiếp theo.

---

### 6. Dungeon Structure
Mỗi Depth là một khu vực được sinh ngẫu nhiên sử dụng **Fog of War** (Sương mù chiến trận).
Người chơi phải thám hiểm bản đồ để tìm kiếm:
- Quái vật, Rương báu.
- Nguyên liệu thô.
- Sự kiện đặc biệt.
- Cổng/Cầu thang dẫn xuống Depth tiếp theo.

---

### 7. Progression Structure
Mỗi Biome bao gồm:
```
Depth N (Bắt đầu Biome) ──> Depth N+8 ──> Elite Encounter ──> Depth N+9 ──> Boss ──> Biome tiếp theo
```

**Bản đồ phân bổ Biome dự kiến:**
- **Biome 1 (Depth 1-10):** Boss: *Stone Guardian*
- **Biome 2 (Depth 11-20):** Boss: *Ice Wyrm*
- **Biome 3 (Depth 21-30):** Boss: *Jungle Queen*
- **Biome 4 (Depth 31-40):** Boss: *Flame Titan*
- **Biome 5 (Depth 41-50):** Boss: *Void King*

---

### 8. Special Floors (Các tầng đặc biệt)
Các tầng ngẫu nhiên có cấu trúc khác biệt để tăng tính bất ngờ và trải nghiệm khám phá của người chơi:
- **Normal:** Tầng thám hiểm cơ bản.
- **Elite:** Mật độ quái tinh anh cao.
- **Treasure:** Nhiều rương báu và tài nguyên hiếm.
- **Merchant / Village:** Điểm nghỉ chân ngẫu nhiên dưới hầm ngục.
- **Arena / Shrine / Punishment / Boss:** Các sự kiện thử thách đặc biệt.

---

### 9. Character Progression (Tiến trình sức mạnh)
Tiến trình sức mạnh của nhân vật được cấu trúc dựa trên việc tự tùy biến và kết hợp các nguồn lực:
- **Level (Cấp độ) = Mở khóa tiềm năng:** Mỗi khi lên cấp, nhân vật không tự động cộng sát thương hay máu. Thay vào đó, người chơi nhận được **+5 điểm chỉ số (Attribute Points)** để tự phân bổ tùy biến (Strength, Dexterity, Vitality, Intelligence, Luck...).
- **Equipment (Trang bị) = Nguồn sức mạnh chính (~50%):** Chỉ số cơ bản và dòng bổ trợ của vũ khí/giáp.
- **Skills (Kỹ năng) = Định hình lối chơi (~25%):** Hệ thống sách kỹ năng học được.
- **Attributes (Chỉ số phân bổ) = Tùy biến build (~15%):** Điểm cộng chỉ số khi tăng cấp.
- **Talent Tree (Nhánh tài năng) = Chuyên sâu hóa build (~10%):** Nhánh tài năng cộng thêm.

---

### 10. Equipment Philosophy
Vũ khí và trang bị có các thuộc tính phụ ngẫu nhiên để tạo tính đa dạng và giá trị cho từng món đồ (Ví dụ: `Iron Sword` có thể đi kèm dòng thuộc tính ngẫu nhiên như `+Strength`, `+Crit Chance`, hoặc `+Attack Speed`). Trang bị chính là nguồn sức mạnh cốt lõi.

---

### 11. Materials & Crafting (Kinh tế Nguyên liệu)
Quái vật và rương sẽ rơi ra các nguyên liệu:
- `Iron Fragment`
- `Beast Fur`
- `Magic Dust`
- `Crystal Core`
- `Ancient Wood`

Nguyên liệu được dùng làm tài nguyên để **Chế tạo**, **Nâng cấp** và **Mở khóa công thức**. Không tập trung mua bán trang bị trực tiếp từ NPC.

---

### 12. Skill System
Kỹ năng **không tự động học**. Người chơi phải thám hiểm Dungeon để tìm kiếm các **Sách Kỹ Năng** (Skill Books) cổ xưa (ví dụ: *Fireball Book*, *Ice Storm Book*, *Shadow Dash Book*), học chúng rồi trang bị vào các ô Skill Slot của mình.

---

### 13. Sanctuary Expansion (Nâng cấp Sanctuary)
Thế giới Sanctuary sẽ phát triển và mở khóa dần các NPC mới sau khi đánh bại các Boss lớn theo tiến trình:
- Hạ Boss Depth 10 ──> Mở rộng Blacksmith & Chế tạo
- Hạ Boss Depth 20 ──> Xuất hiện NPC Alchemist (Luyện kim)
- Hạ Boss Depth 30 ──> Xuất hiện NPC Enchanter (Phù phép)
- Hạ Boss Depth 40 ──> Xuất hiện NPC Relic Scholar (Cổ vật)

---

### 14. End Game
- **Mục tiêu cuối cùng:** Vượt qua Depth 50 và đánh bại **Void King**.
- **Kết cục:** Tiếp cận được lõi **Core of Ruin**, mở ra chương cuối cùng của cốt truyện và khám phá nguyên nhân sụp đổ của thế giới.
