# CosmeticLockIcon DLL Workflow

Scope:
- Source project: 00_DLL-Projects/Projects/DLL_NoEAC-CosmeticLockIcon
- Component: AGF-NoEAC-CosmeticLockIcon
- Draft target DLL: 01_Draft/AGF-NoEAC-CosmeticLockIcon-v3.0.0/CosmeticLockIcon.dll
- Live target DLL: C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-NoEAC-CosmeticLockIcon-v3.0.0/CosmeticLockIcon.dll

Working methods:
- Build with mcs using explicit references:
  - UnityEngine.dll
  - UnityEngine.CoreModule.dll
  - Assembly-CSharp.dll
  - netstandard.dll
  - MemoryPack.dll
  - 0Harmony.dll
- Use UI compatibility helper reflection for player resolution in binding patches (avoid direct get_xui dependency).
- Keep icon/tint overrides guarded by magnitude detection so magnitude/star indicator items are not overridden.
- Deploy by copying built DLL to draft and live targets, then verify SHA256 parity.

Change history:
- 2026-07-01: Added UI compatibility helper to avoid direct xui access in CosmeticLockIcon patches.
- 2026-07-01: Added magnitude guard so cosmetic icon/tint overrides skip armor entries that use magnitude/star indicators.
- 2026-07-01: Rebuilt DLL with MemoryPack reference and deployed to draft/live v3.0.0 targets.
- 2026-07-01: Resolved runtime MissingMethodException on recipeicon by disabling legacy live folder AGF-NoEAC-CosmeticLockIcon-v1.0.2 (moved out of live Mods). Verified no active live DLL contains both get_xui and XUiC_RecipeEntry markers.
- 2026-07-01: Performance pass: added cached reflection member resolution in CosmeticLockIconUiHelpers, cached magnitude capability checks in ArmorIconUIHarmonyPatches, and early binding-name exits in UI prefixes to skip unnecessary helper work.

Do-not-do notes:
- Do not use direct __instance.xui access in these UI Harmony patches; API differences can trigger MissingMethodException at runtime.
- Do not use tuple/pattern syntax in this project unless confirmed mcs-compatible.
- Do not omit MemoryPack.dll reference when compiling against current Assembly-CSharp dependencies.
- Do not keep multiple CosmeticLockIcon versions active in live Mods at the same time; old versions can still patch recipe bindings and reintroduce get_xui failures.
- Do not resolve entity-player reflection before binding-name guards in GetBindingValueInternal prefixes; gate on target bindings first to reduce per-frame UI overhead.
