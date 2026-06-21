# Toolbelt12Slots Workflow

## 1) Human Quick Guide (Easy Read)

### What this DLL does
- Expands player toolbelt public slots from 10 to 12.
- Keeps Shift row offset at 12.
- Adds rebindable hotkeys for slot 11 and slot 12 into game controls UI.
- Persists slot 11/12 keybinds across full game restart.

### What is currently working
- Slot count behavior: 12 slots active.
- Controls UI behavior: slot 11/12 appear and can be rebound.
- Persistence behavior: slot 11/12 survive close and reopen.

### Where code lives
- Main patch file: `_DLL-Projects/DLL_NoEAC-Toolbelt12Slots/ToolbeltPatches.cs`
- Mod entrypoint: `_DLL-Projects/DLL_NoEAC-Toolbelt12Slots/ModAPI.cs`
- Project file: `_DLL-Projects/DLL_NoEAC-Toolbelt12Slots/Toolbelt12Slots.csproj`

### Build
- From repo root:

```powershell
dotnet build "_DLL-Projects/DLL_NoEAC-Toolbelt12Slots/Toolbelt12Slots.csproj" -c Release
```

- Output:
  - `_DLL-Projects/DLL_NoEAC-Toolbelt12Slots/AGFToolbelt12Slots.dll`

### Deploy to game

```powershell
Copy-Item -Force "_DLL-Projects/DLL_NoEAC-Toolbelt12Slots/AGFToolbelt12Slots.dll" "C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-NoEAC-Toolbelt12Slots-v2.0.1/AGFToolbelt12Slots.dll"
```

### Verify deploy hash

```powershell
$src = Get-FileHash "_DLL-Projects/DLL_NoEAC-Toolbelt12Slots/AGFToolbelt12Slots.dll" -Algorithm SHA256
$dst = Get-FileHash "C:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-NoEAC-Toolbelt12Slots-v2.0.1/AGFToolbelt12Slots.dll" -Algorithm SHA256
"SRC=$($src.Hash)"
"DST=$($dst.Hash)"
"MATCH=$($src.Hash -eq $dst.Hash)"
```

### Quick validation checklist
1. Open game and controls UI.
2. Rebind slot 11 and slot 12 to non-default keys.
3. Click Apply.
4. Confirm keys switch to slots 11/12 in gameplay.
5. Close game fully.
6. Reopen game and confirm slot 11/12 bind strings and function persisted.

---

## 2) AI Rebuild Blueprint (Detailed)

### Target behavior contract
- Preserve vanilla behavior as much as possible.
- Add only two extra keyboard toolbelt actions:
  - `InventorySlot11`
  - `InventorySlot12`
- Keep runtime selection and controls UI flow vanilla-driven.
- Persist slot 11/12 rebinds over restart.

### Project constraints
- Framework: `net48`
- Harmony: `0Harmony`
- Runtime refs from game install:
  - `Assembly-CSharp.dll`
  - `UnityEngine.dll`
  - `UnityEngine.CoreModule.dll`
  - `UnityEngine.InputLegacyModule.dll`
  - `InControl.dll`

### Required patch points
Implement these Harmony patches:
1. `Inventory.get_PUBLIC_SLOTS_PLAYMODE`
   - Prefix: return `12`.
2. `Inventory.get_SHIFT_KEY_SLOT_OFFSET`
   - Prefix: return `12`.
3. `PlayerActionsLocal.CreateActions`
   - Postfix: ensure slot 11/12 installed.
4. `PlayerActionsLocal` constructor
   - Postfix: ensure slot 11/12 installed.
5. `XUiC_OptionsControls.createControlsEntries`
   - Prefix: ensure slot 11/12 installed for controls UI.
6. `XUiC_OptionsController.createControlsEntries`
   - Prefix: ensure slot 11/12 installed for controller controls UI.
7. `GameOptionsControls.Save`
   - Prefix: ensure slot 11/12 installed before serialization.
   - Postfix: persist fallback store for slot 11/12.
8. `GameOptionsControls.Load`
   - Prefix: set load guard flag true and ensure installed.
   - Postfix: ensure installed and clear load guard flag in `finally`.
9. `XUiC_OptionsControlsNewBinding.onBindingReceived`
   - Postfix: if binding accepted, persist fallback for slot 11/12.

### Core install algorithm
Inside `ToolbeltExtendedActions.EnsureInstalled(PlayerActionsLocal actions)`:
1. Guard null and `InventorySlot1` null.
2. Retrieve per-action-set state from `ConditionalWeakTable<PlayerActionsLocal, InstalledActions>`.
3. Move cycle actions (`InventorySlotLeft`, `InventorySlotRight`) to a custom group priority after slots.
4. Create slot11 action if missing via reflection call to `PlayerActionSet.CreatePlayerAction(string)`:
   - Try primary name `InventorySlot11`, fallback `Inventory11`.
   - On first create, default bind `Key.Minus`.
5. Create slot12 action similarly:
   - Try primary name `InventorySlot12`, fallback `Inventory12`.
   - On first create, default bind `Key.Equals`.
6. Clone slot metadata from slot1 user data into slot11/12 with action keys:
   - `inpActInventorySlot11Name` / `inpActInventorySlot11Desc`
   - `inpActInventorySlot12Name` / `inpActInventorySlot12Desc`
7. Ensure both are in `InventoryActions` directly after slot10.
8. Ensure both are in `ControllerRebindableActions` list.
9. Restore fallback key combo only when:
   - action was just created, OR
   - controls load guard flag is true.
10. Re-enforce slot order (`slot10`, `slot11`, `slot12`).

### Persistence strategy
Use dual-path persistence:
1. Vanilla path:
   - `GameOptionsControls.Save` stores action-set data.
2. Fallback path:
   - File: `%APPDATA%/7DaysToDie/toolbelt12slots.bindingstore.txt`
   - Format per line:
     - `slot11=includeSize:includeData:excludeSize:excludeData`
     - `slot12=includeSize:includeData:excludeSize:excludeData`

Why fallback exists:
- Dynamic injected actions can be fragile across engine updates/load timing.
- Fallback restores raw `KeyCombo` bits when needed.

### KeyCombo handling details
- Read/write private `KeyCombo` fields via reflection:
  - `includeSize`, `includeData`, `excludeSize`, `excludeData`
- For restore:
  1. Parse stored values.
  2. Build `KeyCombo` from empty keys.
  3. Set four fields.
  4. Compare to current combo; if equal, do nothing.
  5. Remove existing keyboard bindings from target action.
  6. Add new `KeyBindingSource(combo)`.

### Absolute do-not-break rules
1. Do not reintroduce broad slot rewrite logic for slots 1..10.
2. Do not mutate generic toolbelt modifier semantics globally.
3. Do not hook obsolete methods:
   - `GameOptionsManager.SaveControls`
   - `GameOptionsManager.LoadControls`
   These are outdated in current runtime and can cause patch registration failure.
4. Keep runtime slot selection vanilla (no custom `InventorySlotWasPressed` override).

### Initialization and patch registration
- `ModAPI.InitMod` should call:
  - `new Harmony("com.agfprojects.toolbelt12slots").PatchAll();`
- Log patch success/failure to console for diagnosis.

### Log-driven troubleshooting
If persistence fails after restart, check latest game log for:
- `Toolbelt12Slots: Patch registration error:`
- Any `Undefined target method` Harmony exception.

If seen, first verify target API drift in live assembly and update patch targets accordingly.

### Rebuild-from-zero implementation checklist
1. Create project with references in `Toolbelt12Slots.csproj`.
2. Add `ModAPI.cs` patch bootstrap.
3. Add `ToolbeltPatches.cs` with constants, installer, fallback store, and 9 patch classes.
4. Build Release.
5. Deploy DLL to live mod folder.
6. Validate slot 11/12 rebind + restart persistence.

### Acceptance test matrix
1. Default startup:
   - Slot 11 default `-`, slot 12 default `=`.
2. Rebind slot 11/12:
   - New keys show in controls UI.
   - Gameplay switches slots correctly.
3. Apply + restart:
   - Rebinds still present and functional.
4. No regression:
   - Slot 1..10 behavior remains vanilla-like.
   - Shift offset remains 12.
