# AudioOptionsPlus UI Workflow

## Working Methods
- Keep x2.6 files as read-only reference only:
  - `_x2.6/AGF-NoEAC-AudioOptionsPlus-v1.0.1/...`
- Apply active XML UI edits in live mod folder only:
  - `C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-NoEAC-AudioOptionsPlus-v1.0.1/Config/XUi_Menu/windows.xml`
- Use the AutoRun mod patch pattern as the vanilla-flow reference for v3 menu patch paths:
  - `Mods/AGF-NoEAC-AutoRun-v2.1.1/Config/XUi_Menu/windows.xml`
- For optionsAudio v3 compatibility, patch under:
  - `/windows/window[@name='optionsAudio']/rect[@name='tabs']/rect[@name='content']/rect[@name='tabsContents']`
- Keep Custom Volumes as one scrollable single-column list and include both volume profile controls and sound swap controls in the same grid.
- Build and deploy DLL from project root output:
  - Build: `dotnet build -c Release` in `_DLL-Projects/DLL_NoEAC-AudioOptionsPlus`
  - Deploy: copy `AudioOptionsPlus.dll` to live mod root and verify SHA256 hash match.
- Validation after each structural XML change:
  - Parse check with PowerShell XML cast wrapped in a root node.
  - Quick rect-tag sanity check (open/close totals and no malformed closing tags).

## Change History
- 2026-07-15:
  - Merged old two-page AudioOptionsPlus content into one single-column list in live mod `windows.xml`.
  - Updated live localization tab label to `Custom Volumes` in live `Localization.csv`.
  - Corrected targeting mistake by restoring x2.6 reference files after accidental edits.
  - Built `AudioOptionsPlus.dll` (Release) and deployed to live mod folder with hash verification.

## Do-Not-Do Notes
- Do not edit `_x2.6` reference files when implementing live gameplay/menu changes.
- Do not assume old v2.6 `tabsHeader/tabsContents` path works for v3 without checking vanilla v3 menu structure.
- Do not split Custom Volumes back into separate pages unless explicitly requested.
