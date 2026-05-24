# 🌙 Tôi Là Bánh Khúc Đây

Cozy idle/tycoon game về bán bánh khúc đêm Hà Nội. Made with Godot 4.6 Mono + C#.

## 📖 Design Document

Xem [TOI_LA_BANH_KHUC_DAY.md](../TOI_LA_BANH_KHUC_DAY.md) trong thư mục cha để biết toàn bộ thiết kế game.

## 🛠️ Setup

### Yêu cầu
- **Godot 4.6.3 Mono** (.NET edition)
- **.NET SDK 8.0+**
- **Git** (để version control)

### Mở project
1. Mở `Godot_v4.6.3-stable_mono_win64.exe`
2. Click **Import**
3. Chọn file `project.godot` trong thư mục này
4. Lần đầu mở: Godot sẽ import asset (mp3) và build C# solution
5. Press **F5** để chạy

## 📂 Cấu trúc

```
BanhKhucGame/
├── project.godot           # Cấu hình project Godot
├── BanhKhucGame.csproj     # .NET project file
├── BanhKhucGame.sln        # Visual Studio solution
├── icon.svg                # Icon project
├── scenes/                 # Godot scenes (.tscn)
│   └── main.tscn           # Scene chính (M1)
├── scripts/                # C# scripts
│   └── Main.cs             # Script scene chính (M1)
├── assets/
│   ├── audio/
│   │   └── rao/
│   │       └── tieng_rao.mp3   # Tiếng rao "Xôi lạc bánh khúc đây"
│   ├── sprites/            # (sẽ thêm sau)
│   └── fonts/              # (sẽ thêm sau)
└── data/                   # JSON data files (sẽ thêm sau)
```

## 🎯 Milestone hiện tại

**M1 — "Hello World Bánh Khúc"** ✅
- [x] Godot project tạo được
- [x] Scene đơn giản với 1 button + 1 placeholder sprite
- [x] Click button → phát file mp3 tiếng rao
- [x] Audio asset đã import

Tiếp theo: **M2 — "Đạp được xe"**
