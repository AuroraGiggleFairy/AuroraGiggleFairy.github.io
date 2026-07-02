# WORKFLOW - AutoRun DLL

## Working Methods
- Source of truth: _DLL-Projects/DLL_NoEAC-AutoRun/AutoRunPatches.cs
- Build command:
  - dotnet build _DLL-Projects/DLL_NoEAC-AutoRun/AutoRun.csproj -c Release
- Output artifact:
  - _DLL-Projects/DLL_NoEAC-AutoRun/AutoRun.dll
- Live deploy target (when game is closed and deploy is requested):
  - C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-NoEAC-AutoRun-v2.0.2/AutoRun.dll

## Change History
- 2026-07-02: Updated keyboard activation behavior to toggle AutoRun on/off directly via the activation key.
  - Vehicle path: activation-edge now toggles VehicleEnabled and VehicleSprintLocked.
  - On-foot path: activation-edge now toggles OnFootEnabled and OnFootSprintLocked.
  - Added guard so forward-edge processing does not immediately override same-frame activation toggle.
- 2026-07-02: Deployed built DLL to live AutoRun mod after game close.
  - Target: C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-NoEAC-AutoRun-v2.0.2/AutoRun.dll
- 2026-07-02: Updated live localization key inpActAutoRunDesc translations to toggle wording parity with English.
  - File: C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-NoEAC-AutoRun-v2.0.2/Config/Localization.csv
- 2026-07-02: Refined German wording for inpActAutoRunDesc to explicit on/off phrasing.
  - German line updated to: "Schaltet Auto-Run ein/aus."
- 2026-07-02: Performed full localization clarity pass for inpActAutoRunDesc first-line toggle wording.
  - Updated to explicit on/off wording where needed (ES, FR, JA, KO, PL, PT-BR, RU, ZH-CN, ZH-TW).
  - Kept already-clear first lines unchanged (DE, IT, TR).

## Do-Not-Do Notes
- Do not edit 02_ActiveBuild or 03_ReleaseSource lanes unless explicitly requested.
- Do not push DLL to live mod while the game is open/locking files.
- Do not change controller-mode activation flow when making keyboard-only behavior updates.
- Do not remove activation-edge guard from forward-edge logic, or same-frame re-enable can occur after toggle-off.
