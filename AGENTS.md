# AGENTS.md

Guidelines for working in the MapleNecrocer repository.

## Project Overview

MapleNecrocer is a C# (WinForms) reimplementation of the MapleStory GM Client. It is
built on top of the WzComparerR2 WZ file library and uses MonoGame for rendering. The
goal is to replace the Delphi 32-bit GM client with a modern 64-bit version.

## Solution Structure

- `MapleNecrocer.sln` – Solution file (open this in Visual Studio).
- `MapleNecrocer/` – The main WinForms application. Each feature area has a paired
  `Form.cs` and `Form.Designer.cs` (e.g. `AvatarForm.cs` / `AvatarForm.Designer.cs`).
- `WzComparerR2.Common/` – Shared rendering, animation, controls and config code.
- `WzComparerR2.PluginBase/` – Plugin interfaces used by the main app.
- `WzComparerR2.WzLib/` – WZ file format reading library (older and newer WZ versions).
- `Reference/` – Third-party prebuilt DLLs referenced via `<Reference HintPath>`
  (AForge, ImageListView, MonoGame, SharpDX, ManagedBass, DevComponents, etc.).
- `CharaSim/` (under `MapleNecrocer/`) – Character simulation logic.
- `Client/` (under `MapleNecrocer/`) – Rendering / sprite engine / UI code.

## Build

- Requires Visual Studio 2022 or later.
- Target framework: `net8.0-windows7.0`, Windows Forms (`UseWindowsForms=true`).
- Open `MapleNecrocer.sln` in Visual Studio and build, or from the repo root:
  ```powershell
  dotnet build MapleNecrocer.sln
  ```
- `MapleNecrocer` depends on the three `WzComparerR2.*` projects (via `ProjectReference`)
  plus the prebuilt DLLs in `Reference/`. Build `MapleNecrocer` first; the dependencies
  build automatically.

## Conventions

- WinForms UI is declared in the `*.Designer.cs` files and wired to event handlers in
  the corresponding `*.cs` file. Keep the Designer file changes minimal and aligned with
  the actual control usage.
- Use `Wz` helper APIs for reading WZ data (e.g. `Wz.GetNodeA`, `Wz.GetNodeByID`,
  `Wz.GetInt`, node `GetInt`/`GetBmp`/`ImgID`/`ImgName` helpers).
- Item metadata such as required level / required job is read from the WZ `info` path,
  e.g. `Character/<Dir><ID>.img/info/reqLevel` and `.../info/reqJob`.
- `reqJob` is a bitmask where bit `0` corresponds to Warrior, bit `1` Mage, `2` Archer,
  `3` Thief, `4` Pirate (`reqJob == 0` means any class, `-1` means none).
- Follow the existing naming style (PascalCase methods/fields, `_` prefix for private
  backing fields where used).
- Do not add code comments unless the code genuinely needs clarification.

## Tests / Verification

- There is no dedicated test project; verification is done by building the solution and
  running the WinForms app against a WZ folder.
- After changes, confirm the build succeeds (`dotnet build MapleNecrocer.sln`).

## Notes on the Avatar Search Feature

The avatar search feature lives in `AvatarForm.cs` / `AvatarForm.Designer.cs`. It caches
parsed equip data (`EquipCache` / `SearchCache`) and applies class filtering and level
sorting via the `SortComboBox` and `ClassComboBox` controls. Keep the cache invalidation
and `ApplyFilterAndSort` flow intact when modifying the avatar grids.
