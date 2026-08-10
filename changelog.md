# Changelog

All notable changes are recorded here. Entries below the latest release note are
uncommitted working-tree changes.

## Uncommitted (2026-08-09)

### Goal
Add a new search feature for avatar equipment, allowing the avatar and search grids to
be filtered by class and sorted by required level.

### Changes

- **`MapleNecrocer/AvatarForm.cs`**
  - Added `EquipItemData` and `SearchItemData` classes to hold item metadata (ID,
    bitmap/name, required level, required job).
  - Added `EquipCache` (per-part dictionary) and `SearchCache` (list) to cache parsed
    equipment data instead of repopulating the grids from raw WZ nodes on every pass.
  - Read `info/reqLevel` and `info/reqJob` from the item WZ node when building the equip
    cache.
  - Extracted grid population into `PopulateEquipGrid`, which applies class filtering and
    level sorting before filling the `ImageListView`.
  - Added `RebuildSearchGrid` to repopulate the search grid with Level/Job columns.
  - Added `MatchesClass` static helper for job bitmask filtering (0 = any class,
    -1 = none).
  - Added `ApplyFilterAndSort`, `SortComboBox_SelectedIndexChanged`, and
    `ClassComboBox_SelectedIndexChanged` to re-filter/re-sort all grids when the dropdowns
    change.
  - Changed the search grid population to record `reqLevel` / `reqJob` from the WZ
    `info` path, and added hidden Level and Job columns to the search grid (cloned into
    the underlying `SearchGrid`).
  - Re-run the active search after the search grid is rebuilt/loaded.

- **`MapleNecrocer/AvatarForm.Designer.cs`**
  - Added `SortLabel`, `SortComboBox` (None / Level Asc / Level Desc), `ClassLabel`, and
    `ClassComboBox` (All / Warrior / Mage / Archer / Thief / Pirate) controls, wired to
    their change handlers, and added them to the form's controls and fields.

### Fixes (from code review)

- **`MapleNecrocer/AvatarForm.cs`**
  - Fixed a `NullReferenceException` when reading `info/reqLevel` / `info/reqJob`: the
    node-relative `GetInt` extension is not null-safe, so head/body templates (and any
    item missing those fields) crashed the load loop. Replaced with the null-safe static
    `Wz.GetInt` on the full img path.
  - Fixed the search-grid level/class filter being a no-op: `DumpEqpString` padded item
    IDs to 8 digits, which never matched the real 7-digit img file names, so every search
    row resolved `reqLevel`/`reqJob` as `0`. Now uses the unpadded numeric ID
    (`ToInt().ToString()`) for both the display and the WZ lookups.
  - Narrowed `ApplyFilterAndSort` to repopulate only the currently visible equip grid
    (tracked via a new `CurrentPartIndex`) instead of all 20 cached grids, and
    repopulate the shown grid on every part-button click so filter/sort changes are
    reflected when switching back.
  - Hardened the search-view refresh: `ApplyFilterAndSort` now calls
    `SearchGrid.Search(...)` unconditionally, so the linked search grid is always
    re-synced/cleared instead of relying on a prior empty search.
  - Made `EquipItemData` implement `IDisposable` and disposed the cached equip bitmaps
    in the form's `Dispose(bool)` override to release retained GDI+ resources when the
    form is actually disposed.

- **`MapleNecrocer/AvatarForm.Designer.cs`**
  - Repositioned the `Sort`/`Class` controls out of the avatar preview panel
    (`818–1078 × 12–212`) into the free region at `x 666–812`, `y 25–84`, so they no
    longer obscure the preview.

### Files modified
- `MapleNecrocer/AvatarForm.cs`
- `MapleNecrocer/AvatarForm.Designer.cs`
