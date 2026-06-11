# MASTER TODO - CORE OF RUIN (V2 ROADMAP)

> Dự án: **Core of Ruin** — Action RPG Co-op Dungeon Crawler  
> Triết lý cốt lõi: *The deeper you descend, the closer you are to the Core of Ruin.*  
> Cập nhật: 2026-06-11

---

## 🎯 DEFINITION OF SUCCESS (Version 1.0)

Game được coi là hoàn chỉnh khi đạt tất cả mốc sau:

| Mục tiêu | Trạng thái |
| :--- | :---: |
| 100 Depth (10 Biomes × 10 Depth mỗi biome) | ❌ |
| 10 Bosses (mỗi Boss cuối mỗi Biome) | ❌ |
| 50+ Skills có thể học từ Sách Kỹ Năng | ❌ |
| 300+ Items đa dạng | ❌ |
| 100+ Affixes (Tiền tố + Hậu tố trang bị ngẫu nhiên) | ❌ |
| Co-op 4 người ổn định qua WebSocket | ⚠️ Đang phát triển |
| Phát hành trên Steam (Early Access) | ❌ |
| Người chơi đánh bại Heart of Ruin (Depth 100) | ❌ |

---

## 🌍 WORLD STRUCTURE

### Depth 0 — Sanctuary
Khu vực an toàn, căn cứ trung tâm của người chơi. Không có quái vật.
- Portal, Blacksmith, Storage, các NPC mở khóa dần theo tiến trình Boss.

### 10 Biomes (Depth 1 - 100)

| Biome | Tên | Depth | Boss |
| :---: | :--- | :---: | :--- |
| 1 | Forgotten Mines | 1–10 | Stone Guardian |
| 2 | Frozen Depths | 11–20 | Ice Colossus |
| 3 | Ancient Jungle | 21–30 | Ancient Hydra |
| 4 | Sunken Ruins | 31–40 | Tide Leviathan |
| 5 | Crimson Abyss | 41–50 | Blood Tyrant |
| 6 | Void Realm | 51–60 | Void Watcher |
| 7 | Machine Graveyard | 61–70 | Titan MK-01 |
| 8 | Celestial Fracture | 71–80 | Star Eater |
| 9 | Realm of Echoes | 81–90 | Forgotten King |
| 10 | Core of Ruin | 91–100 | **Heart of Ruin** (Final Boss) |

### Special Depths (Xuất hiện ngẫu nhiên)
- **Treasure Depth:** Nhiều rương, ít quái.
- **Arena Depth:** Phòng bị khóa, sống sót nhiều đợt quái.
- **Merchant Depth:** NPC bí ẩn bán vật phẩm hiếm.
- **Ancient Shrine:** Chọn giữa Blessing hoặc Curse.
- **Lost Expedition:** Thu thập di tích và Lore từ những người đã xuống trước đó.

---

## 📋 TIẾN ĐỘ TỔNG QUAN (V2)

| Phase | Tên | Tiến độ |
| :---: | :--- | :---: |
| Phase 0 | Vision & Design | ✅ 95% |
| **Phase 1** | **Foundation** | ⚠️ 50% |
| **Phase 2** | **Core Combat** | ⚠️ 25% |
| **Phase 3** | **Loot System** | ⚠️ 20% |
| **Phase 4** | **Crafting** | ❌ 0% |
| **Phase 5** | **Skill Book System** | ❌ 0% |
| **Phase 6** | **Biome 1 Complete** | ⚠️ 30% |
| **Phase 7** | **Biome 2 Complete** | ❌ 0% |
| **Phase 8** | **Boss Progression** | ⚠️ 20% |
| **Phase 9** | **Sanctuary Evolution** | ⚠️ 60% |
| **Phase 10** | **Talent Tree** | ❌ 0% |
| **Phase 11** | **Lore System** | ❌ 0% |
| **Phase 12** | **Polish** | ❌ 0% |
| **Phase 13** | **Balancing** | ❌ 0% |
| **Phase 14** | **Playtest** | ❌ 0% |
| **Phase 15** | **Steam Preparation** | ❌ 0% |
| **Phase 16** | **Early Access** | ❌ 0% |
| **Phase 17** | **Post-Launch** | ❌ 0% |

---

## 🛠️ CHI TIẾT ĐẦU VIỆC THEO TỪNG PHASE

---

### PHASE 1 — FOUNDATION (Bộ Khung Ổn Định)
Mục tiêu: Tạo bộ khung dữ liệu và hệ thống lưu trữ đủ để mở rộng lâu dài.

**Save System:**
- [ ] Viết hệ thống lưu trữ Player State bền vững vào file JSON cục bộ hoặc SQLite — hiện tại Server lưu in-memory (mất khi restart).
- [ ] Đồng bộ hóa lưu trữ với LocalStorage phía Client cho Session nhẹ.

**Inventory System:**
- [ ] Mở rộng Player Entity với mảng `inventory[]` (tối đa 20 slot) chứa các item chưa trang bị.
- [ ] Thiết kế giao diện Inventory UI trên Client với các ô item có thể click/drag.

**Equipment Slots:**
- [ ] Thêm các slot trang bị vào Player Entity: `weapon`, `helmet`, `armor`, `gloves`, `boots`, `ring_1`, `ring_2`, `necklace`.
- [ ] Viết logic kiểm tra Class phù hợp khi trang bị từng slot.

**Material System:**
- [ ] Thêm mảng `materials[]` vào Player Entity để lưu: Iron Fragment, Beast Fur, Magic Dust, Crystal Core, Ancient Wood.
- [ ] Cập nhật `handleEnemyDeath()` để rơi nguyên liệu theo tỉ lệ phù hợp với Depth.

**Item Database:**
- [ ] Tạo file `shared/item_database.js` định nghĩa toàn bộ công thức chế tạo và các item tĩnh.
- [ ] Định nghĩa cấu trúc dữ liệu Skill để chuẩn bị cho Phase 5.

**Player Progression (Attributes → Combat Formulas):**
- [x] Hệ thống cấp độ, EXP, +5 Attribute Points mỗi cấp.
- [ ] Kết nối chỉ số Strength/DEX/INT vào công thức tính toán sát thương, HP, tốc độ đánh thực tế trên Server.

---

### PHASE 2 — CORE COMBAT (Chiến Đấu Có Cảm Giác)
Mục tiêu: Mỗi đòn đánh và sát thương phải cảm giác đã tay và có chiều sâu.

- [ ] **Knockback:** Quái và boss bị đẩy lùi khi nhận đòn, lực đẩy tỉ lệ với loại vũ khí.
- [ ] **Critical Hits:** Sát thương chí mạng tính dựa trên chỉ số LUCK hoặc DEX, hiển thị chữ nổi màu vàng lớn.
- [ ] **Elite Monsters:** Quái đột biến với HP × 3, Sát thương × 1.5, thường có phần thưởng vật phẩm hiếm hơn.
- [ ] **Status Effects:** Triển khai các trạng thái: Burning (mất máu dần theo tick), Slow (giảm tốc độ di chuyển), Freeze (bất động trong 1–2 giây).
- [ ] **Better Enemy AI:** Quái tìm đường linh hoạt hơn (tránh tường, đổi hướng), Mage bắn đạn theo arc thay vì đường thẳng.

---

### PHASE 3 — LOOT SYSTEM (Người Chơi Luôn Mong Đợi)
Mục tiêu: Mỗi lần mở rương hoặc quái chết phải đem lại cảm giác phần thưởng xứng đáng.

- [x] Rơi Gold, Heart, EXP, Weapon từ quái và rương.
- [ ] **Item Rarity:** Triển khai đầy đủ 5 cấp độ phẩm chất — Common (Trắng), Uncommon (Xanh lá), Rare (Xanh dương), Epic (Tím), Legendary (Cam).
- [ ] **Affix System:** Mỗi vật phẩm có tối đa 2 tiền tố (Prefix) và 2 hậu tố (Suffix) được tạo ngẫu nhiên khi rơi (ví dụ: "Sharp Iron Sword of Vitality").
- [ ] Điều chỉnh tỉ lệ rơi Portal Stone từ 25% rương xuống 10%, hoặc chắc chắn rơi từ Boss/Elite.
- [ ] Rươg báu hiếm (Epic Chest) xuất hiện ở Depth chia hết cho 5.

---

### PHASE 4 — CRAFTING (Vòng Lặp Loot → Craft)
Mục tiêu: Nguyên liệu phải có đầu ra thực sự, người chơi biết chính xác mình đang farm cái gì.

- [ ] Tạo bảng Recipes tĩnh trong `shared/item_database.js`.
- [ ] Tích hợp tab Craft hoàn chỉnh vào Blacksmith UI: hiển thị nguyên liệu đang có, nguyên liệu cần thêm và nút Craft.
- [ ] Gửi packet `"craft_weapon"` lên Server kiểm tra nguyên liệu và thực hiện chế tạo.
- [ ] Mở rộng nâng cấp Weapon yêu cầu cả nguyên liệu lẫn Gold (không chỉ Gold như hiện tại).
- [ ] Crafting Armor: Cho phép chế tạo trang bị giáp từ nguyên liệu Biome tương ứng.

---

### PHASE 5 — SKILL BOOK SYSTEM (Kỹ Năng Là Chiến Lợi Phẩm)
Mục tiêu: Kỹ năng không học tự động mà là đặc quyền tìm thấy trong Dungeon.

- [ ] Tạo cấu trúc dữ liệu Skill (name, manaCost, cooldown, range, damageMultiplier, effect).
- [ ] Triển khai vật phẩm Skill Book: nhặt được, lưu vào inventory, click để học.
- [ ] Thiết kế Skill Hotbar (3–4 slot) ở dưới màn hình với phím tắt Q/W/R/F.
- [ ] Đồng bộ hóa việc thi triển kỹ năng giữa Client và Server (gửi packet `use_skill`).
- [ ] Ví dụ kỹ năng Biome 1: Fireball (Mage), Shield Bash (Knight), Shadow Step (Assassin), Arrow Rain (Archer).

---

### PHASE 6 — BIOME 1 COMPLETE (Forgotten Mines Hoàn Chỉnh)
Mục tiêu: Biome 1 phải là trải nghiệm hoàn chỉnh, cân bằng và đáng nhớ.

- [x] Map sinh ngẫu nhiên Depth 1–10 với gạch đá tối.
- [ ] Quái đặc trưng Biome 1: Skeleton Warrior (melee), Bone Mage (ranged), Giant Rat (swarm).
- [ ] Elite Monsters Biome 1: Elder Skeleton với lượng máu cao và buff khu vực.
- [ ] Boss Stone Guardian: Pattern đa giai đoạn — Ném đá (Phase 1), Dậm đất gây choáng diện rộng (Phase 2 < 50% HP).
- [ ] Stone Guardian rớt Golem Core → Mang về Sanctuary mở khóa Bàn Chế Tạo.
- [ ] Nguyên liệu riêng: Iron Fragment (từ quái melee), Ancient Dust (từ quái mage).
- [ ] Special Depths ngẫu nhiên xuất hiện trong Biome 1.

---

### PHASE 7 — BIOME 2 COMPLETE (Frozen Depths)
Mục tiêu: Cảm giác môi trường băng giá ảnh hưởng gameplay.

- [ ] Gạch băng tuyết màu xanh dương, hiệu ứng giảm tốc khi đứng trên mặt băng.
- [ ] Quái đặc trưng: Ice Wraith, Frozen Golem, Arctic Bat.
- [ ] Nguyên liệu riêng: Frost Crystal, Frozen Fur.
- [ ] Boss Ice Colossus: Dựng tường băng chia cắt địa hình, bắn tia băng hình quạt.

---

### PHASE 8 — BOSS PROGRESSION (Cột Mốc Tiến Trình)
Mục tiêu: Mỗi Depth ×10 là một thử thách định hình và phần thưởng lớn.

- [x] Boss cơ bản tại Depth 10.
- [ ] Triển khai đầy đủ Boss Pattern nhiều giai đoạn (Phase 1 → Phase 2 khi dưới 50% HP) cho tất cả 10 Boss.
- [ ] Logic mở khóa căn cứ (Sanctuary Feature Unlock) sau mỗi Boss bị hạ.
- [ ] Boss rớt trang bị Legendary hoặc Skill Book đặc trưng Biome.

---

### PHASE 9 — SANCTUARY EVOLUTION (Căn Cứ Tiến Hóa)
Mục tiêu: Người chơi cảm nhận thế giới Sanctuary thay đổi theo tiến trình của mình.

- [x] Portal, Blacksmith, Storage Chest cơ bản.
- [ ] Mở khóa Alchemist NPC sau khi hạ Boss Depth 20 (chế thuốc phục hồi và bùa tăng sức mạnh).
- [ ] Mở khóa Enchanter NPC sau khi hạ Boss Depth 30 (gia trì trang bị thêm Affixes).
- [ ] Mở khóa Skill Master NPC sau khi hạ Boss Depth 40 (mua/đổi Skill Books hiếm).
- [ ] Mở khóa Relic Keeper NPC sau khi hạ Boss Depth 50 (Relic items cộng bị động mạnh).
- [ ] Mở khóa Guild Hall sau khi hạ Boss Depth 60 (tính năng lịch sử thành tích đội nhóm).
- [ ] Thay đổi giao diện Sanctuary (ánh sáng, ngoại cảnh) theo từng mốc Boss bị hạ.

---

### PHASE 10 — TALENT TREE (Xây Dựng Build)
Mục tiêu: Cho phép tùy biến build nhân vật theo chiến thuật cá nhân.

- [ ] Thiết kế 3 nhánh tài năng: Offense (Sát thương), Defense (Phòng thủ), Utility (Đa dụng).
- [ ] Tích hợp giao diện Tab Talent vào Stats Panel.
- [ ] Người chơi nhận Talent Point mỗi 5 cấp (cần xác nhận số lượng chính xác).

---

### PHASE 11 — LORE SYSTEM (Câu Chuyện Thế Giới)
Mục tiêu: Người chơi khám phá bí mật của Core of Ruin theo tiến trình tự nhiên.

- [ ] Hệ thống hội thoại NPC (NPC Dialogues) dẫn dắt nhiệm vụ.
- [ ] Bia đá cổ đại (Ancient Records) ngẫu nhiên trong dungeon, thu thập để mở Lore Journal.
- [ ] Sự kiện thế giới (World Events) thay đổi theo Depth: thông điệp từ người đã xuống trước, bức thư tìm thấy trong hầm.
- [ ] Cutscene/overlay ngắn khi người chơi lần đầu vào mỗi Biome.

---

### PHASE 12 — POLISH (Đánh Bóng Trải Nghiệm)
Mục tiêu: Biến prototype thành game có cảm giác chuyên nghiệp.

- [ ] Sound Effects: chém, trúng đòn, nhặt đồ, mở rương, tử vong, lên cấp.
- [ ] Dynamic Music: nhạc nền thay đổi theo khu vực (Sanctuary, Dungeon, Boss Fight).
- [ ] Particles & Visual FX: hiệu ứng hạt khi tung kỹ năng, nhặt tài nguyên, khai mở cổng.
- [ ] Screen Shake: rung mạnh khi Boss dậm đất hoặc khi nhận đòn chí mạng.
- [ ] UI Improvements: Animation mở panel, thanh HP/MP mượt mà hơn.

---

### PHASE 13 — BALANCING (Cân Bằng Chỉ Số)
Mục tiêu: Progression từ Depth 1 đến 100 mượt mà, không gây chán hay quá khó.

- [ ] Cân bằng đường cong HP/Damage của quái từ Depth 1 đến 100.
- [ ] Cân bằng tỉ lệ rơi trang bị và nguyên liệu theo Depth.
- [ ] Cân bằng đường cong XP và tốc độ lên cấp.
- [ ] Cân bằng độ khó Boss (Pattern số lượng, tốc độ, sát thương).
- [ ] Cân bằng kinh tế Gold và chi phí Crafting/Upgrade.

---

### PHASE 14 — PLAYTEST (Thử Nghiệm Thực Tế)
- [ ] Internal Testing: chơi thử và vá lỗi bug mạng/sync nội bộ.
- [ ] Friends Testing: chia sẻ bản thử nghiệm cho nhóm nhỏ.
- [ ] Closed Beta: thu thập phản hồi về độ khó, điều khiển, cảm giác chiến đấu.
- [ ] Tối ưu hiệu năng: FPS và độ trễ WebSocket khi 4 người chơi Co-op cùng lúc.

---

### PHASE 15 — STEAM PREPARATION (Chuẩn Bị Phát Hành)
- [ ] Tích hợp Steamworks SDK.
- [ ] Hệ thống Achievements (Thành tựu) đặc trưng (ví dụ: hạ Boss đầu tiên, đến Depth 50...).
- [ ] Cloud Save đồng bộ dữ liệu người chơi qua Steam Cloud.
- [ ] Quay Trailer gameplay.
- [ ] Thiết lập Steam Store Page: mô tả game, ảnh chụp màn hình, capsule art.

---

### PHASE 16 — EARLY ACCESS (Phát Hành Sớm)
- [ ] Phát hành phiên bản Early Access trên Steam.
- [ ] Hệ thống thu thập phản hồi cộng đồng (Steam Forum, Discord).
- [ ] Lịch cập nhật bản vá định kỳ (Hotfix và Patch Notes).

---

### PHASE 17 — POST-LAUNCH (Hậu Ra Mắt)
- [ ] Phát triển thêm các Biome mới (mở rộng sau Depth 100).
- [ ] Thêm Boss mới và Boss tái hiện theo sự kiện theo mùa (Seasonal Events).
- [ ] Thêm Skill Books và lớp nhân vật mới.
- [ ] Cập nhật nội dung mở rộng (DLC/Free Content Update).
