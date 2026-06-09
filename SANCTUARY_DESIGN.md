# SANCTUARY DESIGN — Tầng 0 Hub

> Phiên bản: v1 (MVP)
> Cập nhật: 2026-06-09

---

## 1. Tầng 0 có gì?

Tầng 0 là **vùng an toàn duy nhất** trong game.
Không có quái. Không có rào cản. Chỉ có nhịp nghỉ và chuẩn bị.

```
┌──────────────────────────────────┐
│         SANCTUARY — TẦNG 0       │
│                                  │
│   [Portal Stone]   [Blacksmith]  │
│                                  │
│        [Player Spawn]            │
│                                  │
│        [Storage Chest]           │
│                                  │
└──────────────────────────────────┘
```

### Quy tắc tầng 0:
- **Không sinh quái** — DungeonGenerator bỏ qua bước Enemies khi `level == 0`.
- **HP tự hồi** — mỗi frame trong tầng 0, player hồi `0.5f HP/tick`.
- **Nền khác biệt** — dùng tile `Floor_Safe` (màu xanh lá/gỗ) thay vì stone.

---

## 2. NPC nào?

### v1: Chỉ có 1 NPC

| NPC | Vai trò | Vị trí |
|-----|---------|--------|
| **Blacksmith** (Thợ Rèn) | Rèn & nâng cấp trang bị | Góc trên phải phòng chính |

Merchant *không có* trong v1 — chưa có economy bán/mua.

---

## 3. Chức năng từng thành phần

### Blacksmith
Mở UI khi đứng gần và nhấn `E`:
```
[RÈNG ĐỒ]            [NÂNG CẤP]
Recipe 1  → Craft     Chọn item → +1, +2, +3...
Recipe 2  → Craft     Tốn: Đá Cường Hóa + Vàng
...
```
- **v1**: chỉ hiển thị placeholder "Coming Soon" — skeleton NPC
- **v2** (Phase 3): gắn `CraftingManager` thật

### Storage Chest
Kho chứa đồ dùng chung giữa các lần xuống hầm:
- Mở UI khi đứng gần `F`
- Drag & drop item vào/ra kho
- **v1**: lưu vào `SanctuarySave.StorageItems` (JSON)

### Portal Stone
Cổng dịch chuyển khứ hồi (xem mục 5).

---

## 4. Boss nào mở khóa NPC nào?

> Áp dụng từ Phase 8 trở đi. Ghi ở đây để không quên thiết kế sau.

| Boss | Tầng | NPC mở khóa |
|------|------|-------------|
| *(Chưa có)* | Tầng 5 | Alchemist (Luyện kim) |
| *(Chưa có)* | Tầng 10 | Enchanter (Gia trì) |
| *(Chưa có)* | Tầng 20 | Oracle (Talent respec) |

**v1 rule**: Blacksmith luôn có mặt từ đầu. Không cần boss để mở.

---

## 5. Portal hoạt động ra sao?

### Kiến trúc

```
Player nhấn [T]
    │
    ├─ Đang ở Dungeon (level >= 1)
    │       └─ Tiêu thụ 1x Portal Stone
    │           └─ Lưu: { returnFloor, returnSeed, returnX, returnY }
    │           └─ Load Tầng 0 (level = 0)
    │
    └─ Đang ở Sanctuary (level == 0)
            └─ Không tốn Portal Stone
                └─ Load lại Dungeon từ { returnFloor, returnSeed }
                    └─ Spawn tại vị trí cũ (hoặc PlayerSpawn nếu mất data)
```

### Portal Stone
- **Item type**: `consumable`
- **Slot**: không có slot (tiêu thụ trực tiếp)
- **Drop**: rơi ngẫu nhiên từ rương hoặc quái boss
- **Giá trị**: không bán được (prevent shortcut abuse)

### State cần lưu khi dùng Portal
```csharp
// Thêm vào SaveData hoặc biến global session
int ReturnFloor;
uint ReturnSeed;
float ReturnX;
float ReturnY;
```

### Giới hạn v1
- Chỉ 1 chiều: Dungeon → Sanctuary dùng Portal Stone.
- Sanctuary → Dungeon: dùng Portal miễn phí (quay về đúng tầng đang dở).
- Không cho phép Portal khi đang trong phòng Boss.

---

## 6. Flow toàn bộ (Reference)

```
[Game Start]
    ↓
Sanctuary (Tầng 0)
    ↓ (đến cổng Portal, nhấn E)
Dungeon Floor 1 → Floor 2 → ...
    ↓ (nhấn T, tốn Portal Stone)
Sanctuary — Hồi máu tự động
    ↓ (Blacksmith → v2: Craft / Upgrade)
    ↓ (Storage → gửi đồ vào kho)
    ↓ (Portal miễn phí → quay về tầng cũ)
Dungeon tiếp tục
```

---

## 7. Việc cần làm (checklist Phase 2)

- [ ] `DungeonGenerator`: thêm nhánh `GenerateSanctuary()` — không quái, tile khác
- [ ] `Program.cs`: xử lý `level == 0` → load Sanctuary, hồi HP
- [ ] `NPC/Blacksmith.cs`: skeleton — đứng gần + nhấn E → hiển thị UI placeholder
- [ ] `Items/PortalStone.cs` hoặc thêm vào `ItemData` — consumable type
- [ ] `Program.cs`: logic nhấn `T` → check Portal Stone, lưu returnState, chuyển scene
- [ ] `Save/SanctuarySave.cs`: lưu `StorageItems`, `ReturnFloor`, `ReturnSeed`
- [ ] Test: vào Sanctuary → không có quái, HP hồi → Portal → quay đúng tầng
