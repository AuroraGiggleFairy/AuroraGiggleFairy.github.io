# EnhancedAGF ItemTypeIconColor Parity Workflow

Scope:
- Source project: _DLL-Projects/DLL_NoEAC-EnhancedAGF
- Feature area: ItemTypeIconColor parity support in EnhancedAGF
- Live target DLL: C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-NoEAC-EnhancedAGF-v4.1.1/EnhancedAGF.dll

Working methods:
- Keep ItemTypeIconColor parity source files under:
  - src/Utility/ItemTypeIconColor/
- Build using mcs with an explicit file list excluding obj/.vs generated C# files.
- Include required references:
  - UnityEngine.dll
  - UnityEngine.CoreModule.dll
  - Assembly-CSharp.dll
  - LogLibrary.dll
  - netstandard.dll
  - MemoryPack.dll
  - 0Harmony.dll
- Deploy by copying built EnhancedAGF.dll to live target and verify SHA256 match.

Change history:
- 2026-07-01: Added ItemTypeIconColor parity patch set into EnhancedAGF from standalone ItemTypeIconColor logic.
- 2026-07-01: Built EnhancedAGF.dll and deployed to live AGF-NoEAC-EnhancedAGF-v4.1.1 with hash match.
- 2026-07-01: Fixed MissingMethodException from XUiController.get_xui by replacing direct __instance.xui access in ItemTypeIconColor UI patches with reflection-based entityPlayer lookup (supports xui/XUi/_xui and playerUI variants), then rebuilt and deployed with hash match.
- 2026-07-01: Removed Harmony AccessTools.Property/Field lookups from ItemTypeIconColor UI helper to stop runtime warning spam on missing optional members (XUiC_RequiredItemStack xui, LocalPlayerUI EntityPlayer variants); switched to silent native reflection helper and redeployed.
- 2026-07-01: Performance pass: added cached reflection member resolution and a direct XUiController fast path in ItemTypeIconColorUiHelpers, replaced Traverse-based lockTypeIcon access with direct field access, and added ParseGameColor result caching.

Do-not-do notes:
- Do not compile with mcs -recurse:*.cs at project root; this pulls obj-generated AssemblyAttributes files and causes duplicate TargetFrameworkAttribute errors.
- Do not assume netstandard + Assembly-CSharp are sufficient for this feature; MemoryPack.dll is required for CS0012 dependency resolution.
- Do not hard-bind to __instance.xui in these XUi Harmony patches; JIT can fail with MissingMethodException on game versions where XUiController.get_xui is absent/changed.
- Do not use AccessTools.Property/Field for optional UI member probes in hot binding paths; failed probes emit Harmony warning logs repeatedly.
- Do not modify 02_ActiveBuild or 03_ReleaseSource lanes for this task unless explicitly requested.
- If live deploy is blocked/locked, notify immediately and wait for user status update before retrying.
- Do not use Harmony Traverse for lockTypeIcon in hot updateLockTypeIcon postfix when direct field access is available; Traverse adds avoidable reflection overhead.
