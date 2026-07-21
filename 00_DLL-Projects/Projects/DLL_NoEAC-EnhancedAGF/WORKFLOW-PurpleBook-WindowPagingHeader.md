# EnhancedAGF PurpleBook WindowPagingHeader Workflow

## Working Methods
- Build project: `msbuild EnhancedAGF.csproj /t:Build /p:Configuration=Release` from `00_DLL-Projects/Projects/DLL_NoEAC-EnhancedAGF`.
- Runtime strategy: patch `XUiWindowGroup.OnOpen` and `XUiWindowGroup.OnClose` with Harmony, scoped to `windowGroup.Id == "schematics"` (case-insensitive).
- On schematics open:
  - If `!windowManager.IsWindowOpen("windowpaging")`, call `windowManager.Open("windowpaging", false)`.
  - Call `SetSelected("schematics")` on `XUiC_WindowSelector` in `windowpaging`.
- On schematics close:
  - If `windowManager.IsWindowOpen("windowpaging")`, call `windowManager.Close("windowpaging")`.

## Change History
- 2026-06-28: Added `src/Utility/PurpleBookSchematicsWindowPagingPatch.cs`.
- 2026-06-28: Implemented DLL-only PurpleBook paging lifecycle handling so custom `schematics` group behaves like vanilla paging groups.
- 2026-06-28: Built `EnhancedAGF.csproj` Release successfully after adapting to current API (`Id` field, `Open/Close`, `IsWindowOpen`).
- 2026-06-28: Deployed test DLL to live game path `C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-NoEAC-EnhancedAGF-v4.1.0/EnhancedAGF.dll` with SHA256 hash match.

## Do-Not-Do Notes
- Do not assume `WindowSelector` has a hardcoded vanilla-only button list; it dynamically collects clickable header children.
- Do not rely on XML-only changes for this specific persistence issue; custom `schematics` group uses base controller and does not reopen `windowpaging` on its own.
- Do not assume decompiled helper method names are present in the current assembly build (`OpenIfNotOpen`, `CloseIfOpen`, `ID` may differ).
- Do not modify `02_ActiveBuild` or `03_ReleaseSource` for this fix unless explicitly requested.
