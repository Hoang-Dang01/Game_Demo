# MASTER PLAN: SAO 2D CO-OP DUNGEON RPG (WEB EDITION)

## Vision

Xây dựng một game RPG 2D phong cách Sword Art Online, nơi người chơi cùng nhau khám phá các tầng hầm ngục, săn trang bị hiếm, đánh Boss và phát triển nhân vật qua nhiều giờ chơi.

Triết lý phát triển:
- **Gameplay First**
- **RPG Loop First**
- **Art Later**

*Nếu game vui khi mọi thứ chỉ là hình vuông thì sau này thêm đồ họa sẽ càng hấp dẫn.*

---

## TRIẾT LÝ THIẾT KẾ PROGRESSION
Game không tập trung hoàn toàn vào loot như Diablo. Thay vào đó, sức mạnh nhân vật được quyết định bởi 4 hệ thống phát triển song song:
```text
Level (Nguồn tài nguyên) ──> Stat Points (Định hướng Build)
                                  │
Skills (Phong cách đấu)  <───  Equipment (Nguồn sức mạnh chính)
```
- **Level:** Nguồn cấp điểm chỉ số để tăng trưởng.
- **Stat Points:** Định hình hướng xây dựng nhân vật (Build).
- **Equipment:** Nguồn sức mạnh chính, quyết định chỉ số chiến đấu cơ bản.
- **Skills:** Phong cách chiến đấu riêng biệt của từng người chơi.

---

## CORE RPG LOOP

Người chơi sẽ lặp lại chu trình:

```text
Vào Dungeon
    ↓
Đánh Quái
    ↓
Nhận EXP
    ↓
Lên Cấp
    ↓
Phân Bổ Điểm Chỉ Số (Vigor, Strength, Dexterity, Intelligence, Vitality)
    ↓
Mạnh Hơn
    ↓
Đánh Boss
    ↓
Nhặt Trang Bị (Weapon, Helmet, Armor, Gloves, Boots, Rings, Necklace)
    ↓
Học Kỹ Năng Mới (Qua Skill Book từ Boss)
    ↓
Lên Tầng
    ↓
Thử Thách Mới & Loot Hiếm Hơn
```

---

## KIẾN TRÚC HỆ THỐNG

### Client (Web Browser):
- HTML5 Canvas để vẽ và dựng giao diện.
- Vanilla Javascript (Gửi Input, render hình ảnh, LERP mượt mà 60FPS).

### Server (Node.js):
- Node.js + WebSockets.
- Authoritative Server Logic (Chạy physics, combat, AI, XP và Loot để chống hack).
- Tốc độ tick: **Server Loop 20Hz (50ms)** | **Network Sync 10Hz (100ms)**.

---

## HỆ THỐNG CHỈ SỐ (STAT SYSTEM)
Mỗi lần lên cấp, nhân vật được cộng **+5 Điểm Chỉ Số Tự Do (Stat Points)** để phân bổ vào:
- **Vigor (Sức sống):** Tăng HP tối đa, tốc độ hồi HP và khả năng chống chịu sát thương cơ bản.
- **Strength (Sức mạnh):** Tăng sát thương vật lý và hiệu quả của các loại vũ khí cận chiến.
- **Dexterity (Khéo léo):** Tăng tốc độ đánh, tỷ lệ chí mạng (Crit Chance), tốc độ di chuyển và độ chính xác.
- **Intelligence (Trí tuệ):** Tăng Mana tối đa (MP), sát thương phép thuật và hiệu quả các kỹ năng ma pháp.
- **Vitality (Thể chất):** Tăng điểm Giáp (Defense), kháng hiệu ứng khống chế và kháng phần trăm sát thương dính phải.

---

## HỆ THỐNG CHỨC NGHIỆP (COMBAT CLASS)
Mỗi Class giới hạn loại vũ khí, trang bị, bộ kỹ năng và đóng vai trò khác nhau trong tổ đội:

1. **Knight (Hiệp sĩ):**
   - *Vai trò:* Đỡ đòn (Tank), tiên phong (Frontline).
   - *Vũ khí:* Kiếm, Khiên, Giáp nặng.
   - *Nhánh nâng cao:* **Guardian** (Thiên hướng bảo vệ) | **Berserker** (Chém nhanh, đổi máu lấy sát thương).
2. **Assassin (Sát thủ):**
   - *Vai trò:* Sát thương chí mạng lớn đột ngột, cơ động cao, ám sát mục tiêu nhanh.
   - *Vũ khí:* Dao găm, Phi tiêu, Kiếm ngắn.
   - *Nhánh nâng cao:* **Shadow Assassin** (Tàng hình, chém lén) | **Ranger Assassin** (Đặt bẫy, cơ động cấu rỉa).
3. **Archer (Cung thủ):**
   - *Vai trò:* Sát thương vật lý tầm xa liên tục (DPS), kỹ năng diện rộng (AOE).
   - *Vũ khí:* Cung, Nỏ.
   - *Nhánh nâng cao:* **Sniper** (Tầm bắn siêu xa, chí mạng khủng) | **Hunter** (Gọi linh thú hỗ trợ, bắn nhanh).
4. **Mage (Pháp sư):**
   - *Vai trò:* Sát thương phép thuật diện rộng (AOE), khống chế đám đông (Crowd Control).
   - *Vũ khí:* Trượng phép, Sách phép.
   - *Nhánh nâng cao:* **Pyromancer** (Phép thuật hỏa nổ lan thiêu đốt) | **Cryomancer** (Phép thuật băng làm chậm, đóng băng).
5. **Support (Hỗ trợ):**
   - *Vai trò:* Hồi máu (Healer), cường hóa đồng đội (Buff), làm yếu kẻ địch (Debuff).
   - *Vũ khí:* Thánh thư, Trượng hỗ trợ.
   - *Nhánh nâng cao:* **Priest** (Hồi phục máu và giáp mạnh) | **Oracle** (Hồi sinh, kháng hiệu ứng, tăng sát thương cho party).
6. **Summoner (Triệu hồi sư):**
   - *Vai trò:* Triệu hồi và điều khiển quái thú phụ trợ chiến đấu.
   - *Vũ khí:* Ngọc triệu hồi, Trượng triệu hồi.
   - *Nhánh nâng cao:* **Beast Master** (Triệu hồi thú dữ đồng hành) | **Necromancer** (Triệu hồi quái vật xương từ xác quái).

---

## HỆ THỐNG NGHỀ NGHIỆP PHỤ (PROFESSION)
Người chơi có thể học một hoặc nhiều nghề nghiệp phụ để chế tạo đồ hoặc thu thập nguyên liệu quý:
- **Alchemist (Giả kim thuật):** Chế thuốc hồi máu HP, thuốc hồi mana MP, thuốc tăng chỉ số tạm thời (Buff Potion), độc dược bôi vũ khí và bom ném.
- **Blacksmith (Rèn):** Chế tạo vũ khí, giáp trụ và nâng cấp/cường hóa trang bị hiện có.
- **Enchanter (Phù phép):** Chế tạo ngọc cường hóa thuộc tính và phù phép hiệu ứng đặc biệt lên trang bị.
- **Cook (Đầu bếp):** Chế biến thức ăn tăng chỉ số và các buff dài hạn khi vượt ải.
- **Hunter (Thợ săn):** Thu thập da thú, xương quái vật và các nguyên liệu sinh vật hiếm.
- **Miner (Thợ mỏ):** Khai thác các mỏ quặng sắt, vàng, và đá quý rải rác trong dungeon.
- **Herbalist (Hái thuốc):** Thu thập các loại thảo dược, hoa lạ phục vụ chế thuốc giả kim.

---

## HỆ THỐNG KỸ NĂNG & SÁCH KỸ NĂNG (SKILL BOOK SYSTEM)
Kỹ năng không tự động mở khóa theo cấp độ. Người chơi phải tìm kiếm **Sách kỹ năng (Skill Book)** từ Boss, rương hiếm trong Dungeon, hoặc làm nhiệm vụ.
- **Kỹ năng chủ động (Active Skills):** Tốn Mana (MP) để kích hoạt (ví dụ Mage: *Fireball*, *Ice Spear*, *Meteor*, *Teleport*).
- **Kỹ năng bị động (Passive Skills):** Kích hoạt vĩnh viễn (ví dụ: *Mana Mastery* tăng mana tối đa, *Spell Crit* tăng chí mạng phép, *Magic Amplification* tăng sát thương phép).
- **Nâng cấp kỹ năng:** Sách kỹ năng cấp cao cho phép nâng cấp chiêu thức (ví dụ: *Fireball Lv1* tốn 10 MP, gây 100 dmg -> *Fireball Lv2* gây 130 dmg -> *Fireball Lv3* thêm hiệu ứng nổ diện rộng AOE).

---

## HỆ THỐNG TRANG BỊ & ĐỘ HIẾM (EQUIPMENT SYSTEM)

### Ô Trang bị (Equipment Slots):
1. Weapon (Vũ khí)
2. Helmet (Mũ)
3. Armor (Giáp ngực)
4. Gloves (Găng tay)
5. Boots (Giày)
6. Ring 1 (Nhẫn 1)
7. Ring 2 (Nhẫn 2)
8. Necklace (Dây chuyền)

### Khóa Trang bị theo Class (Class Constraints):
Vũ khí và giáp có thuộc tính `allowedClasses`. Người chơi khác Class chỉ có thể nhặt nhưng không thể trang bị (ví dụ: Knight nhặt được *Flame Staff* chỉ có thể giữ trong túi đồ hoặc giao dịch cho Mage trong party).

### Hiệu ứng Trang bị Đặc biệt (Unique Effects):
Vũ khí/trang bị hiếm có các chỉ số cơ bản cùng các hiệu ứng đặc biệt:
- *Frost Ring:* +10 Intelligence, có 5% cơ hội đóng băng mục tiêu trong 1 giây.
- *Vampire Sword:* +30 Sát thương vật lý, 10% hút máu (Life Steal).
- *Phoenix Robe:* Cho phép hồi sinh ngay lập tức 1 lần duy nhất với 50% HP khi nhận sát thương chí mạng.

---

## LỘ TRÌNH PHÁT TRIỂN 8 GIAI ĐOẠN (SAO EDITION)

### 🟩 PHASE 1 - RPG FOUNDATION (Đã hoàn thành 95%)
- [x] Di chuyển mượt mà 60FPS, lướt né đòn (Dash).
- [x] AI quái vật tiếp cận tấn công người chơi.
- [x] Hệ thống va chạm tường AABB đồng bộ Client - Server.
- [x] Vượt tầng (Floor Progression) qua ô cầu thang.
- [x] Rương báu mở bằng phím E rơi trang bị cơ bản.
- [x] Sửa triệt để các lỗi `NaN` lực đẩy lùi (Knockback).

### 🟦 PHASE 2 - CHARACTER PROGRESSION & STATS (Trọng tâm tiếp theo)
*Mục tiêu: Đưa hệ thống Level, Stat Points, Mana, và 6 Class vào game.*
- **Bổ sung thanh Mana (MP):** Tích hợp chỉ số Mana vào HUD người chơi, hồi phục mana theo thời gian (ảnh hưởng bởi chỉ số Intelligence).
- **Hệ thống phân bổ điểm (Stat Points allocation):**
  - Mỗi khi lên cấp, cho phép nhấn phím `C` mở bảng thông tin chỉ số.
  - Người chơi tự phân bổ điểm vào: Vigor, Strength, Dexterity, Intelligence, Vitality để định hình Build (ví dụ: Pháp sư dồn DEX để tung chiêu siêu nhanh).
- **Hệ thống 6 Class cơ bản:** Knight, Assassin, Archer, Mage, Support, Summoner (Thiết lập giới hạn vũ khí theo Class).

### 🟦 PHASE 3 - LOOT, EQUIPMENT & UNIQUE EFFECTS
*Mục tiêu: Mở rộng 8 ô trang bị, hệ thống độ hiếm và các hiệu ứng ngẫu nhiên.*
- Mở rộng túi đồ hỗ trợ trang bị: Mũ, Giáp, Găng, Giày, Nhẫn, Dây chuyền.
- Sinh chỉ số phụ ngẫu nhiên (Affixes) cho trang bị rớt ra dưới đất.
- Code hiệu ứng đặc biệt: Đóng băng (Freeze), giật sét lan (Lightning Chain), đốt cháy (Burn), hút máu (Life Steal).

### 🟨 PHASE 4 - SKILL BOOK SYSTEM & ACTIVE SKILLS
*Mục tiêu: Bỏ cơ chế bắn thường vô hạn, tích hợp Sách Kỹ Năng.*
- Tạo vật phẩm Sách Kỹ Năng (Skill Book) rơi từ quái/Boss. Sử dụng sách để học và nâng cấp kỹ năng.
- Gán phím số `1`, `2`, `3` trên bàn phím để kích hoạt kỹ năng chủ động tương ứng với Class và tốn mana.

### 🟨 PHASE 5 - DUNGEON EXPLORATION & MAP THEMES
- Tích hợp 4 chủ đề map theo Floor (Forest, Mine, Ice Cave, Ruined Castle).
- Sinh phòng ẩn (Secret Rooms), bia đá hồi phục (Shrines), và sự kiện bẫy quái.

### 🟨 PHASE 6 - BOSS SPECIAL MECHANICS & DIFFICULTY SCALING
- Tích hợp các Boss Goblin King (Floor 5), Forest Dragon (Floor 10), Ice Titan (Floor 20).
- Hệ thống Cân bằng độ khó động: Tăng máu và sát thương quái dựa theo số lượng người chơi trong phòng.

### 🟨 PHASE 7 - ART & SPRITE INTEGRATION
- Thay thế các khối tròn neon bằng sprite nhân vật pixel, gạch nền và các icons vật phẩm.

### 🟨 PHASE 8 - AUDIO & POLISH
- Tích hợp nhạc nền, âm thanh chém, phép thuật, nhặt đồ và hiệu ứng chuyển cảnh mượt mà.

---

## ĐỊNH NGHĨA MVP TIẾP THEO (PHASE 2 - 3 COMPLETE)
Game đạt trạng thái sẵn sàng thử nghiệm tính năng phát triển nhân vật khi người chơi có thể:
1. Chọn 1 trong 6 Class khi tạo nhân vật.
2. Đánh quái lên cấp, nhận điểm và phân phối điểm chỉ số (Vigor/Strength/DEX/INT/VIT) để thay đổi chỉ số chiến đấu thực tế.
3. Nhặt các loại trang bị khác nhau (Mũ, Giáp, Găng...) và chỉ trang bị được đồ đúng Class của mình.
4. Quái vật rớt ra sách kỹ năng để học chiêu thức tốn Mana trên thanh HUD mới.
