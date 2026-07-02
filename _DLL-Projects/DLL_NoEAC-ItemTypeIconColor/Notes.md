# ItemTypeIconColor - Workflow Notes

This is the component workflow file for the ItemTypeIconColor DLL project.

## Scope
- Source folder: _DLL-Projects/DLL_NoEAC-ItemTypeIconColor
- Draft target DLL: 01_Draft/AGF-NoEAC-ItemTypeIconColor-v1.0.3/ItemTypeIconColor.dll
- Live game target DLL: C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-NoEAC-ItemTypeIconColor-v1.0.3/ItemTypeIconColor.dll

## Working Methods
- Build command (mcs) must include these references:
  - UnityEngine.dll
  - UnityEngine.CoreModule.dll
  - Assembly-CSharp.dll
  - netstandard.dll
  - 0Harmony.dll
  - MemoryPack.dll
- After successful build, copy ItemTypeIconColor.dll to both Draft and live targets.
- Verify deploy with SHA256 hash match between source DLL and each target DLL.

## Change History
- 2026-07-01: Rebuilt ItemTypeIconColor.dll from source and deployed to Draft and live targets.
- 2026-07-01: Added MemoryPack.dll reference to build command after CS0012 compile errors.

## Do-Not-Do Notes
- Do not omit MemoryPack.dll from build references, or compile fails with CS0012 for MemoryPack interfaces.
- Do not modify 02_ActiveBuild or 03_ReleaseSource for this task unless explicitly requested.
- If live deployment is blocked or locked, notify immediately and wait for user blocked/locked status update before retrying.
