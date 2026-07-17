# 7D2D v3 Menu Influence Guide (AudioOptionsPlus)

## Scope
- Target system: 7 Days To Die v3 options UI lifecycle.
- Primary window: `XUiC_OptionsAudio` / `XUiC_OptionsDialogBase`.
- This guide documents what actually worked in live testing for custom menu rows.

## Vanilla Lifecycle (Authoritative Chain)
1. Row change triggers option-entry `SelectionChanged`/`OnComboValueChanged`.
2. Entry raises `ValueChanged`.
3. `XUiC_OptionsDialogBase.Event_SettingChanged` runs.
4. `updateUnsavedState()` recomputes changed/default state from `AllOptions`.
5. Apply button path runs `saveChanges()` -> each option `ApplySelection()`.
6. Defaults button path runs `resetToDefaults()` -> each active option `ResetToDefault()`.
7. Discard path runs `discardChanges()` -> each option `DiscardCurrentChange()`.

## Row Type Rules
- `options_combo` maps to legacy option entry (`XUiC_OptionEntryLegacy`) and does not participate in changed-state correctly for this use case.
- `options_combo_custom` is required for custom delegate-driven options.
- For custom options, delegate assignment is mandatory:
  - `GetSettingValue`
  - `DiscardChanges`
  - `ApplyChanges`
  - `ResetDefaults`
  - `IsChangedDelegate`
  - `IsDefaultDelegate`

## ID and Lookup Rules
- Custom option entry IDs are resolved as `Option<name>` in practice.
- Binding should search both:
  - `Option<name>`
  - `<name>`
- Search both local controller tree and window root tree.
- For performance, build one map of `XUiC_OptionEntryCustom` by ViewComponent ID and bind from that map.

## Critical Failure Pattern Found
- A stale Harmony target signature on `Audio.Manager.Play` caused mod init to fail before UI patches ran.
- Symptom in log: `Undefined target method ... Patch_Audio_Manager_Play_Position::Prefix(...)`.
- Correct v3 signature includes trailing volume scale float:
  - `Play(Vector3, string, int, bool, float)`

## Deterministic Fallbacks Kept
- Save fallback: force-apply AOP controller values on options dialog save.
- Discard fallback: force-reload AOP UI values from config on discard.
- Dirty fallback: mark options dialog changed when `OptionAOP*` custom rows change.

## Performance Notes
- Disable trace logging by default in normal builds.
- Avoid per-row repeated hierarchy scans during bind.
- Avoid repeated expensive pending-change checks each frame:
  - Skip fallback check when unsaved already true.
  - Throttle to one check per frame.
  - Cache pending-change result per frame in `XUiC_AudioOptions`.

## Practical Patch Order
1. Ensure Harmony targets match decompiled signatures for current game version.
2. Confirm mod init has no Harmony exceptions in latest output log.
3. Bind custom option delegates in `XUiC_OptionsAudio.Init` and `XUiC_OptionsAudio.OnOpen`.
4. Keep deterministic save/discard/dirty fallbacks.
5. Validate menu responsiveness after diagnostics are disabled.

## Files
- Logic patch: `c:/GitHub/7D2D-Mods/_DLL-Projects/DLL_NoEAC-AudioOptionsPlus/HarmonyPatches.cs`
- UI controller: `c:/GitHub/7D2D-Mods/_DLL-Projects/DLL_NoEAC-AudioOptionsPlus/XUiC_AudioOptionsPlusAudioOptions.cs`
- Live UI XML: `c:/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Mods/AGF-NoEAC-AudioOptionsPlus-v1.0.1/Config/XUi_Menu/windows.xml`
- Workflow history: `c:/GitHub/7D2D-Mods/_DLL-Projects/DLL_NoEAC-AudioOptionsPlus/WORKFLOW-AudioOptionsPlus-UI.md`
