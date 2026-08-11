# Changelog

All notable changes are recorded here. Entries below the latest release note are working-tree changes.

## (2026-08-10) - Player Sprite, Minimap & Weapon Grid Fixes

### Goal
Fix three rendering/data bugs reported in the current build: the player sprite being
invisible in play mode and the avatar window, the minimap being messed up, and the
avatar screen showing no equipment for the Weapon-1 / Weapon-2 slots.

### Invisible player sprite restored

- **`MapleNecrocer/Client/MapleMap.cs`**
  - `LoadMap` clears `Wz.EquipImageLib` (and disposes its textures) on every map load to
    free memory, but `Wz.EquipData` / `EquipDumpList` were not reset and the player is
    only spawned on the first load. So after the first map change the player's equip
    textures were gone, leaving every `AvatarParts` sprite with a valid `ImageNode` but no
    texture — invisible in play mode and in the avatar window.
  - After clearing the library, `LoadMap` now calls `Player.ReDumpEquip()` when a player
    exists, re-populating the player's equip textures.

- **`MapleNecrocer/Client/MapleCharacter.cs`**
  - Added `Player.ReDumpEquip()`: re-dumps the base body/head template plus every item in
    `EqpList` into `Wz.EquipData` / `Wz.EquipImageLib`, and resets `EquipDumpList`. The
    existing sprites keep their `ImageNode` references, so re-dumping restores their
    textures without recreating sprites.

### Minimap texture lookups hardened

- **`MapleNecrocer/Client/UI/MiniMap.cs`**
  - `DrawImage` / `GetImage` now resolve textures robustly: they look up the node directly,
    then resolve UOLs (`ResolveUol`) and fall back to the node stored under the same path
    in `Wz.UIData`. This matches how `Wz.DumpData` keys `Wz.UIImageLib` for plain PNGs,
    PNGs with `_outlink`/`_inlink`, and UOLs, so the canvas, border tiles, NPC/portal marks
    and the player marker are drawn even when `GetNode(...)` returns a linked source node.
  - Canvas and border-tile lookups now use the raw child node (`Nodes["canvas"]` /
    `Nodes["n"]` etc.) instead of `GetNode(...)`, matching the node the dump stored.
  - Mark lookups (`npc`/`portal`/`user`/map-mark) now consistently use `GetNodeA`.
  - The player marker draw uses the hardened `GetImage` helper.

### Weapon-1 / Weapon-2 avatar grid populated

- **`MapleNecrocer/AvatarForm.cs`**
  - The `default` case in `button1_Click` (weapons, caps, coats, etc.) required
    `Iter.Nodes["icon"]` on each animation sub-node, which only fires when `info` is a
    direct child and the icon lives there. In the newer Character WZ layout that check
    missed every weapon, leaving the Weapon-1 / Weapon-2 grids empty.
  - It now extracts the icon from `itemImgNode.GetNode("info/icon")` (img-level, robust to
    nested `info`) and adds the item once per img using `img.ImgID()`, guarded by a dedup
    check.

### Files modified
- `MapleNecrocer/Client/MapleMap.cs`
- `MapleNecrocer/Client/MapleCharacter.cs`
- `MapleNecrocer/Client/UI/MiniMap.cs`
- `MapleNecrocer/AvatarForm.cs`

## (2026-08-10) - Avatar Export Preview & Keyboard Input Fixes

### Goal
Fix three Avatar-form issues: the Export-tab character preview window was off-screen at
regular window size, oversized sprites (large weapons) were cut off in the Export preview,
and the game character stopped responding to the keyboard after interacting with other
forms (Skill, the Search box).

### Export preview position restored

- **`MapleNecrocer/AvatarForm.Designer.cs`**
  - Restored `panel2` (the character preview on the Export tab) from `Point(660, 144)` to
    its original `Point(155, 144)`. The fork had moved it past the 792px tab page, so it
    was only visible when the window was maximized.

### Fit-to-frame preview and export

- **`MapleNecrocer/SpriteFit.cs`** (new)
  - Added `SpriteFit.FitScale(width, height, maxW, maxH)` and
    `SpriteFit.FootprintOverflows(posX, posY, w, h, frameW, frameH)` — pure math helpers
    used by both the preview and the export path.

- **`MapleNecrocer/FrameListDraw.cs`**
  - The Export preview now detects overflow on the sprite's actual on-screen footprint
    (`posX < 0`, `posY < 0`, or sprite extending past the frame edge), not just oversized
    dimensions, so sprites pushed off-frame by a negative offset are also caught. On
    overflow it re-centers the avatar and scales it down to fit the 512×512 frame.
    Removed the redundant `Math.Min(scale, 1f)`.

- **`MapleNecrocer/AvatarForm.cs`**
  - `GetClipBoundindBox` no longer clamps the source region to 512; the full `AvatarBound`
    is returned and the export methods scale it to fit instead of cropping.
  - `ExportSprite`, `ExportAllSprite`, and `ExportSpriteSheet` now compute a fit scale from
    `SpriteFit` and draw the cropped region scaled, so exported output matches the preview
    (normal-sized avatars are unchanged; oversized sprites are no longer clipped). Debug
    overlay rectangles/lines are scaled to match.

### Keyboard input gating made reliable

- **`MapleNecrocer/MainForm.cs`**
  - Removed the fragile `Deactivate`/`Activated` handlers that flipped
    `SpriteEngine.Keyboard.WindowActive`, which were missed after interacting with other
    forms.

- **`MapleNecrocer/RenderFormDraw.cs`**
  - `Update` now computes `WindowActive` each frame from `Form.ActiveForm == MainForm.Instance`
    (single source of truth, still suppresses input when the app is not focused) OR a left
    click inside the game view, which also returns focus to the game control. Guarded by
    `IsHandleCreated`.

### Tests

- **`MapleNecrocer.Tests/SpriteFitTests.cs`** (new)
  - 12 cases covering `FitScale` (fit, scale-by-width, scale-by-height, exact-frame margin,
    non-positive inputs, never upscales) and `FootprintOverflows` (offset-induced overflow,
    in-frame, negative position). All tests pass.

### Files modified
- `MapleNecrocer/AvatarForm.Designer.cs`
- `MapleNecrocer/FrameListDraw.cs`
- `MapleNecrocer/AvatarForm.cs`
- `MapleNecrocer/SpriteFit.cs` (new)
- `MapleNecrocer/MainForm.cs`
- `MapleNecrocer/RenderFormDraw.cs`
- `MapleNecrocer.Tests/SpriteFitTests.cs` (new)

## (2026-08-10) - Code Review Fix Pass

### Goal
Apply the actionable findings from the PR review: harden the async startup load, make
WZ file discovery deterministic (prefer `Base.wz`), fix the fragile CLI path parsing,
avoid per-frame WZ re-dumping in the minimap, and stop silently swallowing settings/log
failures.

### Async startup load hardening

- **`MapleNecrocer/MainForm.cs`**
  - `AutoLoadWz` changed from `async void` to `async Task`, and the path-existence check
    moved inside the outer `try`/`catch`/`finally`, so no unobserved exception can escape
    the method. Continuations still resume on the WinForms UI thread via the captured
    `SynchronizationContext`; the heavy WZ parse remains on a background thread.
  - WZ file discovery now prefers `Base.wz` first and only falls back to `Data.wz`, instead
    of taking `First()` from an unordered `Base.wz;Data.wz` recursive enumeration.

- **`MapleNecrocer/SelectFolderForm.cs`**
  - Added `Directory.FindMapleWz(string)` which returns the first `Base.wz`, else the first
    `Data.wz`, under the given folder. Both the folder-dialog load and the recent-files load
    now use it instead of `EnumerateFiles("Base.wz;Data.wz").First()`, fixing the
    Data.wz-before-Base.wz ambiguity.

### CLI path parsing

- **`MapleNecrocer/Program.cs`**
  - `ResolveMaplePath` now iterates the full `args` array, requires `--maplePath` to have a
    following non-flag argument, and only accepts the value when it is a non-empty existing
    directory. A trailing `--maplePath` is no longer silently dropped.

### Minimap per-frame re-dump eliminated

- **`MapleNecrocer/Client/UI/MiniMap.cs`**
  - Every `Wz.DumpData(...)` call in `DrawVersionAlpha`, `DrawVersion1`, and `DrawVersion3`
    is now guarded by `if (!Wz.UIData.ContainsKey(node.FullPathToFile2()))`, so the UI image
    library is populated once per node instead of every frame. This matches the existing
    pattern already used for the `UI/UIWindow2.img/MiniMap/MaxMap` entry in `DrawVersion3`
    and in `UI.Utils.cs`.
  - Because the guards rely on the static `Wz.UIData`/`Wz.UIImageLib` caches, `RemoveWz`
    now clears them so switching MapleStory WZ folders re-dumps the UI data instead of
    leaving stale textures from the previous WZ.

### Error/settings logging

- **`MapleNecrocer/MainForm.cs`**
  - `WriteError` is now `public static`, serialized with a `lock`, and its own catch writes
    the failure to the debug output instead of being an empty `catch {}`.

- **`MapleNecrocer/Program.cs`**
  - `AppSettings.Load`/`Save` now route their caught exceptions through `MainForm.WriteError`
    instead of silently swallowing them.

### Verified as not actionable

- **MiniMap `GetInt` null-safety** — `Map.Img.GetInt("miniMap/centerX")` is already safe:
    it routes through `GetValueEx`, which returns the default for a null node. No change needed.
- **`Music.Play` under mute** — already returns before constructing a player/preloading data;
    `OptionForm` never un-mutes on first open. The muted-by-default behavior is intentional.

### Deferred

- Moving the pure WZ enumeration in `DumpMapIDs` to a background thread (UI can still freeze
  briefly for very large map sets). Kept on the UI thread for now; grid updates are already
  marshalled correctly and wrapped in try/catch.

### Files modified
- `MapleNecrocer/MainForm.cs`
- `MapleNecrocer/SelectFolderForm.cs`
- `MapleNecrocer/Program.cs`
- `MapleNecrocer/Client/UI/MiniMap.cs`

## (2026-08-10) - Async WZ Load, Portable Maple Path & Minimap Perf

### Goal
Address the local code-review findings: block the UI thread during startup WZ loading,
remove the machine-specific hardcoded MapleStory path, and avoid per-tile dictionary
lookups in the minimap border drawing.

### Deferred WZ loading off the UI thread

- **`MapleNecrocer/MainForm.cs`**
  - `AutoLoadWz` is now `async void`. The directory scan
    (`Directory.EnumerateFiles`) and the heavy WZ parse (`RemoveWz` / `OpenWZ`) run on
    background threads via `await Task.Run(...)`.
  - UI-bound work — `DumpMapIDs`, `LoadMap`, `WriteError`, and `MessageBox` — is
    marshalled back onto the WinForms dispatcher by the captured `SynchronizationContext`.
  - A wait cursor is shown during the load and cleared in a `finally` block as a lightweight
    loading indicator.
  - Error handling is split into distinct stages (locating files, parsing WZ, loading map)
    so failures are logged and reported with a precise message.

### Portable MapleStory path resolution

- **`MapleNecrocer/Program.cs`**
  - Removed the hardcoded `V:\Nexon\maplestory` default; `MaplePath` now defaults to `""`.
  - Added `ResolveMaplePath` which resolves the path at runtime in order: `--maplePath`
    command-line argument, then the `MAPLESTORY_PATH` environment variable, then the
    persisted `settings.json` value, falling back to `""` (no auto-load) when none exist.
  - Added an `AppSettings` helper that reads/writes `settings.json` in the startup folder,
    persisting both `IsMute` and `MaplePath`. `Load` is backward compatible with existing
    files that only contain `IsMute`.

- **`MapleNecrocer/OptionForm.cs`**
  - `LoadMuteState` / `SaveMuteState` now delegate to `AppSettings`, preserving the saved
    Maple path alongside the mute state.
  - `btnSaveMaplePath_Click` persists the chosen folder to `settings.json` (via
    `AppSettings`) so the path survives restart.
  - Removed the now-unused `MuteSettings` POCO, `SettingsFile` constant, and `System.IO` /
    `System.Text.Json` usings.

- **`run.bat`**
  - Uses `%MAPLESTORY_PATH%` when set, falling back to the previous default; the file check
    and the `--maplePath` argument now reference the resolved variable.

- **`.vscode/launch.json`**
  - The `--maplePath` argument now uses `${env:MAPLESTORY_PATH:V:\\Nexon\\maplestory}` so a
    machine-specific default is only used when the env var is unset.

### Minimap border drawing optimization

- **`MapleNecrocer/Client/UI/MiniMap.cs`**
  - Added a `GetImage` helper that resolves a node to a `Texture2D` once via
    `Wz.UIImageLib.TryGetValue`.
  - The border loops in `DrawVersionAlpha`, `DrawVersion1`, and `DrawVersion3` now resolve
    each border tile texture (`n`/`s`/`w`/`e`/`nw`/`ne`/`sw`/`se`) a single time before the
    loop and reuse the cached reference, instead of doing a dictionary lookup per tile every
    frame.

### Files modified
- `MapleNecrocer/MainForm.cs`
- `MapleNecrocer/Program.cs`
- `MapleNecrocer/OptionForm.cs`
- `MapleNecrocer/Client/UI/MiniMap.cs`
- `run.bat`
- `.vscode/launch.json`

## (2026-08-10) - Show Window on Startup

### Goal
The application was launching minimized, so after the WZ data loaded successfully the user
saw nothing. Make the main window show normally on startup.

### Files modified
- `MapleNecrocer/MainForm.cs`
  - Removed `this.WindowState = FormWindowState.Minimized;` from the constructor so the
    window opens normally instead of minimized on the taskbar.

## (2026-08-10) - Sprite Missing-Texture Crash Fix

### Goal
Fix the `KeyNotFoundException` ("... Wz_Png ... not present in dictionary") raised at
`SpriteEngine.cs:1046` / `MapleCharacter.cs:1732` when a sprite's image node has not been
loaded into the image library, which prevented the map from rendering.

### Null-safe sprite image access

- **`MapleNecrocer/Client/SpriteEngine/SpriteEngine.cs`**
  - Added a `GetImageTexture()` helper that resolves `ImageNode` through
    `ImageLib.TryGetValue` and returns `null` when the node is missing or not loaded.
  - `ImageWidth` / `ImageHeight` now return `0` instead of throwing when the texture is
    absent.
  - All `DoDraw` paths (base `Sprite`, `SpriteSheetMode` override, and tiled
    `BackgroundSprite`) now fetch the texture via `GetImageTexture()` and return early —
    skipping the draw — instead of dereferencing `ImageLib[ImageNode]` and crashing.

### Files modified
- `MapleNecrocer/Client/SpriteEngine/SpriteEngine.cs`

## (2026-08-10) - MiniMap Crash Fix & Muted by Default

### Goal
Fix the `KeyNotFoundException` / `NullReferenceException` shown in the startup error box
(MiniMap lines 263/321 and similar), and start the application muted by default.

### Null-safe MiniMap rendering

- **`MapleNecrocer/Client/UI/MiniMap.cs`**
  - Added a `DrawImage` helper that looks up `Wz.UIImageLib` via `TryGetValue` and skips
    the draw when the texture is missing, eliminating the `KeyNotFoundException` from the
    `Wz.UIImageLib[node]` dictionary indexer.
  - Guarded the `UIEntry` node (`UI/UIWindow.img/MiniMap*`) before `DumpData` and border
    drawing, so a missing UI node no longer crashes.
  - `Map.Img.GetBmp("miniMap/canvas")` can return null; dimensions are now null-safe and
    the canvas draw is skipped when absent.
  - Guarded the `Map/MapHelper.img/minimap`, `mark/*`, and `life`/`portal` node lookups
    before dumping/iterating them.
  - `DoDraw` now checks `PlayerMark` and `RenderTarget` before drawing the player marker.

### Muted by default

- **`MapleNecrocer/Client/Sound.cs`**
  - Changed `Sound.isMute` default from `false` to `true`.
  - `Music.Play` now returns early when `Sound.isMute` is `true`, so background music does
    not play on a fresh start.

### Files modified
- `MapleNecrocer/Client/UI/MiniMap.cs`
- `MapleNecrocer/Client/Sound.cs`

## (2026-08-10) - Startup Fixes & Error Logging

### Goal
Fix build/runtime errors preventing the application from launching, and add error
logging for debugging.

### Missing assembly references removed

- **`WzComparerR2.Common/WzComparerR2.Common.csproj`**
  - Removed `SharpDX.Direct3D9` reference — the DLL does not exist in `Reference/` and
    no code in the project uses it. Resolves MSB3245 warning.

- **`WzComparerR2.WzLib/WzComparerR2.WzLib.csproj`**
  - Removed duplicate `System.Drawing.Common` `Reference` with a relative `bin\Release`
    HintPath that conflicts with the `PackageReference`. The package reference alone is
    sufficient.

### Deferred WZ loading to after form initialization

- **`MapleNecrocer/MainForm.cs`**
  - Moved `AutoLoadWz()` call from the constructor to the `MainForm_Load` event handler.
    The `MapListBox` control is created in `MainForm_Load`, so calling `DumpMapIDs()`
    (which accesses `MapListBox.Handle`) from the constructor caused a
    `NullReferenceException`.
  - Added `_maplePath` field to store the MapleStory path between the constructor and
    the Load event.
  - Added `WriteError(string message)` helper that appends timestamped messages to
    `error.log` in the application startup directory. All `AutoLoadWz` errors now write
    to this file in addition to showing a MessageBox.
  - Wrapped the constructor in a try-catch to log any initialization failures.
  - Set `this.WindowState = FormWindowState.Minimized` so the window opens minimized by
    default.
    > Note: this was later superseded by the "Show Window on Startup" change below, which
    > removed the assignment. The current `MainForm` constructor sets no `WindowState`, so
    > the window opens `Normal`. These two historical entries intentionally document the
    > intermediate flip; the final behavior is the normal startup window.

### Null-safe map ID dump

- **`MapleNecrocer/MainForm.cs`**
  - `DumpMapIDs()` now checks `Wz.HasNode()` before calling `Wz.GetNodes()`. The
    `GetNodes` extension method dereferences `.Nodes` without null-checking, so it would
    throw `NullReferenceException` when the path does not exist in the WZ data.
  - Added null-coalescing (`?.`) and empty-string guards for `Iter2.Text` and
    `Iter.Text` to handle malformed WZ nodes.
  - Wrapped the entire method in a try-catch that writes to `error.log` and shows a
    MessageBox on failure.

### Files modified
- `WzComparerR2.Common/WzComparerR2.Common.csproj`
- `WzComparerR2.WzLib/WzComparerR2.WzLib.csproj`
- `MapleNecrocer/MainForm.cs`

### To be fixed in a future session
- [ ] The `NullReferenceException` / `KeyNotFoundException` errors during WZ loading
      persist. The `error.log` file was not created in testing, suggesting the error
      path is not being reached. Need to run the app and inspect `error.log` (or add
      a file watcher) to capture the actual exception.
- [ ] The `MapleNecrocer/AvatarForm.cs` `MatchesClass` calls were initially changed to
      `MapleCharacter.MatchesClass` but reverted — `MatchesClass` lives in the `Equip`
      class. Ensure no other code references a non-existent `MapleCharacter.MatchesClass`.
- [ ] Consider adding a console output mode (change `OutputType` to `Exe`) so
      `Console.Error.WriteLine` is visible during debugging.

## (2026-08-10) - Avatar Search Crash Fix & Unit Tests

### Goal
Fix the `NullReferenceException` raised when clicking an item in the avatar search tab
whose `Character/<dir><id>.img` does not exist, and replace manual crash verification
with automated unit tests.

### Null-safe avatar preview loading

- **`MapleNecrocer/AvatarForm.cs`**
  - `CellClick` now returns early — and clears `pictureBox1` — when the item has no
    `Character/...` img node, instead of dereferencing `null` and crashing.
  - Added row/column bounds and null-cell guards so clicking a column header or a blank
    row no longer throws.
  - Removed the unused `Name` variable and the commented-out block.
  - `PopulateEquipGrid` skips cached items whose preview bitmap is null before adding them
    to the `ImageListView`.

- **`MapleNecrocer/WzUtils.cs`**
  - `Wz_NodeExtension3.GetBmp` is now null-safe: it returns `null` instead of throwing for
    a null node, a missing path, or a node whose value is not a `Wz_Png`. This closes the
    remaining crash path where the img exists but a sub-node (`info/icon`,
    `default/face`, `default/hairOverHead`) is missing.

### Class filter extracted for testability

- **`MapleNecrocer/Client/MapleCharacter.cs`**
  - Moved `MatchesClass(int reqJob, int selectedClass, bool exclusive)` from `AvatarForm`
    to the `Equip` class and made it public, so the pure job-bitmask logic can be unit
    tested without instantiating the WinForms form.
  - Removed the redundant duplicate `classBit` computation that caused a `CS0136` compile
    error.

- **`MapleNecrocer/AvatarForm.cs`**
  - `PopulateEquipGrid` and `RebuildSearchGrid` now call `Equip.MatchesClass(...)`.

### Unit test project added

- **`MapleNecrocer.Tests/`** (new xUnit project, added to `MapleNecrocer.sln`)
  - `EquipTests`: 41 cases covering `Equip.GetDir`, `Equip.GetPart`, and
    `Equip.MatchesClass` (job bitmask filtering, exclusive mode, and bounds validation).
  - `WzNodeExtensionTests`: `GetBmp` returns `null` for a null node and missing paths
    instead of throwing.
  - Run with `dotnet test MapleNecrocer.Tests/MapleNecrocer.Tests.csproj` (44 tests
    passing).

### Files modified
- `MapleNecrocer/AvatarForm.cs`
- `MapleNecrocer/WzUtils.cs`
- `MapleNecrocer/Client/MapleCharacter.cs`
- `MapleNecrocer.Tests/MapleNecrocer.Tests.csproj` (new)
- `MapleNecrocer.Tests/EquipTests.cs` (new)
- `MapleNecrocer.Tests/WzNodeExtensionTests.cs` (new)
- `MapleNecrocer.sln`

## (2026-08-10) - Mute State Persistence

### Goal
Persist the Mute checkbox state across application restarts so the user does not have to
re-enable it every time they open the Options form.

### Mute state saved to `settings.json`

- **`MapleNecrocer/OptionForm.cs`**
  - Added `using System.IO;` and `using System.Text.Json;`.
  - Added a `SettingsFile` constant (`"settings.json"`) pointing at
    `Application.StartupPath`.
  - Constructor now calls `LoadMuteState()` after `InitializeComponent()` to restore the
    saved state: if the file exists and `IsMute` is `true`, the checkbox is checked,
    `Sound.isMute` is set to `true`, and `Music.Pause()` is called.
  - The existing `FormClosing` hook in `OptionForm_Shown` now also calls `SaveMuteState()`
    before hiding the form, which serializes the current `Sound.isMute` value to
    `settings.json`.
  - Added private `LoadMuteState()` and `SaveMuteState()` helpers and a `MuteSettings`
    POCO (`bool IsMute`). Both helpers swallow exceptions so a missing or corrupt file
    cannot crash the form.

### Files modified
- `MapleNecrocer/OptionForm.cs`

## (2026-08-10)

### Goal
Prevent all in-game keyboard input from firing when the application window is not the
active/focused window (e.g. user alt-tabs away or clicks another window).

### Window-focus gating of keyboard input

- **`MapleNecrocer/Client/SpriteEngine/Keyboard.cs`**
  - Added a `WindowActive` static property (default `true`) that gates all keyboard
    input. `KeyDown`, `KeyUp`, and `KeyPressed` now return `false` immediately when
    `WindowActive` is `false`, so every key — movement, attack, skills, climbing, taming,
    morph, viewer camera, fullscreen toggle, chair removal — is suppressed when the form
    is inactive.

- **`MapleNecrocer/MainForm.cs`**
  - Wired `Deactivate` and `Activated` events on the main form to flip
    `SpriteEngine.Keyboard.WindowActive` to `false` / `true` respectively, so the gate
    tracks real WinForms focus changes.

### Files modified
- `MapleNecrocer/Client/SpriteEngine/Keyboard.cs`
- `MapleNecrocer/MainForm.cs`

## (2026-08-10) - Startup Workflow Improvements

### Goal
Reduce the number of manual steps required to start testing after code changes. Eliminate
the folder selection dialog, auto-load the MapleStory data, and pre-select the fastest
loading map.

### Auto-load MapleStory path on startup

- **`MapleNecrocer/Program.cs`**
  - Added `public static string MaplePath = @"V:\Nexon\maplestory"` as the default path.
  - Updated `Main(string[] args)` to accept and pass the Maple path to `MainForm`.

- **`MapleNecrocer/MainForm.cs`**
  - Updated `MainForm` constructor to accept an optional `maplePath` parameter.
  - Added `AutoLoadWz(string maplePath)` method that:
    - Finds `Base.wz` and `Data.wz` in the specified path
    - Runs WZ loading on a background thread via `Task.Run()` for non-blocking UI
    - Auto-selects map `910000000` (Henesys) for fastest loading
    - Handles errors gracefully with user-friendly messages

### Async WZ loading

- **`MapleNecrocer/MainForm.cs`**
  - `AutoLoadWz()` uses `await Task.Run()` to prevent UI blocking during WZ parsing
  - Loading indicator shown during async operations
  - Added `using System.IO;` for `BinaryReader`/`BinaryWriter` support

### Maple path UI in Options form

- **`MapleNecrocer/OptionForm.cs`**
  - Added `btnSaveMaplePath_Click` handler that opens a folder browser dialog
  - Updates `Program.MaplePath` with the selected path
  - Shows confirmation message with the new path and instructions to restart

### dotnet watch configuration

- **`.vscode/launch.json`**
  - Added ".NET Core Watch" launch configuration for VS Code
  - Configured to run with `--watch` flag for auto-rebuild on file changes
  - Passes MapleStory path as command-line argument

- **`run.bat`**
  - Created batch file for convenient startup from command prompt
  - Builds with `dotnet build -c Release`, then runs with `dotnet run -c Release --no-build`
    using the default MapleStory path
  - Keeps console window open for debugging

### Files modified
- `MapleNecrocer/Program.cs`
- `MapleNecrocer/MainForm.cs`
- `MapleNecrocer/OptionForm.cs`
- `.vscode/launch.json`
- `run.bat` (new file)

## (2026-08-09)

### Goal
Add a new search feature for avatar equipment, allowing the avatar and search grids to
be filtered by class and sorted by required level.

### Exclusive Class Filter

- **`MapleNecrocer/AvatarForm.Designer.cs`**
  - Added `ExclusiveCheckBox` (CheckBox, "Exclusive") positioned directly below
    `ClassComboBox` at `x=692, y=87`, with the same 14pt Tahoma font as surrounding
    controls. Wired `CheckedChanged` to `ExclusiveCheckBox_CheckedChanged`.

- **`MapleNecrocer/AvatarForm.cs`**
  - Updated `MatchesClass(int reqJob, int selectedClass, bool exclusive)` to accept an
    `exclusive` flag. When checked, only items whose `reqJob` is a single bit matching the
    selected class are shown (universal `reqJob == 0` and unusable `reqJob == -1` items are
    excluded). Added bounds validation (`selectedClass < 1 || selectedClass > 5`) so the
    filter degrades safely if the ComboBox is ever extended.
  - Updated `PopulateEquipGrid` and `RebuildSearchGrid` to read `ExclusiveCheckBox.Checked`
    and pass it through to `MatchesClass`.
  - Added `ExclusiveCheckBox_CheckedChanged` handler that calls `ApplyFilterAndSort()`.
  - Disabled the Exclusive checkbox when `ClassComboBox.SelectedIndex == 0` ("All"), since
    exclusive filtering has no meaning in that mode. The disabled state is applied both on
    every `ClassComboBox.SelectedIndexChanged` and on initial form load.

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

- **`MapleNecrocer/AvatarForm.cs`**
  - Removed `MainForm.Instance.ToolTipView.Owner = this;` from the `ImageListView.ItemHover`
    handler and the `Inventory.CellMouseEnter` handler. Setting the tooltip form's owner to
    the avatar form made it a child window; when the tooltip repositioned via
    `Control.MousePosition`, it intercepted mouse messages and caused the `ImageListView`/
    `DataGridView` to lose its selection highlight on hover.

### Files modified
- `MapleNecrocer/AvatarForm.cs`
- `MapleNecrocer/AvatarForm.Designer.cs`
