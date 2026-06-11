# PROGRESSION DESIGN — Biome 1 & Kinh Tế Crafting (Phase 3)

> Phiên bản: v1
> Cập nhật: 2026-06-11
> Trạng thái: Sẵn sàng thực hiện cho Sprint tiếp theo (Phase 3)

---

## 1. Tổng Quan Vòng Lặp Tiến Trình (Core Progression Loop)

Để chuyển đổi dự án từ một bản Tech Demo sang một game nhập vai hoàn chỉnh (**Game Systems**), cấu trúc tiến trình của game được định hình lại xung quanh tài nguyên ma thuật, nguyên liệu và chế tạo.

```
       [ Sanctuary Hub ]
         /           \
   (Dungeon Run)  (Upgrade & Craft)
       /               \
[ Dungeon Floor 1-10 ] ──> [ Thu Thập Nguyên Liệu ] ──> [ Cường Hóa Trang Bị ]
```

---

## 2. Hệ Thống Nguyên Liệu Biome 1 (Materials)

Trong Biome 1 (Tầng 1 đến Tầng 10), quái vật và rương báu sẽ rơi ra các nguyên liệu cụ thể thay vì chỉ rơi trang bị hoàn chỉnh.

| Tên Nguyên Liệu | Loại | Tỉ lệ rơi & Nguồn | Công Dụng |
| :--- | :--- | :--- | :--- |
| **Iron Fragment** (Mảnh Sắt Vụn) | Thường | 40% từ Skeleton Melee / Rương Gỗ | Nguyên liệu cơ bản chế tạo vũ khí sắt và cường hóa thấp (+1 đến +3) |
| **Beast Fur** (Lông Thú) | Thường | 30% từ Skeleton Mage / Rương Gỗ | Dùng chế tạo trang bị hạng nhẹ và giáp/vũ khí thợ săn |
| **Magic Dust** (Bụi Phép Thuật) | Hiếm | 15% từ Quái Elite / Rương Cao Cấp | Dùng cho cường hóa cao (+3 trở lên) và chế tạo vũ khí phép thuật |

---

## 3. Hệ Thống Trang Bị Biome 1 (Weapons)

Người chơi sẽ bắt đầu bằng vũ khí rỉ sét và nâng cấp dần lên các vũ khí có sát thương lớn hơn thông qua Bàn Chế Tạo.

```
[ Rusty Sword ] 
      │
      ├─ +5 Iron Fragment + 50 Vàng  ──> [ Iron Sword ]
      │                                       │
      └───────────────────────────────────────┼─ +5 Beast Fur + 5 Iron Fragment + 150 Vàng ──> [ Hunter Sword ]
```

### Chi tiết chỉ số:

1. **Rusty Sword** (Kiếm Rỉ Sét):
   - **Sát thương gốc**: 10
   - **Mô tả**: Thanh kiếm cũ kỹ, rỉ sét đầy vết nứt.
2. **Iron Sword** (Kiếm Sắt):
   - **Sát thương gốc**: 16
   - **Công thức Craft**: `1x Rusty Sword` + `5x Iron Fragment` + `50 Gold`.
   - **Mô tả**: Rèn chắc chắn từ thép cơ bản, sắc bén hơn hẳn.
3. **Hunter Sword** (Kiếm Thợ Săn):
   - **Sát thương gốc**: 24
   - **Công thức Craft**: `1x Iron Sword` + `5x Beast Fur` + `5x Iron Fragment` + `150 Gold`.
   - **Mô tả**: Kiếm nhẹ bọc da thú tốt, tăng độ linh hoạt khi vung chém.

---

## 4. Hệ Thống Cường Hóa (Blacksmith Upgrades)

Thợ Rèn tại Sanctuary sẽ nâng cấp vũ khí hiện tại của bạn. Chi phí vàng và nguyên liệu tăng dần theo cấp độ nâng cấp của vũ khí.

| Cấp Vũ Khí | Sát Thương Cộng Thêm | Chi Phí Vàng | Nguyên Liệu Yêu Cầu |
| :---: | :---: | :---: | :--- |
| **+1** | +3 damage | 100 Gold | 2x Iron Fragment |
| **+2** | +3 damage | 200 Gold | 4x Iron Fragment |
| **+3** | +3 damage | 300 Gold | 6x Iron Fragment |
| **+4** | +4 damage | 500 Gold | 2x Magic Dust |
| **+5** | +5 damage | 800 Gold | 4x Magic Dust |

---

## 5. Boss Biome 1 & Mở Khóa Tính Năng (Stone Golem)

- **Vị trí**: Xuất hiện tại **Tầng 5** (Boss Mid-Biome) hoặc **Tầng 10** (Biome Gatekeeper).
- **Boss**: **Stone Golem** (Khổng Lồ Đá).
- **Phần thưởng mở khóa**: 
  - Đánh bại Stone Golem lần đầu sẽ rơi ra **Golem Core** (Lõi Khổng Lồ Đá).
  - Đem Golem Core về tương tác với NPC Thợ Rèn để mở khóa **Bàn Chế Tạo** (Crafting Table) vĩnh viễn tại Sanctuary.
  - Trước khi đánh bại Stone Golem, người chơi chỉ có thể **Nâng Cấp (+1 -> +3)**, không thể chế tạo (Craft) vũ khí mới.

---

## 6. Sách Kỹ Năng (Skill Books Progression)

Thay vì cơ chế tự động mở khóa kỹ năng khi nhân vật tăng cấp, game sẽ áp dụng cơ chế loot kỹ năng độc đáo:
- **Nguyên lý**: Kỹ năng ẩn chứa trong các **Sách Kỹ Năng** (Skill Books) cổ xưa.
- **Nguồn nhận**:
  - Tỉ lệ rơi thấp (5%) từ quái thường ở các tầng sâu.
  - Tỉ lệ rơi cao (50%) từ các quái Elite (quái đột biến to lớn phát sáng).
  - Tỉ lệ rơi chắc chắn (100%) từ Boss Stone Golem.
- **Ví dụ Sách Kỹ Năng**:
  - `Fireball Book` (Sách Cầu Lửa): Dành cho Mage.
  - `Shield Bash Book` (Sách Khiên Phản Kích): Dành cho Knight.
  - `Quick Step Book` (Sách Lướt Nhanh): Dành cho Rogue/Archer.

---

## 7. Kế Hoạch Sprint Tiếp Theo (Phase 3 Checklist)

- [ ] **Data Model**: Thêm mảng `inventory` chứa nguyên liệu (`iron_fragment`, `beast_fur`, `magic_dust`) vào Entity Player.
- [ ] **Loot Drop**: Cập nhật logic rơi vật phẩm từ quái và rương để rơi các nguyên liệu mới này với tỉ lệ tương ứng.
- [ ] **Crafting UI**: Thiết kế tab Crafting hoàn chỉnh tại Blacksmith Panel (hiển thị danh sách nguyên liệu đang có / yêu cầu).
- [ ] **Crafting Logic**: Thêm gói tin `"craft_weapon"` lên Server để kiểm tra nguyên liệu, trừ nguyên liệu và thay đổi vũ khí của Player.
- [ ] **Boss Unlock**: Thêm cờ `craftingUnlocked` trong phòng hoặc tài khoản người chơi để kiểm soát quyền chế tạo đồ sau khi hạ Stone Golem.
